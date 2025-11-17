using System.ComponentModel.DataAnnotations;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages.Questions;

public class AddQuestion(ILogger<AddQuestion> logger, IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    public string QuestionNumber { get; set; } = 1.ToString();

    [BindProperty] [Required] public string QuestionContent { get; set; } = "";

    [BindProperty] [Required] public string QuestionHintText { get; set; } = "";

    [BindProperty] [Required] public QuestionType QuestionType { get; set; }

    public IActionResult OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
             await apiClient.CreateQuestionAsync(new CreateQuestionRequestDto
            {
                QuestionnaireId = QuestionnaireId,
                Content = QuestionContent,
                Description = QuestionHintText,
                Type = QuestionType
            });

            return Page();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating question for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }
    }
}