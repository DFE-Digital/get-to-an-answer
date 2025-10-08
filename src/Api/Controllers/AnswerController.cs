using System.Security.Claims;
using Api.Infrastructure.Persistence;
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
    public IActionResult CreateAnswer(CreateAnswerRequestDto request)
    {
        var userId = User.FindFirstValue("oid")!;   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        db.Answers.Add(new AnswerEntity
        {
            OwnerId = userId,
            TenantId = tenantId,
            QuestionId = request.QuestionId,
            Content = request.Content,
            Description = request.Description,
        });
        
        db.SaveChanges();
        
        return Ok("Answer created.");
    }
    
    [HttpGet("answers/{id}")]
    public IActionResult GetAnswer(int id)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        // Example: check ownership in your persistence layer
        var answer = db.Answers
            .FirstOrDefault(q => q.Id == id && q.TenantId == tenantId);

        if (answer == null)
            return NotFound();

        if (answer.OwnerId != userId)
            return Forbid();
        
        // Logic to create a answer
        return Ok(answer);
    }
    
    [HttpGet("questions/{questionId}/answers")]
    public IActionResult GetAnswers(int questionId)
    {
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        var answers = db.Answers
            .Where(q => q.QuestionId == questionId && q.TenantId == tenantId);
        
        return Ok(answers);
    }

    [HttpPut("answers/{id}")]
    public IActionResult UpdateAnswer(int id, UpdateAnswerRequestDto request)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID
        
        if (tenantId == null)
            return Unauthorized();
        
        var answer = new AnswerEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Answers.Attach(answer);
        
        answer.Content = request.Content;
        answer.Description = request.Description;
        
        db.Entry(answer).Property(s => s.Content).IsModified = true;
        db.Entry(answer).Property(s => s.Description).IsModified = true;
        
        // Logic to update a answer, answer, or branching logic
        return Ok("Answer updated.");
    }

    [HttpDelete("answers/{id}")]
    public IActionResult DeleteAnswer(int id)
    {
        // Extract user and tenant from claims
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        var answer = new AnswerEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Answers.Attach(answer);
        db.Answers.Remove(answer);
        
        db.SaveChanges();
        
        // Logic to delete a answer
        return Ok("Answer deleted.");
    }
}
