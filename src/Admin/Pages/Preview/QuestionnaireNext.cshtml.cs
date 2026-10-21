using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Preview;

[Authorize]
[IgnoreAntiforgeryToken]
public class QuestionnaireNext(IApiClient apiClient, ILogger<QuestionnaireNext> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    [BindProperty] public required GetNextStateRequest NextStateRequest { get; set; }
    [BindProperty] public required bool IsEmbedded { get; set; }
    [BindProperty] public string AnswerHistory { get; set; } = "{}"; // JSON string to store answer history
    public required QuestionnaireInfoDto Questionnaire { get; set; }
    public required DestinationDto Destination { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);

        if (questionnaire == null)
        {
            return NotFound();
        }

        Questionnaire = questionnaire;
        IsEmbedded = Embed;

        try
        {
            Destination = new DestinationDto
            {
                Type = DestinationType.Question,
                Question = await apiClient.GetInitialQuestion(questionnaire.Id, true)
            };
        }
        catch (GetToAnAnswerApiException)
        {
            ModelState.AddModelError(string.Empty, "Preview is not available until content has been added.");
            return Page();
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        string? buttonValue = Request.Form["submitButton"];
        return buttonValue switch
        {
            "continueButton" => await HandleContinueAsync(),
            "backButton" => await HandleBackAsync(),
            "customContentBackButton" => await HandleCustomContentAsync(),
            "backButtonForStartPage" => await HandleBackButtonForStartPageAsync(),
            _ => HandleUnknownButton(buttonValue ?? string.Empty)
        };
    }

    private async Task<IActionResult> HandleContinueAsync()
    {
        try
        {
            var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);
            if (questionnaire == null)
                return NotFound();

            await InitializeDestination(questionnaire);

            if (!ModelState.IsValid)
                return await HandleValidationErrorAsync();

            SaveCurrentAnswersToHistory();

            var destination = await apiClient.GetNextState(Questionnaire.Id, NextStateRequest, true);
            if (destination == null)
                return NotFound();

            if (destination is { Type: DestinationType.ExternalLink, Content: not null })
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
            var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);
            if (questionnaire == null)
                return NotFound();

            Questionnaire = questionnaire;
            IsEmbedded = Embed;

            SaveCurrentAnswersToHistory();

            var previousDestination = await apiClient.GetPreviousState(Questionnaire.Id, NextStateRequest.CurrentQuestionOrder, preview: true);
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

    private async Task<IActionResult> HandleCustomContentAsync()
    {
        try
        {
            var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);
            if (questionnaire == null)
                return NotFound();

            Questionnaire = questionnaire;
            Destination = new DestinationDto
            {
                Type = DestinationType.Question,
                Question = await apiClient.GetLastQuestion(Questionnaire.Id, true)
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
            logger.LogError(e, "Error loading last question. Error: {EMessage}", e.Message);
            throw;
        }
    }

    private async Task<IActionResult> HandleBackButtonForStartPageAsync()
    {
        try
        {
            // Redirect to the questionnaire start page in preview mode
            return Redirect($"/admin/questionnaires/{QuestionnaireId}/start-preview?embed={Embed}");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error navigating to start page. Error: {EMessage}", e.Message);
            throw;
        }
    }

    private IActionResult HandleUnknownButton(string buttonValue)
    {
        logger.LogWarning("Unknown button value submitted: {ButtonValue}", buttonValue);
        return BadRequest("Unknown action");
    }

    private async Task<IActionResult> HandleValidationErrorAsync()
    {
        ModelState.Clear();
        ValidateAndAddErrors();
        return await Task.FromResult(Page());
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

    private async Task InitializeDestination(QuestionnaireInfoDto questionnaire)
    {
        Questionnaire = questionnaire;
        Destination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = await apiClient.GetCurrentQuestion(Questionnaire.Id, NextStateRequest.CurrentQuestionId, true)
        };
        IsEmbedded = Embed;
    }

    private void ValidateAndAddErrors()
    {
        if (Destination.Question is not null && NextStateRequest.ValidateAnswers)
        {
            switch (Destination.Question.Type)
            {
                case QuestionType.MultiSelect:
                    ModelState.AddModelError("NextStateRequest.SelectedAnswerIds", "Select at least one answer");
                    break;
                case QuestionType.SingleSelect:
                    ModelState.AddModelError("NextStateRequest.SelectedAnswerIds", "Select an answer");
                    break;
                case QuestionType.DropdownSelect:
                    ModelState.AddModelError("NextStateRequest.SelectedAnswerIds", "Select an option");
                    break;
            }
        }
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
