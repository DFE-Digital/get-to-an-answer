using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;

namespace Api.Services;

public interface IContentService
{
    Task<ServiceResult> CreateContent(string email, CreateContentRequestDto request);
    ServiceResult GetContent(string email, Guid id);
    ServiceResult GetContents(string email, Guid questionId);
    Task<ServiceResult> UpdateContent(string email, Guid id, UpdateContentRequestDto request);
    Task<ServiceResult> DeleteContent(string email, Guid id);
}

public class ContentService(GetToAnAnswerDbContext db) : AbstractService, IContentService
{
    public async Task<ServiceResult> CreateContent(string email, CreateContentRequestDto request)
    {
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, request.QuestionnaireId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var entity = new ContentEntity
        {
            QuestionnaireId = request.QuestionnaireId,
            Title = request.Title,
            Content = request.Content,
            CreatedBy = email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        
        db.Contents.Add(entity);
        
        await db.SaveChangesAsync();
        
        return Created(EntityToDto(entity));
    }
    
    public ServiceResult GetContent(string email, Guid id)
    {
        var access = db.HasAccessToEntity<ContentEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var answer = db.Contents
            .FirstOrDefault(q => q.Id == id && !q.IsDeleted);

        if (answer == null)
            return NotFound();
        
        return Ok(EntityToDto(answer));
    }
    
    public ServiceResult GetContents(string email, Guid questionnaireId)
    {
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var answers = db.Contents
            .Where(q => q.QuestionnaireId == questionnaireId && !q.IsDeleted);
        
        return Ok(answers.Select(answer => EntityToDto(answer)).ToList());
    }

    public async Task<ServiceResult> UpdateContent(string email, Guid id, UpdateContentRequestDto request)
    {
        if (request.Content == null && request.Title == null)
        {
            return BadRequest("No fields to update");       
        }
        
        var access = db.HasAccessToEntity<ContentEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();
        
        var answer = db.Contents.FirstOrDefault(q => q.Id == id && !q.IsDeleted);
        
        if (answer == null)
            return NotFound();
        
        answer.Content = request.Content ?? answer.Content;
        answer.Title = request.Title ?? answer.Title;
        answer.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        await db.ResetQuestionnaireToDraft(answer.QuestionnaireId);
        
        return NoContent();
    }

    public async Task<ServiceResult> DeleteContent(string email, Guid id)
    {
        var access = db.HasAccessToEntity<ContentEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();
        
        var answer = db.Contents.FirstOrDefault(q => q.Id == id && !q.IsDeleted);
        
        if (answer == null)
            return NotFound();
        
        answer.IsDeleted = true;
        answer.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        return NoContent();
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