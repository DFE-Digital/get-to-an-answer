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
public class AnswerController(CheckerDbContext db) : Controller
{
    [HttpPost("answers")]
    public async Task<IActionResult> CreateAnswer(CreateAnswerRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, request.QuestionId))
            return Unauthorized();

        var entity = new AnswerEntity
        {
            QuestionnaireId = request.QuestionnaireId,
            QuestionId = request.QuestionId,
            Content = request.Content,
            Description = request.Description,
            Score = request.Score,
            Destination = request.Destination,
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
            Destination = entity.Destination,
            DestinationType = entity.DestinationType,
            DestinationQuestionId = request.DestinationQuestionId,
            DestinationContentId = request.DestinationContentId,
            QuestionId = entity.QuestionId,
            QuestionnaireId = entity.QuestionnaireId,
            Score = entity.Score,
        });
    }
    
    [HttpGet("answers/{id}")]
    public async Task<IActionResult> GetAnswer(int id)
    {
        // Extract user and tenant from claims
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<AnswerEntity>(email, id))
            return Unauthorized();

        // Example: check ownership in your persistence layer
        var answer = db.Answers
            .FirstOrDefault(q => q.Id == id);

        if (answer == null)
            return NotFound();
        
        return Ok(answer);
    }
    
    [HttpGet("questions/{questionId}/answers")]
    public async Task<IActionResult> GetAnswers(int questionId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, questionId))
            return Unauthorized();

        var answers = db.Answers
            .Where(q => q.QuestionId == questionId);
        
        return Ok(answers.ToList());
    }

    [HttpPut("answers/{id}")]
    public async Task<IActionResult> UpdateAnswer(int id, UpdateAnswerRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<AnswerEntity>(email, id))
            return Unauthorized();
        
        var answer = db.Answers.FirstOrDefault(q => q.Id == id);
        
        if (answer == null)
            return NotFound();
        
        answer.Content = request.Content;
        answer.Description = request.Description;
        answer.Destination = request.Destination;
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
    public async Task<IActionResult> DeleteAnswer(int id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<AnswerEntity>(email, id))
            return Unauthorized();
        
        var answer = new AnswerEntity
        {
            Id = id,
        };

        db.Answers.Attach(answer);
        db.Answers.Remove(answer);
        
        await db.SaveChangesAsync();
        
        // Logic to delete a answer
        return Ok("Answer deleted.");
    }
}
