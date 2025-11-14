using System.Diagnostics;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;

namespace Api.Services;

public interface IAnswerService
{
    Task<ServiceResult> CreateAnswer(string email, CreateAnswerRequestDto request);
    ServiceResult GetAnswer(string email, Guid id);
    ServiceResult GetAnswers(string email, Guid questionId);
    Task<ServiceResult> UpdateAnswer(string email, Guid id, UpdateAnswerRequestDto request);
    Task<ServiceResult> DeleteAnswer(string email, Guid id);
}

public class AnswerService(GetToAnAnswerDbContext db, ILogger<AnswerService> logger) : AbstractService, IAnswerService
{
    public async Task<ServiceResult> CreateAnswer(string email, CreateAnswerRequestDto request)
    {
        try
        {
            logger.LogInformation("CreateAnswer started QuestionnaireId={QuestionnaireId} QuestionId={QuestionId}", request.QuestionnaireId, request.QuestionId);

            var access = db.HasAccessToEntity<QuestionEntity>(email, request.QuestionId);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that question", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var isQuestionFromQuestionnaire = db.Questions.Any(q =>
                q.Id == request.QuestionId &&
                q.QuestionnaireId == request.QuestionnaireId &&
                !q.IsDeleted);

            if (!isQuestionFromQuestionnaire)
                return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                {
                    ["questionId"] = ["The question must belong to the specified questionnaire."]
                }));

            switch (request.DestinationType)
            {
                case DestinationType.Question when request.DestinationQuestionId == null:
                    return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                    {
                        ["destinationQuestionId"] = ["The DestinationQuestionId field is required when DestinationType is Question."]
                    }));
                case DestinationType.ExternalLink when request.DestinationUrl == null:
                    return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                    {
                        ["destinationUrl"] = ["The DestinationUrl field is required when DestinationType is ExternalLink."]
                    }));
                case DestinationType.CustomContent when request.DestinationContentId == null:
                    return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                    {
                        ["destinationContentId"] = ["The DestinationContentId field is required when DestinationType is CustomContent."]
                    }));
            }

            if (request is { DestinationType: DestinationType.Question, DestinationQuestionId: not null })
            {
                if (request.QuestionId == request.DestinationQuestionId)
                    return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                    {
                        ["destinationQuestionId"] = ["You cannot branch to the same question."]
                    }));

                var isDestQuestionFromQuestionnaire = db.Questions.Any(q =>
                    q.Id == request.DestinationQuestionId &&
                    q.QuestionnaireId == request.QuestionnaireId &&
                    !q.IsDeleted);

                if (!isDestQuestionFromQuestionnaire)
                    return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                    {
                        ["destinationQuestionId"] = ["The destination question must belong to the specified questionnaire."]
                    }));
            }

            var entity = new AnswerEntity
            {
                QuestionnaireId = request.QuestionnaireId,
                QuestionId = request.QuestionId,
                Content = request.Content,
                Description = request.Description,
                Priority = request.Priority,
                DestinationUrl = request.DestinationUrl,
                DestinationQuestionId = request.DestinationQuestionId,
                DestinationType = request.DestinationType,
                CreatedBy = email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            db.Answers.Add(entity);
            await db.SaveChangesAsync();
            await db.ResetQuestionnaireToDraft(request.QuestionnaireId);

            logger.LogInformation("CreateAnswer succeeded AnswerId={AnswerId}", entity.Id);
            return Created(EntityToDto(entity));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateAnswer failed");
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public ServiceResult GetAnswer(string email, Guid id)
    {
        try
        {
            logger.LogInformation("GetAnswer started AnswerId={AnswerId}", id);

            var access = db.HasAccessToEntity<AnswerEntity>(email, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that answer", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var answer = db.Answers.FirstOrDefault(q => q.Id == id && !q.IsDeleted);
            if (answer == null)
                return NotFound(ProblemTrace("We could not find that answer", 404));

            logger.LogInformation("GetAnswer succeeded AnswerId={AnswerId}", id);
            return Ok(EntityToDto(answer));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAnswer failed AnswerId={AnswerId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public ServiceResult GetAnswers(string email, Guid questionId)
    {
        try
        {
            logger.LogInformation("GetAnswers started QuestionId={QuestionId}", questionId);

            var access = db.HasAccessToEntity<QuestionEntity>(email, questionId);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that question", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var answers = db.Answers.Where(q => q.QuestionId == questionId && !q.IsDeleted);
            var list = answers.AsEnumerable().Select(EntityToDto).ToList();

            logger.LogInformation("GetAnswers succeeded QuestionId={QuestionId} Count={Count}", questionId, list.Count);
            return Ok(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAnswers failed QuestionId={QuestionId}", questionId);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> UpdateAnswer(string email, Guid id, UpdateAnswerRequestDto request)
    {
        try
        {
            logger.LogInformation("UpdateAnswer started AnswerId={AnswerId}", id);

            var access = db.HasAccessToEntity<AnswerEntity>(email, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that answer", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var answer = db.Answers.FirstOrDefault(q => q.Id == id && !q.IsDeleted);
            if (answer == null)
                return NotFound(ProblemTrace("We could not find that answer", 404));

            switch (request.DestinationType)
            {
                case DestinationType.Question when request.DestinationQuestionId == null:
                    return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                    {
                        ["destinationQuestionId"] = ["The DestinationQuestionId field is required when DestinationType is Question."]
                    }));
                case DestinationType.ExternalLink when request.DestinationUrl == null:
                    return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                    {
                        ["destinationUrl"] = ["The DestinationUrl field is required when DestinationType is ExternalLink."]
                    }));
                case DestinationType.CustomContent when request.DestinationContentId == null:
                    return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                    {
                        ["destinationContentId"] = ["The DestinationContentId field is required when DestinationType is CustomContent."]
                    }));
            }

            if (request is { DestinationType: DestinationType.Question, DestinationQuestionId: not null })
            {
                if (answer.QuestionId == request.DestinationQuestionId)
                    return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                    {
                        ["destinationQuestionId"] = ["You cannot branch to the same question."]
                    }));

                var isQuestionFromQuestionnaire = db.Questions.Any(q =>
                    q.Id == request.DestinationQuestionId &&
                    q.QuestionnaireId == answer.QuestionnaireId &&
                    !q.IsDeleted);

                if (!isQuestionFromQuestionnaire)
                    return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                    {
                        ["destinationQuestionId"] = ["The destination question must belong to the specified questionnaire."]
                    }));
            }

            answer.Content = request.Content;
            answer.Description = request.Description ?? answer.Description ?? string.Empty;
            answer.DestinationUrl = request.DestinationUrl ?? answer.DestinationUrl;
            answer.DestinationType = request.DestinationType ?? answer.DestinationType;
            answer.DestinationQuestionId = request.DestinationQuestionId ?? answer.DestinationQuestionId;
            answer.Priority = request.Priority ?? answer.Priority;
            answer.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            await db.ResetQuestionnaireToDraft(answer.QuestionnaireId);

            logger.LogInformation("UpdateAnswer succeeded AnswerId={AnswerId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateAnswer failed AnswerId={AnswerId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> DeleteAnswer(string email, Guid id)
    {
        try
        {
            logger.LogInformation("DeleteAnswer started AnswerId={AnswerId}", id);

            var access = db.HasAccessToEntity<AnswerEntity>(email, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that answer", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var answer = db.Answers.FirstOrDefault(q => q.Id == id);
            if (answer == null)
                return NotFound(ProblemTrace("We could not find that answer", 404));

            answer.IsDeleted = true;
            answer.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            logger.LogInformation("DeleteAnswer succeeded AnswerId={AnswerId} traceId={TraceId}", id, Activity.Current?.TraceId.ToString());
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteAnswer failed AnswerId={AnswerId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    private static AnswerDto EntityToDto(AnswerEntity entity)
    {
        return new AnswerDto
        {
            Id = entity.Id,
            Content = entity.Content,
            Description = entity.Description,
            DestinationUrl = entity.DestinationUrl,
            DestinationType = entity.DestinationType,
            DestinationQuestionId = entity.DestinationQuestionId,
            QuestionId = entity.QuestionId,
            QuestionnaireId = entity.QuestionnaireId,
            Priority = entity.Priority,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }
}