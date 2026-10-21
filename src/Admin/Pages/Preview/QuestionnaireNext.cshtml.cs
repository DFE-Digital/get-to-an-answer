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

    public async Task<IActionResult> OnPostBack()
    {
        try
        {
            var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);
            if (questionnaire == null)
                return NotFound();

            Questionnaire = questionnaire;
            IsEmbedded = Embed;

            // Save the current question's answers to history before going back
            // Save even if empty - this ensures answer history stays accurate
            if (NextStateRequest.CurrentQuestionId != Guid.Empty)
            {
                SaveAnswersToHistory(NextStateRequest.CurrentQuestionId, NextStateRequest.SelectedAnswerIds);
            }

            // Get the previous question based on the current question's order
            var previousDestination = await apiClient.GetPreviousState(Questionnaire.Id, NextStateRequest.CurrentQuestionOrder, preview: true);
            if (previousDestination == null)
                return NotFound();

            // Restore previously selected answers for this question from history
            var previousQuestionId = previousDestination.Question?.Id ?? Guid.Empty;
            var restoredAnswers = GetAnswersFromHistory(previousQuestionId);
            previousDestination.PreviouslySelectedAnswerIds = restoredAnswers;

            logger.LogInformation("OnPostBack: Question={QuestionId}, PreviouslySelectedAnswers={AnswerCount}, AnswerHistory={HistoryJson}", 
                previousQuestionId, previousDestination.PreviouslySelectedAnswerIds.Count, AnswerHistory);

            Destination = previousDestination;

            // Pre-populate NextStateRequest with the restored answers so validation passes
            NextStateRequest.CurrentQuestionId = previousQuestionId;
            NextStateRequest.CurrentQuestionOrder = previousDestination.Question?.Order ?? 0;
            NextStateRequest.SelectedAnswerIds = restoredAnswers;

            // Clear ModelState so the pre-populated SelectedAnswerIds is used for validation
            // This is necessary because when back is clicked, the form doesn't actually submit
            // the selected checkboxes/radio values, so ModelState would be invalid otherwise
            ModelState.Clear();
            //TryUpdateModelAsync(NextStateRequest);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading previous question. Error: {EMessage}", e.Message);
            throw;
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
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
                Question = await apiClient.GetCurrentQuestion(Questionnaire.Id, NextStateRequest.CurrentQuestionId, true)
            };
            IsEmbedded = Embed;
            
            if (!ModelState.IsValid)
            {
                ModelState.Clear();
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
                return Page();
            }

            // Save the current answers to history before moving to the next question
            if (NextStateRequest.CurrentQuestionId != Guid.Empty && NextStateRequest.SelectedAnswerIds.Count > 0)
            {
                SaveAnswersToHistory(NextStateRequest.CurrentQuestionId, NextStateRequest.SelectedAnswerIds);
            }

            var destination = await apiClient.GetNextState(Questionnaire.Id, NextStateRequest, true);
            if (destination == null)
                return NotFound();

            // Redirect to external-link (if embedded or not, but different for Frontend, which is only standalone)
            if (destination is { Type: DestinationType.ExternalLink, Content: not null })
            {
                var url = Uri.TryCreate(destination.Content, UriKind.Absolute, out var uri)
                    ? uri.ToString()
                    : "https://" + destination.Content;

                return Redirect(url);
            }

            IsEmbedded = Embed;
            Destination = destination;
            NextStateRequest = new GetNextStateRequest();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error load next question or final destination. Error: {EMessage}", e.Message);
            throw;
        }

        return Page();
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
