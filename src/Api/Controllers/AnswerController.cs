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

namespace Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AnswerController(IAnswerService answerService) : Controller
{
    [HttpPost("answers")]
    public async Task<IActionResult> CreateAnswer(CreateAnswerRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await answerService.CreateAnswer(email, request)).ToActionResult();
    }
    
    [HttpGet("answers/{id}")]
    public IActionResult GetAnswer(Guid id)
    {
        // Extract user and tenant from claims
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return answerService.GetAnswer(email, id).ToActionResult();
    }
    
    [HttpGet("questions/{questionId}/answers")]
    public IActionResult GetAnswers(Guid questionId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return answerService.GetAnswers(email, questionId).ToActionResult();
    }

    [HttpPut("answers/{id}")]
    public async Task<IActionResult> UpdateAnswer(Guid id, UpdateAnswerRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await answerService.UpdateAnswer(email, id, request)).ToActionResult();
    }

    [HttpDelete("answers/{id}")]
    public async Task<IActionResult> DeleteAnswer(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        return (await answerService.DeleteAnswer(email, id)).ToActionResult();
    }
}