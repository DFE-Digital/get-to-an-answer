using System.Security.Claims;
using Checker.Api.Infrastructure.Persistence;
using Checker.Common.Domain.Request.Create;
using Checker.Common.Domain.Request.Update;
using Checker.Common.Infrastructure.Persistence;
using Checker.Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Checker.Api.Controllers;

[ApiController]
[Route("/")]
[Authorize]
public class BranchingController(CheckerDbContext db) : Controller
{
    [HttpPost("branchings")]
    public IActionResult CreateBranching(CreateBranchingRequestDto request)
    {
        var userId = User.FindFirstValue("oid")!;   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        db.Branching.Add(new BranchingEntity
        {
            OwnerId = userId,
            TenantId = tenantId,
            QuestionnaireId = request.QuestionnaireId,
            Title = request.Title,
            Description = request.Description,
            
        });
        
        db.SaveChanges();
        
        return Ok("Branching created.");
    }
    
    [HttpGet("branchings/{id}")]
    public IActionResult GetBranching(int id)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        // Example: check ownership in your persistence layer
        var branching = db.Branching
            .FirstOrDefault(q => q.Id == id && q.TenantId == tenantId);

        if (branching == null)
            return NotFound();

        if (branching.OwnerId != userId)
            return Forbid();
        
        // Logic to create a branching
        return Ok(branching);
    }
    
    [HttpGet("questionnaires/{questionnaireId}/branchings")]
    public IActionResult GetBranchings(int questionnaireId)
    {
        var tenantId = User.FindFirstValue("tid"); // Tenant ID

        var branchings = db.Branching
            .Where(q => q.QuestionnaireId == questionnaireId && q.TenantId == tenantId);
        
        return Ok(branchings);
    }

    [HttpPut("branchings/{id}")]
    public IActionResult UpdateBranching(int id, UpdateBranchingRequestDto request)
    {
        // Extract user and tenant from claims
        var userId = User.FindFirstValue("oid");   // Azure AD Object ID
        var tenantId = User.FindFirstValue("tid"); // Tenant ID
        
        if (tenantId == null)
            return Unauthorized();
        
        var branching = new BranchingEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Branching.Attach(branching);
        
        branching.Title = request.Title;
        branching.Description = request.Description;
        
        db.Entry(branching).Property(s => s.Title).IsModified = true;
        db.Entry(branching).Property(s => s.Description).IsModified = true;
        
        // Logic to update a branching, answer, or branching logic
        return Ok("Branching updated.");
    }

    [HttpDelete("branchings/{id}")]
    public IActionResult DeleteBranching(int id)
    {
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        var branching = new BranchingEntity
        {
            Id = id,
            TenantId = tenantId,
        };

        db.Branching.Attach(branching);
        db.Branching.Remove(branching);
        
        db.SaveChanges();
        
        // Logic to delete a branching
        return Ok("Branching deleted.");
    }
}
