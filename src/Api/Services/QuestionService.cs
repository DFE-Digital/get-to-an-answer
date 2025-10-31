using System.Security.Claims;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public interface IQuestionService
{
    Task<ServiceResult> CreateQuestion(string email, CreateQuestionRequestDto request);
    ServiceResult GetQuestion(string email, Guid id);
    ServiceResult GetQuestions(string email, Guid questionnaireId);
    Task<ServiceResult> UpdateQuestion(string email, Guid id, UpdateQuestionRequestDto request);
    Task<ServiceResult> DeleteQuestion(string email, Guid id);
    Task<ServiceResult> MoveQuestionDownOne(string email, Guid questionnaireId, Guid id);
    Task<ServiceResult> MoveQuestionUpOne(string email, Guid questionnaireId, Guid id);
}

public class QuestionService(GetToAnAnswerDbContext db) : AbstractService, IQuestionService
{
    public async Task<ServiceResult> CreateQuestion(string email, CreateQuestionRequestDto request)
    {
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, request.QuestionnaireId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var entity = new QuestionEntity
        {
            QuestionnaireId = request.QuestionnaireId,
            Content = request.Content,
            Description = request.Description,
            Type = request.Type,
            Order = db.Questions.Count(x => x.QuestionnaireId == request.QuestionnaireId
                                            && !x.IsDeleted) + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        
        db.Questions.Add(entity);
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(request.QuestionnaireId);
        
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
    
    public ServiceResult GetQuestion(string email, Guid id)
    {
        var access = db.HasAccessToEntity<QuestionEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        // Example: check ownership in your persistence layer
        var question = db.Questions
            .Include(q => q.Answers.Where(a => !a.IsDeleted))
            .FirstOrDefault(q => q.Id == id && !q.IsDeleted);

        if (question == null)
            return NotFound();
        
        // Logic to create a question
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
                Score = a.Score,
                DestinationType = a.DestinationType,
            }).ToList(),
            Type = question.Type,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
        });
    }

    public ServiceResult GetQuestions(string email, Guid questionnaireId)
    {
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();
        
        var questions = db.Questions
            .Where(q => q.QuestionnaireId == questionnaireId
                        && !q.IsDeleted);
        
        return Ok(questions
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
        }).ToList());
    }

    public async Task<ServiceResult> UpdateQuestion(string email, Guid id, UpdateQuestionRequestDto request)
    {
        var access = db.HasAccessToEntity<QuestionEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();
        
        var question = db.Questions.FirstOrDefault(q => q.Id == id);
        
        if (question == null)
            return NotFound();
        
        question.Content = request.Content;
        question.Description = request.Description ?? question.Description;
        question.Type = request.Type ?? question.Type;
        question.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(question.QuestionnaireId);
        
        return NoContent();
    }

    public async Task<ServiceResult> DeleteQuestion(string email, Guid id)
    {
        var access = db.HasAccessToEntity<QuestionEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();
        
        var question = db.Questions.FirstOrDefault(q => q.Id == id);
        
        if (question == null)
            return NotFound();
        
        question.IsDeleted = true;
        question.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(question.QuestionnaireId);
        
        return NoContent();
    }
    
    public async Task<ServiceResult> MoveQuestionDownOne(string email, Guid questionnaireId, Guid id)
    {
        return await MoveQuestionByOne(email, questionnaireId, id, +1);
    }
    
    public async Task<ServiceResult> MoveQuestionUpOne(string email, Guid questionnaireId, Guid id)
    {
        return await MoveQuestionByOne(email, questionnaireId, id, -1);
    }
    
    private async Task<ServiceResult> MoveQuestionByOne(string email, Guid questionnaireId, Guid id, int direction)
    {
        var access = db.HasAccessToEntity<QuestionEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        // Load the target item
        var current = await db.Questions
            .FirstOrDefaultAsync(x => 
                x.Id == id && 
                x.QuestionnaireId == questionnaireId && 
                !x.IsDeleted);

        if (current == null) return NotFound();

        // Find the next item in order within the same scope
        var next = await db.Questions
            .FirstOrDefaultAsync(x => 
                x.QuestionnaireId == questionnaireId && 
                x.Order == current.Order + direction && 
                !x.IsDeleted);

        // If already first or last, nothing to do
        if (next == null) return BadRequest();

        // Swap orders
        (current.Order, next.Order) = (next.Order, current.Order);
        
        current.UpdatedAt = DateTime.UtcNow;
        next.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(questionnaireId);
        
        return NoContent();
    }
}