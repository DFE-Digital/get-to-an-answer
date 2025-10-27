using System.Diagnostics;
using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using Microsoft.AspNetCore.Authorization;

namespace Frontend.Controllers;

[AllowAnonymous]
public class HomeController(IApiClient apiClient, ILogger<HomeController> logger) : Controller
{
    [HttpGet("questionnaires/{questionnaireSlug}/start")]
    public async Task<IActionResult> GetStartPage(string questionnaireSlug, [FromQuery] bool? embed = null)
    {
        return View("QuestionnaireStart", new QuestionnaireViewModel
        {
            IsEmbedded = embed ?? false,
            Questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(questionnaireSlug),
        });
    }
    
    [HttpGet("questionnaires/{questionnaireSlug}")]
    public async Task<IActionResult> GetInitialQuestionPage(string questionnaireSlug, [FromQuery] bool? embed = null)
    {
        var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(questionnaireSlug);
        
        if (questionnaire == null)
            return NotFound();
        
        return View("QuestionnaireQuestion", new QuestionnaireViewModel
        {
            IsEmbedded = embed ?? false,
            Questionnaire = questionnaire,
            NextStateRequest = new GetNextStateRequest(),
            Destination = new DestinationDto
            {
                Type = DestinationType.Question,
                Question = await apiClient.GetInitialQuestion(questionnaire.Id)
            }
        });
    }
    
    [HttpPost("/questionnaires/{questionnaireSlug}")]
    public async Task<IActionResult> GetNextStatePage(string questionnaireSlug, GetNextStateRequest request, 
        [FromForm(Name = "Scores")] Dictionary<Guid, float> scores, [FromQuery] bool? embed = null)
    {
        var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(questionnaireSlug);
        
        if (questionnaire == null)
            return NotFound();
        
        if (request.SelectedAnswerIds.Count > 1)
        {
            request.SelectedAnswerId = scores.OrderByDescending(kv => kv.Value).First().Key;
        } 
        else if (request.SelectedAnswerIds.Count == 1)
        {
            request.SelectedAnswerId = request.SelectedAnswerIds.First();
        }
        
        var destination = await apiClient.GetNextState(questionnaire.Id, request);
        
        if (destination == null)
            return NotFound();

        if (destination is { Type: DestinationType.ExternalLink, Content: not null })
            return Redirect(destination.Content);
        
        return View("QuestionnaireQuestion", new QuestionnaireViewModel
        {
            IsEmbedded = embed ?? false,
            Questionnaire = questionnaire,
            NextStateRequest = new GetNextStateRequest(),
            Destination = destination
        });
    }

    public IActionResult Index()
    {
        return View(new QuestionnaireViewModel());
    }

    public IActionResult Privacy()
    {
        return View(new QuestionnaireViewModel());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}