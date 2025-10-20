using System.Diagnostics;
using Admin.Models;
using Common.Client;
using Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Microsoft.AspNetCore.Authorization;

namespace Admin.Controllers;

[Controller]
[Authorize]
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
    public async Task<IActionResult> PerformQuestionnaireCreation(Guid questionnaireId, CreateQuestionRequestDto request)
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
    public async Task<IActionResult> ConfirmQuestionnairePublish(Guid questionnaireId)
    {
        return View("PublishQuestionnaireConfirmation", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/Publish")]
    public async Task<IActionResult> PublishQuestionnaire(Guid questionnaireId)
    {
        await apiClient.UpdateQuestionnaireStatusAsync(questionnaireId, new UpdateQuestionnaireStatusRequestDto
        {
            Id = questionnaireId,
            Status = EntityStatus.Published
        });
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new { questionnaireId });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/Delete/Confirm")]
    public async Task<IActionResult> ConfirmQuestionnaireDelete(Guid questionnaireId)
    {
        return View("DeleteQuestionnaireConfirmation", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/Delete")]
    public async Task<IActionResult> DeleteQuestionnaire(Guid questionnaireId)
    {
        await apiClient.DeleteQuestionnaireAsync(questionnaireId);
        
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/invite-request")]
    public async Task<IActionResult> QuestionnaireInviteRequest(Guid questionnaireId)
    {
        return View("QuestionnaireContributorInvitation", new QuestionnaireViewModel
        {
            Questionnaire = new QuestionnaireDto { Id = questionnaireId }
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/invite-response")]
    public async Task<IActionResult> QuestionnaireInvite(Guid questionnaireId, [FromForm(Name = "Accepted")] bool accepted)
    {
        if (accepted)
        {
            await apiClient.AddSelfToQuestionnaireContributorAsync(questionnaireId);
        }
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new { questionnaireId, accepted });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/track")]
    public async Task<IActionResult> QuestionnaireTrackingPage(Guid questionnaireId, bool accepted = false)
    {
        return View("TrackQuestionnaire", new QuestionnaireViewModel
        {
            InviteAccepted = accepted,
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/clone")]
    public async Task<IActionResult> QuestionnaireClonePage(Guid questionnaireId)
    {
        var questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId);
        
        if (questionnaire == null)
            return NotFound();
        
        return View("CloneQuestionnaire", new QuestionnaireViewModel
        {
            CloneQuestionnaire = new CloneQuestionnaireRequestDto
            {
                OriginalQuestionnaireId = questionnaireId,
                Title = questionnaire.Title, 
                Description = questionnaire.Description
            }
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/clone")]
    public async Task<IActionResult> CloneQuestionnaire(Guid questionnaireId, CloneQuestionnaireRequestDto request)
    {
        var cloneQuestionnaire = await apiClient.CloneQuestionnaireAsync(questionnaireId, request);
        
        if (cloneQuestionnaire == null)
            return BadRequest();
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new { questionnaireId = cloneQuestionnaire.Id });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/edit")]
    public async Task<IActionResult> QuestionnaireEditPage(Guid questionnaireId)
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
                Description = questionnaire.Description,
                Slug = questionnaire.Slug
            }
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/edit")]
    public async Task<IActionResult> SaveQuestionnaire(Guid questionnaireId, UpdateQuestionnaireRequestDto request)
    {
        await apiClient.UpdateQuestionnaireAsync(questionnaireId, request);
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new { questionnaireId });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/questions")]
    public async Task<IActionResult> QuestionManagementPage(Guid questionnaireId)
    {
        return View("ManageQuestions", new QuestionnaireViewModel
        {
            QuestionnaireId = questionnaireId,
            Questions = await apiClient.GetQuestionsAsync(questionnaireId),
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/questions/Create")]
    public async Task<IActionResult> QuestionCreationPage(Guid questionnaireId)
    {
        return View("AddQuestion", new QuestionnaireViewModel
        {
            QuestionnaireId = questionnaireId
        });
    }

    [HttpGet("admin/questions/{questionId}/edit")]
    public async Task<IActionResult> QuestionPage(Guid questionId)
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
    public async Task<IActionResult> SaveQuestion(Guid questionId, UpdateQuestionRequestDto request)
    {
        await apiClient.UpdateQuestionAsync(questionId, request);
        
        return RedirectToAction(nameof(QuestionManagementPage), new { questionnaireId = request.QuestionnaireId });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/questions/{questionId}/answers/edit")]
    public async Task<IActionResult> AddAnswersPage(
        Guid questionnaireId, 
        Guid questionId, 
        bool addEmptyAnswerOption = false)
    {
        return View("AddAnswers", new QuestionnaireViewModel
        {
            AddEmptyAnswerOption = addEmptyAnswerOption,
            QuestionnaireId = questionnaireId,
            QuestionId = questionId,
            Answers = await apiClient.GetAnswersAsync(questionId),
            Contents = await apiClient.GetContentsAsync(questionnaireId),   
            Questions = (await apiClient.GetQuestionsAsync(questionnaireId))
                .Where(q => q.Id != questionId).ToList()
        });
    }

    [HttpPost("admin/questionnaires/{questionnaireId}/questions/{questionId}/answers/edit")]
    public async Task<IActionResult> SaveAnswers(Guid questionnaireId, Guid questionId, 
        [FromForm(Name = "Answers")] List<UpdateAnswerRequestDto> requests, 
        [FromForm(Name = "AddEmptyAnswerOption")] bool addEmptyAnswerOption,
        [FromForm(Name = "RemoveAnswerOption")] Guid removeAnswerOption)
    {
        await Task.WhenAll(requests.Select<UpdateAnswerRequestDto, Task>(req =>
        {
            if (req.Id == removeAnswerOption)
            {
                return apiClient.DeleteAnswerAsync(removeAnswerOption);
            }
            
            if (req.Id is { } id)
            {
                return apiClient.UpdateAnswerAsync(id, req);
            }
            
            return apiClient.CreateAnswerAsync(new CreateAnswerRequestDto
            {
                Content = req.Content,
                Description = req.Description,
                DestinationUrl = req.DestinationUrl,
                DestinationType = req.DestinationType,
                DestinationQuestionId = req.DestinationQuestionId,
                DestinationContentId = req.DestinationContentId,
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
    
    [HttpPost("admin/questionnaires/{questionnaireId}/content/create")]
    public async Task<IActionResult> PerformContentCreation(Guid questionnaireId, CreateContentRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return View("CreateContent", new QuestionnaireViewModel
            {
                CreateContent = request
            });
        }

        request.QuestionnaireId = questionnaireId;

        var content = await apiClient.CreateContentAsync(request);

        if (content == null)
        {
            return BadRequest();
        }

        return RedirectToAction(nameof(ContentPage), new { contentId = content.Id });
    }

    [HttpGet("admin/contents/{contentId}/edit")]
    public async Task<IActionResult> ContentPage(Guid contentId)
    {
        var content = await apiClient.GetContentAsync(contentId);

        if (content == null)
            return NotFound();

        return View("EditContent", new QuestionnaireViewModel
        {
            QuestionnaireId = content.QuestionnaireId,
            UpdateContent = new UpdateContentRequestDto
            {
                Id = content.Id,
                QuestionnaireId = content.QuestionnaireId,
                Title = content.Title,
                Content = content.Content,
            }
        });
    }

    [HttpPost("admin/contents/{contentId}/edit")]
    public async Task<IActionResult> SaveContent(Guid contentId, UpdateContentRequestDto request)
    {
        await apiClient.UpdateContentAsync(contentId, request);

        return RedirectToAction(nameof(ManageContentPage), new { questionnaireId = request.QuestionnaireId });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/contents")]
    public async Task<IActionResult> ManageContentPage(Guid questionnaireId)
    {
        return View("ManageContent", new QuestionnaireViewModel
        {
            Contents = await apiClient.GetContentsAsync(questionnaireId),
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/content/create")]
    public async Task<IActionResult> ContentCreationPage(Guid questionnaireId)
    {
        return View("CreateContent", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpGet("admin/questions/{questionId}/delete/confirm")]
    public async Task<IActionResult> ConfirmQuestionDelete(Guid questionId)
    {
        var question = await apiClient.GetQuestionAsync(questionId);

        if (question == null)
            return NotFound();

        return View("DeleteQuestionConfirmation", new QuestionnaireViewModel
        {
            Question = question,
            QuestionnaireId = question.QuestionnaireId
        });
    }

    [HttpPost("admin/questions/{questionId}/delete")]
    public async Task<IActionResult> DeleteQuestion(Guid questionId)
    {
        var question = await apiClient.GetQuestionAsync(questionId);

        if (question == null)
            return NotFound();

        await apiClient.DeleteQuestionAsync(questionId);

        return RedirectToAction(nameof(QuestionManagementPage), new { questionnaireId = question.QuestionnaireId });
    }
    
    [HttpGet("admin/contents/{contentId}/delete/confirm")]
    public async Task<IActionResult> ConfirmContentDelete(Guid questionId)
    {
        var content = await apiClient.GetContentAsync(questionId);

        if (content == null)
            return NotFound();

        return View("DeleteContentConfirmation", new QuestionnaireViewModel
        {
            Content = content,
            QuestionnaireId = content.QuestionnaireId
        });
    }

    [HttpPost("admin/contents/{contentId}/delete")]
    public async Task<IActionResult> DeleteContent(Guid contentId)
    {
        var question = await apiClient.GetQuestionAsync(contentId);

        if (question == null)
            return NotFound();

        await apiClient.DeleteQuestionAsync(contentId);

        return RedirectToAction(nameof(QuestionManagementPage), new { questionnaireId = question.QuestionnaireId });
    }
}