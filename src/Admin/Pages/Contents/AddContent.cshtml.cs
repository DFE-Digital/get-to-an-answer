using System.ComponentModel.DataAnnotations;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Contents;

[Authorize]
public class AddContent(ILogger<AddContent> logger, IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }
    
    public string? QuestionnaireTitle { get; set; }

    [BindProperty] 
    [Required(ErrorMessage = "Enter a title")] 
    public string ContentTitle { get; set; } = "";

    [BindProperty] 
    [Required(ErrorMessage = "Enter some content")] 
    public string ContentValue { get; set; } = "";

    [BindProperty] 
    [Required(ErrorMessage = "Enter a reference name")] 
    public string ContentRefName { get; set; } = "";

    public IActionResult OnGet()
    {
        BackLinkSlug = string.Format(Routes.AddAndEditResultPages, QuestionnaireId);

        if (TempData.Peek("QuestionnaireTitle") is string title)
        {
            QuestionnaireTitle = title;
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await apiClient.CreateContentAsync(new CreateContentRequestDto
            {
                QuestionnaireId = QuestionnaireId,
                Title = ContentTitle,
                Content = ContentValue,
                ReferenceName = ContentRefName
            });
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustUpdated = true });

            return Page();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating content for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }
    }
}