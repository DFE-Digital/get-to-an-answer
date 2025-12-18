using Admin.Models;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Answers;

[Authorize]
public class AddAnswerOptionOptions(ILogger<AddAnswerOptionOptions> logger, IApiClient apiClient) :
    AnswerOptionsPageModel(apiClient)
{
    private readonly IApiClient _apiClient = apiClient;

    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.AddAndEditQuestionsAndAnswers, QuestionnaireId);

        try
        {
            await PopulateFieldWithExistingValues();
            
            if (Options.Count == 0)
            {
                Options.Add(new AnswerOptionsViewModel { OptionNumber = 0 });
                Options.Add(new AnswerOptionsViewModel { OptionNumber = 1 });
            }
            
            await HydrateOptionListsAsync();
            ReassignOptionNumbers();

            return Page();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading data for question {QuestionId}", QuestionId);
            return NotFound();
        }
    }

    // Handler for "Continue"
    public async Task<IActionResult> OnPostContinue()
    {
        ValidateSelectedQuestionsIfAny();

        if (!ModelState.IsValid)
        {
            await HydrateOptionListsAsync();
            ReassignOptionNumbers();
            return Page();
        }

        try
        {
            
            foreach (var option in Options)
            {
                await _apiClient.CreateAnswerAsync(new CreateAnswerRequestDto
                {
                    QuestionnaireId = QuestionnaireId,
                    QuestionId = QuestionId,
                    Content = option.OptionContent,
                    Description = option.OptionHint ?? string.Empty,
                    DestinationType = MapDestination(option.AnswerDestination),
                    DestinationQuestionId = !string.IsNullOrEmpty(option.SelectedDestinationQuestion)
                        ? Guid.Parse(option.SelectedDestinationQuestion)
                        : null,
                    DestinationContentId = !string.IsNullOrEmpty(option.SelectedResultsPage)
                        ? Guid.Parse(option.SelectedResultsPage)
                        : null,
                    DestinationUrl = option.ExternalLink,
                    Priority = Convert.ToSingle(option.RankPriority)
                });
            }

            TempData[nameof(QuestionnaireState)] =
                JsonConvert.SerializeObject(new QuestionnaireState { JustUpdated = true });

            return Redirect(string.Format(Routes.AddAndEditQuestionsAndAnswers, QuestionnaireId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error creating answer options for questionnaire {QuestionnaireId}, question {QuestionId}",
                QuestionnaireId, QuestionId);
            return RedirectToErrorPage();
        }
    }

    public async Task<IActionResult> OnPostRemoveOption(int index)
    {
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
}