using Api.Extensions;
using Api.Services;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Extensions;
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
        var userId = User.GetIdClaim()!;

        return (await answerService.CreateAnswer(userId, request)).ToActionResult();
    }
    
    [HttpGet("answers/{id:guid}")]
    public IActionResult GetAnswer(Guid id)
    {
        // Extract user and tenant from claims
        var userId = User.GetIdClaim()!;
        
        return answerService.GetAnswer(userId, id).ToActionResult();
    }
    
    [HttpGet("questions/{questionId:guid}/answers")]
    public IActionResult GetAnswers(Guid questionId)
    {
        var userId = User.GetIdClaim()!;
        
        return answerService.GetAnswers(userId, questionId).ToActionResult();
    }

    [HttpPut("answers/{id:guid}")]
    public async Task<IActionResult> UpdateAnswer(Guid id, UpdateAnswerRequestDto request)
    {
        var userId = User.GetIdClaim()!;
        
        return (await answerService.UpdateAnswer(userId, id, request)).ToActionResult();
    }
    
    [HttpPost("answers/bulk")]
    public async Task<IActionResult> BulkUpsertAnswersAsync(BulkUpsertAnswersRequestDto bulkRequest)
    {
        var userId = User.GetIdClaim()!;
        
        return (await answerService.BulkUpsertAnswersAsync(userId, bulkRequest)).ToActionResult();
    }

    [HttpDelete("answers/{id:guid}")]
    public async Task<IActionResult> DeleteAnswer(Guid id)
    {
        var userId = User.GetIdClaim()!;
        
        return (await answerService.DeleteAnswer(userId, id)).ToActionResult();
    }
}