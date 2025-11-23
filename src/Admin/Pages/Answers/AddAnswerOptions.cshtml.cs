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

    public string QuestionNumber { get; set; } = "1";

    // Bind a collection of options
    [BindProperty] public List<AnswerOptionsViewModel> Options { get; set; } = [];

    [TempData(Key = "OptionNumber")] public int OptionNumber { get; set; }

    [TempData(Key = "ErrorMessage")] public string? ErrorMessage { get; set; }


    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);

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
            RemoveGenericOptionErrors();
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
            RemoveGenericOptionErrors();
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
                    DestinationQuestionId = Guid.Parse(option.SelectedDestinationQuestion),
                    DestinationUrl = option.ResultPageUrl
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
        // var resultsPages = await apiClient.GetResultsPagesAsync(QuestionnaireId);

        var questionSelect = questions.Select(q => new SelectListItem(q.Content, q.Id.ToString())).ToList();
        // var resultsSelect = resultsPages.Select(r => new SelectListItem(r.Title, r.Id.ToString())).ToList();

        foreach (var option in Options)
        {
            option.QuestionSelectList = questionSelect;
            // option.ResultsPageSelectList = resultsSelect;
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

    private void RemoveGenericOptionErrors()
    {
        foreach (var key in ModelState.Keys.ToList())
        {
            if (key.StartsWith("Options[", StringComparison.Ordinal) && ModelState.TryGetValue(key, out var entry))
            {
                var custom = entry.Errors.FirstOrDefault(e => e.ErrorMessage.Contains("Option "));
                if (custom != null)
                {
                    entry.Errors.Clear();
                    entry.Errors.Add(custom);
                }
            }
        }
    }

    private static DestinationType MapDestination(AnswerDestination answerDestination)
    {
        switch (answerDestination)
        {
            case AnswerDestination.NextQuestion:
            case AnswerDestination.SpecificQuestion:
                return DestinationType.Question;
            case AnswerDestination.InternalResultsPage:
                break;
            case AnswerDestination.ExternalResultsPage:
                return DestinationType.ExternalLink;
            default:
                throw new ArgumentOutOfRangeException(nameof(answerDestination), answerDestination, null);
        }

        return 0;
    }
}