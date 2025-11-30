using System.Globalization;
using Common.Client;
using Common.Domain;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Questionnaire;

[Authorize]
public class AddQuestionnaireStart(IApiClient apiClient, ILogger<AddQuestionnaireStart> logger) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }
    
    public string? QuestionnaireTitle { get; set; }

    [BindProperty] public string? DisplayTitle { get; set; }

    [BindProperty] public string? Description { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);

        try
        {
            var questionnaire = await GetQuestionnaire();

            if (questionnaire == null)
            {
                logger.LogWarning("Questionnaire {QuestionnaireId} not found", QuestionnaireId);
                throw new Exception($"Questionnaire {QuestionnaireId} not found");
            }
            
            QuestionnaireTitle = questionnaire.Title;

            PopulateFields(questionnaire);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return RedirectToErrorPage();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveStartPageAsync()
    {
        try
        {
            if (!ModelState.IsValid)
                return Page();

            await apiClient.UpdateQuestionnaireAsync(QuestionnaireId, new UpdateQuestionnaireRequestDto
            {
                DisplayTitle = DisplayTitle,
                Description = Description,
            });
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustAddedStartPage = true });
            
            return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return RedirectToErrorPage();
        }
    }
    
    public async Task<IActionResult> OnPostClearStartPageAsync()
    {
        try
        {
            if (!ModelState.IsValid)
                return Page();

            await apiClient.UpdateQuestionnaireAsync(QuestionnaireId, new UpdateQuestionnaireRequestDto
            {
                DisplayTitle = string.Empty, // when empty the questionnaire run starts at the first question
                Description = string.Empty,
            });
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustRemovedStartPage = true });
            
            return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return RedirectToErrorPage();
        }
    }
    
    private async Task<QuestionnaireDto?> GetQuestionnaire() => await apiClient.GetQuestionnaireAsync(QuestionnaireId);

    private void PopulateFields(QuestionnaireDto content)
    {
        DisplayTitle = content.DisplayTitle;
        Description = content.Description;
    }
}