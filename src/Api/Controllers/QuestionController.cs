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
    public IActionResult CreateQuestion(CreateQuestionRequestDto request)
    {
        var userId = User.FindFirstValue("oid")!;   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID

        var entity = new QuestionEntity
        {
            OwnerId = userId,
            TenantId = tenantId,
            QuestionnaireId = request.QuestionnaireId,
            Content = request.Content,
            Description = request.Description,
            Type = request.Type,
            Order = db.Questions.Count(x => x.QuestionnaireId == request.QuestionnaireId
                                            && x.Status != EntityStatus.Deleted
                                            && x.TenantId == tenantId) + 1,
        };
        
        db.Questions.Add(entity);
        
        db.SaveChanges();
        
        return Ok(new QuestionDto
        {
            Id = entity.Id,
            Content = entity.Content,
            Description = entity.Description,
            Type = entity.Type,
            Order = entity.Order,
            Status = entity.Status
        });
    }
    
    [HttpGet("questions/{id}")]
    public IActionResult GetQuestion(int id)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        // Example: check ownership in your persistence layer
        var question = db.Questions
            .FirstOrDefault(q => q.Id == id 
                                 && q.TenantId == tenantId
                                 && q.Status != EntityStatus.Deleted);

        if (question == null)
            return NotFound();

        if (question.OwnerId != userId)
            return Forbid();
        
        // Logic to create a question
        return Ok(question);
    }
    
    [HttpGet("questionnaires/{questionnaireId}/questions")]
    public IActionResult GetQuestions(int questionnaireId)
    {
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        var questions = db.Questions
            .Where(q => q.QuestionnaireId == questionnaireId 
                        && q.TenantId == tenantId
                        && q.Status != EntityStatus.Deleted);
        
        return Ok(questions);
    }

    [HttpPut("questions/{id}")]
    public IActionResult UpdateQuestion(int id, UpdateQuestionRequestDto request)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID
        
        if (tenantId == null)
            return Unauthorized();
        
        var question = new QuestionEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Questions.Attach(question);
        
        question.Content = request.Content;
        question.Description = request.Description;
        
        db.Entry(question).Property(s => s.Content).IsModified = true;
        db.Entry(question).Property(s => s.Description).IsModified = true;
        
        // Logic to update a question, answer, or branching logic
        return Ok("Question updated.");
    }
    
    [HttpPut("questions/{id}/status")]
    public IActionResult UpdateQuestionStatus(int id, UpdateQuestionStatusRequestDto request)
    {
        // Extract user and tenant from claims
        var tenantId = User.FindFirstValue("tid"); // Tenant ID
        
        if (tenantId == null)
            return Unauthorized();
        
        var question = new QuestionEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Questions.Attach(question);
        
        question.Status = request.Status;
        
        db.Entry(question).Property(s => s.Status).IsModified = true;
        
        // Logic to update a question, answer, or branching logic
        return Ok("Question updated.");
    }

    [HttpDelete("questions/{id}")]
    public IActionResult DeleteQuestion(int id)
    {
        return UpdateQuestionStatus(id, new UpdateQuestionStatusRequestDto
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
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID

        // Load the target item
        var current = await db.Questions
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (current == null) return NotFound();

        // Find the next item in order within the same scope
        var next = await db.Questions
            .FirstOrDefaultAsync(x => x.QuestionnaireId == questionnaireId 
                                      && x.Order == current.Order + direction 
                                      && x.TenantId == tenantId
                                      && x.Status != EntityStatus.Deleted);

        // If already last, nothing to do
        if (next == null) return NoContent();

        // Swap orders
        (current.Order, next.Order) = (next.Order, current.Order);

        await db.SaveChangesAsync();
        
        // Logic to delete a question
        return Ok();
    }
}
