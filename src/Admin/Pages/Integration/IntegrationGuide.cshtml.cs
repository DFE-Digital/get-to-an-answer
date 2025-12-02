using AngleSharp.Common;
using Common.Client;
using Common.Domain;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Integration;

[Authorize]
public class IntegrationGuide(IApiClient apiClient, IMsGraphClient graphClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    [BindProperty] public required QuestionnaireDto? Questionnaire { get; set; }
    
    public async Task OnGet()
    {
        Questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
    }
}