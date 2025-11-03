using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain;
using Common.Extensions;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Common.Local;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Services;

public interface IQuestionnaireVersionService
{
    Task<ServiceResult> GetQuestionnaireVersions(string email, Guid questionnaireId);
}

public class QuestionnaireVersionService(GetToAnAnswerDbContext db, ILogger<QuestionnaireVersionService> logger) : AbstractService, IQuestionnaireVersionService
{
    public async Task<ServiceResult> GetQuestionnaireVersions(string email, Guid questionnaireId)
    {
        try
        {
            logger.LogInformation("GetQuestionnaireVersions started QuestionnaireId={QuestionnaireId}", questionnaireId);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

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
                    .Include(q => q.Questions.Where(a => !a.IsDeleted))
                    .ThenInclude(qq => qq.Answers.Where(a => !a.IsDeleted))
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
                        ChangeLog = changeMap?.Values.ToList()!
                    });
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Failed to build draft change list QuestionnaireId={QuestionnaireId}", questionnaireId);
                }
            }

            logger.LogInformation("GetQuestionnaireVersions succeeded QuestionnaireId={QuestionnaireId} Count={Count}", questionnaireId, questionnaireVersions.Count);
            return Ok(questionnaireVersions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetQuestionnaireVersions failed QuestionnaireId={QuestionnaireId}", questionnaireId);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }
}