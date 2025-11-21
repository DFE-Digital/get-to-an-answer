using System.ComponentModel.DataAnnotations;
using Admin.Models;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
        if (!ModelState.IsValid)
        {
            RemoveGenericOptionErrors();
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
        if (!ModelState.IsValid)
        {
            RemoveGenericOptionErrors();
            await HydrateOptionListsAsync();
            ReassignOptionNumbers();
            return Page();
        }

        try
        {
            // TODO: extend to handle all options; for now just show how you’d iterate
            foreach (var option in Options)
            {
                await apiClient.CreateAnswerAsync(new CreateAnswerRequestDto
                {
                    QuestionnaireId = QuestionnaireId,
                    QuestionId = QuestionId,
                    Content = option.OptionContent,
                    Description = option.OptionHint,
                    // DestinationType = map from option.Destination when ready
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
}