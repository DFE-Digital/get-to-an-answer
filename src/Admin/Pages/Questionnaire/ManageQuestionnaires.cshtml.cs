using AngleSharp.Common;
using Common.Models;
using Common.Models.PageModels;
using Common.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Questionnaire;

[Authorize]
public class ManageQuestionnaires(IApiClient apiClient, IMsGraphClient msGraphClient, ILogger<ManageQuestionnaires> logger) : QuestionnairesPageModel
{
    public async Task<IActionResult> OnGet()
    {
        try
        {
            var questionnaires= await apiClient.GetQuestionnairesAsync();

            if (questionnaires.Count > 0)
            {
                try
                {
                    var contributors = await msGraphClient.GetGraphUsersAsync(questionnaires
                        .Select(q => q.CreatedBy).ToArray());

                    var contributorMap = contributors.Value.ToDictionary(g => g.Id, g => g.DisplayName);

                    questionnaires.ForEach(q => q.CreatedBy = contributorMap.GetOrDefault(q.CreatedBy!, q.CreatedBy));
                } 
                catch (Exception e)
                {
                    logger.LogError(e, "Error getting contributors for questionnaires");
                }
            }
            
            ViewModel.Questionnaires = questionnaires;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return Redirect(Routes.GlobalErrorPage);
        }
        
        return Page();
    }
}