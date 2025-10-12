using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
public class QuestionnaireVersionController(CheckerDbContext db) : ControllerBase
{
    
    [HttpGet("questionnaires/{questionnaireId}/versions/current")]
    public async Task<IActionResult> GetLatestQuestionnaireVersion(int questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId))
            return Unauthorized();

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
    public async Task<IActionResult> GetQuestionnaireVersion(int questionnaireId, int versionNumber)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId))
            return Unauthorized();

        var questionnaireVersion = db.QuestionnaireVersions
            .FirstOrDefault(q => q.QuestionnaireId == questionnaireId
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
    public async Task<IActionResult> GetQuestionnaireVersions(int questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId))
            return Unauthorized();

        var questionnaireVersions = db.QuestionnaireVersions
            .Where(q => q.QuestionnaireId == questionnaireId)
            .Select(q => new QuestionnaireVersionDto
            {
                Id = q.Id,
                QuestionnaireId = q.QuestionnaireId,
                Version = q.Version,
                CreatedAt = q.CreatedAt,
            })
            .OrderByDescending(q => q.Version)
            .ToList();
        
        return Ok(questionnaireVersions);
    }
}
