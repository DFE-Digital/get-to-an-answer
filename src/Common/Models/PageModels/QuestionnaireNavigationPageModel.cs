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
    [BindProperty] public string AnswerHistory { get; set; } = "{}"; // JSON string to store answer history
    public required QuestionnaireInfoDto Questionnaire { get; set; }
    public required DestinationDto Destination { get; set; }

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

            var previousDestination = await ApiClient.GetPreviousState(Questionnaire.Id, NextStateRequest.CurrentQuestionOrder, GetPreviewFlag());
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
    /// Handles custom content back button - returns to last question in questionnaire
    /// </summary>
    private async Task<IActionResult> HandleCustomContentAsync()
    {
        try
        {
            var questionnaire = await GetQuestionnaireAsync();
            if (questionnaire == null)
                throw new InvalidOperationException("Questionnaire not found");

            Questionnaire = questionnaire;
            Destination = new DestinationDto
            {
                Type = DestinationType.Question,
                Question = await ApiClient.GetLastQuestion(Questionnaire.Id, GetPreviewFlag())
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
            AnswerHistory = AnswerHistoryHelper.SaveAnswersToHistory(AnswerHistory, NextStateRequest.CurrentQuestionId, NextStateRequest.SelectedAnswerIds);
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
        var restoredAnswers = GetAnswersFromHistory(previousQuestionId);
        previousDestination.PreviouslySelectedAnswerIds = restoredAnswers;

        Logger.LogInformation("OnPostBack: Question={QuestionId}, PreviouslySelectedAnswers={AnswerCount}, AnswerHistory={HistoryJson}",
            previousQuestionId, previousDestination.PreviouslySelectedAnswerIds.Count, AnswerHistory);

        Destination = previousDestination;

        NextStateRequest.CurrentQuestionId = previousQuestionId;
        NextStateRequest.CurrentQuestionOrder = previousDestination.Question?.Order ?? 0;
        NextStateRequest.SelectedAnswerIds = restoredAnswers;
    }

    /// <summary>
    /// Retrieves previously selected answers for a question from the history
    /// </summary>
    protected List<Guid> GetAnswersFromHistory(Guid questionId)
    {
        return AnswerHistoryHelper.GetAnswersFromHistory(AnswerHistory, questionId);
    }

    /// <summary>
    /// Gets the questionnaire based on route parameters.
    /// Must be implemented by derived classes to provide route-specific logic.
    /// </summary>
    protected abstract Task<QuestionnaireInfoDto?> GetQuestionnaireAsync();

    /// <summary>
    /// Gets the initial question for first-time page load.
    /// Must be implemented by derived classes to provide route-specific logic.
    /// </summary>
    protected abstract Task<QuestionDto?> GetInitialQuestionAsync();

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
