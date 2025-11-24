using System.Diagnostics;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging;

namespace Api.Services;

public interface IContentService
{
    Task<ServiceResult> CreateContent(string userId, CreateContentRequestDto request);
    ServiceResult GetContent(string userId, Guid id);
    ServiceResult GetContents(string userId, Guid questionId);
    Task<ServiceResult> UpdateContent(string userId, Guid id, UpdateContentRequestDto request);
    Task<ServiceResult> DeleteContent(string userId, Guid id);
}

public class ContentService(GetToAnAnswerDbContext db, ILogger<ContentService> logger) : AbstractService, IContentService
{
    public async Task<ServiceResult> CreateContent(string userId, CreateContentRequestDto request)
    {
        try
        {
            logger.LogInformation("CreateContent started QuestionnaireId={QuestionnaireId}", request.QuestionnaireId);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, request.QuestionnaireId);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var entity = new ContentEntity
            {
                QuestionnaireId = request.QuestionnaireId,
                Title = request.Title,
                Content = request.Content,
                ReferenceName = request.ReferenceName,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            db.Contents.Add(entity);
            await db.SaveChangesAsync();

            logger.LogInformation("CreateContent succeeded ContentId={ContentId}", entity.Id);
            return Created(EntityToDto(entity));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateContent failed");
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public ServiceResult GetContent(string userId, Guid id)
    {
        try
        {
            logger.LogInformation("GetContent started ContentId={ContentId}", id);

            var access = db.HasAccessToEntity<ContentEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that content", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var answer = db.Contents.FirstOrDefault(q => q.Id == id && !q.IsDeleted);
            if (answer == null)
                return NotFound(ProblemTrace("We could not find that content", 404));

            logger.LogInformation("GetContent succeeded ContentId={ContentId}", id);
            return Ok(EntityToDto(answer));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetContent failed ContentId={ContentId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public ServiceResult GetContents(string userId, Guid questionnaireId)
    {
        try
        {
            logger.LogInformation("GetContents started QuestionnaireId={QuestionnaireId}", questionnaireId);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, questionnaireId);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var answers = db.Contents.Where(q => q.QuestionnaireId == questionnaireId && !q.IsDeleted);
            var list = answers.Select(answer => EntityToDto(answer)).ToList();

            logger.LogInformation("GetContents succeeded QuestionnaireId={QuestionnaireId} Count={Count}", questionnaireId, list.Count);
            return Ok(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetContents failed QuestionnaireId={QuestionnaireId}", questionnaireId);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> UpdateContent(string userId, Guid id, UpdateContentRequestDto request)
    {
        try
        {
            logger.LogInformation("UpdateContent started ContentId={ContentId}", id);

            if (request.Content == null && request.Title == null)
            {
                return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                {
                    ["request"] = ["No fields to update."]
                }));
            }

            var access = db.HasAccessToEntity<ContentEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that content", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var answer = db.Contents.FirstOrDefault(q => q.Id == id && !q.IsDeleted);
            if (answer == null)
                return NotFound(ProblemTrace("We could not find that content", 404));

            answer.Content = request.Content ?? answer.Content;
            answer.Title = request.Title ?? answer.Title;
            answer.ReferenceName = request.ReferenceName ?? answer.ReferenceName;
            answer.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            await db.ResetQuestionnaireToDraft(answer.QuestionnaireId);

            logger.LogInformation("UpdateContent succeeded ContentId={ContentId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateContent failed ContentId={ContentId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> DeleteContent(string userId, Guid id)
    {
        try
        {
            logger.LogInformation("DeleteContent started ContentId={ContentId}", id);

            var access = db.HasAccessToEntity<ContentEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that content", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var answer = db.Contents.FirstOrDefault(q => q.Id == id && !q.IsDeleted);
            if (answer == null)
                return NotFound(ProblemTrace("We could not find that content", 404));

            answer.IsDeleted = true;
            answer.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            logger.LogInformation("DeleteContent succeeded ContentId={ContentId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteContent failed ContentId={ContentId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    private static ContentDto EntityToDto(ContentEntity entity)
    {
        return new ContentDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            QuestionnaireId = entity.QuestionnaireId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }
}