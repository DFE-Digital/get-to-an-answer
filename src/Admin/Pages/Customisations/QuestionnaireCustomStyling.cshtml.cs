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
using Newtonsoft.Json;

namespace Admin.Pages.Customisations;

[Authorize]
public class QuestionnaireCustomStyling(
    ILogger<QuestionnaireCustomStyling> logger, 
    IApiClient apiClient, 
    IImageStorageClient imageStorageClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }
    
    [BindProperty] public required UpdateCustomStylingRequestDto UpdateRequest { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        var questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
        var request = new UpdateCustomStylingRequestDto();
        request.TextColor = questionnaire?.TextColor ?? request.TextColor;
        request.BackgroundColor = questionnaire?.BackgroundColor ?? request.BackgroundColor;
        request.PrimaryButtonColor = questionnaire?.PrimaryButtonColor ?? request.PrimaryButtonColor;
        request.SecondaryButtonColor = questionnaire?.SecondaryButtonColor ?? request.SecondaryButtonColor;
        request.StateColor = questionnaire?.StateColor ?? request.StateColor;
        request.ErrorMessageColor = questionnaire?.ErrorMessageColor ?? request.ErrorMessageColor;
        request.DecorativeImage = questionnaire?.DecorativeImage ?? request.DecorativeImage;
        request.IsAccessibilityAgreementAccepted = questionnaire?.IsAccessibilityAgreementAccepted ?? false;
        UpdateRequest = request;
        return Page();
    }
    
    public async Task<IActionResult> OnPost(
        [FromForm(Name = "ResetStyling")] bool resetStyling = false,
        [FromForm(Name = "RemoveImage")] bool removeImage = false,
        [FromForm(Name = "UpdateRequest.DecorativeImage")] IFormFile? file = null)
    {
        if (!ModelState.IsValid)
            return Page();
        
        var questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
        
        if (resetStyling)
        {
            var imageName = UpdateRequest.DecorativeImage;
            
            UpdateRequest = new UpdateCustomStylingRequestDto
            {
                DecorativeImage = imageName
            };

            await apiClient.UpdateQuestionnaireStylingAsync(QuestionnaireId, UpdateRequest);
            
            await apiClient.UpdateCompletionStateAsync(QuestionnaireId, new UpdateCompletionStateRequestDto
            {
                Task = CompletableTask.CustomiseStyling,
                Status = imageName is null ? 
                    CompletionStatus.Optional : CompletionStatus.Completed
            });
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustResetStyling = true });
            
            return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
        }

        if (removeImage)
        {
            if (questionnaire?.DecorativeImage is not null)
            {
                await imageStorageClient.DeleteImageAsync($"{QuestionnaireId}/{questionnaire.DecorativeImage}");
                
                await apiClient.DeleteQuestionnaireDecorativeImageAsync(QuestionnaireId);
                
                await apiClient.UpdateCompletionStateAsync(QuestionnaireId, new UpdateCompletionStateRequestDto
                {
                    Task = CompletableTask.CustomiseStyling,
                    Status = IsStylingDefault(UpdateRequest) ? 
                        CompletionStatus.Optional : CompletionStatus.Completed
                });
                
                TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustRemovedStartPageImage = true });
                
                return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
            }
        }
        
        if (!UpdateRequest.IsAccessibilityAgreementAccepted)
        {
            ModelState.AddModelError("UpdateRequest.IsAccessibilityAgreementAccepted", 
                "You must accept the accessibility agreement");

            return Page();
        }

        if (file is not null && questionnaire is not null)
        {
            if (questionnaire.DecorativeImage is not null)
            {
                await imageStorageClient.DeleteImageAsync(
                    $"{QuestionnaireId}/latest");
                
                await apiClient.DeleteQuestionnaireDecorativeImageAsync(QuestionnaireId);
            }
            
            await imageStorageClient.UploadImageAsync(file.OpenReadStream(), 
                $"{QuestionnaireId}/latest", file.ContentType);
            
            UpdateRequest.DecorativeImage = file.FileName;
        }

        await apiClient.UpdateQuestionnaireStylingAsync(QuestionnaireId, UpdateRequest);
        
        await apiClient.UpdateCompletionStateAsync(QuestionnaireId, new UpdateCompletionStateRequestDto
        {
            Task = CompletableTask.CustomiseStyling,
            Status = IsStylingDefault(UpdateRequest) ? 
                CompletionStatus.Optional : CompletionStatus.Completed
        });
        
        TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustCustomisedStyling = true });

        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }

    private bool IsStylingDefault(UpdateCustomStylingRequestDto request)
    {
        var defaultStyling = new UpdateCustomStylingRequestDto();
        
        return request.TextColor == defaultStyling.TextColor && 
               request.BackgroundColor == defaultStyling.BackgroundColor && 
               request.PrimaryButtonColor == defaultStyling.PrimaryButtonColor && 
               request.SecondaryButtonColor == defaultStyling.SecondaryButtonColor && 
               request.StateColor == defaultStyling.StateColor && 
               request.ErrorMessageColor == defaultStyling.ErrorMessageColor &&
               request.DecorativeImage == defaultStyling.DecorativeImage;
    }
}