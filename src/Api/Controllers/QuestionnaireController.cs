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
public class QuestionnaireController(GetToAnAnswerDbContext db) : ControllerBase
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
            Slug = request.Slug,
            Contributors = [email],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
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
        questionnaire.Slug = request.Slug;
        questionnaire.Description = request.Description;
        questionnaire.UpdatedAt = DateTime.UtcNow;
        
        db.Entry(questionnaire).Property(s => s.Title).IsModified = true;
        db.Entry(questionnaire).Property(s => s.Slug).IsModified = true;
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
        return await UpdateQuestionnaireStatus(id, new UpdateQuestionnaireStatusRequestDto
        {
            Id = id,
            Status = EntityStatus.Deleted
        });
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
        }
        
        return Ok("Question updated.");
    }

    [HttpPost("questionnaires/{id}/clones")]
    public async Task<IActionResult> CloneQuestionnaire(int id, CloneQuestionnaireRequestDto request)
    {
        var questionnaire = await db.Questionnaires
            .AsNoTracking()
            .Where(q => q.Id == id)
            .Include(q => q.Questions)
            .ThenInclude(qq => qq.Answers)
            .FirstOrDefaultAsync();

        if (questionnaire == null)
            return NotFound();

        var cloneQuestionnaire = new QuestionnaireEntity
        {
            Title = request.Title,
            Description = request.Description,
            Status = EntityStatus.Draft,
            Contributors = questionnaire.Contributors,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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
                Status = EntityStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
                
            cloneQuestions.Add(question.Order, cloneQuestion);
            
            orderAnswers.Add(question.Id, question.Answers);
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
                    Destination = answer.Destination,
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
        
        return Ok(cloneQuestionnaire);
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
