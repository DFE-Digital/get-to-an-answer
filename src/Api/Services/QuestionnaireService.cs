using System.Collections.ObjectModel;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public interface IQuestionnaireService
{
    Task<ServiceResult> CreateQuestionnaire(string email, CreateQuestionnaireRequestDto request);

    ServiceResult GetQuestionnaire(string email, Guid id);

    ServiceResult GetQuestionnaires(string email);

    Task<ServiceResult> UpdateQuestionnaire(string email, Guid id, UpdateQuestionnaireRequestDto request);
    Task<ServiceResult> PublishQuestionnaire(string email, Guid id);
    Task<ServiceResult> UnpublishQuestionnaire(string email, Guid id);
    Task<ServiceResult> DeleteQuestionnaire(string email, Guid id);
    Task<ServiceResult> AddSelfToQuestionnaireContributors(string email, Guid id);
    Task<ServiceResult> CloneQuestionnaire(string email, Guid id, CloneQuestionnaireRequestDto request);
    Task<ServiceResult> GetContributors(string email, Guid questionnaireId);
    Task<ServiceResult> AddContributor(string email, Guid id, AddContributorRequestDto request);
    Task<ServiceResult> RemoveContributor(string email, Guid id, string contributorEmail);
}

public class QuestionnaireService(GetToAnAnswerDbContext db) : AbstractService, IQuestionnaireService
{
    [HttpPost("questionnaires")]
    public async Task<ServiceResult> CreateQuestionnaire(string email, CreateQuestionnaireRequestDto request)
    {
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
            UpdatedAt = entity.CreatedAt,
        };
        
        return Created(dto);
    }

    public ServiceResult GetQuestionnaire(string email, Guid id)
    {
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

    public ServiceResult GetQuestionnaires(string email)
    {
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

    public async Task<ServiceResult> UpdateQuestionnaire(string email, Guid id, UpdateQuestionnaireRequestDto request)
    {
        if (request.DisplayTitle == null && request.Title == null && request.Slug == null && request.Description == null)
        {
            return BadRequest("No fields to update");
        }
        
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

    public async Task<ServiceResult> PublishQuestionnaire(string email, Guid id)
    {
        // check if the question answer branching is cycling back to a question before
        
        var questionnaire = await db.Questionnaires
            .AsNoTracking()    
            .Where(q => q.Id == id)
            .Include(q => q.Questions.Where(a => !a.IsDeleted))
            .ThenInclude(qq => qq.Answers.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync();

        if (questionnaire == null)
            return NotFound();

        var branchingHealth = IsBranchingHealthy(questionnaire);

        if (branchingHealth != BranchingHealthType.Ok)
        {
            return BadRequest("Branching is not healthy");       
        }
        
        return await UpdateQuestionnaireStatus(email, id, EntityStatus.Published);
    }

    internal static BranchingHealthType IsBranchingHealthy(QuestionnaireEntity current)
    {
        var questionMap = current.Questions.ToDictionary(q => q.Id, q => q);
        
        foreach (var question in current.Questions)
        {
            var branchingHealth = IsBranchingHealthy(question, new Dictionary<Guid, bool>(), questionMap);
            
            if (branchingHealth != BranchingHealthType.Ok)
                return branchingHealth;
        }
        
        return BranchingHealthType.Ok;
    }

    // make visible for unit testing
    
    internal static BranchingHealthType IsBranchingHealthy(
        QuestionEntity current, 
        Dictionary<Guid, bool> visited, 
        Dictionary<Guid, QuestionEntity> questionMap)
    {
        if (visited.ContainsKey(current.Id))
        {
            return BranchingHealthType.Cyclic;
        }
        
        var visitedList = new Dictionary<Guid, bool>(visited) { { current.Id, true } };

        foreach (var answer in current.Answers)
        {
            if (answer is { DestinationType: DestinationType.Question, DestinationQuestionId: { } id })
            {
                var type = IsBranchingHealthy(questionMap[id], 
                    new Dictionary<Guid, bool>(visitedList), questionMap);
                
                if (type != BranchingHealthType.Ok)
                    return type;
            }
            else if (answer.DestinationType != DestinationType.ExternalLink && current.Order == questionMap.Count)
            {
                // if is the last question and the answer has not destination,
                // then it's a broken branching
                return BranchingHealthType.Broken; 
            }    
        }

        return BranchingHealthType.Ok;
    }

    public async Task<ServiceResult> UnpublishQuestionnaire(string email, Guid id)
    {
        return await UpdateQuestionnaireStatus(email, id, EntityStatus.Draft);
    }

    public async Task<ServiceResult> DeleteQuestionnaire(string email, Guid id)
    {
        // soft delete questions under the questionnaire
        await db.Questions.Where(q => q.QuestionnaireId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.IsDeleted, true));
        
        // soft delete answers under the questionnaire
        await db.Answers.Where(q => q.QuestionnaireId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.IsDeleted, true));
        
        return await UpdateQuestionnaireStatus(email, id, EntityStatus.Deleted);
    }

    public async Task<ServiceResult> AddSelfToQuestionnaireContributors(string email, Guid id)
    {
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

    public async Task<ServiceResult> CloneQuestionnaire(string email, Guid id, CloneQuestionnaireRequestDto request)
    {
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();
        
        var questionnaire = await db.Questionnaires
            .AsNoTracking()
            .Where(q => q.Id == id && q.Status != EntityStatus.Deleted)
            .Include(q => q.Questions.Where(a => !a.IsDeleted))
            .ThenInclude(qq => qq.Answers.Where(a => !a.IsDeleted))
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
        
        return Created(EntityToDto(cloneQuestionnaire));
    }

    public async Task<ServiceResult> GetContributors(string email, Guid questionnaireId)
    {
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

    public async Task<ServiceResult> AddContributor(string email, Guid id, AddContributorRequestDto request)
    {
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

    public async Task<ServiceResult> RemoveContributor(string email, Guid id, string contributorEmail)
    {
        var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
        
        if (access == EntityAccess.NotFound)
            return NotFound();
        
        if (access == EntityAccess.Deny)
            return Forbid();

        var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => x.Id == id);

        if (questionnaire == null)
            return NotFound();

        if (questionnaire.Contributors.Count > 2 && questionnaire.Contributors.Contains(contributorEmail))
        {
            questionnaire.Contributors.Remove(contributorEmail);
            await db.SaveChangesAsync();
        }

        return Ok();
    }

    private async Task StoreQuestionnaireVersion(Guid id, int versionNumber)
    {
        var questionnaire = await db.Questionnaires
            .AsNoTracking()
            .Include(q => q.Questions.Where(a => !a.IsDeleted))
            .ThenInclude(qq => qq.Answers.Where(a => !a.IsDeleted))
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
    
    private async Task<ServiceResult> UpdateQuestionnaireStatus(string email, Guid id, EntityStatus status)
    {
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