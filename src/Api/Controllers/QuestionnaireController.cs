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
    public async Task<IActionResult> GetQuestionnaires()
    {
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.GetQuestionnaires(userId)).ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateQuestionnaire(Guid id, UpdateQuestionnaireRequestDto request)
    {
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.UpdateQuestionnaire(userId, id, request)).ToActionResult();
    }

    [HttpPatch("{id:guid}/styling")]
    public async Task<IActionResult> UpdateQuestionnaireCustomStyling(Guid id, UpdateCustomStylingRequestDto request)
    {
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.UpdateQuestionnaireCustomStyling(userId, id, request)).ToActionResult();
    }
    
    [HttpPatch("{id:guid}/continue-button")]
    public async Task<IActionResult> UpdateQuestionnaireContinueButton(Guid id, UpdateContinueButtonRequestDto request)
    {
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.UpdateQuestionnaireContinueButton(userId, id, request)).ToActionResult();
    }

    [HttpPatch("{id:guid}/decorative-image")]
    public async Task<IActionResult> DeleteQuestionnaireDecorativeImage(Guid id)
    {
        var userId = User.GetIdClaim()!;
        
        return (await questionnaireService.DeleteQuestionnaireDecorativeImage(userId, id)).ToActionResult();
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

    [HttpDelete("{id:guid}/contributors/{contributorId}")]
    public async Task<IActionResult> RemoveContributor(Guid id, string contributorId)
    {
        var currentUserId = User.GetIdClaim()!;
        
        return (await questionnaireService.RemoveContributor(currentUserId, id, contributorId)).ToActionResult();
    }

    [HttpPatch("{id:guid}/completion-state")]
    public async Task<IActionResult> UpdateCompletionState(Guid id, UpdateCompletionStateRequestDto request)
    {
        var currentUserId = User.GetIdClaim()!;
        
        return (await questionnaireService.UpdateCompletionState(currentUserId, id, request)).ToActionResult();
    }
}