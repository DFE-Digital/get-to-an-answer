using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Common.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Common.Models.PageModels;

/// <summary>
/// Base class for questionnaire navigation pages that provides shared logic for handling
/// questionnaire navigation, form submission, and answer history management.
/// Derived classes must implement abstract methods to provide route-specific behavior.
/// </summary>
public abstract class QuestionnaireNavigationPageModel : QuestionnairesPageModel
{
    protected readonly IApiClient ApiClient;
    protected readonly ILogger Logger;

    [BindProperty] public required GetNextStateRequest NextStateRequest { get; set; }
    [BindProperty] public required bool IsEmbedded { get; set; }
    [BindProperty] public string? AnswerHistory { get; set; } = string.Empty; // Compact hex-encoded answer history
    [BindProperty] public string? CurrentSelectedAnswers { get; set; } = string.Empty; // Compact hex-encoded selected answers per question
    public required QuestionnaireInfoDto Questionnaire { get; set; }
    public required DestinationDto Destination { get; set; }
    public List<ResultsSummaryRow> ResultsSummaryRows { get; set; } = new();
    
    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }

    protected QuestionnaireNavigationPageModel(IApiClient apiClient, ILogger<QuestionnaireNavigationPageModel> logger)
    {
        ApiClient = apiClient;
        Logger = logger;
    }

    /// <summary>
    /// Handles the form submission by routing to appropriate handler based on button value
    /// </summary>
    public async Task<IActionResult> OnPost()
    {
        if (Request.Form.ContainsKey("changeQuestionId"))
            return await HandleChangeAsync();

        string? buttonValue = Request.Form["submitButton"];
        return buttonValue switch
        {
            "continueButton" => await HandleContinueAsync(),
            "backButton" => await HandleBackAsync(),
            "customContentBackButton" => await HandleCustomContentAsync(),
            "backButtonForStartPage" => await HandleStartPageBackAsync(),            
            _ => HandleUnknownButton(buttonValue ?? string.Empty)
        };
    }

    /// <summary>
    /// Handles the Change button on the results summary - renders the selected question using the posted history
    /// </summary>
    private async Task<IActionResult> HandleChangeAsync()
    {
        if (!Guid.TryParse(Request.Form["changeQuestionId"], out var questionId))
            return BadRequest("Invalid question");

        var questionnaire = await GetQuestionnaireAsync();
        if (questionnaire == null)
            return NotFound();

        Questionnaire = questionnaire;
        IsEmbedded = Embed;        
        ResultsSummaryRows = new();

        var question = await ApiClient.GetCurrentQuestion(Questionnaire.Id, questionId, GetPreviewFlag());
        if (question == null)
            return NotFound();

        RestorePreviousQuestion(new DestinationDto { Type = DestinationType.Question, Question = question });
        ModelState.Clear();
        return Page();
    }

    /// <summary>
    /// Handles the continue/next button submission - validates answers and navigates to next state
    /// </summary>
    protected virtual async Task<IActionResult> HandleContinueAsync()
    {
        try
        {
            await InitializeQuestionnaireAsync();

            if (!ModelState.IsValid)
                return await HandleValidationErrorAsync();            

            var destination = await ApiClient.GetNextState(Questionnaire.Id, NextStateRequest, GetPreviewFlag());
            if (destination == null)
                return NotFound();

            SaveCurrentAnswersToHistory();
            if (destination.Question is not null)
                destination.PreviouslySelectedAnswerIds = AnswerHistoryHelper.GetSelectedAnswers(CurrentSelectedAnswers, destination.Question.Id);

            if (destination is { Type: DestinationType.CustomContent, ShowResultsSummary: true } or {Type: DestinationType.ExternalLink })
                return await ShowResultsSummaryAsync(destination);

            if (!Embed && destination is { Type: DestinationType.ExternalLink, Content: not null })
                return HandleExternalLinkRedirect(destination.Content);

            IsEmbedded = Embed;
            Destination = destination;            
            NextStateRequest = new GetNextStateRequest();
            return Page();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error load next question or final destination. Error: {EMessage}", e.Message);
            throw;
        }
    }

    /// <summary>
    /// Handles the back button - restores previous question from answer history
    /// </summary>
    protected virtual async Task<IActionResult> HandleBackAsync()
    {
        try
        {
            await InitializeQuestionnaireAsync();

            SaveCurrentAnswersToHistory();

            DestinationDto? previousDestination;
            var previousQuestionId = AnswerHistoryHelper.GetPreviousQuestionId(AnswerHistory, NextStateRequest.CurrentQuestionId);
            if (previousQuestionId is { } questionId)
            {
                var previousQuestion = await ApiClient.GetCurrentQuestion(Questionnaire.Id, questionId, GetPreviewFlag());
                previousDestination = previousQuestion == null
                    ? null
                    : new DestinationDto { Type = DestinationType.Question, Question = previousQuestion };
            }
            else
            {
                previousDestination = await ApiClient.GetPreviousState(Questionnaire.Id, NextStateRequest.CurrentQuestionOrder, GetPreviewFlag());
            }

            if (previousDestination == null)
                return NotFound();

            RestorePreviousQuestion(previousDestination);
            ModelState.Clear();
            return Page();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error loading previous question. Error: {EMessage}", e.Message);
            throw;
        }
    }

    /// <summary>
    /// Handles custom content back button - returns to the last question on the user's route taken from answer history
    /// </summary>
    private async Task<IActionResult> HandleCustomContentAsync()
    {
        try
        {
            var questionnaire = await GetQuestionnaireAsync();
            if (questionnaire == null)
                throw new InvalidOperationException("Questionnaire not found");

            Questionnaire = questionnaire;
            var routeQuestionIds = AnswerHistoryHelper.GetAnsweredQuestionIds(AnswerHistory);
            Destination = new DestinationDto
            {
                Type = DestinationType.Question,
                Question = routeQuestionIds.Count > 0
                    ? await ApiClient.GetCurrentQuestion(Questionnaire.Id, routeQuestionIds[^1])
                    : await ApiClient.GetLastQuestion(Questionnaire.Id)
            };

            if (Destination?.Question == null)
                return NotFound();

            IsEmbedded = Embed;

            RestorePreviousQuestion(Destination);
            ModelState.Clear();
            return Page();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error loading last question. Error: {EMessage}", e.Message);
            throw;
        }
    }

    /// <summary>
    /// Handles the back to start page button - redirects to appropriate start page route
    /// </summary>
    private async Task<IActionResult> HandleStartPageBackAsync()
    {
        try
        {
            var startPageRoute = await GetStartPageRouteAsync();
            return Redirect(startPageRoute);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error navigating to start page. Error: {EMessage}", e.Message);
            throw;
        }
    }

    /// <summary>
    /// Handles unknown button submissions
    /// </summary>
    private IActionResult HandleUnknownButton(string buttonValue)
    {
        Logger.LogWarning("Unknown button value submitted: {ButtonValue}", buttonValue);
        return BadRequest("Unknown action");
    }

    /// <summary>
    /// Displays the results summary before the final destination is shown
    /// </summary>
    private async Task<IActionResult> ShowResultsSummaryAsync(DestinationDto destination)
    {
        ResultsSummaryRows = await BuildResultsSummaryRowsAsync();        
        Destination = destination;
        IsEmbedded = Embed;
        return Page();
    }

    /// <summary>
    /// Decodes the selected answers into human readable question and answer text
    /// </summary>
    private async Task<List<ResultsSummaryRow>> BuildResultsSummaryRowsAsync()
    {
        var rows = new List<ResultsSummaryRow>();
        foreach (var questionId in AnswerHistoryHelper.GetAnsweredQuestionIds(CurrentSelectedAnswers))
        {
            var question = await ApiClient.GetCurrentQuestion(Questionnaire.Id, questionId, GetPreviewFlag());
            if (question == null)
                continue;

            var answers = new List<string>();
            foreach (var answerId in AnswerHistoryHelper.GetSelectedAnswers(CurrentSelectedAnswers, questionId))
            {
                var answer = question.Answers.FirstOrDefault(a => a.Id == answerId);
                if (answer != null)
                    answers.Add(answer.Content);
            }

            rows.Add(new ResultsSummaryRow(questionId, question.Content, answers));
        }

        return rows;
    }

    /// <summary>
    /// Initializes the questionnaire and current question for post-back operations
    /// </summary>
    private async Task InitializeQuestionnaireAsync()
    {
        var questionnaire = await GetQuestionnaireAsync();
        if (questionnaire == null)
            throw new InvalidOperationException("Questionnaire not found");

        Questionnaire = questionnaire;
        Destination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = await ApiClient.GetCurrentQuestion(Questionnaire.Id, NextStateRequest.CurrentQuestionId, GetPreviewFlag())
        };
        IsEmbedded = Embed;
    }

    /// <summary>
    /// Handles validation errors by adding appropriate error messages based on question type
    /// </summary>
    private async Task<IActionResult> HandleValidationErrorAsync()
    {
        ModelState.Clear();

        if (Destination.Question is null || !NextStateRequest.ValidateAnswers)
            return Page();

        AddValidationError(Destination.Question.Type);
        return await Task.FromResult(Page());
    }

    /// <summary>
    /// Adds appropriate validation error message based on question type
    /// </summary>
    private void AddValidationError(QuestionType questionType)
    {
        var errorMessage = questionType switch
        {
            QuestionType.MultiSelect => "Select at least one answer",
            QuestionType.SingleSelect => "Select an answer",
            QuestionType.DropdownSelect => "Select an option",
            _ => "Please provide an answer"
        };

        ModelState.AddModelError("NextStateRequest.SelectedAnswerIds", errorMessage);
    }

    /// <summary>
    /// Saves current question's selected answers to the answer history
    /// </summary>
    protected void SaveCurrentAnswersToHistory()
    {
        if (NextStateRequest.CurrentQuestionId != Guid.Empty && NextStateRequest.SelectedAnswerIds.Count > 0)
                {
                    AnswerHistory = AnswerHistoryHelper.SaveAnswersToHistory(AnswerHistory, NextStateRequest.CurrentQuestionId, NextStateRequest.SelectedAnswerIds);
                    CurrentSelectedAnswers = AnswerHistoryHelper.UpsertAnswers(CurrentSelectedAnswers, NextStateRequest.CurrentQuestionId, NextStateRequest.SelectedAnswerIds);
                }
            }

    /// <summary>
    /// Handles external link redirects with protocol validation
    /// </summary>
    protected IActionResult HandleExternalLinkRedirect(string content)
    {
        var url = Uri.TryCreate(content, UriKind.Absolute, out var uri)
            ? uri.ToString()
            : "https://" + content;

        return Redirect(url);
    }

    /// <summary>
    /// Restores the previous question and its previously selected answers from history
    /// </summary>
    protected void RestorePreviousQuestion(DestinationDto previousDestination)
    {
        var previousQuestionId = previousDestination.Question?.Id ?? Guid.Empty;
        var restoredAnswers = AnswerHistoryHelper.GetSelectedAnswers(CurrentSelectedAnswers, previousQuestionId);
        previousDestination.PreviouslySelectedAnswerIds = restoredAnswers;

        Logger.LogInformation("OnPostBack: Question={QuestionId}, PreviouslySelectedAnswers={AnswerCount}, AnswerHistory={HistoryJson}",
            previousQuestionId, previousDestination.PreviouslySelectedAnswerIds.Count, AnswerHistory);

        Destination = previousDestination;

        NextStateRequest.CurrentQuestionId = previousQuestionId;
        NextStateRequest.CurrentQuestionOrder = previousDestination.Question?.Order ?? 0;
        NextStateRequest.SelectedAnswerIds = restoredAnswers;
    }    

    /// <summary>
    /// Gets the questionnaire based on route parameters.
    /// Must be implemented by derived classes to provide route-specific logic.
    /// </summary>
    protected abstract Task<QuestionnaireInfoDto?> GetQuestionnaireAsync();

    /// <summary>
    /// Gets the start page route for back-to-start navigation.
    /// Must be implemented by derived classes to provide route-specific URLs.
    /// </summary>
    protected abstract Task<string> GetStartPageRouteAsync();

    /// <summary>
    /// Gets the preview flag for API calls.
    /// Default is false for non-preview implementations.
    /// Override in Admin preview class to return true.
    /// </summary>
    protected virtual bool GetPreviewFlag() => false;
}
