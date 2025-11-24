using System.ComponentModel.DataAnnotations;
using Common.Models;
using Common.Models.PageModels;
using Common.Models.ViewModels;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Questionnaire;

[Authorize]
public class CreateQuestionnaires(IApiClient apiClient, ILogger<CreateQuestionnaires> logger) : QuestionnairesPageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Enter a questionnaire title")]
    [GdsTitle]
    public string? Title { get; set; }
    
    public IActionResult OnGet()
    {
        BackLinkSlug = Routes.QuestionnairesManage;
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

            var response = await apiClient.CreateQuestionnaireAsync(new CreateQuestionnaireRequestDto
            {
                Title = Title!
            });

            TempData[nameof(QuestionnaireState)] =
                JsonConvert.SerializeObject(new QuestionnaireState { JustCreated = true });

            return Redirect($"/admin/questionnaires/{response?.Id}/track");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return RedirectToErrorPage();
        }
    }
}