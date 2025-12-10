using Common.Client;
using Common.Domain;
using Common.Domain.Admin;
using Common.Domain.Request.Update;
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
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustResetStyling = true });
            
            return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
        }

        if (removeImage)
        {
            if (questionnaire?.DecorativeImage is not null)
            {
                await imageStorageClient.DeleteImageAsync($"{QuestionnaireId}/{questionnaire.DecorativeImage}");
                
                await apiClient.DeleteQuestionnaireDecorativeImageAsync(QuestionnaireId);
                
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
        
        TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustCustomisedStyling = true });

        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}