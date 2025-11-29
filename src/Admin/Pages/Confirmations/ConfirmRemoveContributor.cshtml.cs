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
    public Guid QuestionnaireId { get; set; }
    
    public string? QuestionnaireTitle { get; set; } = string.Empty;
    
    [FromRoute(Name = "contentId")]
    public Guid ContributorId { get; set; }
    
    [TempData(Key = "ContributorEmail")] public string? ContributorEmail { get; set; }
    
    [BindProperty] public bool RemoveContributor { get; set; }

    public void OnGet()
    {
        QuestionnaireTitle = TempData.Peek("QuestionnaireTitle") as string;
        ContributorEmail = TempData.Peek("ContributorEmail") as string;
    }
    
    public async Task<IActionResult> OnPostContinueAsync()
    {
        if (RemoveContributor)
        {
            await apiClient.RemoveQuestionnaireContributor(QuestionnaireId, ContributorId.ToString());
                
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustUpdated = true });
        }
        return Redirect(string.Format(Routes.AddAndEditQuestionnaireContributors, QuestionnaireId));
    }
}