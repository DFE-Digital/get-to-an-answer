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
public class QuestionnaireController(CheckerDbContext db) : ControllerBase
{
    [HttpPost("questionnaires")]
    [Authorize]
    public async Task<IActionResult> CreateQuestionnaire(CreateQuestionnaireRequestDto request)
    {
        var userId = User.FindFirstValue("oid")!;   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var entity = new QuestionnaireEntity
        {
            OwnerId = userId,
            TenantId = tenantId,
            Title = request.Title,
            Description = request.Description,
            Contributors = [email],
            CreatedAt = DateTime.UtcNow
        };
        
        db.Questionnaires.Add(entity);
        
        await db.SaveChangesAsync();
        
        var dto = new QuestionnaireDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description
        };
        
        return Ok(dto);
    }
    
    [HttpGet("questionnaires/{id}")]
    public IActionResult GetQuestionnaire(int id)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        // Example: check ownership in your persistence layer
        var questionnaire = db.Questionnaires
            .FirstOrDefault(q => q.Id == id && q.TenantId == tenantId
                                            && q.Status != EntityStatus.Deleted);

        if (questionnaire == null)
            return NotFound();

        if (questionnaire.OwnerId != userId)
            return Forbid();
        
        return Ok(questionnaire);
    }
    
    [HttpGet("questionnaires")]
    public IActionResult GetQuestionnaires()
    {
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        var questionnaires = db.Questionnaires
            .Where(q => q.TenantId == tenantId
                        && q.Status != EntityStatus.Deleted);
        
        return Ok(questionnaires);
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
        questionnaire.UpdatedAt = DateTime.UtcNow;
        
        db.Entry(questionnaire).Property(s => s.Title).IsModified = true;
        db.Entry(questionnaire).Property(s => s.Description).IsModified = true;
        db.Entry(questionnaire).Property(s => s.UpdatedAt).IsModified = true;
        
        db.SaveChanges();
        
        return Ok("Questionnaire updated.");
    }
    
    [HttpPut("questionnaires/{id}/status")]
    public IActionResult UpdateQuestionnaireStatus(int id, UpdateQuestionnaireStatusRequestDto request)
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
        question.UpdatedAt = DateTime.UtcNow;
        
        db.Entry(question).Property(s => s.Status).IsModified = true;
        
        db.SaveChanges();
        
        return Ok("Question updated.");
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
    
    [HttpPut("questionnaires/{id}/contributors")]
    public async Task<IActionResult> AddQuestionnaireContributor(int id)
    {
        // Extract user and tenant from claims
        var tenantId = User.FindFirstValue("tid");
        var email = User.FindFirstValue(ClaimTypes.Email)!;// Tenant ID
        
        if (tenantId == null)
            return Unauthorized();
        
        var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        
        if (questionnaire == null) 
            return NotFound();

        if (!questionnaire.Contributors.Contains(email))
        {
            questionnaire.Contributors.Add(email);
        
            await db.SaveChangesAsync();
        }
        
        return Ok("Question updated.");
    }
}
