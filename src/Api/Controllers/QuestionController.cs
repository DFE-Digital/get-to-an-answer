using System.Security.Claims;
using Api.Extensions;
using Api.Services;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class QuestionController(IQuestionService questionService) : Controller
{
    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion(CreateQuestionRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await questionService.CreateQuestion(email, request)).ToActionResult();
    }
    
    [HttpGet("questions/{id}")]
    public IActionResult GetQuestion(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return questionService.GetQuestion(email, id).ToActionResult();
    }

    [HttpGet("questionnaires/{questionnaireId}/questions")]
    public IActionResult GetQuestions(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return questionService.GetQuestions(email, questionnaireId).ToActionResult();
    }

    [HttpPut("questions/{id}")]
    public async Task<IActionResult> UpdateQuestion(Guid id, UpdateQuestionRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await questionService.UpdateQuestion(email, id, request)).ToActionResult();
    }

    [HttpDelete("questions/{id}")]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await questionService.DeleteQuestion(email, id)).ToActionResult();
    }
    
    [HttpPatch("questionnaires/{questionnaireId}/questions/{id}/move-down")]
    public async Task<IActionResult> MoveQuestionDownOne(Guid questionnaireId, Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await questionService.MoveQuestionDownOne(email, questionnaireId, id)).ToActionResult();
    }
    
    [HttpPatch("questionnaires/{questionnaireId}/questions/{id}/move-up")]
    public async Task<IActionResult> MoveQuestionUpOne(Guid questionnaireId, Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await questionService.MoveQuestionUpOne(email, questionnaireId, id)).ToActionResult();
    }
}