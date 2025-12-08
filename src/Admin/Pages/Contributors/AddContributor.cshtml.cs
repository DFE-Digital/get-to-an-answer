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
    [BindProperty] 
    [Required(ErrorMessage = "Enter a person's email")]
    public string ContributorEmail { get; set; } = "";

    public IActionResult OnGet()
    {
        BackLinkSlug = string.Format(Routes.AddAndEditQuestionnaireContributors, QuestionnaireId);
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var graphUser = await graphClient.GetGraphUserAsync(ContributorEmail);
            
            if (graphUser == null)
                return NotFound();

            await apiClient.AddQuestionnaireContributor(QuestionnaireId, new AddContributorRequestDto()
            {
                Id = graphUser.Id
            });

            TempData[nameof(QuestionnaireState)] =
                JsonConvert.SerializeObject(new QuestionnaireState { JustUpdated = true });

            return Redirect(string.Format(Routes.AddAndEditQuestionnaireContributors, QuestionnaireId));
        }
        catch (GetToAnAnswerApiException e) when (e.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            ModelState.AddModelError(nameof(ContributorEmail), e.ProblemDetails?.Detail!);
            return Page();
        }
        catch (MsGraphException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
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