using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain;
using Common.Domain.Admin;
using Common.Domain.Request.Add;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Extensions;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Common.Local;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    Task<ServiceResult> GetBranchingMap(string email, Guid questionnaireId);
}

public class QuestionnaireService(GetToAnAnswerDbContext db, ILogger<QuestionnaireService> logger) : AbstractService, IQuestionnaireService
{
    [HttpPost("questionnaires")]
    public async Task<ServiceResult> CreateQuestionnaire(string email, CreateQuestionnaireRequestDto request)
    {
        try
        {
            logger.LogInformation("CreateQuestionnaire started Title={Title}", request.Title);

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

            logger.LogInformation("CreateQuestionnaire succeeded QuestionnaireId={QuestionnaireId}", entity.Id);
            return Created(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateQuestionnaire failed");
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public ServiceResult GetQuestionnaire(string email, Guid id)
    {
        try
        {
            logger.LogInformation("GetQuestionnaire started QuestionnaireId={QuestionnaireId}", id);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var questionnaire = db.Questionnaires
                .FirstOrDefault(q => q.Id == id && q.Status != EntityStatus.Deleted);

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            
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
        catch (Exception ex)
        {
            logger.LogError(ex, "GetQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public ServiceResult GetQuestionnaires(string email)
    {
        try
        {
            logger.LogInformation("GetQuestionnaires started");

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

            logger.LogInformation("GetQuestionnaires succeeded Count={Count}", questionnaires.Count);
            return Ok(questionnaires);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetQuestionnaires failed");
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> UpdateQuestionnaire(string email, Guid id, UpdateQuestionnaireRequestDto request)
    {
        try
        {
            logger.LogInformation("UpdateQuestionnaire started QuestionnaireId={QuestionnaireId}", id);

            if (request.DisplayTitle == null && request.Title == null && request.Slug == null && request.Description == null)
            {
                return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                {
                    ["request"] = ["No fields to update."]
                }));
            }
            
            var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));
            
            var questionnaire = await db.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == id);
            
            if (questionnaire == null) 
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            questionnaire.DisplayTitle = request.DisplayTitle ?? questionnaire.DisplayTitle;
            questionnaire.Title = request.Title ?? questionnaire.Title;
            questionnaire.Status = EntityStatus.Draft;
            questionnaire.Slug = request.Slug ?? questionnaire.Slug;
            questionnaire.Description = request.Description ?? questionnaire.Description  ?? string.Empty;
            questionnaire.UpdatedAt = DateTime.UtcNow;
            
            await db.SaveChangesAsync();
            
            logger.LogInformation("UpdateQuestionnaire succeeded QuestionnaireId={QuestionnaireId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> PublishQuestionnaire(string email, Guid id)
    {
        try
        {
            logger.LogInformation("PublishQuestionnaire started QuestionnaireId={QuestionnaireId}", id);

            var questionnaire = await db.Questionnaires
                .AsNoTracking()    
                .Where(q => q.Id == id)
                .Include(q => q.Questions.Where(a => !a.IsDeleted))
                .ThenInclude(qq => qq.Answers.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync();

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            
            if (questionnaire.Questions.Count == 0)
                return BadRequest(ProblemTrace("The questionnaire has no questions", 400));

            if (questionnaire.Status == EntityStatus.Published)
                return BadRequest(ProblemTrace("The questionnaire is already published", 400));

            var branchingHealth = IsBranchingHealthy(questionnaire);

            if (branchingHealth != BranchingHealthType.Ok)
            {
                return BadRequest(ProblemTrace("Branching is not healthy", 400));       
            }
            
            return await UpdateQuestionnaireStatus(email, id, EntityStatus.Published);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PublishQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
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
                return BranchingHealthType.Broken; 
            }    
        }

        return BranchingHealthType.Ok;
    }

    public async Task<ServiceResult> UnpublishQuestionnaire(string email, Guid id)
    {
        try
        {
            logger.LogInformation("UnpublishQuestionnaire started QuestionnaireId={QuestionnaireId}", id);
            return await UpdateQuestionnaireStatus(email, id, EntityStatus.Private);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UnpublishQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> DeleteQuestionnaire(string email, Guid id)
    {
        try
        {
            logger.LogInformation("DeleteQuestionnaire started QuestionnaireId={QuestionnaireId}", id);

            await db.Questions.Where(q => q.QuestionnaireId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsDeleted, true));
            
            await db.Answers.Where(q => q.QuestionnaireId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsDeleted, true));
            
            return await UpdateQuestionnaireStatus(email, id, EntityStatus.Deleted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> AddSelfToQuestionnaireContributors(string email, Guid id)
    {
        try
        {
            logger.LogInformation("AddSelfToQuestionnaireContributors started QuestionnaireId={QuestionnaireId}", id);

            var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => x.Id == id);
            if (questionnaire == null) 
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            if (!questionnaire.Contributors.Contains(email))
            {
                questionnaire.Contributors.Add(email);
                await db.SaveChangesAsync();
            }
            else
            {
                return Conflict(ProblemTrace("You are already a contributor.", 409));       
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AddSelfToQuestionnaireContributors failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> CloneQuestionnaire(string email, Guid id, CloneQuestionnaireRequestDto request)
    {
        try
        {
            logger.LogInformation("CloneQuestionnaire started QuestionnaireId={QuestionnaireId}", id);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));
            
            var questionnaire = await db.Questionnaires
                .AsNoTracking()
                .Where(q => q.Id == id && q.Status != EntityStatus.Deleted)
                .Include(q => q.Questions.Where(a => !a.IsDeleted))
                .ThenInclude(qq => qq.Answers.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync();

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

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
                    CreatedBy = email,
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
                        CreatedBy = email,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };
                    
                    cloneAnswers.Add(cloneAnswer);
                }
            }
            
            await db.Answers.AddRangeAsync(cloneAnswers);
            await db.SaveChangesAsync();
            
            logger.LogInformation("CloneQuestionnaire succeeded OriginalId={OriginalId} NewId={NewId}", id, cloneQuestionnaireId);
            return Created(EntityToDto(cloneQuestionnaire));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CloneQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> GetContributors(string email, Guid questionnaireId)
    {
        try
        {
            logger.LogInformation("GetContributors started QuestionnaireId={QuestionnaireId}", questionnaireId);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => 
                x.Id == questionnaireId && x.Status != EntityStatus.Deleted);

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            return Ok(questionnaire.Contributors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetContributors failed QuestionnaireId={QuestionnaireId}", questionnaireId);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> AddContributor(string email, Guid id, AddContributorRequestDto request)
    {
        try
        {
            logger.LogInformation("AddContributor started QuestionnaireId={QuestionnaireId} Contributor={Contributor}", id, request.Email);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => 
                x.Id == id && x.Status != EntityStatus.Deleted);

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            if (!questionnaire.Contributors.Contains(request.Email))
            {
                questionnaire.Contributors.Add(request.Email);
                await db.SaveChangesAsync();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AddContributor failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> RemoveContributor(string email, Guid id, string contributorEmail)
    {
        try
        {
            logger.LogInformation("RemoveContributor started QuestionnaireId={QuestionnaireId} Contributor={Contributor}", id, contributorEmail);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => x.Id == id);

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            if (questionnaire.Contributors.Count > 2 && questionnaire.Contributors.Contains(contributorEmail))
            {
                questionnaire.Contributors.Remove(contributorEmail);
                await db.SaveChangesAsync();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RemoveContributor failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    private async Task StoreQuestionnaireVersion(Guid id, int versionNumber)
    {
        var questionnaire = await db.Questionnaires
            .AsNoTracking()
            .Include(q => q.Contents.Where(a => !a.IsDeleted))
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
            return;
        }
        
        var previousVersion = await db.QuestionnaireVersions.FirstOrDefaultAsync(v => 
            v.QuestionnaireId == id && v.Version == versionNumber - 1);
        
        if (previousVersion == null)
            return;
        
        var changeMap = VersionDiffRenderer.RenderCompare(previousVersion.QuestionnaireJson, json);

        var newChangeMap = changeMap?.Values.ToJson() ?? string.Empty;

        snapshot.ChangeLog = newChangeMap;
        
        await db.SaveChangesAsync();
    }
    
    private async Task<ServiceResult> UpdateQuestionnaireStatus(string email, Guid id, EntityStatus status)
    {
        try
        {
            logger.LogInformation("UpdateQuestionnaireStatus started QuestionnaireId={QuestionnaireId} Status={Status}", id, status);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(email, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => x.Id == id);

            if (questionnaire == null) 
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            
            var versionNumber = 1;

            questionnaire.Status = status;
            if (status == EntityStatus.Published) 
            {
                versionNumber = ++questionnaire.Version;
            }
            else if (status == EntityStatus.Draft)
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
            
            logger.LogInformation("UpdateQuestionnaireStatus succeeded QuestionnaireId={QuestionnaireId} NewStatus={Status}", id, status);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateQuestionnaireStatus failed QuestionnaireId={QuestionnaireId} Status={Status}", id, status);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
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
    
    public async Task<ServiceResult> GetBranchingMap(string email, Guid questionnaireId)
    {
        try
        {
            logger.LogInformation("GetBranchingMap started QuestionnaireId={QuestionnaireId}", questionnaireId);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var questionnaire = await db.Questionnaires
                .AsNoTracking()    
                .Where(q => q.Id == questionnaireId)
                .Include(q => q.Questions.Where(a => !a.IsDeleted))
                .ThenInclude(qq => qq.Answers.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync();

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            var contentMap = await db.Contents
                .AsNoTracking()
                .Where(x => x.QuestionnaireId == questionnaireId)
                .ToDictionaryAsync(c => c.Id, c => c.Title);
            
            return Ok(new QuestionnaireBranchingMap
            {
                QuestionnaireId = questionnaire.Id,
                QuestionnaireTitle = questionnaire.Title,
                Source = questionnaire.ToMermaidDiagram(contentMap)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetBranchingMap failed QuestionnaireId={QuestionnaireId}", questionnaireId);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }
}