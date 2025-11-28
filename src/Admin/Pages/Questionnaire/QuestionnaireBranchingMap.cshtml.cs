using Common.Client;
using Common.Domain.Admin;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Questionnaire;

public class QuestionnaireBranchingMap(IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    [BindProperty] public required QuestionnaireBranchingMapDto QuestionnaireBranchingMapDto { get; set; }
    
    public void OnGet()
    {
        apiClient.GetBranchingMap(QuestionnaireId);
    }
}