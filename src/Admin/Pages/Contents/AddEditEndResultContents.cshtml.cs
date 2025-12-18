using Common.Client;
using Common.Domain;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Admin.Pages.Contents;

[Authorize]
public class AddEditEndResultContents(ILogger<AddEditEndResultContents> logger, IApiClient apiClient)
    : BasePageModel
{
    [FromRoute] public Guid QuestionnaireId { get; set; }

    [BindProperty] public List<ContentDto> Contents { get; set; } = [];
    
    public QuestionnaireState? QuestionnaireState { get; set;}
    
    [BindProperty]
    public bool FinishedEditing { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);

        try
        {
            logger.LogInformation("Getting questions for questionnaire {QuestionnaireId}", QuestionnaireId);
            var contents = await apiClient.GetContentsAsync(QuestionnaireId);

            Contents = contents;
            
            if (TempData.Peek("CompletionTrackingMap") is string trackingMapJson)
            {
                try
                {
                    var trackingMap = JsonSerializer
                        .Deserialize<Dictionary<CompletableTask, CompletionStatus>>(trackingMapJson);
                    FinishedEditing = trackingMap?[CompletableTask.AddContents] ==
                                      CompletionStatus.Completed;
                } 
                catch (Exception e)
                {
                    logger.LogError(e, "Error deserializing completion tracking map");
                }
            }
            
            var stateFound = TempData.TryGetValue(nameof(QuestionnaireState), out var value);

            if (stateFound)
            {
                var deserialized = JsonConvert.DeserializeObject<QuestionnaireState>(value?.ToString() ?? string.Empty);
                if (deserialized == null)
                    logger.LogError("Failed to deserialize QuestionnaireState from TempData.");
                
                QuestionnaireState = deserialized;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting questions for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAndContinueAsync()
    {
        await apiClient.UpdateCompletionStateAsync(QuestionnaireId, new UpdateCompletionStateRequestDto
        {
            Task = CompletableTask.AddContents,
            Status = FinishedEditing ? CompletionStatus.Completed : CompletionStatus.Optional
        });
        
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}