using System.ComponentModel.DataAnnotations;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Answers;

public class AddAnswerOptions(ILogger<AddAnswerOptions> logger, IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")]
    public Guid QuestionId { get; set; }

    // For now we show "Question 1" statically – this can be wired up later
    public string QuestionNumber { get; set; } = "1";

    // Option 1 fields
    [BindProperty]
    [Required(ErrorMessage = "Enter option 1")]
    public string Option1Content { get; set; } = string.Empty;

    [BindProperty]
    public string? Option1Hint { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select where people go next for option 1")]
    public DestinationType Option1Destination { get; set; }

    public IActionResult OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // TODO: extend to handle multiple options. For now we just send one.
            await apiClient.CreateAnswerAsync(new CreateAnswerRequestDto
            {
                QuestionnaireId = QuestionnaireId,
                QuestionId = QuestionId,
                Content = Option1Content,
                Description = Option1Hint,
                DestinationType = Option1Destination
                // DestinationQuestionId / DestinationUrl can be added later
            });

            // For now, go back to tracking the questionnaire
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