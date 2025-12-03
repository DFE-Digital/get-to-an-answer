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
public class AddAnswerOptionOptions(ILogger<AddAnswerOptionOptions> logger, IApiClient apiClient) : 
    AnswerOptionsPageModel(apiClient)
{
    private readonly IApiClient _apiClient = apiClient;
    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.AddAndEditQuestionsAndAnswers, QuestionnaireId);

        var questionTypeValue = TempData.Peek("QuestionType");
        if (questionTypeValue is int intVal && Enum.IsDefined(typeof(QuestionType), intVal))
        {
            RetrievedQuestionType = (QuestionType)intVal;
        }
        
        Options.Add(new AnswerOptionsViewModel { OptionNumber = 0 });
        Options.Add(new AnswerOptionsViewModel { OptionNumber = 1 });

        await HydrateOptionListsAsync();
        ReassignOptionNumbers();

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
                    DestinationUrl = option.ExternalLink,
                    Priority = Convert.ToSingle(option.RankPriority)
                });
            }

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