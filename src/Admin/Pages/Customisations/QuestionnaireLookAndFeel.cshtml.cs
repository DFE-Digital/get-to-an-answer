using Common.Client;
using Common.Domain;
using Common.Domain.Admin;
using Common.Domain.Request.Update;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Customisations;

public class QuestionnaireLookAndFeel(IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    [BindProperty] public required QuestionnaireDto Questionnaire { get; set; }
    
    public void OnGet()
    {
        var request = new UpdateLookAndFeelRequestDto
        {
            
        };
        
        apiClient.UpdateQuestionnaireLookAndFeelAsync(QuestionnaireId, request);
    }
}