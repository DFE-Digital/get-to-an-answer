using System.Diagnostics;
using Admin.Models;
using Common.Client;
using Common.Domain;
using Common.Domain.Request.Add;
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

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        return View("Index", new QuestionnaireViewModel
        {
            Questionnaires = await apiClient.GetQuestionnairesAsync()
        });
    }

    [HttpGet("admin/questionnaires")]
    public async Task<IActionResult> ManageQuestionnairesPage()
    {
        return View("ManageQuestionnaires", new QuestionnaireViewModel
        {
            Questionnaires = await apiClient.GetQuestionnairesAsync()
        });
    }

    [HttpGet("admin/questionnaires/create")]
    public IActionResult QuestionnaireCreationPage(CreateQuestionnaireRequestDto request)
    {
        return View("CreateQuestionnaire", new QuestionnaireViewModel());
    }

    [HttpPost("admin/questionnaires/create")]
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
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new
        {
            questionnaireId = questionnaire.Id,
            justCreated = true
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/questions/{id}/move-down")]
    public async Task<IActionResult> PerformQuestionnaireMoveDown(Guid questionnaireId, Guid id)
    {
        await apiClient.MoveQuestionDownOneAsync(questionnaireId, id);
        
        return RedirectToAction(nameof(QuestionManagementPage), new
        {
            questionnaireId,
            questionId = id,
            justCreated = true
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/questions/{id}/move-up")]
    public async Task<IActionResult> PerformQuestionnaireMoveUp(Guid questionnaireId, Guid id)
    {
        await apiClient.MoveQuestionUpOneAsync(questionnaireId, id);
        
        return RedirectToAction(nameof(QuestionManagementPage), new
        {
            questionnaireId,
            questionId = id,
            justCreated = true
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/questions/create")]
    public async Task<IActionResult> PerformQuestionCreation(Guid questionnaireId, CreateQuestionRequestDto request)
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
    
    [HttpGet("admin/questionnaires/{questionnaireId}/publish/confirm")]
    public async Task<IActionResult> ConfirmQuestionnairePublish(Guid questionnaireId)
    {
        return View("PublishQuestionnaireConfirmation", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/publish")]
    public async Task<IActionResult> PublishQuestionnaire(Guid questionnaireId)
    {
        await apiClient.PublishQuestionnaireAsync(questionnaireId);
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new
        {
            questionnaireId,
            justPublished = true
        });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/unpublish/confirm")]
    public async Task<IActionResult> ConfirmQuestionnaireUnpublish(Guid questionnaireId)
    {
        return View("UnpublishQuestionnaireConfirmation", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/unpublish")]
    public async Task<IActionResult> UnpublishQuestionnaire(Guid questionnaireId)
    {
        await apiClient.UnpublishQuestionnaireAsync(questionnaireId);
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new
        {
            questionnaireId,
            justUnpublished = true
        });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/delete/confirm")]
    public async Task<IActionResult> ConfirmQuestionnaireDelete(Guid questionnaireId)
    {
        return View("DeleteQuestionnaireConfirmation", new QuestionnaireViewModel
        {
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/delete")]
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
    public async Task<IActionResult> QuestionnaireTrackingPage(Guid questionnaireId, 
        bool accepted = false, 
        bool justCreated = false, 
        bool justCloned = false, 
        bool justUpdated = false,
        bool justDeleted = false,
        bool justPublished = false,
        bool justUnpublished = false)
    {
        return View("TrackQuestionnaire", new QuestionnaireViewModel
        {
            JustUnpublished = justUnpublished,
            JustPublished = justPublished,
            JustDeleted = justDeleted,
            JustUpdated = justUpdated,
            JustCloned = justCloned,
            JustCreated = justCreated,
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
                Title = questionnaire.Title
            }
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/clone")]
    public async Task<IActionResult> CloneQuestionnaire(Guid questionnaireId, CloneQuestionnaireRequestDto request)
    {
        var cloneQuestionnaire = await apiClient.CloneQuestionnaireAsync(questionnaireId, request);
        
        if (cloneQuestionnaire == null)
            return BadRequest();
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new
        {
            questionnaireId = cloneQuestionnaire.Id,
            justCloned = true,
        });
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
    
    [HttpGet("admin/questionnaires/{questionnaireId}/start-page/edit")]
    public async Task<IActionResult> QuestionnaireStartPageEditPage(Guid questionnaireId)
    {
        var questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId);
        
        if (questionnaire == null)
            return NotFound();
        
        return View("EditQuestionnaireStartPage", new QuestionnaireViewModel
        {
            UpdateQuestionnaire = new UpdateQuestionnaireRequestDto
            {
                Id = questionnaireId,
                Title = questionnaire.Title, 
                Description = questionnaire.Description,
                Slug = questionnaire.Slug,
                DisplayTitle = questionnaire.DisplayTitle,
            }
        });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/slug/edit")]
    public async Task<IActionResult> QuestionnaireSlugEditPage(Guid questionnaireId)
    {
        var questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId);
        
        if (questionnaire == null)
            return NotFound();
        
        return View("EditQuestionnaireSlug", new QuestionnaireViewModel
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
        
        return RedirectToAction(nameof(QuestionnaireTrackingPage), new
        {
            questionnaireId, 
            justUpdated = true
        });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/contributors/new")]
    public IActionResult AddQuestionnaireContributorPage(Guid questionnaireId)
    {
        return View("AddContributor", new QuestionnaireViewModel
        {
            QuestionnaireId = questionnaireId,
        });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/contributors")]
    public async Task<IActionResult> QuestionnaireContributorsPage(Guid questionnaireId, 
        bool justCreated = false, bool justDeleted = false)
    {
        return View("ManageContributors", new QuestionnaireViewModel
        {
            JustDeleted = justDeleted,
            JustCreated = justCreated,
            QuestionnaireId = questionnaireId,
            Contributors = await apiClient.GetQuestionnaireContributors(questionnaireId),
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }
    
    [HttpPost("admin/questionnaires/{questionnaireId}/contributors")]
    public async Task<IActionResult> AddQuestionnaireContributor(Guid questionnaireId, AddContributorRequestDto request)
    {
        await apiClient.AddQuestionnaireContributor(questionnaireId, request);
        
        return RedirectToAction(nameof(QuestionnaireContributorsPage), new
        {
            questionnaireId, 
            justCreated = true
        });
    }
    
    [HttpDelete("admin/questionnaires/{questionnaireId}/contributors")]
    public async Task<IActionResult> DeleteQuestionnaireContributor(Guid questionnaireId, [FromForm(Name = "Email")] string email)
    {
        await apiClient.RemoveQuestionnaireContributor(questionnaireId, email);
        
        return RedirectToAction(nameof(QuestionnaireContributorsPage), new
        {
            questionnaireId, 
            justDeleted = true
        });
    }
    
    [HttpGet("admin/questionnaires/{questionnaireId}/questions")]
    public async Task<IActionResult> QuestionManagementPage(
        Guid questionnaireId, 
        Guid? questionId = null, 
        bool justCreated = false, 
        bool justUpdated = false,
        bool justDeleted = false, 
        bool justMovedUp = false, 
        bool justMovedDown = false)
    {
        return View("ManageQuestions", new QuestionnaireViewModel
        {
            JustMovedUp = justMovedUp,
            JustMovedDown = justMovedDown,
            JustCreated = justCreated,
            JustUpdated = justUpdated,
            JustDeleted = justDeleted,
            QuestionnaireId = questionnaireId,
            QuestionId = questionId,
            Questions = await apiClient.GetQuestionsAsync(questionnaireId),
            Questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId)
        });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/questions/create")]
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
        
        return RedirectToAction(nameof(QuestionManagementPage), new
        {
            questionnaireId = request.QuestionnaireId, 
            justUpdated = false
        });
    }

    [HttpGet("admin/questionnaires/{questionnaireId}/questions/{questionId}/answers/edit")]
    public async Task<IActionResult> AddAnswersPage(
        Guid questionnaireId, 
        Guid questionId, 
        bool addEmptyAnswerOption = false, 
        bool justUpdated = false, 
        bool justDeleted = false)
    {
        return View("AddAnswers", new QuestionnaireViewModel
        {
            JustDeleted = justDeleted,
            JustUpdated = justUpdated,
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
        return View(new QuestionnaireViewModel());
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
    public async Task<IActionResult> ManageContentPage(Guid questionnaireId, 
        bool justCreated = false, 
        bool justUpdated = false,
        bool justDeleted = false)
    {
        return View("ManageContent", new QuestionnaireViewModel
        {
            JustDeleted = justDeleted,
            JustCreated = justCreated,
            JustUpdated = justUpdated,
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

        return RedirectToAction(nameof(QuestionManagementPage), new
        {
            questionnaireId = question.QuestionnaireId,
            justDeleted = true
        });
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

        return RedirectToAction(nameof(QuestionManagementPage), new
        {
            questionnaireId = question.QuestionnaireId,
            justDeleted = true
        });
    }
    private const string TermsCookieName = "accepAgreementCookieString"; // from selection
    private const int TermsCookieDays = 365;

    // Show the terms page unless already accepted
    [HttpGet("/admin/terms-of-service-agreement")]
    public IActionResult TermsOfServiceAgreement()
    {
        if (HasAcceptedTerms())
        {
            return RedirectToAction(nameof(ManageQuestionnairesPage));
        }

        return View("TermsOfServiceAgreement", new QuestionnaireViewModel());
    }

    // Called when the user clicks "Accept"
    [HttpPost("/admin/terms-of-service-agreement")]
    //[ValidateAntiForgeryToken]
    public IActionResult AcceptTerms([FromForm(Name = "Accepted")] string? accepted)
    {
        if (accepted == "true")
        {
            SetTermsAcceptedCookie();
            return RedirectToAction(nameof(ManageQuestionnairesPage));
        }
        
        ModelState.AddModelError("Accepted", "You must accept the terms to continue.");
        
        return View("TermsOfServiceAgreement", new QuestionnaireViewModel());
    }

    private bool HasAcceptedTerms()
    {
        var cookie = Request.Cookies[TermsCookieName];
        return string.Equals(cookie, "accepted", StringComparison.OrdinalIgnoreCase);
    }

    private void SetTermsAcceptedCookie()
    {
        var options = new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(TermsCookieDays),
            Secure = true,              // send only over HTTPS
            HttpOnly = false,           // allow client-side checks if needed
            SameSite = SameSiteMode.Lax,
            Path = "/"                  // available to entire site
        };

        Response.Cookies.Append(TermsCookieName, "accepted", options);
    }
}