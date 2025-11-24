using Api.Extensions;
using Api.Services;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Extensions;
using Common.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class QuestionController(IQuestionService questionService) : Controller
{
    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion(CreateQuestionRequestDto request)
    {
        var userId = User.GetIdClaim()!;

        return (await questionService.CreateQuestion(userId, request)).ToActionResult();
    }
    
    [HttpGet("questions/{id:guid}")]
    public IActionResult GetQuestion(Guid id)
    {
        var userId = User.GetIdClaim()!;

        return questionService.GetQuestion(userId, id).ToActionResult();
    }

    [HttpGet("questionnaires/{questionnaireId:guid}/questions")]
    public IActionResult GetQuestions(Guid questionnaireId)
    {
        var userId = User.GetIdClaim()!;

        return questionService.GetQuestions(userId, questionnaireId).ToActionResult();
    }

    [HttpPut("questions/{id:guid}")]
    public async Task<IActionResult> UpdateQuestion(Guid id, UpdateQuestionRequestDto request)
    {
        var userId = User.GetIdClaim()!;

        return (await questionService.UpdateQuestion(userId, id, request)).ToActionResult();
    }

    [HttpDelete("questions/{id:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        var userId = User.GetIdClaim()!;

        return (await questionService.DeleteQuestion(userId, id)).ToActionResult();
    }
    
    [HttpPatch("questionnaires/{questionnaireId:guid}/questions/{id:guid}")]
    public async Task<IActionResult> MoveQuestionDownOne(Guid questionnaireId, Guid id, [FromQuery] [EnumDefined] QuestionAction action)
    {
        var userId = User.GetIdClaim()!;

        if (action == QuestionAction.MoveDown)
        {
            return (await questionService.MoveQuestionDownOne(userId, questionnaireId, id)).ToActionResult();
        }
        
        if (action == QuestionAction.MoveUp)
        {
            return (await questionService.MoveQuestionUpOne(userId, questionnaireId, id)).ToActionResult();
        }
        
        return BadRequest();
    }
}