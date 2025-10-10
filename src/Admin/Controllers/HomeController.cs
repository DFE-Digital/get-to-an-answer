using System.Diagnostics;
using System.Net.Http.Headers;
using Admin.Client;
using Admin.Models;
using Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;

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

    [HttpGet("admin/questionnaires/Create")]
    public IActionResult QuestionnaireCreationPage(CreateQuestionnaireRequestDto request)
    {
        return View("CreateQuestionnaire", new QuestionnaireViewModel());
    }

    [HttpPost("admin/questionnaires/Create")]
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
    
    [HttpPost("admin/questionnaires/{questionnaireId}/questions/Create")]
    public async Task<IActionResult> PerformQuestionnaireCreation(int questionnaireId, CreateQuestionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return View("AddQuestion", new QuestionnaireViewModel
            {
                CreateQuestion = request
            }); // return errors to the same page
        }
        
        request.QuestionnaireId = questionnaireId;
        
        var question = await apiClient.CreateQuestionAsync(request);

        if (question == null)
        {
            return BadRequest();
        }
        
        return RedirectToAction(nameof(QuestionPage), new { questionId = question.Id });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/Publish/Confirm")]
    public async Task<IActionResult> ConfirmQuestionnairePublish(int questionnaireId)
    {
        return View("PublishQuestionnaireConfirmation", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/Publish")]
    public async Task<IActionResult> PublishQuestionnaire(int questionnaireId)
    {
        await apiClient.UpdateQuestionStatusAsync(questionnaireId, new UpdateQuestionStatusRequestDto
        {
            Id = questionnaireId,
            Status = EntityStatus.Published
        });
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new { questionnaireId });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/Delete/Confirm")]
    public async Task<IActionResult> ConfirmQuestionnaireDelete(int questionnaireId)
    {
        return View("DeleteQuestionnaireConfirmation", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/Delete")]
    public async Task<IActionResult> DeleteQuestionnaire(int questionnaireId)
    {
        await apiClient.UpdateQuestionStatusAsync(questionnaireId, new UpdateQuestionStatusRequestDto
        {
            Id = questionnaireId,
            Status = EntityStatus.Deleted
        });
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new { questionnaireId });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/invite-request")]
    public async Task<IActionResult> QuestionnaireInviteRequest(int questionnaireId)
    {
        return View("QuestionnaireContributorInvitation", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/invite-response")]
    public async Task<IActionResult> QuestionnaireInvite(int questionnaireId, [FromForm(Name = "Accepted")] bool accepted)
    {
        if (accepted)
        {
            await apiClient.AddSelfToQuestionnaireContributorAsync(questionnaireId);
        }
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new { questionnaireId, accepted });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/Track")]
    public async Task<IActionResult> QuestionnaireTrackingPage(int questionnaireId, bool accepted = false)
    {
        return View("TrackQuestionnaire", new QuestionnaireViewModel
        {
            InviteAccepted = accepted,
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/edit")]
    public async Task<IActionResult> QuestionnaireEditPage(int questionnaireId)
    {
        var questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId);
        
        if (questionnaire == null)
            return NotFound();
        
        return View("EditQuestionnaire", new QuestionnaireViewModel
        {
            UpdateQuestionnaire = new UpdateQuestionnaireRequestDto
            {
                Id = questionnaireId,
                Title = questionnaire.Title, 
                Description = questionnaire.Description
            }
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/edit")]
    public async Task<IActionResult> SaveQuestionnaire(int questionnaireId, UpdateQuestionnaireRequestDto request)
    {
        await apiClient.UpdateQuestionnaireAsync(questionnaireId, request);
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new { questionnaireId });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/questions")]
    public async Task<IActionResult> QuestionManagementPage(int questionnaireId)
    {
        return View("ManageQuestions", new QuestionnaireViewModel
        {
            QuestionnaireId = questionnaireId,
            Questions = await apiClient.GetQuestionsAsync(questionnaireId)
        });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/questions/Create")]
    public async Task<IActionResult> QuestionCreationPage(int questionnaireId)
    {
        return View("AddQuestion", new QuestionnaireViewModel
        {
            QuestionnaireId = questionnaireId
        });
    }

    [HttpGet("admin/questions/{questionId}/edit")]
    public async Task<IActionResult> QuestionPage(int questionId)
    {
        var question = await apiClient.GetQuestionAsync(questionId);
        
        if (question == null)
            return NotFound();
        
        return View("EditQuestion", new QuestionnaireViewModel
        {
            QuestionnaireId = question.QuestionnaireId,
            UpdateQuestion = new UpdateQuestionRequestDto
            {
                Id = question.Id,
                QuestionnaireId = question.QuestionnaireId,
                Content = question.Content,
                Description = question.Description,
                Type = question.Type
            },
            Answers = question.Answers
        });
    }
    
    [HttpPost("admin/questions/{questionId}/edit")]
    public async Task<IActionResult> SaveQuestion(int questionId, UpdateQuestionRequestDto request)
    {
        await apiClient.UpdateQuestionAsync(questionId, request);
        
        return RedirectToAction(nameof(QuestionManagementPage), new { questionnaireId = request.QuestionnaireId });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/questions/{questionId}/answers/edit")]
    public async Task<IActionResult> AddAnswersPage(int questionnaireId, int questionId, bool addEmptyAnswerOption = false)
    {
        return View("AddAnswers", new QuestionnaireViewModel
        {
            AddEmptyAnswerOption = addEmptyAnswerOption,
            QuestionnaireId = questionnaireId,
            QuestionId = questionId,
            Answers = await apiClient.GetAnswersAsync(questionId),
            Questions = (await apiClient.GetQuestionsAsync(questionnaireId))
                .Where(q => q.Id != questionId).ToList()
        });
    }

    [HttpPost("admin/questionnaires/{questionnaireId}/questions/{questionId}/answers/edit")]
    public async Task<IActionResult> SaveAnswers(int questionnaireId, int questionId, 
        [FromForm(Name = "Answers")] List<UpdateAnswerRequestDto> requests, 
        [FromForm(Name = "AddEmptyAnswerOption")] bool addEmptyAnswerOption)
    {
        await Task.WhenAll(requests.Select<UpdateAnswerRequestDto, Task>(req =>
        {
            if (req.Id is { } id)
            {
                return apiClient.UpdateAnswerAsync(id, req);
            }
            
            return apiClient.CreateAnswerAsync(new CreateAnswerRequestDto
            {
                Content = req.Content,
                Description = req.Description,
                Destination = req.Destination,
                DestinationType = req.DestinationType,
                QuestionId = questionId,
                QuestionnaireId = req.QuestionnaireId,
                Score = req.Score,
            });
        }));

        return await AddAnswersPage(questionnaireId, questionId, addEmptyAnswerOption);
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