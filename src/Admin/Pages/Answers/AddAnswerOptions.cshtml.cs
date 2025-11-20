using System.ComponentModel.DataAnnotations;
using Admin.Models;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Answers;

[Authorize]
public class AddAnswerOptions(ILogger<AddAnswerOptions> logger, IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    public string QuestionNumber { get; set; } = "1";

    // Bind a collection of options
    [BindProperty] public List<AnswerOptionsViewModel> Options { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);

        // Start with one empty option if none exist yet
        if (Options.Count == 0)
        {
            Options.Add(new AnswerOptionsViewModel());
        }

        var questions = await apiClient.GetQuestionsAsync(QuestionnaireId);

        foreach (var option in Options)
        {
            
        }

        return Page();
    }

    // Handler for clicking "Add another option"
    public IActionResult OnPostAddOption()
    {
        // Ensure existing options are bound

        Options.Add(new AnswerOptionsViewModel());

        // Re-render page with the extra option
        return Page();
    }

    // Handler for "Continue"
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
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
}