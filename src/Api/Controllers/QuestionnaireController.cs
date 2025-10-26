using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain;
using Common.Domain.Request.Add;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Common.Local;
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
            CreatedAt = entity.CreatedAt,
        };
        
        return Created($"api/questionnaires/{dto.Id}", dto);
    }
    
    [HttpGet("questionnaires/{id}")]
    public IActionResult GetQuestionnaire(Guid id)
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
            Status = questionnaire.Status,
            Version = questionnaire.Version,
            CreatedAt = questionnaire.CreatedAt,
            UpdatedAt = questionnaire.UpdatedAt
        });
    }
    
    [HttpGet("questionnaires")]
    public IActionResult GetQuestionnaires()
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
            Status = q.Status,
            Version = q.Version,
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt
        }).ToList();
        
        return Ok(questionnaires);
    }

    [HttpPut("questionnaires/{id}")]
    public async Task<IActionResult> UpdateQuestionnaire(Guid id, UpdateQuestionnaireRequestDto request)
    {
        if (request.DisplayTitle == null && request.Title == null && request.Slug == null && request.Description == null)
        {
            return BadRequest("No fields to update");
        }
        
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

        questionnaire.DisplayTitle = request.DisplayTitle ?? questionnaire.DisplayTitle;
        questionnaire.Title = request.Title ?? questionnaire.Title;
        questionnaire.Status = EntityStatus.Draft;
        questionnaire.Slug = request.Slug ?? questionnaire.Slug;
        questionnaire.Description = request.Description ?? questionnaire.Description  ?? string.Empty;
        questionnaire.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpPut("questionnaires/{id}/publish")]
    public async Task<IActionResult> PublishQuestionnaire(Guid id)
    {
        return await UpdateQuestionnaireStatus(id, EntityStatus.Published);
    }

    [HttpDelete("questionnaires/{id}/unpublish")]
    public async Task<IActionResult> UnpublishQuestionnaire(Guid id)
    {
        return await UpdateQuestionnaireStatus(id, EntityStatus.Draft);
    }

    [HttpDelete("questionnaires/{id}")]
    public async Task<IActionResult> DeleteQuestionnaire(Guid id)
    {
        // soft delete questions under the questionnaire
        await db.Questions.Where(q => q.QuestionnaireId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.IsDeleted, true));
        
        // soft delete answers under the questionnaire
        await db.Answers.Where(q => q.QuestionnaireId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.IsDeleted, true));
        
        return await UpdateQuestionnaireStatus(id, EntityStatus.Deleted);
    }
    
    [HttpPut("questionnaires/{id}/contributors/self")]
    public async Task<IActionResult> AddSelfToQuestionnaireContributors(Guid id)
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
        else
        {
            return Conflict();       
        }
        
        return NoContent();
    }

    [HttpPost("questionnaires/{id}/clones")]
    public async Task<IActionResult> CloneQuestionnaire(Guid id, CloneQuestionnaireRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();
        
        var questionnaire = await db.Questionnaires
            .AsNoTracking()
            .Where(q => q.Id == id && q.Status != EntityStatus.Deleted)
            .Include(q => q.Questions)
            .ThenInclude(qq => qq.Answers)
            .FirstOrDefaultAsync();

        if (questionnaire == null)
            return NotFound();

        var cloneQuestionnaire = new QuestionnaireEntity
        {
            Title = request.Title,
            Status = EntityStatus.Draft,
            Contributors = questionnaire.Contributors,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = email
        };

        db.Questionnaires.Add(cloneQuestionnaire);
        
        await db.SaveChangesAsync();
        
        var cloneQuestionnaireId = cloneQuestionnaire.Id;
        
        var cloneQuestions = new Dictionary<int, QuestionEntity>();
        var orderAnswers = new Dictionary<int, ICollection<AnswerEntity>>();
        
        foreach (var question in questionnaire.Questions)
        {
            var cloneQuestion = new QuestionEntity
            {
                QuestionnaireId = cloneQuestionnaireId,
                Content = question.Content,
                Description = question.Description,
                Type = question.Type,
                Order = question.Order,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
                
            cloneQuestions.Add(question.Order, cloneQuestion);
            
            orderAnswers.Add(question.Order, question.Answers);
        }
        
        await db.Questions.AddRangeAsync(cloneQuestions.Values);
        
        await db.SaveChangesAsync();

        var cloneAnswers = new List<AnswerEntity>();
            
        foreach (var (questionOrder, answers) in orderAnswers)
        {
            var cloneQuestionId = cloneQuestions[questionOrder].Id;
            
            foreach (var answer in answers)
            {
                var cloneAnswer = new AnswerEntity
                {
                    QuestionId = cloneQuestionId,
                    QuestionnaireId = cloneQuestionnaireId,
                    Content = answer.Content,
                    Description = answer.Description,
                    DestinationUrl = answer.DestinationUrl,
                    DestinationQuestionId = answer.DestinationQuestionId,
                    DestinationType = answer.DestinationType,
                    Score = answer.Score,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                
                cloneAnswers.Add(cloneAnswer);
            }
        }
        
        await db.Answers.AddRangeAsync(cloneAnswers);
        
        await db.SaveChangesAsync();
        
        return Created($"/api/questionnaires/{cloneQuestionnaire.Id}", 
            EntityToDto(cloneQuestionnaire));
    }
    
    [HttpGet("questionnaires/{questionnaireId}/contributors")]
    public async Task<IActionResult> GetContributors(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => 
            x.Id == questionnaireId && x.Status != EntityStatus.Deleted);

        if (questionnaire == null)
            return NotFound();

        return Ok(questionnaire.Contributors);
    }

    [HttpPut("questionnaires/{id}/contributors")]
    public async Task<IActionResult> AddContributor(Guid id, AddContributorRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => 
            x.Id == id && x.Status != EntityStatus.Deleted);

        if (questionnaire == null)
            return NotFound();

        if (!questionnaire.Contributors.Contains(request.Email))
        {
            questionnaire.Contributors.Add(request.Email);
            await db.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpDelete("questionnaires/{id}/contributors/{email}")]
    public async Task<IActionResult> RemoveContributor(Guid id, string email)
    {
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email)!;

        var access = db.HasAccessToEntity<QuestionnaireEntity>(currentUserEmail, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => x.Id == id);

        if (questionnaire == null)
            return NotFound();

        if (questionnaire.Contributors.Count > 2 && questionnaire.Contributors.Contains(email))
        {
            questionnaire.Contributors.Remove(email);
            await db.SaveChangesAsync();
        }

        return Ok();
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    private async Task StoreQuestionnaireVersion(Guid id, int versionNumber)
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
            CreatedAt = DateTime.UtcNow,
            CreatedBy = questionnaire.PublishedBy ?? string.Empty,
            ChangeDescription = "TODO"
        };

        db.QuestionnaireVersions.Add(snapshot);
        
        await db.SaveChangesAsync();

        if (versionNumber <= 1)
        {
            return; // No need to compare to the previous version
        }
        
        var previousVersion = await db.QuestionnaireVersions.FirstOrDefaultAsync(v => 
            v.QuestionnaireId == id && v.Version == versionNumber - 1);
        
        if (previousVersion == null)
            return;
        
        // compare the new published version to the old one, then populate teh ChangeLog and the ChangeDescription

        var changeMap = VersionDiffRenderer.RenderCompare(previousVersion.QuestionnaireJson, json);

        var newChangeMap = changeMap?.FilterChangesForSide(true).Values.ToJson() ?? string.Empty;

        snapshot.ChangeLog = newChangeMap;
        
        await db.SaveChangesAsync();
        
        Console.WriteLine(newChangeMap);
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> UpdateQuestionnaireStatus(Guid id, EntityStatus status)
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

        questionnaire.Status = status;
        if (status == EntityStatus.Published) 
        {
            versionNumber = ++questionnaire.Version;
        }
        else if (status == EntityStatus.Draft) // Rollback to previous version
        {
            versionNumber = questionnaire.Version--;
        }

        questionnaire.UpdatedAt = DateTime.UtcNow;
        questionnaire.PublishedBy = email;
        questionnaire.PublishedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();

        if (status == EntityStatus.Published)
        {
            await StoreQuestionnaireVersion(id, versionNumber);
        }
        else if (status == EntityStatus.Draft)
        {
            await RemoveQuestionnaireVersion(id, versionNumber);       
        }
        
        return NoContent();
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    private QuestionnaireDto EntityToDto(QuestionnaireEntity entity)
    {
        return new QuestionnaireDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Slug = entity.Slug,
            Status = entity.Status,
            Version = entity.Version,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Questions = entity.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Content = q.Content,
                Description = q.Description,
                Type = q.Type,
                Order = q.Order,
                QuestionnaireId = q.QuestionnaireId,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt,
                Answers = q.Answers.Select(a => new AnswerDto
                {
                    Id = a.Id,
                    Content = a.Content,
                    Description = a.Description,
                    DestinationUrl = a.DestinationUrl,
                    DestinationQuestionId = a.DestinationQuestionId,
                    DestinationType = a.DestinationType,
                    Score = a.Score,
                    QuestionId = a.QuestionId,
                    QuestionnaireId = a.QuestionnaireId,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList()
            }).ToList()
        };
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    private async Task RemoveQuestionnaireVersion(Guid id, int versionNumber)
    {
        var version = await db.QuestionnaireVersions
            .FirstOrDefaultAsync(q => q.Id == id && q.Version == versionNumber);

        if (version == null)
            return;
        
        db.QuestionnaireVersions.Remove(version);
        
        await db.SaveChangesAsync();
    }
}