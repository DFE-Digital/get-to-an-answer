using System.ComponentModel.DataAnnotations;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Common.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Questionnaire;

[Authorize]
public class CloneQuestionnaire(IApiClient apiClient, ILogger<CloneQuestionnaire> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [BindProperty(Name = "Title")]
    [Required(ErrorMessage = "Enter a questionnaire title")]
    [GdsTitle]
    public required string Title { get; set; }
    
    [BindProperty]
    public string? CopiedQuestionnaireTitle { get; set; }

    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);

        var currentQuestionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);

        CopiedQuestionnaireTitle = currentQuestionnaire?.Title;
        
        Title = $"Copy of {currentQuestionnaire?.Title ?? QuestionnaireTitle}";
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var cloneQuestionnaireRequestDto = new CloneQuestionnaireRequestDto { Title = Title };

            var cloneQuestionnaire =
                await apiClient.CloneQuestionnaireAsync(QuestionnaireId, cloneQuestionnaireRequestDto);

            if (cloneQuestionnaire == null)
                return RedirectToErrorPage();

            TempData[nameof(QuestionnaireState)] =
                JsonConvert.SerializeObject(new QuestionnaireState { JustCloned = true, CopiedQuestionnaireTitle = CopiedQuestionnaireTitle });

            return Redirect(string.Format(Routes.QuestionnaireTrackById, cloneQuestionnaire.Id));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error copying the questionnaire. Error: {EMessage}", e.Message);
            return RedirectToErrorPage();
        }
    }
}