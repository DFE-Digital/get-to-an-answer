using Common.Client;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Confirmations;

[Authorize]
public class ConfirmRemoveContributor(IApiClient apiClient) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public required Guid QuestionnaireId { get; set; }
    
    [FromRoute(Name = "contributorId")]
    public required string ContributorId { get; set; }
    
    [BindProperty]
    public string? ContributorEmail { get; set; }
    
    [BindProperty]
    public bool RemoveContributor { get; set; }

    public void OnGet()
    {
        ContributorEmail = TempData.Peek("ContributorEmail")?.ToString();
    }
    
    public async Task<IActionResult> OnPostContinueAsync()
    {
        if (RemoveContributor)
        {
            await apiClient.RemoveQuestionnaireContributor(QuestionnaireId, ContributorId);
                
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState
            {
                JustRemovedContributor = true,
                ContributorEmail = ContributorEmail
            });
        }
        return Redirect(string.Format(Routes.AddAndEditQuestionnaireContributors, QuestionnaireId));
    }
}