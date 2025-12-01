using Admin.Models;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Admin.Pages.Answers;

[Authorize]
public class AddAnswerOptions(ILogger<AddAnswerOptions> logger, IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [BindProperty]
    public string? QuestionNumber { get; set; } = "1";

    // Bind a collection of options
    [BindProperty] public List<AnswerOptionsViewModel> Options { get; set; } = [];

    [TempData(Key = "OptionNumber")] public int OptionNumber { get; set; }

    [BindProperty] public QuestionType? RetrievedQuestionType { get; set; }

    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.AddAndEditQuestionsAndAnswers, QuestionnaireId);
        
        var questionTypeValue = TempData.Peek("QuestionType");
        if (questionTypeValue is int intVal && Enum.IsDefined(typeof(QuestionType), intVal))
        {
            RetrievedQuestionType = (QuestionType)intVal;
        }

        await HydrateOptionListsAsync();
        ReassignOptionNumbers();

        return Page();
    }

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

    // Handler for "Continue"
    public async Task<IActionResult> OnPostContinue()
    {
        ValidateSelectedQuestionsIfAny();

        if (!ModelState.IsValid)
        {
            // RemoveGenericOptionErrors();
            await HydrateOptionListsAsync();
            ReassignOptionNumbers();
            return Page();
        }
        
        try
        {
            foreach (var option in Options)
            {
                await apiClient.CreateAnswerAsync(new CreateAnswerRequestDto
                {
                    QuestionnaireId = QuestionnaireId,
                    QuestionId = QuestionId,
                    Content = option.OptionContent,
                    Description = option.OptionHint,
                    DestinationType = MapDestination(option.AnswerDestination),
                    DestinationQuestionId = !string.IsNullOrEmpty(option.SelectedDestinationQuestion)
                        ? Guid.Parse(option.SelectedDestinationQuestion)
                        : null,
                    DestinationUrl = option.ResultPageUrl,
                    Priority = Convert.ToSingle(option.RankPriority)
                });
            }

            return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error creating answer options for questionnaire {QuestionnaireId}, question {QuestionId}",
                QuestionnaireId, QuestionId);
            return RedirectToErrorPage();
        }
    }

    private async Task HydrateOptionListsAsync()
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

    private void ReassignOptionNumbers()
    {
        for (var index = 0; index < Options.Count; index++)
        {
            Options[index].OptionNumber = index + 1;
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
    
    public async Task<IActionResult> OnPostRemoveOption(int index)
    {
        Options.RemoveAt(index);
        RemoveModelStateEntriesForOption(index);

        await HydrateOptionListsAsync();
        ReassignOptionNumbers();
        return Page();
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

    private static DestinationType? MapDestination(AnswerDestination? answerDestination) =>
        answerDestination switch
        {
            AnswerDestination.NextQuestion => null,
            AnswerDestination.SpecificQuestion => DestinationType.Question,
            AnswerDestination.InternalResultsPage => DestinationType.CustomContent,
            AnswerDestination.ExternalResultsPage => DestinationType.ExternalLink,
            _ => throw new ArgumentOutOfRangeException(nameof(answerDestination), answerDestination, null)
        };
}