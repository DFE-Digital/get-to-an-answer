using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Extensions;
using Api.Services;
using Common.Domain;
using Common.Domain.Request.Add;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Common.Local;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class QuestionnaireController(IQuestionnaireService questionnaireService) : ControllerBase
{
    [HttpPost("questionnaires")]
    public async Task<IActionResult> CreateQuestionnaire(CreateQuestionnaireRequestDto request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);
        
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.CreateQuestionnaire(email, request)).ToActionResult();
    }
    
    [HttpGet("questionnaires/{id}")]
    public IActionResult GetQuestionnaire(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return questionnaireService.GetQuestionnaire(email, id).ToActionResult();
    }
    
    [HttpGet("questionnaires")]
    public IActionResult GetQuestionnaires()
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return questionnaireService.GetQuestionnaires(email).ToActionResult();
    }

    [HttpPut("questionnaires/{id}")]
    public async Task<IActionResult> UpdateQuestionnaire(Guid id, UpdateQuestionnaireRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.UpdateQuestionnaire(email, id, request)).ToActionResult();
    }

    [HttpPut("questionnaires/{id}/publish")]
    public async Task<IActionResult> PublishQuestionnaire(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.PublishQuestionnaire(email, id)).ToActionResult();
    }

    [HttpDelete("questionnaires/{id}/unpublish")]
    public async Task<IActionResult> UnpublishQuestionnaire(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.UnpublishQuestionnaire(email, id)).ToActionResult();
    }

    [HttpDelete("questionnaires/{id}")]
    public async Task<IActionResult> DeleteQuestionnaire(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.DeleteQuestionnaire(email, id)).ToActionResult();
    }
    
    [HttpPut("questionnaires/{id}/contributors/self")]
    public async Task<IActionResult> AddSelfToQuestionnaireContributors(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.AddSelfToQuestionnaireContributors(email, id)).ToActionResult();
    }

    [HttpPost("questionnaires/{id}/clones")]
    public async Task<IActionResult> CloneQuestionnaire(Guid id, CloneQuestionnaireRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.CloneQuestionnaire(email, id, request)).ToActionResult();
    }
    
    [HttpGet("questionnaires/{questionnaireId}/contributors")]
    public async Task<IActionResult> GetContributors(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.GetContributors(email, questionnaireId)).ToActionResult();
    }

    [HttpPut("questionnaires/{id}/contributors")]
    public async Task<IActionResult> AddContributor(Guid id, AddContributorRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.AddContributor(email, id, request)).ToActionResult();
    }

    [HttpDelete("questionnaires/{id}/contributors/{email}")]
    public async Task<IActionResult> RemoveContributor(Guid id, string email)
    {
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await questionnaireService.RemoveContributor(currentUserEmail, id, email)).ToActionResult();
    }
}