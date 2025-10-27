using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Extensions;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Common.Local;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
public class QuestionnaireVersionController(GetToAnAnswerDbContext db) : ControllerBase
{
    
    [HttpGet("questionnaires/{questionnaireId}/versions/current")]
    public async Task<IActionResult> GetLatestQuestionnaireVersion(Guid questionnaireId)
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
    public async Task<IActionResult> GetQuestionnaireVersion(Guid questionnaireId, int versionNumber)
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
    public async Task<IActionResult> GetQuestionnaireVersions(Guid questionnaireId)
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
                ChangeDescription = q.ChangeDescription,
                CreatedBy = q.CreatedBy,
                ChangeLog = q.ChangeLog.ToChangeDataList()
            })
            .OrderByDescending(q => q.Version)
            .ToList();

        if (questionnaireVersions.Count > 0)
        {
            var previousVersion = db.QuestionnaireVersions
                .Where(q => q.QuestionnaireId == questionnaireId)
                .Select(q => new QuestionnaireVersionDto
                {
                    Version = q.Version,
                    QuestionnaireJson = q.QuestionnaireJson,
                })
                .OrderByDescending(q => q.Version)
                .First();
            
            var questionnaire = await db.Questionnaires
                .AsNoTracking()
                .Include(q => q.Questions)
                .ThenInclude(qq => qq.Answers)
                .FirstAsync(q => q.Id == questionnaireId);
        
            var json = JsonSerializer.Serialize(questionnaire, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            });

            try
            {
                var changeMap = VersionDiffRenderer.RenderCompare(previousVersion.QuestionnaireJson, json);

                questionnaireVersions.Insert(0, new QuestionnaireVersionDto
                {
                    Id = questionnaireId,
                    QuestionnaireId = questionnaireId,
                    Version = previousVersion.Version + 1,
                    CreatedAt = DateTime.UtcNow,
                    ChangeDescription = "Current draft changes",
                    CreatedBy = email,
                    ChangeLog = changeMap?.FilterChangesForSide(true).Values.ToList()!
                });
            }
            catch (Exception e) 
            {
                Console.WriteLine(e);
            }
        }

        return Ok(questionnaireVersions);
    }
}
