using Common.Client;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Confirmations;

[Authorize]
public class ConfirmDeleteContent(IApiClient apiClient) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }
    
    [FromRoute(Name = "contentId")]
    public Guid ContentId { get; set; }

    public string? QuestionnaireTitle { get; set; }
    
    [BindProperty] public bool DeleteContent { get; set; }

    public void OnGet()
    {
        if (TempData.Peek("QuestionnaireTitle") is string title)
        {
            QuestionnaireTitle = title;
        }
    }

    public async Task<IActionResult> OnPostContinueAsync()
    {
        if (DeleteContent)
        {
            await apiClient.DeleteContentAsync(ContentId);
        }
        
        return Redirect(string.Format(Routes.AddAndEditResultPages, QuestionnaireId));
    }
}