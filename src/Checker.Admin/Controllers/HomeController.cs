using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Checker.Admin.Models;

namespace Checker.Admin.Controllers;

public class HomeController(ILogger<HomeController> logger) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    public IActionResult Index()
    {
        return View(new ConfigViewModel());
    }

    [HttpGet("Page/NewQuestionnaire")]
    public IActionResult CreateQuestionnairePage()
    {
        return View("CreateQuestionnaire", new ConfigViewModel());
    }

    [HttpGet("Page/Questionnaires/{questionnaireId}/Questions")]
    public IActionResult ManageQuestionsPage(int questionnaireId)
    {
        return View("ManageQuestions", new ConfigViewModel());
    }

    [HttpGet("Page/Questions/{questionId}")]
    public IActionResult CreateQuestionPage(int questionId)
    {
        return View("AddQuestion", new ConfigViewModel());
    }

    [HttpGet("Page/Branches/{branchId}")]
    public IActionResult AddBranchingPage(int branchId)
    {
        return View("AddBranching", new ConfigViewModel());
    }

    [HttpGet("Page/Questions/{branchId}/Answers")]
    public IActionResult AddAnswersPage(int branchId)
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