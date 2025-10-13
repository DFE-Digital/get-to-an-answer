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
public class ContentController(GetToAnAnswerDbContext db) : Controller
{
    [HttpPost("contents")]
    public async Task<IActionResult> CreateContent(CreateContentRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, request.QuestionnaireId))
            return Unauthorized();

        var entity = new ContentEntity
        {
            QuestionnaireId = request.QuestionnaireId,
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        db.Contents.Add(entity);
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(request.QuestionnaireId);
        
        return Ok(new ContentDto
        {
            Id = entity.Id,
            Title = entity.Content,
            Content = entity.Content,
            QuestionnaireId = entity.QuestionnaireId,
        });
    }
    
    [HttpGet("contents/{id}")]
    public async Task<IActionResult> GetContent(int id)
    {
        // Extract user and tenant from claims
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<ContentEntity>(email, id))
            return Unauthorized();

        // Example: check ownership in your persistence layer
        var content = db.Contents
            .FirstOrDefault(q => q.Id == id);

        if (content == null)
            return NotFound();
        
        return Ok(content);
    }
    
    [HttpGet("questionnaires/{questionnaireId}/contents")]
    public async Task<IActionResult> GetContents(int questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId))
            return Unauthorized();

        var contents = db.Contents
            .Where(q => q.QuestionnaireId == questionnaireId);
        
        return Ok(contents.ToList());
    }

    [HttpPut("contents/{id}")]
    public async Task<IActionResult> UpdateContent(int id, UpdateContentRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<ContentEntity>(email, id))
            return Unauthorized();
        
        var content = db.Contents.FirstOrDefault(q => q.Id == id);
        
        if (content == null)
            return NotFound();
        
        content.Title = request.Title;
        content.Content = request.Content;
        content.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(content.QuestionnaireId);
        
        return Ok("Content updated.");
    }

    [HttpDelete("contents/{id}")]
    public async Task<IActionResult> DeleteContent(int id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<ContentEntity>(email, id))
            return Unauthorized();
        
        var content = new ContentEntity
        {
            Id = id,
        };

        db.Contents.Attach(content);
        db.Contents.Remove(content);
        
        await db.SaveChangesAsync();
        
        // Logic to delete a Content
        return Ok("Content deleted.");
    }
}
