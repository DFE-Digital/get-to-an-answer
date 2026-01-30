using Common.Client;
using Common.Domain;
using Common.Domain.Graph;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Admin.Pages.Contributors;

[Authorize]
public class AddEditQuestionnaireContributors(ILogger<AddEditQuestionnaireContributors> logger, 
    IApiClient apiClient, IMsGraphClient graphClient)
    : BasePageModel
{
    [FromRoute] public Guid QuestionnaireId { get; set; }

    [BindProperty] public List<GraphUser> Contributors { get; set; } = [];
    
    public QuestionnaireState? QuestionnaireState { get; set;}
    
    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);

        try
        {
            logger.LogInformation("Getting questions for questionnaire {QuestionnaireId}", QuestionnaireId);
            var contributors = await apiClient.GetQuestionnaireContributors(QuestionnaireId);

            var graphUsers = await graphClient.GetGraphUsersAsync(contributors.ToArray());

            Contributors = graphUsers.Value;

            var stateFound = TempData.TryGetValue(nameof(QuestionnaireState), out var value);

            if (stateFound)
            {
                var deserialized = JsonConvert.DeserializeObject<QuestionnaireState>(value?.ToString() ?? string.Empty);
                if (deserialized == null)
                    logger.LogError("Failed to deserialize QuestionnaireState from TempData.");

                QuestionnaireState = deserialized;
            }
        }
        catch (GetToAnAnswerApiException e) when (e.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            logger.LogWarning("User does not have permission to access this questionnaire {QuestionnaireId}", QuestionnaireId);
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState
            {
                JustRemovedAsContributor = true, QuestionnaireTitle = QuestionnaireTitle
            });
            return Redirect(Routes.QuestionnairesManage);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting questions for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }

        return Page();
    }

    public ActionResult OnPostDeleteContributorAsync(
        [FromForm(Name = "ContributorEmail")] string contributorEmail,
        [FromForm(Name = "ContributorId")] string contributorId)
    {
        TempData["ContributorEmail"] = contributorEmail;
        
        return Redirect(string.Format(Routes.ConfirmRemoveContributor, QuestionnaireId, contributorId));
    }

    public ActionResult OnPostSaveAndContinueAsync()
    {
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}