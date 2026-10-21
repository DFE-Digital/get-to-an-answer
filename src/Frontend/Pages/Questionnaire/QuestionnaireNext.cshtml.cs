using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Pages.Questionnaire;

[IgnoreAntiforgeryToken]
public class QuestionnaireNext(IApiClient apiClient, ILogger<QuestionnaireNext> logger) : QuestionnairesPageModel
{
    [BindProperty] public required GetNextStateRequest NextStateRequest { get; set; }
    [BindProperty] public required bool IsEmbedded { get; set; }
    [BindProperty] public string AnswerHistory { get; set; } = "{}"; // JSON string to store answer history
    public required QuestionnaireInfoDto Questionnaire { get; set; }
    public required DestinationDto Destination { get; set; }
   
    [FromRoute(Name = "questionnaireSlug")] 
    public new string? QuestionnaireSlug { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }

    public async Task<IActionResult> OnGet()
    {
        HttpContext.Features.Set(new QuestionnaireRunFeature
        {
            IsEmbedded = Embed
        });
        if (QuestionnaireSlug == null)
            return NotFound();
        var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireSlug);
        if (questionnaire == null)
            return NotFound();

        Questionnaire = questionnaire;
        IsEmbedded = Embed;
        Destination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = await apiClient.GetInitialQuestion(questionnaire.Id)
        };
        
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        string? buttonValue = Request.Form["submitButton"];
        return buttonValue switch
        {
            "continueButton" => await HandleContinueAsync(),
            "backButton" => await HandleBackAsync(),
            _ => HandleUnknownButton(buttonValue ?? string.Empty)
        };
    }

    private async Task<IActionResult> HandleContinueAsync()
    {
        try
        {
            await InitializeQuestionnaireAsync();

            if (!ModelState.IsValid)
                return await HandleValidationErrorAsync();

            var destination = await apiClient.GetNextState(Questionnaire.Id, NextStateRequest);
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
            logger.LogError(e, "Error load next question or final destination. Error: {EMessage}", e.Message);
            throw;
        }
    }

    private async Task<IActionResult> HandleBackAsync()
    {
        try
        {
            await InitializeQuestionnaireAsync();

            SaveCurrentAnswersToHistory();

            var previousDestination = await apiClient.GetPreviousState(Questionnaire.Id, NextStateRequest.CurrentQuestionOrder);
            if (previousDestination == null)
                return NotFound();

            RestorePreviousQuestion(previousDestination);
            ModelState.Clear();
            return Page();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading previous question. Error: {EMessage}", e.Message);
            throw;
        }
    }

    private IActionResult HandleUnknownButton(string buttonValue)
    {
        logger.LogWarning("Unknown button value submitted: {ButtonValue}", buttonValue);
        return BadRequest("Unknown action");
    }

    private async Task InitializeQuestionnaireAsync()
    {
        HttpContext.Features.Set(new QuestionnaireRunFeature
        {
            IsEmbedded = Embed
        });

        if (QuestionnaireSlug == null)
            throw new InvalidOperationException("QuestionnaireSlug is required");

        var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireSlug);
        if (questionnaire == null)
            throw new InvalidOperationException("Questionnaire not found");

        Questionnaire = questionnaire;
        Destination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = await apiClient.GetCurrentQuestion(Questionnaire.Id, NextStateRequest.CurrentQuestionId)
        };
        IsEmbedded = Embed;
    }

    private async Task<IActionResult> HandleValidationErrorAsync()
    {
        ModelState.Clear();

        if (Destination.Question is null || !NextStateRequest.ValidateAnswers)
            return Page();

        AddValidationError(Destination.Question.Type);
        return Page();
    }

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

    private void SaveCurrentAnswersToHistory()
    {
        if (NextStateRequest.CurrentQuestionId != Guid.Empty && NextStateRequest.SelectedAnswerIds.Count > 0)
            SaveAnswersToHistory(NextStateRequest.CurrentQuestionId, NextStateRequest.SelectedAnswerIds);
    }

    private IActionResult HandleExternalLinkRedirect(string content)
    {
        var url = Uri.TryCreate(content, UriKind.Absolute, out var uri)
            ? uri.ToString()
            : "https://" + content;

        return Redirect(url);
    }

    private void RestorePreviousQuestion(DestinationDto previousDestination)
    {
        var previousQuestionId = previousDestination.Question?.Id ?? Guid.Empty;
        var restoredAnswers = GetAnswersFromHistory(previousQuestionId);
        previousDestination.PreviouslySelectedAnswerIds = restoredAnswers;

        logger.LogInformation("OnPostBack: Question={QuestionId}, PreviouslySelectedAnswers={AnswerCount}, AnswerHistory={HistoryJson}",
            previousQuestionId, previousDestination.PreviouslySelectedAnswerIds.Count, AnswerHistory);

        Destination = previousDestination;

        NextStateRequest.CurrentQuestionId = previousQuestionId;
        NextStateRequest.CurrentQuestionOrder = previousDestination.Question?.Order ?? 0;
        NextStateRequest.SelectedAnswerIds = restoredAnswers;
    }

    /// <summary>
    /// Saves the currently selected answers to the answer history for the current question
    /// </summary>
    private void SaveAnswersToHistory(Guid questionId, List<Guid> selectedAnswerIds)
    {
        try
        {
            // Validate inputs
            if (questionId == Guid.Empty)
            {
                logger.LogWarning("SaveAnswersToHistory called with empty questionId");                
                return;
            }

            if (selectedAnswerIds == null)
            {
                logger.LogWarning("SaveAnswersToHistory called with null selectedAnswerIds");
                return;
            }

            // Ensure AnswerHistory is not null
            if (string.IsNullOrEmpty(AnswerHistory))
            {
                AnswerHistory = "{}";
            }

            var options = new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
            var history = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<Guid>>>(AnswerHistory, options) ?? new();
            history[questionId.ToString()] = selectedAnswerIds;
            AnswerHistory = System.Text.Json.JsonSerializer.Serialize(history, options);
            logger.LogInformation("Saved answers to history for question {QuestionId}. Total questions in history: {Count}", questionId, history.Count);            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving answers to history for question {QuestionId}: {ExceptionMessage}", questionId, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves previously selected answers for a question from the history
    /// </summary>
    private List<Guid> GetAnswersFromHistory(Guid questionId)
    {
        try
        {
            // Validate input
            if (questionId == Guid.Empty)
            {
                logger.LogWarning("GetAnswersFromHistory called with empty questionId");
                return new();
            }

            // Ensure AnswerHistory is not null
            if (string.IsNullOrEmpty(AnswerHistory))
            {
                AnswerHistory = "{}";
            }

            var options = new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true
            };
            var history = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<Guid>>>(AnswerHistory, options) ?? new();
            var questionKey = questionId.ToString();
            var hasAnswers = history.TryGetValue(questionKey, out var answers);
            logger.LogInformation("Retrieved answers for question {QuestionId}: Found={Found}, Answers={Count}", questionId, hasAnswers, answers?.Count ?? 0);            
            return hasAnswers && answers != null ? answers : new();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving answers from history for question {QuestionId}: {ExceptionMessage}", questionId, ex.Message);
            return new();
        }
    }
}
