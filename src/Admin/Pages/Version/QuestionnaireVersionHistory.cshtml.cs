using AngleSharp.Common;
using Common.Client;
using Common.Domain;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Version;

[Authorize]
public class QuestionnaireVersionHistory(IApiClient apiClient, IMsGraphClient graphClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    [BindProperty] public required List<QuestionnaireVersionDto> QuestionnaireVersions { get; set; }
    
    public async Task OnGet()
    {
        var questionnaireVersions = await apiClient.GetQuestionnaireVersionsAsync(QuestionnaireId);

        var contributors = await graphClient.GetGraphUsersAsync(
            questionnaireVersions.Select(v => v.CreatedBy).ToArray());

        var contributorMap = contributors.Value.ToDictionary(g => g.Id, g => g.DisplayName);
        
        questionnaireVersions.ForEach(q => q.CreatedBy = contributorMap.GetOrDefault(q.CreatedBy!, q.CreatedBy));

        QuestionnaireVersions = questionnaireVersions;
    }
}