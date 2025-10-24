using System.Security.Claims;
using Api.Infrastructure.Persistence;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("/")]
[Authorize]
public class AnswerController(GetToAnAnswerDbContext db) : Controller
{
    [HttpPost("answers")]
    public async Task<IActionResult> CreateAnswer(CreateAnswerRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, request.QuestionId))
            return Forbid();

        var entity = new AnswerEntity
        {
            QuestionnaireId = request.QuestionnaireId,
            QuestionId = request.QuestionId,
            Content = request.Content,
            Description = request.Description,
            Score = request.Score,
            DestinationUrl = request.DestinationUrl,
            DestinationQuestionId = request.DestinationQuestionId,
            DestinationContentId = request.DestinationContentId,
            DestinationType = request.DestinationType,
            CreatedAt = DateTime.UtcNow
        };
        
        db.Answers.Add(entity);
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(request.QuestionnaireId);
        
        return Ok(new AnswerDto
        {
            Id = entity.Id,
            Content = entity.Content,
            Description = entity.Description,
            DestinationUrl = entity.DestinationUrl,
            DestinationType = entity.DestinationType,
            DestinationQuestionId = request.DestinationQuestionId,
            DestinationContentId = request.DestinationContentId,
            QuestionId = entity.QuestionId,
            QuestionnaireId = entity.QuestionnaireId,
            Score = entity.Score,
        });
    }
    
    [HttpGet("answers/{id}")]
    public async Task<IActionResult> GetAnswer(Guid id)
    {
        // Extract user and tenant from claims
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<AnswerEntity>(email, id))
            return Forbid();

        // Example: check ownership in your persistence layer
        var answer = db.Answers
            .FirstOrDefault(q => q.Id == id && 
                                 !q.IsDeleted);

        if (answer == null)
            return NotFound();
        
        return Ok(answer);
    }
    
    [HttpGet("questions/{questionId}/answers")]
    public async Task<IActionResult> GetAnswers(Guid questionId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, questionId))
            return Forbid();

        var answers = db.Answers
            .Where(q => q.QuestionId == questionId && 
                        !q.IsDeleted);
        
        return Ok(answers.ToList());
    }

    [HttpPut("answers/{id}")]
    public async Task<IActionResult> UpdateAnswer(Guid id, UpdateAnswerRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<AnswerEntity>(email, id))
            return Forbid();
        
        var answer = db.Answers.FirstOrDefault(q => q.Id == id && 
                                                    !q.IsDeleted);
        
        if (answer == null)
            return NotFound();
        
        answer.Content = request.Content;
        answer.Description = request.Description ?? answer.Description;
        answer.DestinationUrl = request.DestinationUrl;
        answer.DestinationType = request.DestinationType;
        answer.DestinationQuestionId = request.DestinationQuestionId;
        answer.DestinationContentId = request.DestinationContentId;
        answer.Score = request.Score;
        answer.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(answer.QuestionnaireId);
        
        return Ok("Answer updated.");
    }

    [HttpDelete("answers/{id}")]
    public async Task<IActionResult> DeleteAnswer(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var access = await db.HasAccessToEntity<AnswerEntity>(email, id);
        
        if (!access)
            return Forbid();
        
        var answer = db.Answers.FirstOrDefault(q => q.Id == id);
        
        if (answer == null)
            return NotFound();
        
        answer.IsDeleted = true;
        answer.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        return NoContent();
    }
}
