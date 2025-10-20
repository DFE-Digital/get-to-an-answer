using Admin.Models;
using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

public class PreviewController(ILogger<HomeController> logger, IApiClient apiClient) : Controller
{
    [HttpGet("admin/questionnaires/{questionnaireId}/Start/Preview")]
    public async Task<IActionResult> GetStartPage(Guid questionnaireId)
    {
        return View("PreviewQuestionnaireStart", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId),
        });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/Initial/Preview")]
    public async Task<IActionResult> GetInitialQuestionPage(Guid questionnaireId)
    {
        return View("PreviewQuestionnaire", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId),
            NextStateRequest = new GetNextStateRequest(),
            Destination = new DestinationDto
            {
                Type = DestinationType.Question,
                Question = await apiClient.GetInitialQuestion(questionnaireId)
            }
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/Next/Preview")]
    public async Task<IActionResult> GetNextStatePage(Guid questionnaireId, GetNextStateRequest request, 
        [FromForm(Name = "Scores")] Dictionary<Guid, float> scores)
    {
        if (request.SelectedAnswerIds.Count > 1)
        {
            request.SelectedAnswerId = scores.OrderByDescending(kv => kv.Value).First().Key;
        } 
        else if (request.SelectedAnswerIds.Count == 1)
        {
            request.SelectedAnswerId = request.SelectedAnswerIds.First();
        }
        
        var destination = await apiClient.GetNextState(questionnaireId, request);
        
        if (destination == null)
            return NotFound();

        if (destination is { Type: DestinationType.ExternalLink, Content: not null })
            return Redirect(destination.Content);
        
        return View("PreviewQuestionnaire", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId),
            NextStateRequest = new GetNextStateRequest(),
            Destination = destination
        });
    }
}