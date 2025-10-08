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
public class QuestionController(CheckerDbContext db) : Controller
{
    [HttpPost("questions")]
    public IActionResult CreateQuestion(CreateQuestionRequestDto request)
    {
        var userId = User.FindFirstValue("oid")!;   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        db.Questions.Add(new QuestionEntity
        {
            OwnerId = userId,
            TenantId = tenantId,
            QuestionnaireId = request.QuestionnaireId,
            Content = request.Content,
            Description = request.Description,
            Type = request.Type,
        });
        
        db.SaveChanges();
        
        return Ok("Question created.");
    }
    
    [HttpGet("questions/{id}")]
    public IActionResult GetQuestion(int id)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        // Example: check ownership in your persistence layer
        var question = db.Questions
            .FirstOrDefault(q => q.Id == id && q.TenantId == tenantId);

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
            .Where(q => q.QuestionnaireId == questionnaireId && q.TenantId == tenantId);
        
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

    [HttpDelete("questions/{id}")]
    public IActionResult DeleteQuestion(int id)
    {
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        var question = new QuestionEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Questions.Attach(question);
        db.Questions.Remove(question);
        
        db.SaveChanges();
        
        // Logic to delete a question
        return Ok("Question deleted.");
    }
}
