using System.ComponentModel.DataAnnotations;
using Common.Client;
using Common.Domain.Request.Add;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Contributors;

[Authorize]
public class AddContributor(ILogger<AddContributor> logger, 
    IApiClient apiClient, IMsGraphClient graphClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }
    
    public string? QuestionnaireTitle { get; set; } = string.Empty;

    [BindProperty] 
    [Required(ErrorMessage = "Enter a contributor email")]
    public string ContributorEmail { get; set; } = "";

    public IActionResult OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);
        QuestionnaireTitle = TempData.Peek("QuestionnaireTitle") as string;
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await graphClient.GetGraphUserAsync(ContributorEmail);

            await apiClient.AddQuestionnaireContributor(QuestionnaireId, new AddContributorRequestDto()
            {
                Email = ContributorEmail
            });

            TempData[nameof(QuestionnaireState)] =
                JsonConvert.SerializeObject(new QuestionnaireState { JustUpdated = true });

            return Page();
        }
        catch (MsGraphException)
        {
            ModelState.AddModelError(nameof(ContributorEmail), "Contributor not found");
            return Page();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating content for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }
    }
}