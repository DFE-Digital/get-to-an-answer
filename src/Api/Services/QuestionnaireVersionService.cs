using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public interface IQuestionnaireVersionService
{
    Task<ServiceResult> GetLatestQuestionnaireVersion(string email, Guid questionnaireId);
    Task<ServiceResult> GetQuestionnaireVersion(string email, Guid questionnaireId, int versionNumber);
    Task<ServiceResult> GetQuestionnaireVersions(string email, Guid questionnaireId);
}

public class QuestionnaireVersionService(GetToAnAnswerDbContext db) : AbstractService, IQuestionnaireVersionService
{
    public async Task<ServiceResult> GetLatestQuestionnaireVersion(string email, Guid questionnaireId)
    {
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var questionnaire = await db.Questionnaires
            .AsNoTracking()    
            .Where(q => q.Id == questionnaireId)
            .Include(q => q.Questions.Where(a => !a.IsDeleted))
            .ThenInclude(qq => qq.Answers.Where(a => !a.IsDeleted))
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

    public async Task<ServiceResult> GetQuestionnaireVersion(string email, Guid questionnaireId, int versionNumber)
    {
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

    public async Task<ServiceResult> GetQuestionnaireVersions(string email, Guid questionnaireId)
    {
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