using Api.Extensions;
using Api.Services;
using Common.Domain.Request.Add;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Extensions;
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
        
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.CreateQuestionnaire(userId, request)).ToActionResult();
    }
    
    [HttpGet("{id:guid}")]
    public IActionResult GetQuestionnaire(Guid id)
    {
        var userId = User.GetIdClaim()!;
        
        return questionnaireService.GetQuestionnaire(userId, id).ToActionResult();
    }
    
    [HttpGet]
    public IActionResult GetQuestionnaires()
    {
        var userId = User.GetIdClaim()!;
        
        return questionnaireService.GetQuestionnaires(userId).ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateQuestionnaire(Guid id, UpdateQuestionnaireRequestDto request)
    {
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.UpdateQuestionnaire(userId, id, request)).ToActionResult();
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> PublishQuestionnaire(Guid id, [FromQuery] [EnumDefined] QuestionnaireAction action)
    {
        var userId = User.GetIdClaim()!;

        if (action == QuestionnaireAction.Publish)
        {
            return (await questionnaireService.PublishQuestionnaire(userId, id)).ToActionResult();
        }
        else if (action == QuestionnaireAction.Unpublish)
        {
            return (await questionnaireService.UnpublishQuestionnaire(userId, id)).ToActionResult();
        }
        
        return BadRequest();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteQuestionnaire(Guid id)
    {
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.DeleteQuestionnaire(userId, id)).ToActionResult();
    }

    [HttpPost("{id:guid}/clones")]
    public async Task<IActionResult> CloneQuestionnaire(Guid id, CloneQuestionnaireRequestDto request)
    {
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.CloneQuestionnaire(userId, id, request)).ToActionResult();
    }
    
    [HttpGet("{questionnaireId:guid}/contributors")]
    public async Task<IActionResult> GetContributors(Guid questionnaireId)
    {
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.GetContributors(userId, questionnaireId)).ToActionResult();
    }

    [HttpPut("{id:guid}/contributors")]
    public async Task<IActionResult> AddContributor(Guid id, AddContributorRequestDto request)
    {
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.AddContributor(userId, id, request)).ToActionResult();
    }

    [HttpDelete("{id:guid}/contributors/{contributorEmailAddress}")]
    public async Task<IActionResult> RemoveContributor(Guid id, string contributorEmailAddress)
    {
        var currentUserId = User.GetIdClaim()!;
        
        return (await questionnaireService.RemoveContributor(currentUserId, id, contributorEmailAddress)).ToActionResult();
    }
}