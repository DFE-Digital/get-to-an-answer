using AngleSharp.Common;
using Common.Models;
using Common.Models.PageModels;
using Common.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
                        .Select(q => q.CreatedBy).Distinct().ToArray());

                    var contributorMap = contributors.Value.ToDictionary(g => g.Id, g => g.DisplayName);

                    questionnaires.ForEach(q => q.CreatedBy = contributorMap.GetOrDefault(q.CreatedBy!, q.CreatedBy));
                } 
                catch (MsGraphException e)
                {
                    logger.LogError(e, "Error getting contributors for questionnaires");
                    return RedirectToPage("/error");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error loading questionnaires");
                    return RedirectToPage("/error");
                }
            }
        
            var stateFound = TempData.TryGetValue(nameof(QuestionnaireState), out var value);

            if (stateFound)
            {
                var deserialized = JsonConvert.DeserializeObject<QuestionnaireState>(value?.ToString() ?? string.Empty);
                if (deserialized == null)
                    logger.LogError("Failed to deserialize QuestionnaireState from TempData.");
                
                ViewModel.QuestionnaireState = deserialized;
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