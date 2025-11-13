using System.Security.Claims;
using Api.Extensions;
using Api.Services;
using Common.Domain.Request.Add;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Common.Local;
using Common.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/questionnaires")]
[Authorize]
public class QuestionnaireController(IQuestionnaireService questionnaireService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateQuestionnaire(CreateQuestionnaireRequestDto request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);
        
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.CreateQuestionnaire(email, request)).ToActionResult();
    }
    
    [HttpGet("{id:guid}")]
    public IActionResult GetQuestionnaire(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return questionnaireService.GetQuestionnaire(email, id).ToActionResult();
    }
    
    [HttpGet]
    public IActionResult GetQuestionnaires()
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return questionnaireService.GetQuestionnaires(email).ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateQuestionnaire(Guid id, UpdateQuestionnaireRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.UpdateQuestionnaire(email, id, request)).ToActionResult();
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> PublishQuestionnaire(Guid id, [FromQuery] [EnumDefined] QuestionnaireAction action)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        if (action == QuestionnaireAction.Publish)
        {
            return (await questionnaireService.PublishQuestionnaire(email, id)).ToActionResult();
        }
        else if (action == QuestionnaireAction.Unpublish)
        {
            return (await questionnaireService.UnpublishQuestionnaire(email, id)).ToActionResult();
        }
        
        return BadRequest();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteQuestionnaire(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.DeleteQuestionnaire(email, id)).ToActionResult();
    }

    [HttpPost("{id:guid}/clones")]
    public async Task<IActionResult> CloneQuestionnaire(Guid id, CloneQuestionnaireRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.CloneQuestionnaire(email, id, request)).ToActionResult();
    }
    
    [HttpGet("{questionnaireId:guid}/contributors")]
    public async Task<IActionResult> GetContributors(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.GetContributors(email, questionnaireId)).ToActionResult();
    }

    [HttpPut("{id:guid}/contributors")]
    public async Task<IActionResult> AddContributor(Guid id, AddContributorRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.AddContributor(email, id, request)).ToActionResult();
    }

    [HttpDelete("{id:guid}/contributors/{email}")]
    public async Task<IActionResult> RemoveContributor(Guid id, string email)
    {
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.RemoveContributor(currentUserEmail, id, email)).ToActionResult();
    }
}