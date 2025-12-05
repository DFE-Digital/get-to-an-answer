using System.ComponentModel.DataAnnotations;
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

namespace Admin.Pages.Contents;

[Authorize]
public class EditContent(IApiClient apiClient, ILogger<EditContent> logger) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }
    [FromRoute(Name = "contentId")] public Guid ContentId { get; set; }

    [Required(ErrorMessage = "Enter a title")] 
    [BindProperty] public string ContentTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter some details")] 
    [BindProperty] public string ContentValue { get; set; } = string.Empty;

    [BindProperty] public string? ContentRefName { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        BackLinkSlug = string.Format(Routes.AddAndEditResultPages, QuestionnaireId);

        try
        {
            var content = await GetContent();

            if (content == null)
            {
                logger.LogWarning("Content {ContentId} not found", ContentId);
                throw new Exception($"Content {ContentId} not found");
            }

            PopulateFields(content);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return RedirectToErrorPage();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveContentAsync()
    {
        try
        {
            if (!ModelState.IsValid)
                return Page();

            await apiClient.UpdateContentAsync(ContentId, new UpdateContentRequestDto
            {
                Title = ContentTitle,
                Content = ContentValue,
                ReferenceName = ContentRefName
            });
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustUpdated = true });
            
            return Redirect(string.Format(Routes.AddAndEditResultPages, QuestionnaireId));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return RedirectToErrorPage();
        }
    }

    public IActionResult OnPostDeleteContent()
    {
        TempData["ContentReferenceName"] = ContentRefName;
        
        return Redirect(string.Format(Routes.ConfirmDeleteContent, QuestionnaireId, ContentId));
    }
    
    private async Task<ContentDto?> GetContent() => await apiClient.GetContentAsync(ContentId);

    private void PopulateFields(ContentDto content)
    {
        ContentTitle = content.Title;
        ContentValue = content.Content;
        ContentRefName = content.ReferenceName!;
    }
}