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
public class QuestionController(GetToAnAnswerDbContext db) : Controller
{
    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion(CreateQuestionRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, request.QuestionnaireId))
            return Unauthorized();

        var entity = new QuestionEntity
        {
            QuestionnaireId = request.QuestionnaireId,
            Content = request.Content,
            Description = request.Description,
            Type = request.Type,
            Order = db.Questions.Count(x => x.QuestionnaireId == request.QuestionnaireId
                                            && !x.IsDeleted) + 1,
            CreatedAt = DateTime.UtcNow
        };
        
        db.Questions.Add(entity);
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(request.QuestionnaireId);
        
        return Ok(new QuestionDto
        {
            Id = entity.Id,
            QuestionnaireId = entity.QuestionnaireId,
            Content = entity.Content,
            Description = entity.Description,
            Type = entity.Type,
            Order = entity.Order
        });
    }
    
    [HttpGet("questions/{id}")]
    public async Task<IActionResult> GetQuestion(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, id))
            return Unauthorized();

        // Example: check ownership in your persistence layer
        var question = db.Questions
            .Include(q => q.Answers)
            .FirstOrDefault(q => q.Id == id 
                                 && !q.IsDeleted);

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
    public async Task<IActionResult> GetQuestions(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId))
            return Unauthorized();
        
        var questions = db.Questions
            .Where(q => q.QuestionnaireId == questionnaireId
                        && !q.IsDeleted);
        
        return Ok(questions);
    }

    [HttpPut("questions/{id}")]
    public async Task<IActionResult> UpdateQuestion(Guid id, UpdateQuestionRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, id))
            return Unauthorized();
        
        var question = db.Questions.FirstOrDefault(q => q.Id == id);
        
        if (question == null)
            return NotFound();
        
        question.Content = request.Content;
        question.Description = request.Description ?? question.Description;
        question.Type = request.Type;
        question.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(question.QuestionnaireId);
        
        return Ok("Question updated.");
    }

    [HttpDelete("questions/{id}")]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionEntity>(email, id))
            return Unauthorized();
        
        var question = db.Questions.FirstOrDefault(q => q.Id == id);
        
        if (question == null)
            return NotFound();
        
        question.IsDeleted = true;
        question.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(question.QuestionnaireId);
        
        return Ok("Question updated.");
    }
    
    [HttpPut("questionnaires/{questionnaireId}/questions/{id}/move-down")]
    public async Task<IActionResult> MoveQuestionDownOne(Guid questionnaireId, Guid id)
    {
        return await MoveQuestionByOne(questionnaireId, id, +1);
    }
    
    [HttpPut("questionnaires/{questionnaireId}/questions/{id}/move-up")]
    public async Task<IActionResult> MoveQuestionUpOne(Guid questionnaireId, Guid id)
    {
        return await MoveQuestionByOne(questionnaireId, id, -1);
    }
    
    private async Task<IActionResult> MoveQuestionByOne(Guid questionnaireId, Guid id, int direction)
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
                                      && !x.IsDeleted);

        // If already last, nothing to do
        if (next == null) return NoContent();

        // Swap orders
        (current.Order, next.Order) = (next.Order, current.Order);
        
        current.UpdatedAt = DateTime.UtcNow;
        next.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(questionnaireId);
        
        return Ok();
    }
}
