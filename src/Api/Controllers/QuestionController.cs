using System.Security.Claims;
using Api.Infrastructure.Persistence;
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
[Route("/")]
[Authorize]
public class QuestionController(CheckerDbContext db) : Controller
{
    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion(CreateQuestionRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, request.QuestionnaireId))
            return Unauthorized();

        var entity = new QuestionEntity
        {
            QuestionnaireId = request.QuestionnaireId,
            Content = request.Content,
            Description = request.Description,
            Type = request.Type,
            Order = db.Questions.Count(x => x.QuestionnaireId == request.QuestionnaireId
                                            && x.Status != EntityStatus.Deleted) + 1,
            CreatedAt = DateTime.UtcNow
        };
        
        db.Questions.Add(entity);
        
        await db.SaveChangesAsync();
        
        return Ok(new QuestionDto
        {
            Id = entity.Id,
            QuestionnaireId = entity.QuestionnaireId,
            Content = entity.Content,
            Description = entity.Description,
            Type = entity.Type,
            Order = entity.Order,
            Status = entity.Status
        });
    }
    
    [HttpGet("questions/{id}")]
    public async Task<IActionResult> GetQuestion(int id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, id))
            return Unauthorized();

        // Example: check ownership in your persistence layer
        var question = db.Questions
            .Include(q => q.Answers)
            .FirstOrDefault(q => q.Id == id 
                                 && q.Status != EntityStatus.Deleted);

        if (question == null)
            return NotFound();
        
        // Logic to create a question
        return Ok(new QuestionDto
        {
            Id = question.Id,
            QuestionnaireId = question.QuestionnaireId,
            Content = question.Content,
            Description = question.Description,
            Order = question.Order,
            Status = question.Status,
            Answers = question.Answers.Select(a => new AnswerDto
            {
                Id = a.Id,
                Content = a.Content,
                QuestionId = a.QuestionId,
                Score = a.Score,
                DestinationType = a.DestinationType,
            }).ToList(),
            Type = question.Type
        });
    }

    

    [HttpGet("questionnaires/{questionnaireId}/questions")]
    public async Task<IActionResult> GetQuestions(int questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId))
            return Unauthorized();
        
        var questions = db.Questions
            .Where(q => q.QuestionnaireId == questionnaireId
                        && q.Status != EntityStatus.Deleted);
        
        return Ok(questions);
    }

    [HttpPut("questions/{id}")]
    public async Task<IActionResult> UpdateQuestion(int id, UpdateQuestionRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, id))
            return Unauthorized();
        
        var question = new QuestionEntity
        {
            Id = id
        };

        db.Questions.Attach(question);
        
        question.Content = request.Content;
        question.Description = request.Description;
        question.Type = request.Type;
        question.UpdatedAt = DateTime.UtcNow;
        
        db.Entry(question).Property(s => s.Content).IsModified = true;
        db.Entry(question).Property(s => s.Description).IsModified = true;
        db.Entry(question).Property(s => s.Type).IsModified = true;
        db.Entry(question).Property(s => s.UpdatedAt).IsModified = true;
        
        await db.SaveChangesAsync();
        
        // Logic to update a question, answer, or branching logic
        return Ok("Question updated.");
    }
    
    [HttpPut("questions/{id}/status")]
    public async Task<IActionResult> UpdateQuestionStatus(int id, UpdateQuestionStatusRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, id))
            return Unauthorized();
        
        var question = new QuestionEntity
        {
            Id = id
        };

        db.Questions.Attach(question);
        
        question.Status = request.Status;
        question.UpdatedAt = DateTime.UtcNow;
        
        db.Entry(question).Property(s => s.Status).IsModified = true;
        
        await db.SaveChangesAsync();
        
        return Ok("Question updated.");
    }

    [HttpDelete("questions/{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        return await UpdateQuestionStatus(id, new UpdateQuestionStatusRequestDto
        {
            Id = id,
            Status = EntityStatus.Deleted
        });
    }
    
    [HttpDelete("questionnaires/{questionnaireId}/questions/{id}/move-down")]
    public async Task<IActionResult> MoveQuestionDownOne(int questionnaireId, int id)
    {
        return await MoveQuestionByOne(questionnaireId, id, +1);
    }
    
    [HttpDelete("questionnaires/{questionnaireId}/questions/{id}/move-up")]
    public async Task<IActionResult> MoveQuestionUpOne(int questionnaireId, int id)
    {
        return await MoveQuestionByOne(questionnaireId, id, -1);
    }
    
    private async Task<IActionResult> MoveQuestionByOne(int questionnaireId, int id, int direction)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId))
            return Unauthorized();

        // Load the target item
        var current = await db.Questions
            .FirstOrDefaultAsync(x => x.Id == id
                && x.QuestionnaireId == questionnaireId);

        if (current == null) return NotFound();

        // Find the next item in order within the same scope
        var next = await db.Questions
            .FirstOrDefaultAsync(x => x.QuestionnaireId == questionnaireId 
                                      && x.Order == current.Order + direction
                                      && x.Status != EntityStatus.Deleted);

        // If already last, nothing to do
        if (next == null) return NoContent();

        // Swap orders
        (current.Order, next.Order) = (next.Order, current.Order);
        
        current.UpdatedAt = DateTime.UtcNow;
        next.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        
        // Logic to delete a question
        return Ok();
    }
}
