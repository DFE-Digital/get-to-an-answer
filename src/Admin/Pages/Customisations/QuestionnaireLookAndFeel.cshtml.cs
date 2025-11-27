using Common.Client;
using Common.Domain;
using Common.Domain.Admin;
using Common.Domain.Request.Update;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Customisations;

public class QuestionnaireLookAndFeel(
    ILogger<QuestionnaireLookAndFeel> logger, 
    IApiClient apiClient, 
    IImageStorageClient imageStorageClient) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    [BindProperty] public QuestionnaireDto? Questionnaire { get; set; }
    
    [BindProperty] public required UpdateLookAndFeelRequestDto UpdateRequest { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        Questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
        var request = new UpdateLookAndFeelRequestDto();
        request.TextColor = Questionnaire?.TextColor ?? request.TextColor;
        request.BackgroundColor = Questionnaire?.BackgroundColor ?? request.BackgroundColor;
        request.PrimaryButtonColor = Questionnaire?.PrimaryButtonColor ?? request.PrimaryButtonColor;
        request.SecondaryButtonColor = Questionnaire?.SecondaryButtonColor ?? request.SecondaryButtonColor;
        request.StateColor = Questionnaire?.StateColor ?? request.StateColor;
        request.ErrorMessageColor = Questionnaire?.ErrorMessageColor ?? request.ErrorMessageColor;
        request.DecorativeImage = Questionnaire?.DecorativeImage ?? request.DecorativeImage;
        request.IsAccessibilityAgreementAccepted = Questionnaire?.IsAccessibilityAgreementAccepted ?? false;
        UpdateRequest = request;
        return Page();
    }
    
    public async Task<IActionResult> OnPost(
        [FromForm(Name = "ResetStyling")] bool resetStyling = false,
        [FromForm(Name = "RemoveImage")] bool removeImage = false,
        [FromForm(Name = "UpdateRequest.DecorativeImage")] IFormFile? file = null)
    {
        if (!UpdateRequest.IsAccessibilityAgreementAccepted)
        {
            ModelState.AddModelError("IsAccessibilityAgreementAccepted", 
                "You must accept the accessibility agreement");

            return Page();
        }
        
        if (resetStyling)
        {
            var imageName = UpdateRequest.DecorativeImage;
            
            UpdateRequest = new UpdateLookAndFeelRequestDto
            {
                DecorativeImage = imageName
            };

            await apiClient.UpdateQuestionnaireLookAndFeelAsync(QuestionnaireId, UpdateRequest);
            
            return Page();
        }

        if (removeImage)
        {
            if (Questionnaire?.DecorativeImage is not null)
            {
                await imageStorageClient.DeleteImageAsync($"{QuestionnaireId}/{Questionnaire.DecorativeImage}");
                
                await apiClient.DeleteQuestionnaireDecorativeImageAsync(QuestionnaireId);
            } 
            
            return Page();
        }

        if (file is not null && Questionnaire is not null)
        {
            if (Questionnaire.DecorativeImage is not null)
            {
                await imageStorageClient.DeleteImageAsync(
                    $"{QuestionnaireId}/{Questionnaire.DecorativeImage}");
                
                await apiClient.DeleteQuestionnaireDecorativeImageAsync(QuestionnaireId);
            }
            
            await imageStorageClient.UploadImageAsync(file.OpenReadStream(), 
                $"{QuestionnaireId}/{file.FileName}", file.ContentType);
            
            UpdateRequest.DecorativeImage = file.FileName;
        }

        await apiClient.UpdateQuestionnaireLookAndFeelAsync(QuestionnaireId, UpdateRequest);

        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}