using Admin.Models;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Admin.Pages.Answers;

[Authorize]
public class EditAnswerOptionOptions(ILogger<EditAnswerOptionOptions> logger, IApiClient apiClient) : 
    AnswerOptionsPageModel(apiClient)
{
    private readonly IApiClient _apiClient = apiClient;

    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.EditQuestion, QuestionnaireId, QuestionId);

        var existingOptionsFromTempData = TempData.Peek("AnswersSnapshot");

        if (existingOptionsFromTempData is string optionsJson)
        {
            Options = JsonConvert.DeserializeObject<List<AnswerOptionsViewModel>>(optionsJson) ?? [];

            if (Options.Count != 0)
            {
                ReassignOptionNumbers();
                await PopulateOptionSelectionLists();
                return Page();
            }
        }

        await PopulateFieldWithExistingValues();

        ReassignOptionNumbers();

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAnswerOptions()
    {
        ValidateSelectedQuestionsIfAny();

        if (!ModelState.IsValid)
        {
            await PopulateFieldWithExistingValues();
            return Page();
        }

        try
        {
            foreach (var option in Options)
            {
                if (option.AnswerId != Guid.Empty)
                {
                    await UpdateAnswer(option);
                }
                else
                {
                    await CreateAnswer(option);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating answer options");
            return RedirectToErrorPage();
        }

        return Redirect(string.Format(Routes.EditQuestion, QuestionnaireId, QuestionId));
    }

    private void RemoveModelStateEntriesForOption(int index)
    {
        var prefixBracket = $"Options[{index}]";
        var prefixDash = $"Options-{index}-";
        var keysToRemove = ModelState.Keys
            .Where(key =>
                key.StartsWith(prefixBracket, StringComparison.Ordinal) ||
                key.StartsWith(prefixDash, StringComparison.Ordinal))
            .ToList();

        foreach (var key in keysToRemove)
        {
            ModelState.Remove(key);
        }
    }

    private async Task PopulateFieldWithExistingValues()
    {
        Options?.Clear();

        var existingAnswers = await _apiClient.GetAnswersAsync(QuestionId);
        var questionForSelection = await _apiClient.GetQuestionsAsync(QuestionnaireId);
        var resultsPages = await _apiClient.GetContentsAsync(QuestionnaireId);

        var questionSelectionList = questionForSelection.Where(x => x.Id != QuestionId)
            .Select(q => new SelectListItem(q.Content, q.Id.ToString())).ToList();

        var resultsPagesForSelection = resultsPages
            .Select(r => new SelectListItem(r.Title, r.Id.ToString())).ToList();

        var currentQuestion = questionForSelection.SingleOrDefault(q => q.Id == QuestionId);

        foreach (var existingAnswer in existingAnswers)
        {
            Options?.Add(new AnswerOptionsViewModel
            {
                AnswerId = existingAnswer.Id,
                QuestionSelectList = questionSelectionList,
                AnswerDestination =
                    MapAnswerDestination(existingAnswer.DestinationType),
                OptionContent = existingAnswer.Content,
                OptionHint = existingAnswer.Description,
                ExternalLink = existingAnswer.DestinationUrl,
                SelectedDestinationQuestion = existingAnswer.DestinationQuestionId?.ToString(),
                OptionNumber = existingAnswers.Count - 1,
                QuestionType = currentQuestion?.Type,
                RankPriority = existingAnswer.Priority.ToString(),
                ResultPageUrl = existingAnswer.DestinationUrl,
                ResultsPageSelectList = resultsPagesForSelection,
                SelectedResultsPage = existingAnswer.DestinationContentId.ToString()
            });
        }
    }
    
    private static AnswerDestination MapAnswerDestination(DestinationType? destinationType) =>
        destinationType switch
        {
            DestinationType.Question => AnswerDestination.NextQuestion,
            DestinationType.CustomContent => AnswerDestination.InternalResultsPage,
            DestinationType.ExternalLink => AnswerDestination.ExternalResultsPage,
            null => AnswerDestination.NextQuestion,
            _ => throw new ArgumentOutOfRangeException(nameof(destinationType), destinationType, null)
        };
    
    
}