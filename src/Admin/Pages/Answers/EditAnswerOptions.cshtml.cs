using Admin.Models;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Admin.Pages.Answers;

public class EditAnswerOptions(ILogger<EditAnswerOptions> logger, IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [BindProperty] public string? QuestionNumber { get; set; } = "1";

    [BindProperty] public List<AnswerOptionsViewModel> Options { get; set; } = [];

    [BindProperty] public int OptionNumber { get; set; }

    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.EditQuestion, QuestionnaireId, QuestionId);

        await PopulateFieldWithExistingValues();

        ReassignOptionNumbers();

        return Page();
    }


    public async Task<IActionResult> OnPostAddOption()
    {
        ValidateSelectedQuestionsIfAny();

        if (!ModelState.IsValid)
        {
            await EnsureSelectListsForOptions();
            return Page();
        }


        OptionNumber++;

        Options.Add(new AnswerOptionsViewModel
        {
            OptionNumber = OptionNumber
        });

        await EnsureSelectListsForOptions();
        ReassignOptionNumbers();

        return Page();
    }

    public IActionResult OnPostRemoveOption(int index)
    {
        Options.RemoveAt(index);
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

    private async Task CreateAnswer(AnswerOptionsViewModel option)
    {
        await apiClient.CreateAnswerAsync(new CreateAnswerRequestDto
        {
            QuestionnaireId = QuestionnaireId,
            QuestionId = QuestionId,
            Content = option.OptionContent,
            Description = option.OptionHint,
            DestinationType = MapDestination(option.AnswerDestination),
            DestinationQuestionId = option.SelectedDestinationQuestion != null
                ? Guid.Parse(option.SelectedDestinationQuestion)
                : null,
            DestinationUrl = option.ResultPageUrl,
            Priority = Convert.ToSingle(option.RankPriority),
        });
    }

    private async Task UpdateAnswer(AnswerOptionsViewModel option)
    {
        await apiClient.UpdateAnswerAsync(option.AnswerId, new UpdateAnswerRequestDto
        {
            Content = option.OptionContent,
            DestinationType = MapDestination(option.AnswerDestination),
            DestinationQuestionId = option.SelectedDestinationQuestion != null
                ? Guid.Parse(option.SelectedDestinationQuestion)
                : null,
            DestinationUrl = option.ResultPageUrl,
            Priority = Convert.ToSingle(option.RankPriority),
            DestinationContentId = option.SelectedResultsPage != null
                ? Guid.Parse(option.SelectedResultsPage)
                : null,
            Description = option.OptionHint
        });
    }

    private async Task PopulateFieldWithExistingValues()
    {
        Options.Clear();

        var existingAnswers = await apiClient.GetAnswersAsync(QuestionId);
        var questionForSelection = await apiClient.GetQuestionsAsync(QuestionnaireId);
        var resultsPages = await apiClient.GetContentsAsync(QuestionnaireId);

        var questionSelectionList = questionForSelection.Where(x => x.Id != QuestionId)
            .Select(q => new SelectListItem(q.Content, q.Id.ToString())).ToList();

        var resultsPagesForSelection = resultsPages
            .Select(r => new SelectListItem(r.Title, r.Id.ToString())).ToList();

        var currentQuestion = questionForSelection.SingleOrDefault(q => q.Id == QuestionId);

        foreach (var existingAnswer in existingAnswers)
        {
            Options.Add(new AnswerOptionsViewModel
            {
                AnswerId = existingAnswer.Id,
                QuestionSelectList = questionSelectionList,
                AnswerDestination =
                    MapAnswerDestination(existingAnswer.DestinationType, existingAnswer.DestinationQuestionId),
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

    private async Task EnsureSelectListsForOptions()
    {
        var questionForSelection = await apiClient.GetQuestionsAsync(QuestionnaireId);
        var resultsPages = await apiClient.GetContentsAsync(QuestionnaireId);

        var questionSelectionList = questionForSelection.Where(x => x.Id != QuestionId)
            .Select(q => new SelectListItem(q.Content, q.Id.ToString())).ToList();

        var resultsPagesForSelection = resultsPages
            .Select(r => new SelectListItem(r.Title, r.Id.ToString())).ToList();

        foreach (var option in Options)
        {
            option.QuestionSelectList = questionSelectionList;
            option.ResultsPageSelectList = resultsPagesForSelection;
        }
    }

    private void ValidateSelectedQuestionsIfAny()
    {
        var optionsWithSpecificQuestionNoSelection =
            Options.Where(x =>
                x.AnswerDestination == AnswerDestination.SpecificQuestion &&
                string.IsNullOrEmpty(x.SelectedDestinationQuestion));

        foreach (var specificQuestion in optionsWithSpecificQuestionNoSelection)
        {
            var index = specificQuestion.OptionNumber - 1;
            var errorMessage = $"Please select a question for option {specificQuestion.OptionNumber}";

            var destinationKey = $"Options-{index}-AnswerDestination";
            var specificQuestionRadioInputId = $"Options-{index}-destination-specific";

            ModelState.AddModelError(destinationKey, string.Empty);
            ModelState.AddModelError(specificQuestionRadioInputId, errorMessage);
        }

        var optionsWithResultsPageNoSelection = Options.Where(o =>
            o.AnswerDestination == AnswerDestination.InternalResultsPage &&
            string.IsNullOrEmpty(o.SelectedResultsPage));

        foreach (var resultsPage in optionsWithResultsPageNoSelection)
        {
            var index = resultsPage.OptionNumber - 1;
            var errorMessage = $"Please select a results page for option {resultsPage.OptionNumber}";

            var selectKey = $"Options[{index}].SelectedResultsPage";
            var resultsPageRadioInputId = $"Options-{index}-destination-internal";

            ModelState.AddModelError(selectKey, string.Empty);
            ModelState.AddModelError(resultsPageRadioInputId, errorMessage);
        }
    }


    private void ReassignOptionNumbers()
    {
        for (var index = 0; index < Options.Count; index++)
        {
            Options[index].OptionNumber = index + 1;
        }
    }

    private static AnswerDestination MapAnswerDestination(DestinationType? destinationType,
        Guid? destinationQuestionId = null) =>
        destinationType switch
        {
            DestinationType.Question when destinationQuestionId != null => AnswerDestination.SpecificQuestion,
            DestinationType.Question => AnswerDestination.NextQuestion,
            DestinationType.CustomContent => AnswerDestination.InternalResultsPage,
            DestinationType.ExternalLink => AnswerDestination.ExternalResultsPage,
            _ => throw new ArgumentOutOfRangeException(nameof(destinationType), destinationType, null)
        };

    private static DestinationType MapDestination(AnswerDestination answerDestination) =>
        answerDestination switch
        {
            AnswerDestination.NextQuestion => DestinationType.Question,
            AnswerDestination.SpecificQuestion => DestinationType.Question,
            AnswerDestination.InternalResultsPage => DestinationType.CustomContent,
            AnswerDestination.ExternalResultsPage => DestinationType.ExternalLink,
            _ => throw new ArgumentOutOfRangeException(nameof(answerDestination), answerDestination, null)
        };
}