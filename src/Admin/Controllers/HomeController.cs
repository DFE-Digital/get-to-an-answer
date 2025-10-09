using System.Diagnostics;
using System.Net.Http.Headers;
using Admin.Client;
using Admin.Models;
using Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;

namespace Admin.Controllers;


public class HomeController(ILogger<HomeController> logger, IApiClient apiClient) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    public async Task<IActionResult> Index()
    {
        return View("Index", new QuestionnaireViewModel
        {
            Questionnaires = await apiClient.GetQuestionnairesAsync()
        });
    }

    [HttpGet("Admin/Questionnaires/Create")]
    public IActionResult QuestionnaireCreationPage(CreateQuestionnaireRequestDto request)
    {
        return View("CreateQuestionnaire", new QuestionnaireViewModel());
    }

    [HttpPost("Admin/Questionnaires/Create")]
    public async Task<IActionResult> PerformQuestionnaireCreation(CreateQuestionnaireRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return View("CreateQuestionnaire", new QuestionnaireViewModel
            {
                CreateQuestionnaire = request
            }); // return errors to the same page
        }
        
        var questionnaire = await apiClient.CreateQuestionnaireAsync(request);

        if (questionnaire == null)
        {
            return BadRequest();
        }
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new { questionnaireId = questionnaire.Id });
    }

    [HttpGet("Admin/Questionnaires/{questionnaireId}/Edit")]
    public async Task<IActionResult> QuestionnaireTrackingPage(int questionnaireId)
    {
        return View("TrackQuestionnaire", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }

    [HttpGet("Admin/Questionnaires/{questionnaireId}/Questions")]
    public async Task<IActionResult> QuestionManagementPage(int questionnaireId)
    {
        return View("ManageQuestions", new QuestionnaireViewModel
        {
            Questions = await apiClient.GetQuestionsAsync(questionnaireId)
        });
    }

    [HttpGet("Admin/Questions/{questionId}")]
    public IActionResult QuestionPage(int questionId)
    {
        return View("AddQuestion", new ConfigViewModel());
    }

    [HttpGet("Admin/Questions/{branchId}/Answers")]
    public IActionResult AddAnswersPage(int branchId)
    {
        return View("AddAnswers", new ConfigViewModel());
    }

    [HttpGet("Admin/Questionnaires/{questionnaireId}/Preview")]
    public IActionResult PreviewModePage(int questionnaireId)
    {
        return View("AddAnswers", new ConfigViewModel());
    }

    public IActionResult Privacy()
    {
        return View(new ConfigViewModel());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}