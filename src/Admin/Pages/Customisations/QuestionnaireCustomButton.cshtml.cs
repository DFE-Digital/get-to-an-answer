using Common.Client;
using Common.Domain;
using Common.Domain.Admin;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Customisations;

[Authorize]
public class QuestionnaireCustomButton(
    ILogger<QuestionnaireCustomButton> logger, 
    IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    [BindProperty] public QuestionnaireDto? Questionnaire { get; set; }
    
    [BindProperty] public required UpdateContinueButtonRequestDto UpdateRequest { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        Questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
        var request = new UpdateContinueButtonRequestDto();
        request.ContinueButtonText = Questionnaire?.ContinueButtonText ?? request.ContinueButtonText;
        UpdateRequest = request;
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        await apiClient.UpdateContinueButtonAsync(QuestionnaireId, UpdateRequest);

        await apiClient.UpdateCompletionStateAsync(QuestionnaireId, new UpdateCompletionStateRequestDto
        {
            Task = CompletableTask.AddCustomButton,
            Status = UpdateRequest.ContinueButtonText != "Continue" ? 
                CompletionStatus.Completed : CompletionStatus.Optional
        });
        
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}