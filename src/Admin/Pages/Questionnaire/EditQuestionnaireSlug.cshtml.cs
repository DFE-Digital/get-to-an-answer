using System.ComponentModel.DataAnnotations;
using Common.Client;
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
public class EditQuestionnaireSlug(IApiClient apiClient, ILogger<EditQuestionnaireSlug> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }
    
    [BindProperty(Name = "Slug")]
    [Required(ErrorMessage = "Enter a questionnaire slug")]
    [GdsTitle]
    public string? Slug { get; set; }
    
    public IActionResult OnGet()
    {
        if (TempData.Peek("QuestionnaireSlug") is string title)
        {
            Slug = title;
        }
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);
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
            
            var updateQuestionnaireRequest = new UpdateQuestionnaireRequestDto { Slug = Slug };

            try
            {
                await apiClient.UpdateQuestionnaireAsync(QuestionnaireId, updateQuestionnaireRequest);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(nameof(Slug), "A questionnaire with this slug already exists");
                return Page();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating questionnaire {QuestionnaireId}", QuestionnaireId);
                return RedirectToErrorPage();
            }
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustUpdated = true });
            
            await apiClient.UpdateCompletionStateAsync(QuestionnaireId, new UpdateCompletionStateRequestDto
            {
                Task = CompletableTask.EditQuestionnaireSlug,
                Status = CompletionStatus.Completed // given the title is required
            });
            
            return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return RedirectToErrorPage();
        }
    }
}