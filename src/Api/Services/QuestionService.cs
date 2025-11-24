using System.Diagnostics;
using System.Security.Claims;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Services;

public interface IQuestionService
{
    Task<ServiceResult> CreateQuestion(string userId, CreateQuestionRequestDto request);
    ServiceResult GetQuestion(string userId, Guid id);
    ServiceResult GetQuestions(string userId, Guid questionnaireId);
    Task<ServiceResult> UpdateQuestion(string userId, Guid id, UpdateQuestionRequestDto request);
    Task<ServiceResult> DeleteQuestion(string userId, Guid id);
    Task<ServiceResult> MoveQuestionDownOne(string userId, Guid questionnaireId, Guid id);
    Task<ServiceResult> MoveQuestionUpOne(string userId, Guid questionnaireId, Guid id);
}

public class QuestionService(GetToAnAnswerDbContext db, ILogger<QuestionService> logger) : AbstractService, IQuestionService
{
    public async Task<ServiceResult> CreateQuestion(string userId, CreateQuestionRequestDto request)
    {
        try
        {
            logger.LogInformation("CreateQuestion started QuestionnaireId={QuestionnaireId}", request.QuestionnaireId);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, request.QuestionnaireId);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var entity = new QuestionEntity
            {
                QuestionnaireId = request.QuestionnaireId,
                Content = request.Content,
                Description = request.Description,
                Type = request.Type,
                Order = db.Questions.Count(x => x.QuestionnaireId == request.QuestionnaireId
                                                && !x.IsDeleted) + 1,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            db.Questions.Add(entity);

            await db.SaveChangesAsync();
            await db.ResetQuestionnaireToDraft(request.QuestionnaireId);

            logger.LogInformation("CreateQuestion succeeded QuestionId={QuestionId}", entity.Id);
            return Created(new QuestionDto
            {
                Id = entity.Id,
                QuestionnaireId = entity.QuestionnaireId,
                Content = entity.Content,
                Description = entity.Description,
                Type = entity.Type,
                Order = entity.Order,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateQuestion failed");
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }
    
    public ServiceResult GetQuestion(string userId, Guid id)
    {
        try
        {
            logger.LogInformation("GetQuestion started QuestionId={QuestionId}", id);

            var access = db.HasAccessToEntity<QuestionEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that question", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var question = db.Questions
                .Include(q => q.Answers.Where(a => !a.IsDeleted))
                .FirstOrDefault(q => q.Id == id && !q.IsDeleted);

            if (question == null)
                return NotFound(ProblemTrace("We could not find that question", 404));

            return Ok(new QuestionDto
            {
                Id = question.Id,
                QuestionnaireId = question.QuestionnaireId,
                Content = question.Content,
                Description = question.Description,
                Order = question.Order,
                Answers = question.Answers.Select(a => new AnswerDto
                {
                    Id = a.Id,
                    Content = a.Content,
                    QuestionId = a.QuestionId,
                    Priority = a.Priority,
                    DestinationType = a.DestinationType,
                }).ToList(),
                Type = question.Type,
                CreatedAt = question.CreatedAt,
                UpdatedAt = question.UpdatedAt,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetQuestion failed QuestionId={QuestionId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public ServiceResult GetQuestions(string userId, Guid questionnaireId)
    {
        try
        {
            logger.LogInformation("GetQuestions started QuestionnaireId={QuestionnaireId}", questionnaireId);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, questionnaireId);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var questions = db.Questions
                .Where(q => q.QuestionnaireId == questionnaireId
                            && !q.IsDeleted);

            var list = questions
                .OrderBy(q => q.Order)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    QuestionnaireId = q.QuestionnaireId,
                    Content = q.Content,
                    Description = q.Description,
                    Order = q.Order,
                    Type = q.Type,
                    CreatedAt = q.CreatedAt,
                    UpdatedAt = q.UpdatedAt,
                }).ToList();

            logger.LogInformation("GetQuestions succeeded QuestionnaireId={QuestionnaireId} Count={Count}", questionnaireId, list.Count);
            return Ok(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetQuestions failed QuestionnaireId={QuestionnaireId}", questionnaireId);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> UpdateQuestion(string userId, Guid id, UpdateQuestionRequestDto request)
    {
        try
        {
            logger.LogInformation("UpdateQuestion started QuestionId={QuestionId}", id);

            var access = db.HasAccessToEntity<QuestionEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that question", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var question = db.Questions.FirstOrDefault(q => q.Id == id);
            if (question == null)
                return NotFound(ProblemTrace("We could not find that question", 404));

            question.Content = request.Content;
            question.Description = request.Description ?? question.Description;
            question.Type = request.Type ?? question.Type;
            question.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            await db.ResetQuestionnaireToDraft(question.QuestionnaireId);

            logger.LogInformation("UpdateQuestion succeeded QuestionId={QuestionId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateQuestion failed QuestionId={QuestionId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> DeleteQuestion(string userId, Guid id)
    {
        try
        {
            logger.LogInformation("DeleteQuestion started QuestionId={QuestionId}", id);

            var access = db.HasAccessToEntity<QuestionEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that question", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var question = db.Questions.FirstOrDefault(q => q.Id == id && !q.IsDeleted);
            if (question == null)
                return NotFound(ProblemTrace("We could not find that question", 404));

            question.IsDeleted = true;
            question.UpdatedAt = DateTime.UtcNow;
            
            // change the order of all the questions after this one
            var questions = db.Questions.Where(q => 
                q.QuestionnaireId == question.QuestionnaireId && q.Order > question.Order);
            
            foreach (var q in questions) q.Order--;

            await db.SaveChangesAsync();
            await db.ResetQuestionnaireToDraft(question.QuestionnaireId);

            logger.LogInformation("DeleteQuestion succeeded QuestionId={QuestionId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteQuestion failed QuestionId={QuestionId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }
    
    public async Task<ServiceResult> MoveQuestionDownOne(string userId, Guid questionnaireId, Guid id)
    {
        return await MoveQuestionByOne(userId, questionnaireId, id, +1);
    }
    
    public async Task<ServiceResult> MoveQuestionUpOne(string userId, Guid questionnaireId, Guid id)
    {
        return await MoveQuestionByOne(userId, questionnaireId, id, -1);
    }
    
    private async Task<ServiceResult> MoveQuestionByOne(string userId, Guid questionnaireId, Guid id, int direction)
    {
        try
        {
            logger.LogInformation("MoveQuestionByOne started QuestionnaireId={QuestionnaireId} QuestionId={QuestionId} Direction={Direction}", questionnaireId, id, direction);

            var access = db.HasAccessToEntity<QuestionEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that question", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var current = await db.Questions
                .FirstOrDefaultAsync(x => 
                    x.Id == id && 
                    x.QuestionnaireId == questionnaireId && 
                    !x.IsDeleted);

            if (current == null) return NotFound(ProblemTrace("We could not find that question", 404));

            var next = await db.Questions
                .FirstOrDefaultAsync(x => 
                    x.QuestionnaireId == questionnaireId && 
                    x.Order == current.Order + direction && 
                    !x.IsDeleted);

            if (next == null) return BadRequest(ProblemTrace("Cannot move beyond the first or last question.", 400));

            (current.Order, next.Order) = (next.Order, current.Order);
            
            current.UpdatedAt = DateTime.UtcNow;
            next.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            await db.ResetQuestionnaireToDraft(questionnaireId);

            logger.LogInformation("MoveQuestionByOne succeeded QuestionnaireId={QuestionnaireId} QuestionId={QuestionId}", questionnaireId, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MoveQuestionByOne failed QuestionnaireId={QuestionnaireId} QuestionId={QuestionId}", questionnaireId, id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }
}