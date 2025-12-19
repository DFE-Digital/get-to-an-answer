using System.Globalization;
using Admin.Models;
using Common.Client;
using Common.Enum;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Answers;

[Authorize]
public class EditAnswerOptionOptions(ILogger<EditAnswerOptionOptions> logger, IApiClient apiClient) :
    AnswerOptionsPageModel(apiClient)
{
    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.EditQuestion, QuestionnaireId, QuestionId);

        await PopulateFieldWithExistingValues();

        if (Options.Count == 0)
        {
            Options.Add(new AnswerOptionsViewModel { OptionNumber = 0 });
            Options.Add(new AnswerOptionsViewModel { OptionNumber = 1 });
        }

        ReassignOptionNumbers();

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAnswerOptions()
    {
        ValidateForDuplicateAnswers();

        ValidateSelectedQuestionsIfAny();

        if (!ModelState.IsValid)
        {
            await PopulateFieldWithExistingValues();
            return Page();
        }

        try
        {
            await UpdateOrCreateAnswers();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating answer options");
            return RedirectToErrorPage();
        }

        TempData[nameof(QuestionnaireState)] =
            JsonConvert.SerializeObject(new QuestionnaireState { JustUpdated = true });

        return Redirect(string.Format(Routes.EditQuestion, QuestionnaireId, QuestionId));
    }

    public async Task<IActionResult> OnPostRemoveOption(int index)
    {
        var removedOption = Options[index];
        if (removedOption.AnswerId != Guid.Empty)
            DeletedAnswerIds.Add(removedOption.AnswerId);

        Options.RemoveAt(index);

        RemoveModelStateEntriesForOption(index);
        RemoveModelStateErrorsForFields();

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