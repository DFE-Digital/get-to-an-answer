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

public class AnswerService(GetToAnAnswerDbContext db) : AbstractService, IAnswerService
{
    public async Task<ServiceResult> CreateAnswer(string email, CreateAnswerRequestDto request)
    {
        var access = db.HasAccessToEntity<QuestionEntity>(email, request.QuestionId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        // Check if the question is part of the questionnaire
        var isQuestionFromQuestionnaire = db.Questions.Any(q => 
            q.Id == request.QuestionId &&
            q.QuestionnaireId == request.QuestionnaireId &&
            !q.IsDeleted);

        if (!isQuestionFromQuestionnaire)
        {
            return BadRequest();
        }

        switch (request.DestinationType)
        {
            case DestinationType.Question when request.DestinationQuestionId == null:
            case DestinationType.ExternalLink when request.DestinationUrl == null:
            case DestinationType.CustomContent when request.DestinationContentId == null:
                return BadRequest();
        }

        if (request is { DestinationType: DestinationType.Question, DestinationQuestionId: not null })
        {
            // Check if the parent question of this answer is different from the destination question
            if (request.QuestionId == request.DestinationQuestionId)
            {
                return BadRequest();
            }
            
            // Check if the destination question is part of the questionnaire
            var isDestQuestionFromQuestionnaire = db.Questions.Any(q => 
                q.Id == request.DestinationQuestionId &&
                q.QuestionnaireId == request.QuestionnaireId &&
                !q.IsDeleted);

            if (!isDestQuestionFromQuestionnaire)
            {
                return BadRequest();
            }
        }

        var entity = new AnswerEntity
        {
            QuestionnaireId = request.QuestionnaireId,
            QuestionId = request.QuestionId,
            Content = request.Content,
            Description = request.Description,
            Score = request.Score,
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
        
        return Ok(EntityToDto(entity));
    }
    
    public ServiceResult GetAnswer(string email, Guid id)
    {
        var access = db.HasAccessToEntity<AnswerEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        // Example: check ownership in your persistence layer
        var answer = db.Answers
            .FirstOrDefault(q => q.Id == id && !q.IsDeleted);

        if (answer == null)
            return NotFound();
        
        return Ok(EntityToDto(answer));
    }
    
    public ServiceResult GetAnswers(string email, Guid questionId)
    {
        var access = db.HasAccessToEntity<QuestionEntity>(email, questionId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var answers = db.Answers
            .Where(q => q.QuestionId == questionId && !q.IsDeleted);
        
        return Ok(answers.Select(answer => EntityToDto(answer)).ToList());
    }

    public async Task<ServiceResult> UpdateAnswer(string email, Guid id, UpdateAnswerRequestDto request)
    {
        var access = db.HasAccessToEntity<AnswerEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();
        
        var answer = db.Answers.FirstOrDefault(q => q.Id == id && !q.IsDeleted);
        
        if (answer == null)
            return NotFound();

        switch (request.DestinationType)
        {
            case DestinationType.Question when request.DestinationQuestionId == null:
            case DestinationType.ExternalLink when request.DestinationUrl == null:
            case DestinationType.CustomContent when request.DestinationContentId == null:
                return BadRequest();
        }

        if (request is { DestinationType: DestinationType.Question, DestinationQuestionId: not null })
        {
            // Check if the parent question of this answer is different from the destination question
            if (answer.QuestionId == request.DestinationQuestionId)
            {
                return BadRequest();
            }
            
            // Check if the question is part of the questionnaire
            var isQuestionFromQuestionnaire = db.Questions.Any(q => 
                q.Id == request.DestinationQuestionId &&
                q.QuestionnaireId == answer.QuestionnaireId &&
                !q.IsDeleted);

            if (!isQuestionFromQuestionnaire)
            {
                return BadRequest();
            }
        }
        
        answer.Content = request.Content;
        answer.Description = request.Description ?? answer.Description ?? string.Empty;
        answer.DestinationUrl = request.DestinationUrl ?? answer.DestinationUrl;
        answer.DestinationType = request.DestinationType ?? answer.DestinationType;
        answer.DestinationQuestionId = request.DestinationQuestionId ?? answer.DestinationQuestionId;
        answer.Score = request.Score ?? answer.Score;
        answer.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(answer.QuestionnaireId);
        
        return NoContent();
    }

    public async Task<ServiceResult> DeleteAnswer(string email, Guid id)
    {
        var access = db.HasAccessToEntity<AnswerEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();
        
        var answer = db.Answers.FirstOrDefault(q => q.Id == id);
        
        if (answer == null)
            return NotFound();
        
        answer.IsDeleted = true;
        answer.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        return NoContent();
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
            Score = entity.Score,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }
}