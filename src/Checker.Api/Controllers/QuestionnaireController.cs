using System.Security.Claims;
using Checker.Api.Infrastructure.Persistence;
using Checker.Common.Domain.Request.Create;
using Checker.Common.Domain.Request.Update;
using Checker.Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Checker.Api.Controllers;

[ApiController]
[Route("/")]
[Authorize]
public class QuestionnaireController(CheckerDbContext db) : Controller
{
    [HttpPost("questionnaires")]
    public IActionResult CreateQuestionnaire(CreateQuestionnaireRequestDto request)
    {
        var userId = User.FindFirstValue("oid")!;   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        db.Questionnaires.Add(new QuestionnaireEntity
        {
            OwnerId = userId,
            TenantId = tenantId,
            Title = request.Title,
            Description = request.Description
        });
        
        db.SaveChanges();
        
        return Ok("Questionnaire created.");
    }
    
    [HttpGet("questionnaires/{id}")]
    public IActionResult GetQuestionnaire(int id)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        // Example: check ownership in your persistence layer
        var questionnaire = db.Questionnaires
            .FirstOrDefault(q => q.Id == id && q.TenantId == tenantId);

        if (questionnaire == null)
            return NotFound();

        if (questionnaire.OwnerId != userId)
            return Forbid();
        
        // Logic to create a questionnaire
        return Ok(questionnaire);
    }
    
    [HttpGet("questionnaires")]
    public IActionResult GetQuestionnaires()
    {
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        var questions = db.Questions
            .Where(q => q.TenantId == tenantId);
        
        return Ok(questions);
    }

    [HttpPut("questionnaires/{id}")]
    public IActionResult UpdateQuestionnaire(int id, UpdateQuestionnaireRequestDto request)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID
        
        if (tenantId == null)
            return Unauthorized();
        
        var questionnaire = new QuestionnaireEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Questionnaires.Attach(questionnaire);
        
        questionnaire.Title = request.Title;
        questionnaire.Description = request.Description;
        
        db.Entry(questionnaire).Property(s => s.Title).IsModified = true;
        db.Entry(questionnaire).Property(s => s.Description).IsModified = true;
        
        // Logic to update a questionnaire, answer, or branching logic
        return Ok("Questionnaire updated.");
    }

    [HttpDelete("questionnaires/{id}")]
    public IActionResult DeleteQuestionnaire(int id)
    {
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        var questionnaire = new QuestionnaireEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Questionnaires.Attach(questionnaire);
        db.Questionnaires.Remove(questionnaire);
        
        db.SaveChanges();
        
        // Logic to delete a questionnaire
        return Ok("Questionnaire deleted.");
    }
}
