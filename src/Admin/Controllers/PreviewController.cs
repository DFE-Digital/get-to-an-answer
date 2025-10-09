using Admin.Client;
using Admin.Models;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

public class PreviewController(ILogger<HomeController> logger, IApiClient apiClient) : Controller
{
    [HttpGet("Admin/Questionnaires/{questionnaireId}/Initial/Preview")]
    public async Task<IActionResult> GetInitialQuestionPage(int questionnaireId)
    {
        return View("PreviewQuestionnaire", new QuestionnaireViewModel
        {
            NextStateRequest = new GetNextStateRequest(),
            Destination = new DestinationDto
            {
                Type = DestinationType.Question,
                Question = await apiClient.GetInitialQuestion(questionnaireId)
            }
        });
    }
    
    [HttpPost("Admin/Questionnaires/{questionnaireId}/Next/Preview")]
    public async Task<IActionResult> GetNextStatePage(int questionnaireId, GetNextStateRequest request)
    {
        var destination = await apiClient.GetNextState(questionnaireId, request);
        
        if (destination == null)
            return NotFound();

        if (destination.Type == DestinationType.ExternalLink && destination.Content != null)
            return Redirect(destination.Content);
        
        return View("PreviewQuestionnaire", new QuestionnaireViewModel
        {
            NextStateRequest = new GetNextStateRequest(),
            Destination = destination
        });
    }
}