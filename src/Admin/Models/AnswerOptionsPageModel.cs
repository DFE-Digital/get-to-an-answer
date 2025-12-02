using Admin.Models;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Common.Models.PageModels;

public class AnswerOptionsPageModel(IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [BindProperty]
    public string? QuestionNumber { get; set; } = "1";

    // Bind a collection of options
    [BindProperty] public List<AnswerOptionsViewModel> Options { get; set; } = [];

    [TempData(Key = "OptionNumber")] public int OptionNumber { get; set; }

    [BindProperty] public QuestionType? RetrievedQuestionType { get; set; }
    
    // Handler for clicking "Add another option"
    public async Task<IActionResult> OnPostAddOption()
    {
        ValidateSelectedQuestionsIfAny();

        if (!ModelState.IsValid)
        {
            // RemoveGenericOptionErrors();
            await HydrateOptionListsAsync();
            return Page();
        }

        OptionNumber++;

        Options.Add(new AnswerOptionsViewModel
        {
            OptionNumber = OptionNumber
        });

        await HydrateOptionListsAsync();
        ReassignOptionNumbers();

        // Re-render page with the extra option
        return Page();
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
    
    protected async Task HydrateOptionListsAsync()
    {
        var questions = await apiClient.GetQuestionsAsync(QuestionnaireId);
        var resultsPages = await apiClient.GetContentsAsync(QuestionnaireId);
        
        QuestionNumber = questions.Max(x => x.Order.ToString());
        
        var questionSelect = questions.Where(x => x.Id != QuestionId)
            .Select(q => new SelectListItem(q.Content, q.Id.ToString())).ToList();
        var resultsSelect = resultsPages.Select(r => new SelectListItem(r.Title, r.Id.ToString())).ToList();

        foreach (var option in Options)
        {
            option.QuestionType = RetrievedQuestionType;
            option.QuestionSelectList = questionSelect;
            option.ResultsPageSelectList = resultsSelect;
        }
    }
    
    public async Task<IActionResult> OnPostRedirectToBulkEntry(string? returnUrl)
    {
        ValidateSelectedQuestionsIfAny();

        if (!ModelState.IsValid)
        {
            await EnsureSelectListsForOptions();
            return Page();
        }

        TempData["AnswersSnapshot"] = JsonConvert.SerializeObject(Options);

        var targetUrl = Url.Page("/Answers/BulkAnswerOptions", null, new
        {
            questionnaireId = QuestionnaireId,
            questionId = QuestionId,
            returnUrl
        });

        return Redirect(targetUrl ?? string.Empty);
    }

    protected async Task EnsureSelectListsForOptions()
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
    
    

    protected async Task CreateAnswer(AnswerOptionsViewModel option)
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

    protected async Task UpdateAnswer(AnswerOptionsViewModel option)
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
    
    protected static DestinationType MapDestination(AnswerDestination answerDestination) =>
        answerDestination switch
        {
            AnswerDestination.NextQuestion => DestinationType.Auto, // is stored as null in db
            AnswerDestination.SpecificQuestion => DestinationType.Question,
            AnswerDestination.InternalResultsPage => DestinationType.CustomContent,
            AnswerDestination.ExternalResultsPage => DestinationType.ExternalLink,
            _ => throw new ArgumentOutOfRangeException(nameof(answerDestination), answerDestination, null)
        };
}