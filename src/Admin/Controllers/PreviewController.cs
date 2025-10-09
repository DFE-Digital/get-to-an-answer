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
    
    [HttpGet("Admin/Questionnaires/{questionnaireId}/Next/Preview")]
    public async Task<IActionResult> GetNextStatePage(int questionnaireId, GetNextStateRequest request)
    {
        return View("PreviewQuestionnaire", new QuestionnaireViewModel
        {
            NextStateRequest = new GetNextStateRequest(),
            Destination = await apiClient.GetNextState(questionnaireId, request)
        });
    }
}