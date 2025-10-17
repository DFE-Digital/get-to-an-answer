using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class QuestionnaireController(GetToAnAnswerDbContext db) : ControllerBase
{
    [HttpPost("questionnaires")]
    public async Task<IActionResult> CreateQuestionnaire(CreateQuestionnaireRequestDto request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);
        
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var entity = new QuestionnaireEntity
        {
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Slug = request.Slug ?? string.Empty,
            Contributors = [email],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = email
        };
        
        db.Questionnaires.Add(entity);
        
        await db.SaveChangesAsync();
        
        var dto = new QuestionnaireDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Slug = entity.Slug,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
        
        return Created($"api/questionnaires/{dto.Id}", dto);
    }
    
    [HttpGet("questionnaires/{id}")]
    public async Task<IActionResult> GetQuestionnaire(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var questionnaire = db.Questionnaires
            .FirstOrDefault(q => q.Id == id
                                            && q.Status != EntityStatus.Deleted);

        if (questionnaire == null)
            return NotFound();
        
        return Ok(new QuestionnaireDto
        {
            Id = questionnaire.Id,
            Title = questionnaire.Title,
            Description = questionnaire.Description,
            Slug = questionnaire.Slug,
            CreatedAt = questionnaire.CreatedAt,
            UpdatedAt = questionnaire.UpdatedAt
        });
    }
    
    [HttpGet("questionnaires")]
    public async Task<IActionResult> GetQuestionnaires()
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        var questionnairesQuery = db.Questionnaires
            .Where(q => q.Contributors.Contains(email)
                        && q.Status != EntityStatus.Deleted);

        var questionnaires = questionnairesQuery.Select(q => new QuestionnaireDto
        {
            Id = q.Id,
            Title = q.Title,
            Description = q.Description,
            Slug = q.Slug,
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt
        }).ToList();
        
        return Ok(questionnaires);
    }

    [HttpPut("questionnaires/{id}")]
    public async Task<IActionResult> UpdateQuestionnaire(Guid id, UpdateQuestionnaireRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();
        
        var questionnaire = await db.Questionnaires
            .FirstOrDefaultAsync(q => q.Id == id);
        
        if (questionnaire == null) 
            return NotFound();
        
        questionnaire.Title = request.Title;
        questionnaire.Status = EntityStatus.Draft;
        questionnaire.Slug = request.Slug ?? questionnaire.Slug ?? string.Empty;
        questionnaire.Description = request.Description ?? questionnaire.Description  ?? string.Empty;
        questionnaire.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpPut("questionnaires/{id}/status")]
    public async Task<IActionResult> UpdateQuestionnaireStatus(Guid id, UpdateQuestionnaireStatusRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => x.Id == id);

        if (questionnaire == null) 
            return NotFound();
        
        var versionNumber = 1;

        questionnaire.Status = request.Status;
        if (request.Status == EntityStatus.Published) {
            versionNumber = ++questionnaire.Version;
        }

        questionnaire.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();

        if (request.Status == EntityStatus.Published)
        {
            await StoreQuestionnaireVersion(id, versionNumber);
        }
        
        return NoContent();
    }

    [HttpDelete("questionnaires/{id}")]
    public async Task<IActionResult> DeleteQuestionnaire(Guid id)
    {
        return await UpdateQuestionnaireStatus(id, new UpdateQuestionnaireStatusRequestDto
        {
            Id = id,
            Status = EntityStatus.Deleted
        });
    }
    
    [HttpPut("questionnaires/{id}/contributors")]
    public async Task<IActionResult> AddQuestionnaireContributor(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => x.Id == id);
        
        if (questionnaire == null) 
            return NotFound();

        if (!questionnaire.Contributors.Contains(email))
        {
            questionnaire.Contributors.Add(email);
        
            await db.SaveChangesAsync();
        }
        
        return NoContent();
    }

    private async Task StoreQuestionnaireVersion(Guid id, int versionNumber)
    {
        var questionnaire = await db.Questionnaires
            .AsNoTracking()
            //.Include(q => q.Questions)
            //.ThenInclude(qq => qq.Answers)
            .FirstAsync(q => q.Id == id);
        
        var json = JsonSerializer.Serialize(questionnaire, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        });
        
        var snapshot = new QuestionnaireVersionEntity
        {
            QuestionnaireId = questionnaire.Id,
            Version = versionNumber,
            QuestionnaireJson = json,
            CreatedAt = DateTime.UtcNow
        };

        db.QuestionnaireVersions.Add(snapshot);
        
        await db.SaveChangesAsync();
    }
}