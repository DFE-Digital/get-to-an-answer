using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Infrastructure.Persistence;
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
public class QuestionnaireController(CheckerDbContext db) : ControllerBase
{
    [HttpPost("questionnaires")]
    [Authorize]
    public async Task<IActionResult> CreateQuestionnaire(CreateQuestionnaireRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var entity = new QuestionnaireEntity
        {
            Title = request.Title,
            Description = request.Description,
            Contributors = [email],
            CreatedAt = DateTime.UtcNow
        };
        
        db.Questionnaires.Add(entity);
        
        await db.SaveChangesAsync();
        
        var dto = new QuestionnaireDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description
        };
        
        return Ok(dto);
    }
    
    [HttpGet("questionnaires/{id}")]
    public async Task<IActionResult> GetQuestionnaire(int id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, id))
            return Unauthorized();

        // Example: check ownership in your persistence layer
        var questionnaire = db.Questionnaires
            .FirstOrDefault(q => q.Id == id
                                            && q.Status != EntityStatus.Deleted);

        if (questionnaire == null)
            return NotFound();
        
        return Ok(questionnaire);
    }
    
    [HttpGet("questionnaires")]
    public async Task<IActionResult> GetQuestionnaires()
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        var questionnaires = db.Questionnaires
            .Where(q => q.Contributors.Contains(email)
                        && q.Status != EntityStatus.Deleted);
        
        return Ok(questionnaires);
    }

    [HttpPut("questionnaires/{id}")]
    public async Task<IActionResult> UpdateQuestionnaire(int id, UpdateQuestionnaireRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, id))
            return Unauthorized();
        
        var questionnaire = new QuestionnaireEntity
        {
            Id = id,
        };

        db.Questionnaires.Attach(questionnaire);
        
        questionnaire.Title = request.Title;
        questionnaire.Status = EntityStatus.Draft;
        questionnaire.Description = request.Description;
        questionnaire.UpdatedAt = DateTime.UtcNow;
        
        db.Entry(questionnaire).Property(s => s.Title).IsModified = true;
        db.Entry(questionnaire).Property(s => s.Status).IsModified = true;
        db.Entry(questionnaire).Property(s => s.Description).IsModified = true;
        db.Entry(questionnaire).Property(s => s.UpdatedAt).IsModified = true;
        
        await db.SaveChangesAsync();
        
        return Ok("Questionnaire updated.");
    }

    [HttpPut("questionnaires/{id}/status")]
    public async Task<IActionResult> UpdateQuestionnaireStatus(int id, UpdateQuestionnaireStatusRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, id))
            return Unauthorized();

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
        
        return Ok("Question updated.");
    }

    [HttpDelete("questionnaires/{id}")]
    public async Task<IActionResult> DeleteQuestionnaire(int id)
    {
        return Ok(await UpdateQuestionnaireStatus(id, new UpdateQuestionnaireStatusRequestDto
        {
            Id = id,
            Status = EntityStatus.Deleted
        }));
    }
    
    [HttpPut("questionnaires/{id}/contributors")]
    public async Task<IActionResult> AddQuestionnaireContributor(int id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => x.Id == id);
        
        if (questionnaire == null) 
            return NotFound();

        if (!questionnaire.Contributors.Contains(email))
        {
            questionnaire.Contributors.Add(email);
        
            await db.SaveChangesAsync();
        
            await db.ResetQuestionnaireToDraft(id);
        }
        
        return Ok("Question updated.");
    }

    private async Task StoreQuestionnaireVersion(int id, int versionNumber)
    {
        var questionnaire = await db.Questionnaires
            .AsNoTracking()
            .Include(q => q.Questions)
            .ThenInclude(qq => qq.Answers)
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
