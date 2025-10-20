using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// The QuestionnaireVersionController class provides API endpoints for managing and retrieving versions of questionnaires.
/// It includes functionality to fetch the latest questionnaire version, fetch a specific version by version number,
/// and retrieve a list of all versions for a particular questionnaire.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public class QuestionnaireVersionController(GetToAnAnswerDbContext db) : ControllerBase
{
    
    [HttpGet("questionnaires/{questionnaireId}/versions/current")]
    public async Task<IActionResult> GetLatestQuestionnaireVersion(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var questionnaire = await db.Questionnaires
            .AsNoTracking()    
            .Where(q => q.Id == questionnaireId)
            .Include(q => q.Questions)
            .ThenInclude(qq => qq.Answers)
            .FirstOrDefaultAsync();

        if (questionnaire == null)
            return NotFound();
        
        var json = JsonSerializer.Serialize(questionnaire, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        });
        
        return Ok(new QuestionnaireVersionDto
        {
            Id = questionnaire.Id,
            QuestionnaireId = questionnaire.Id,
            Version = questionnaire.Version,
            CreatedAt = questionnaire.UpdatedAt,
            QuestionnaireJson = json,
        });
    }
    
    [HttpGet("questionnaires/{questionnaireId}/versions/{versionNumber}")]
    public async Task<IActionResult> GetQuestionnaireVersion(Guid questionnaireId, int versionNumber)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var questionnaireVersion = await db.QuestionnaireVersions
            .FirstOrDefaultAsync(q => q.QuestionnaireId == questionnaireId
                                 && q.Version == versionNumber);

        if (questionnaireVersion == null)
            return NotFound();
        
        return Ok(new QuestionnaireVersionDto
        {
            Id = questionnaireVersion.Id,
            QuestionnaireId = questionnaireVersion.QuestionnaireId,
            Version = questionnaireVersion.Version,
            CreatedAt = questionnaireVersion.CreatedAt,
            QuestionnaireJson = questionnaireVersion.QuestionnaireJson,
        });
    }
    
    [HttpGet("questionnaires/{questionnaireId}/versions")]
    public async Task<IActionResult> GetQuestionnaireVersions(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var questionnaireVersions = await db.QuestionnaireVersions
            .Where(q => q.QuestionnaireId == questionnaireId)
            .Select(q => new QuestionnaireVersionDto
            {
                Id = q.Id,
                QuestionnaireId = q.QuestionnaireId,
                Version = q.Version,
                CreatedAt = q.CreatedAt,
            })
            .OrderByDescending(q => q.Version)
            .ToListAsync();
        
        return Ok(questionnaireVersions);
    }
}