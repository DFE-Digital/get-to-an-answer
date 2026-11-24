using System.Globalization;
using Common.Client;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Admin.Models;

public class AnswerOptionsPageModel(IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [BindProperty] public string? QuestionNumber { get; set; } = "1";

    // Bind a collection of options
    [BindProperty] public List<AnswerOptionsViewModel> Options { get; set; } = [];

    [TempData(Key = "OptionNumber")] public int OptionNumber { get; set; }

    [BindProperty] public List<Guid> DeletedAnswerIds { get; set; } = [];

    [BindProperty] public QuestionType? QuestionType { get; set; } = null;
    
    // Handler for clicking "Add another option"
    public async Task<IActionResult> OnPostAddOption()
    {
        ValidateForDuplicateAnswers();
        ValidateSelectedQuestionsIfAny();

        if (!ModelState.IsValid)
        {
            await PopulateFieldsWithExistingValues();
            return Page();
        }

        OptionNumber++;

        Options.Add(new AnswerOptionsViewModel
        {
            OptionNumber = OptionNumber
        });

        await PopulateFieldsWithExistingValues();

        ReassignOptionNumbers();

        // Re-render page with the extra option
        return Page();
    }

    protected void RemoveModelStateErrorsForFields()
    {
        foreach (var key in ModelState.Keys)
        {
            ModelState[key]?.Errors.Clear();
        }
    }

    protected void ReassignOptionNumbers()
    {
        for (var index = 0; index < Options.Count; index++)
        {
            Options[index].OptionNumber = index + 1;
        }
    }

    protected void ValidateSelectedQuestionsIfAny()
    {
        AddDestinationErrors(
            Options.Where(o =>
                o.AnswerDestination is AnswerDestination.SpecificQuestion &&
                string.IsNullOrEmpty(o.SelectedDestinationQuestion)),
            "Select a question as the destination to save",
            "destination-specific");

        AddDestinationErrors(
            Options.Where(o =>
                o.AnswerDestination is AnswerDestination.InterimWithSpecificQuestion &&
                string.IsNullOrEmpty(o.SelectedInterimDestinationQuestion)),
            "Select a question as the destination to save",
            "destination-interim-question");

        AddDestinationErrors(
            Options.Where(o =>
                o.AnswerDestination is AnswerDestination.InternalResultsPage &&
                string.IsNullOrEmpty(o.SelectedResultsPage)),
            "Select a results page as the destination to save",
            "destination-internal");

        AddDestinationErrors(
            Options.Where(o =>
                o.AnswerDestination is AnswerDestination.InterimWithSpecificQuestion &&
                string.IsNullOrEmpty(o.SelectedInterimResultsPage)),
            "Select a results page as the destination to save",
            "destination-interim-result");

        AddDestinationErrors(
            Options.Where(o =>
                o.AnswerDestination == AnswerDestination.ExternalResultsPage &&
                string.IsNullOrEmpty(o.ExternalLink)),
            "Enter a link as the destination to save",
            "destination-external");

        if (QuestionType is Common.Enum.QuestionType.MultiSelect)
        {
            ValidateRankPriorities();
        }

        if (QuestionType is Common.Enum.QuestionType.VoteValue)
        {
            ValidateVoteValues();
        }
    }

    private void AddDestinationErrors(IEnumerable<AnswerOptionsViewModel> options, string errorMessage, string inputIdSuffix)
    {
        foreach (var option in options)
        {
            var index = option.OptionNumber - 1;

            ModelState.AddModelError($"Options-{index}-AnswerDestination", string.Empty);
            ModelState.AddModelError($"Options-{index}-{inputIdSuffix}", errorMessage);
        }
    }

    private void ValidateRankPriorities()
    {
        var optionsWithInvalidPriorities = Options.Where(o =>
            int.TryParse(o.RankPriority, out var priority) && priority < 0);

        foreach (var invalidPriority in optionsWithInvalidPriorities)
        {
            var index = invalidPriority.OptionNumber - 1;
            var errorMessage = $"Priority must be a positive number (0, 1, ...) for option {invalidPriority.OptionNumber}";

            ModelState.AddModelError($"Options-{index}-RankPriority", errorMessage);
        }
    }

    private void ValidateVoteValues()
    {
        var optionsWithoutVoteValue = Options.Where(o => o.VoteValue is null);

        foreach (var option in optionsWithoutVoteValue)
        {
            var index = option.OptionNumber - 1;
            var errorMessage = $"Select yes or no for option {option.OptionNumber}";

            ModelState.AddModelError($"Options-{index}-VoteValue", errorMessage);
        }
    }

    private SelectListItem ToResultsPageSelectListItem(ContentDto content) =>
        new(!string.IsNullOrWhiteSpace(content.ReferenceName) ? content.ReferenceName : content.Title,
            content.Id.ToString());
    
    public async Task<IActionResult> OnPostRedirectToBulkEntry(string? returnUrl)
    {
        ValidateSelectedQuestionsIfAny();

        var targetUrl = Url.Page("/Answers/BulkAnswerOptions", null, new
        {
            questionnaireId = QuestionnaireId,
            questionId = QuestionId,
            returnUrl
        });

        if (Options.All(o => string.IsNullOrWhiteSpace(o.OptionContent)))
            return Redirect(targetUrl ?? string.Empty);

        if (!ModelState.IsValid)
        {
            await PopulateOptionSelectionLists();
            return Page();
        }

        await UpdateOrCreateAnswers();

        return Redirect(targetUrl ?? string.Empty);
    }

    protected async Task PopulateFieldsWithExistingValues()
    {
        var existingStoredAnswers = await apiClient.GetAnswersAsync(QuestionId);
        var existingAnswerOptionIds = Options.Select(o => o.AnswerId).ToHashSet();

        var (
            questionForSelection,
            _,
            questionSelectionList,
            resultsPagesForSelection
            ) = await GetPopulatePrerequisites();

        var currentQuestion = questionForSelection.SingleOrDefault(q => q.Id == QuestionId);

        QuestionType = currentQuestion?.Type;
        
        foreach (var existingAnswer in existingStoredAnswers.Where(a => !DeletedAnswerIds.Contains(a.Id)))
        {
            if (existingAnswerOptionIds.Contains(existingAnswer.Id))
                continue;

            Options.Add(new AnswerOptionsViewModel
            {
                AnswerId = existingAnswer.Id,
                QuestionSelectList = questionSelectionList,
                AnswerDestination = MapStoredAnswerDestination(existingAnswer),
                OptionContent = existingAnswer.Content,
                OptionHint = existingAnswer.Description,
                VoteValue = existingAnswer.VoteValue,
                SelectedDestinationQuestion = existingAnswer.DestinationType != DestinationType.InterimThenQuestion ?
                    existingAnswer.DestinationQuestionId?.ToString() : null,
                SelectedInterimDestinationQuestion =  existingAnswer.DestinationType == DestinationType.InterimThenQuestion ?
                    existingAnswer.DestinationQuestionId?.ToString() : null,
                QuestionType = currentQuestion?.Type,
                RankPriority = existingAnswer.Priority.ToString(CultureInfo.InvariantCulture),
                ExternalLink = existingAnswer.DestinationUrl,
                ResultsPageSelectList = resultsPagesForSelection,
                SelectedResultsPage = existingAnswer.DestinationType != DestinationType.InterimThenQuestion ?
                    existingAnswer.DestinationContentId.ToString() : null,
                SelectedInterimResultsPage = existingAnswer.DestinationType == DestinationType.InterimThenQuestion ?
                    existingAnswer.DestinationContentId.ToString() : null
            });
        }

        await PopulateOptionSelectionLists();
    }

    protected async Task<(
        List<QuestionDto> Questions,
        List<ContentDto> ResultsPages,
        List<SelectListItem> questionSelectionList,
        List<SelectListItem> resultsPagesForSelection)> GetPopulatePrerequisites()
    {
        var questionForSelection = await apiClient.GetQuestionsAsync(QuestionnaireId);
        var resultsPages = await apiClient.GetContentsAsync(QuestionnaireId);

        var questionSelectionList = questionForSelection.Where(x => x.Id != QuestionId)
            .Select(q => new SelectListItem(q.ReferenceName ?? q.Content, q.Id.ToString())).ToList();

        var resultsPagesForSelection = resultsPages
            .Select(ToResultsPageSelectListItem).ToList();

        return (questionForSelection, resultsPages, questionSelectionList, resultsPagesForSelection);
    }

    protected async Task PopulateOptionSelectionLists()
    {
        var (
            questions,
            _,
            questionSelectionList,
            resultsPagesForSelection
            ) = await GetPopulatePrerequisites();

        foreach (var option in Options)
        {
            option.QuestionType = questions.Where(q => q.Id == QuestionId).Select(q => q.Type).FirstOrDefault();
            option.QuestionSelectList = questionSelectionList;
            option.ResultsPageSelectList = resultsPagesForSelection;
        }
    }

    protected async Task CreateAnswer(AnswerOptionsViewModel option)
    {
        await apiClient.CreateAnswerAsync(new CreateAnswerRequestDto
        {
            QuestionnaireId = QuestionnaireId,
            QuestionId = QuestionId,
            Content = option.OptionContent,
            Description = option.OptionHint,
            DestinationType = MapDestination(option.AnswerDestination),
            DestinationQuestionId = 
                option switch
                {
                    { SelectedDestinationQuestion: not null, AnswerDestination: AnswerDestination.SpecificQuestion } 
                        => Guid.Parse(option.SelectedDestinationQuestion),
                    { SelectedInterimDestinationQuestion: not null, AnswerDestination: AnswerDestination.InterimWithSpecificQuestion } 
                        => Guid.Parse(option.SelectedInterimDestinationQuestion),
                    _ => null
                },
            DestinationContentId = 
                option switch
                {
                    { SelectedResultsPage: not null, AnswerDestination: AnswerDestination.InternalResultsPage } 
                        => Guid.Parse(option.SelectedResultsPage),
                    { SelectedInterimResultsPage: not null, AnswerDestination: AnswerDestination.InterimWithSpecificQuestion } 
                        => Guid.Parse(option.SelectedInterimResultsPage),
                    _ => null
                },
            DestinationUrl = option.ExternalLink,
            Priority = Convert.ToSingle(option.RankPriority),
            VoteValue = option.VoteValue,
        });
    }

    protected async Task UpdateAnswer(AnswerOptionsViewModel option)
    {
        await apiClient.UpdateAnswerAsync(option.AnswerId, new UpdateAnswerRequestDto
        {
            Content = option.OptionContent,
            DestinationType = MapDestination(option.AnswerDestination),
            DestinationQuestionId = 
                option switch
                {
                    { SelectedDestinationQuestion: not null, AnswerDestination: AnswerDestination.SpecificQuestion } 
                        => Guid.Parse(option.SelectedDestinationQuestion),
                    { SelectedInterimDestinationQuestion: not null, AnswerDestination: AnswerDestination.InterimWithSpecificQuestion } 
                        => Guid.Parse(option.SelectedInterimDestinationQuestion),
                    _ => null
                },
            DestinationContentId = 
                option switch
                {
                    { SelectedResultsPage: not null, AnswerDestination: AnswerDestination.InternalResultsPage } 
                        => Guid.Parse(option.SelectedResultsPage),
                    { SelectedInterimResultsPage: not null, AnswerDestination: AnswerDestination.InterimWithSpecificQuestion } 
                        => Guid.Parse(option.SelectedInterimResultsPage),
                    _ => null
                },
            DestinationUrl = option.ExternalLink,
            Priority = Convert.ToSingle(option.RankPriority),
            Description = option.OptionHint,
            VoteValue = option.VoteValue
        });
    }

    protected void ValidateForDuplicateAnswers()
    {
        var duplicates = Options
            .Select((o, index) => new { o.OptionContent, index })
            .Where(x => !string.IsNullOrWhiteSpace(x.OptionContent))
            .GroupBy(x => x.OptionContent?.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);

        foreach (var group in duplicates)
        {
            foreach (var item in group)
            {
                ModelState.AddModelError($"Options[{item.index}].OptionContent",
                    $"Option {item.index + 1} already exists");
            }
        }
    }

    protected async Task UpdateOrCreateAnswers()
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

        if (DeletedAnswerIds.Count > 0)
        {
            foreach (var answerId in DeletedAnswerIds.Where(x => x != Guid.Empty).Distinct().ToList())
            {
                await apiClient.DeleteAnswerAsync(answerId);
                DeletedAnswerIds.Remove(answerId);
            }
        }
    }


    protected static AnswerDestination MapStoredAnswerDestination(AnswerDto existingAnswer) =>
        existingAnswer.DestinationType switch
        {
            DestinationType.Question when existingAnswer.DestinationQuestionId is null
                                          || existingAnswer.DestinationQuestionId == Guid.Empty 
                => AnswerDestination.NextQuestion,
            DestinationType.InterimThenQuestion => AnswerDestination.InterimWithSpecificQuestion,
            DestinationType.Question => AnswerDestination.SpecificQuestion,
            DestinationType.CustomContent => AnswerDestination.InternalResultsPage,
            DestinationType.ExternalLink => AnswerDestination.ExternalResultsPage,
            null => AnswerDestination.NextQuestion,
            _ => throw new ArgumentOutOfRangeException(nameof(existingAnswer.DestinationType),
                existingAnswer.DestinationType, null)
        };

    protected static DestinationType MapDestination(AnswerDestination answerDestination) =>
        answerDestination switch
        {
            AnswerDestination.NextQuestion => DestinationType.Auto, // is stored as null in db
            AnswerDestination.SpecificQuestion => DestinationType.Question,
            AnswerDestination.InternalResultsPage => DestinationType.CustomContent,
            AnswerDestination.ExternalResultsPage => DestinationType.ExternalLink,
            AnswerDestination.InterimWithSpecificQuestion => DestinationType.InterimThenQuestion,
            _ => throw new ArgumentOutOfRangeException(nameof(answerDestination), answerDestination, null)
        };
}