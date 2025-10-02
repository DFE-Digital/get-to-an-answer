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
public class Conditionontroller(CheckerDbContext db) : Controller
{
    [HttpPost("conditions")]
    public IActionResult CreateCondition(CreateConditionRequestDto request)
    {
        var userId = User.FindFirstValue("oid")!;   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        db.Conditions.Add(new ConditionEntity
        {
            OwnerId = userId,
            TenantId = tenantId,
            BranchingId = request.BranchingId,
            QuestionId = request.QuestionId,
            AnswerIds = request.AnswerIds,
        });
        
        db.SaveChanges();
        
        return Ok("Condition created.");
    }
    
    [HttpGet("conditions/{id}")]
    public IActionResult GetCondition(int id)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        // Example: check ownership in your persistence layer
        var condition = db.Conditions
            .FirstOrDefault(q => q.Id == id && q.TenantId == tenantId);

        if (condition == null)
            return NotFound();

        if (condition.OwnerId != userId)
            return Forbid();
        
        // Logic to create a condition
        return Ok(condition);
    }
    
    [HttpGet("branchings/{branchingId}/conditions")]
    public IActionResult GetConditions(int branchingId)
    {
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        var conditions = db.Conditions
            .Where(q => q.BranchingId == branchingId && q.TenantId == tenantId);
        
        return Ok(conditions);
    }

    [HttpPut("conditions/{id}")]
    public IActionResult UpdateCondition(int id, UpdateConditionRequestDto request)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID
        
        if (tenantId == null)
            return Unauthorized();
        
        var condition = new ConditionEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Conditions.Attach(condition);
        
        condition.QuestionId = request.QuestionId;
        condition.AnswerIds = request.AnswerIds;
        
        db.Entry(condition).Property(s => s.QuestionId).IsModified = true;
        db.Entry(condition).Property(s => s.AnswerIds).IsModified = true;
        
        // Logic to update a condition, answer, or condition logic
        return Ok("Condition updated.");
    }

    [HttpDelete("conditions/{id}")]
    public IActionResult DeleteCondition(int id)
    {
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        var condition = new ConditionEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Conditions.Attach(condition);
        db.Conditions.Remove(condition);
        
        db.SaveChanges();
        
        return Ok("Condition deleted.");
    }
}
