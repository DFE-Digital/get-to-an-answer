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
    Task<ServiceResult> CreateQuestionnaire(string userId, CreateQuestionnaireRequestDto request);

    ServiceResult GetQuestionnaire(string userId, Guid id);

    Task<ServiceResult> GetQuestionnaires(string userId);

    Task<ServiceResult> UpdateQuestionnaire(string userId, Guid id, UpdateQuestionnaireRequestDto request);
    Task<ServiceResult> UpdateQuestionnaireLookAndFeel(string userId, Guid id, UpdateLookAndFeelRequestDto request);
    Task<ServiceResult> UpdateQuestionnaireContinueButton(string userId, Guid id,
        UpdateContinueButtonRequestDto request);
    Task<ServiceResult> DeleteQuestionnaireDecorativeImage(string userId, Guid id);
    Task<ServiceResult> PublishQuestionnaire(string userId, Guid id);
    Task<ServiceResult> UnpublishQuestionnaire(string userId, Guid id);
    Task<ServiceResult> DeleteQuestionnaire(string userId, Guid id);
    Task<ServiceResult> CloneQuestionnaire(string userId, Guid id, CloneQuestionnaireRequestDto request);
    Task<ServiceResult> GetContributors(string userId, Guid questionnaireId);
    Task<ServiceResult> AddContributor(string userId, Guid id, AddContributorRequestDto request);
    Task<ServiceResult> RemoveContributor(string userId, Guid id, string contributorId);
    Task<ServiceResult> GetBranchingMap(string userId, Guid questionnaireId);
    Task<ServiceResult> UpdateCompletionState(string userId, Guid id, UpdateCompletionStateRequestDto request);
}

public class QuestionnaireService(GetToAnAnswerDbContext db, ILogger<QuestionnaireService> logger) : AbstractService, IQuestionnaireService
{
    [HttpPost("questionnaires")]
    public async Task<ServiceResult> CreateQuestionnaire(string userId, CreateQuestionnaireRequestDto request)
    {
        try
        {
            logger.LogInformation("CreateQuestionnaire started Title={Title}", request.Title);

            var entity = new QuestionnaireEntity
            {
                Title = request.Title,
                Contributors = [userId],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            
            db.Questionnaires.Add(entity);
            await db.SaveChangesAsync();
            
            var dto = new QuestionnaireDto
            {
                Id = entity.Id,
                Title = entity.Title,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy
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

    public ServiceResult GetQuestionnaire(string userId, Guid id)
    {
        try
        {
            logger.LogInformation("GetQuestionnaire started QuestionnaireId={QuestionnaireId}", id);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var questionnaire = db.Questionnaires
                .FirstOrDefault(q => q.Id == id && q.Status != EntityStatus.Deleted);

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            
            return Ok(ToDto(questionnaire, includeCustomisations: true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> GetQuestionnaires(string userId)
    {
        try
        {
            logger.LogInformation("GetQuestionnaires started");

            var questionnairesQuery = db.Questionnaires
                .Where(q => q.Contributors.Contains(userId)
                            && q.Status != EntityStatus.Deleted);

            var questionnaires = await questionnairesQuery.ToListAsync();

            logger.LogInformation("GetQuestionnaires succeeded Count={Count}", questionnaires.Count);
            return Ok(questionnaires.Select(q => ToDto(q, true)).ToList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetQuestionnaires failed");
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> UpdateQuestionnaire(string userId, Guid id, UpdateQuestionnaireRequestDto request)
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
            
            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));
            
            var questionnaire = await db.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == id);
            
            if (questionnaire == null) 
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            if (request.Slug != null && await db.Questionnaires.AnyAsync(q => q.Id != id && q.Slug == request.Slug))
            {
                return Conflict(ProblemTrace("A questionnaire with the same slug already exists", 409));
            }

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

    public async Task<ServiceResult> UpdateQuestionnaireLookAndFeel(string userId, Guid id, UpdateLookAndFeelRequestDto request)
    {
        try
        {
            logger.LogInformation("UpdateQuestionnaireLookAndFeel started QuestionnaireId={QuestionnaireId}", id);

            if (request.TextColor == null && request.BackgroundColor == null && request.PrimaryButtonColor == null && 
                request.SecondaryButtonColor == null && request.StateColor == null && request.ErrorMessageColor == null)
            {
                return BadRequest(ValidationProblemTrace(new Dictionary<string, string[]>
                {
                    ["request"] = ["No fields to update."]
                }));
            }
            
            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));
            
            var questionnaire = await db.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == id);
            
            if (questionnaire == null) 
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            questionnaire.TextColor = request.TextColor ?? questionnaire.TextColor;
            questionnaire.BackgroundColor = request.BackgroundColor ?? questionnaire.BackgroundColor;
            questionnaire.PrimaryButtonColor = request.PrimaryButtonColor ?? questionnaire.PrimaryButtonColor;
            questionnaire.SecondaryButtonColor = request.SecondaryButtonColor ?? questionnaire.SecondaryButtonColor;
            questionnaire.StateColor = request.StateColor ?? questionnaire.StateColor;
            questionnaire.ErrorMessageColor = request.ErrorMessageColor ?? questionnaire.ErrorMessageColor;
            questionnaire.DecorativeImage = request.DecorativeImage ?? questionnaire.DecorativeImage;
            questionnaire.IsAccessibilityAgreementAccepted = request.IsAccessibilityAgreementAccepted;
            questionnaire.Status = EntityStatus.Draft;
            questionnaire.UpdatedAt = DateTime.UtcNow;
            
            await db.SaveChangesAsync();
            
            logger.LogInformation("UpdateQuestionnaireLookAndFeel succeeded QuestionnaireId={QuestionnaireId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateQuestionnaireLookAndFeel failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }
    
    public async Task<ServiceResult> UpdateQuestionnaireContinueButton(string userId, Guid id, UpdateContinueButtonRequestDto request)
    {
        try
        {
            logger.LogInformation("UpdateQuestionnaireContinueButton started QuestionnaireId={QuestionnaireId}", id);
            
            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));
            
            var questionnaire = await db.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == id);
            
            if (questionnaire == null) 
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            questionnaire.ContinueButtonText = request.ContinueButtonText ?? questionnaire.ContinueButtonText;
            questionnaire.Status = EntityStatus.Draft;
            questionnaire.UpdatedAt = DateTime.UtcNow;
            
            await db.SaveChangesAsync();
            
            logger.LogInformation("UpdateQuestionnaireContinueButton succeeded QuestionnaireId={QuestionnaireId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateQuestionnaireContinueButton failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> DeleteQuestionnaireDecorativeImage(string userId, Guid id)
    {
        try
        {
            logger.LogInformation("UpdateQuestionnaireDecorativeImage started QuestionnaireId={QuestionnaireId}", id);
            
            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));
            
            var questionnaire = await db.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == id);
            
            if (questionnaire == null) 
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            questionnaire.DecorativeImage = null;
            questionnaire.Status = EntityStatus.Draft;
            questionnaire.UpdatedAt = DateTime.UtcNow;
            
            await db.SaveChangesAsync();
            
            logger.LogInformation("UpdateQuestionnaireDecorativeImage succeeded QuestionnaireId={QuestionnaireId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateQuestionnaireDecorativeImage failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> PublishQuestionnaire(string userId, Guid id)
    {
        try
        {
            logger.LogInformation("PublishQuestionnaire started QuestionnaireId={QuestionnaireId}", id);

            var questionnaire = await db.Questionnaires
                .AsNoTracking()    
                .Where(q => q.Id == id)
                .Include(q => q.Contents.Where(a => !a.IsDeleted))
                .Include(q => q.Questions.Where(a => !a.IsDeleted))
                .ThenInclude(qq => qq.Answers.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync();

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            
            if (questionnaire.Questions.Count == 0)
                return BadRequest(ProblemTrace("The questionnaire has no questions", 400));

            foreach (var question in questionnaire.Questions)
            {
                if (question.Answers.Count == 0)
                    return BadRequest(ProblemTrace($"Question {question.Order} has no answers", 400));
            }
            
            if (questionnaire.Status == EntityStatus.Published)
                return BadRequest(ProblemTrace("The questionnaire is already published", 400));

            var (branchingHealth, healthMessage) = IsBranchingHealthy(questionnaire);

            if (branchingHealth != BranchingHealthType.Ok)
            {
                return BadRequest(ProblemTrace(healthMessage, 400));
            }
            
            return await UpdateQuestionnaireStatus(userId, id, EntityStatus.Published);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PublishQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    internal static (BranchingHealthType BranchingHealth, string HealthMessage) IsBranchingHealthy(QuestionnaireEntity current)
    {
        var questionMap = current.Questions.ToDictionary(q => q.Id, q => q);
        var contentMap = current.Contents.ToDictionary(q => q.Id, q => q);

        foreach (var question in current.Questions)
        {
            var (branchingHealth, healthMessage) = IsBranchingHealthy(question, new Dictionary<Guid, bool>(), questionMap, contentMap);
            
            if (branchingHealth != BranchingHealthType.Ok)
                return (branchingHealth, healthMessage);
        }
        
        return (BranchingHealthType.Ok, "Ok");
    }

    // make visible for unit testing
    
    internal static (BranchingHealthType BranchingHealth, string HealthMessage) IsBranchingHealthy(
        QuestionEntity current, 
        Dictionary<Guid, bool> visited, 
        Dictionary<Guid, QuestionEntity> questionMap,
        Dictionary<Guid, ContentEntity> contentMap)
    {
        if (visited.ContainsKey(current.Id))
        {
            return (BranchingHealthType.Cyclic, $"Question {current.Order} was be referenced twice in the same flow.");
        }
        
        var visitedList = new Dictionary<Guid, bool>(visited) { { current.Id, true } };

        foreach (var answer in current.Answers)
        {
            if (answer is { DestinationType: DestinationType.Question, DestinationQuestionId: { } id })
            {
                var (type, message) = IsBranchingHealthy(questionMap[id], 
                    new Dictionary<Guid, bool>(visitedList), questionMap, contentMap);
                
                if (type != BranchingHealthType.Ok)
                    return (type, message);
            }
            else if (answer is { DestinationType: DestinationType.CustomContent, DestinationContentId: not null } && 
                         contentMap.ContainsKey(answer.DestinationContentId.Value))
            {
                // Continue
            }  
            else if (answer.DestinationType != DestinationType.ExternalLink && current.Order == questionMap.Count)
            {
                return (BranchingHealthType.Broken, 
                    $"Answer '{answer.Content}' of the last question should have an external link or results page."); 
            }    
        }

        return (BranchingHealthType.Ok, "Ok");
    }

    public async Task<ServiceResult> UnpublishQuestionnaire(string userId, Guid id)
    {
        try
        {
            logger.LogInformation("UnpublishQuestionnaire started QuestionnaireId={QuestionnaireId}", id);
            return await UpdateQuestionnaireStatus(userId, id, EntityStatus.Private);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UnpublishQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> DeleteQuestionnaire(string userId, Guid id)
    {
        try
        {
            logger.LogInformation("DeleteQuestionnaire started QuestionnaireId={QuestionnaireId}", id);

            await db.Questions.Where(q => q.QuestionnaireId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsDeleted, true));
            
            await db.Answers.Where(q => q.QuestionnaireId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsDeleted, true));
            
            await db.Contents.Where(q => q.QuestionnaireId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsDeleted, true));
            
            return await UpdateQuestionnaireStatus(userId, id, EntityStatus.Deleted, entity =>
            {
                // free up the unique slug for use by a new questionnaire
                entity.Slug = entity.Id.ToString();
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> CloneQuestionnaire(string userId, Guid id, CloneQuestionnaireRequestDto request)
    {
        try
        {
            logger.LogInformation("CloneQuestionnaire started QuestionnaireId={QuestionnaireId}", id);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, id);
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
                CreatedBy = userId
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
                    CreatedBy = userId,
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
                        Priority = answer.Priority,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };
                    
                    cloneAnswers.Add(cloneAnswer);
                }
            }
            
            await db.Answers.AddRangeAsync(cloneAnswers);
            await db.SaveChangesAsync();
            
            logger.LogInformation("CloneQuestionnaire succeeded OriginalId={OriginalId} NewId={NewId}", id, cloneQuestionnaireId);
            return Created(ToNestedDto(cloneQuestionnaire));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CloneQuestionnaire failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> GetContributors(string userId, Guid questionnaireId)
    {
        try
        {
            logger.LogInformation("GetContributors started QuestionnaireId={QuestionnaireId}", questionnaireId);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, questionnaireId);
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

    public async Task<ServiceResult> AddContributor(string userId, Guid id, AddContributorRequestDto request)
    {
        try
        {
            logger.LogInformation("AddContributor started QuestionnaireId={QuestionnaireId} Contributor={Contributor}", id, request.Id);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => 
                x.Id == id && x.Status != EntityStatus.Deleted);

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            if (!questionnaire.Contributors.Contains(request.Id))
            {
                questionnaire.Contributors.Add(request.Id);
                await db.SaveChangesAsync();
            }
            else
            {
                return BadRequest(ProblemTrace("The contributor already exists", 409));
            }

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AddContributor failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> RemoveContributor(string userId, Guid id, string contributorId)
    {
        try
        {
            logger.LogInformation("RemoveContributor started QuestionnaireId={QuestionnaireId} ContributorId={ContributorId}", id, contributorId);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));

            var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(x => x.Id == id);

            if (questionnaire == null)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            if (questionnaire.Contributors.Count > 2 && questionnaire.Contributors.Contains(contributorId))
            {
                questionnaire.Contributors.Remove(contributorId);
                await db.SaveChangesAsync();
            }
            else if (!questionnaire.Contributors.Contains(userId))
            {
                return NotFound(ProblemTrace("Cannot find the contributor", 404));
            }
            else if (questionnaire.Contributors.Count <= 2)
            {
                return BadRequest(ProblemTrace("You cannot remove a contributor, when there are less than 3", 400));
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
            ChangeDescription = "Questionnaire Published"
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
    
    private async Task<ServiceResult> UpdateQuestionnaireStatus(string userId, Guid id, EntityStatus status, Action<QuestionnaireEntity>? preUpdateAction = null)
    {
        try
        {
            logger.LogInformation("UpdateQuestionnaireStatus started QuestionnaireId={QuestionnaireId} Status={Status}", id, status);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, id);
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
            
            preUpdateAction?.Invoke(questionnaire);

            questionnaire.UpdatedAt = DateTime.UtcNow;
            questionnaire.PublishedBy = userId;
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
    
    private QuestionnaireDto ToNestedDto(QuestionnaireEntity entity)
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
                    Priority = a.Priority,
                    QuestionId = a.QuestionId,
                    QuestionnaireId = a.QuestionnaireId,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList()
            }).ToList()
        };
    }
    
    public async Task<ServiceResult> GetBranchingMap(string userId, Guid questionnaireId)
    {
        try
        {
            logger.LogInformation("GetBranchingMap started QuestionnaireId={QuestionnaireId}", questionnaireId);

            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, questionnaireId);
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
                .ToDictionaryAsync(c => c.Id, c => c.ReferenceName ?? c.Title);
            
            return Ok(new QuestionnaireBranchingMapDto
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

    public async Task<ServiceResult> UpdateCompletionState(string userId, Guid id, UpdateCompletionStateRequestDto request)
    {
        try
        {
            logger.LogInformation("UpdateCompletionState started QuestionnaireId={QuestionnaireId}", id);
            
            var access = db.HasAccessToEntity<QuestionnaireEntity>(userId, id);
            if (access == EntityAccess.NotFound)
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));
            if (access == EntityAccess.Deny)
                return Forbid(ProblemTrace("You do not have permission to do this", 403));
            
            var questionnaire = await db.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == id);
            
            if (questionnaire == null) 
                return NotFound(ProblemTrace("We could not find that questionnaire", 404));

            questionnaire.CompletionTrackingMap ??= new Dictionary<CompletableTask, CompletionStatus>();
            questionnaire.CompletionTrackingMap[request.Task] = request.Status;
            // Doesn't need to be set to draft: questionnaire.Status = EntityStatus.Draft;
            questionnaire.UpdatedAt = DateTime.UtcNow;
            
            await db.SaveChangesAsync();
            
            logger.LogInformation("UpdateCompletionState succeeded QuestionnaireId={QuestionnaireId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateCompletionState failed QuestionnaireId={QuestionnaireId}", id);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    private QuestionnaireDto ToDto(QuestionnaireEntity q, bool includeCreatedBy = false, bool includeCustomisations = false)
    {
        var dto = new QuestionnaireDto
        {
            Id = q.Id,
            Title = q.Title,
            DisplayTitle = q.DisplayTitle,
            Description = q.Description,
            Slug = q.Slug,
            Status = q.Status,
            Version = q.Version,
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt,
            CreatedBy = includeCreatedBy ? q.CreatedBy : null,
            
            CompletionTrackingMap = !includeCreatedBy ? q.CompletionTrackingMap : new()
        };

        if (includeCustomisations)
        {
            dto.TextColor = q.TextColor;
            dto.BackgroundColor = q.BackgroundColor;
            dto.PrimaryButtonColor = q.PrimaryButtonColor;
            dto.SecondaryButtonColor = q.SecondaryButtonColor;
            dto.StateColor = q.StateColor;
            dto.ErrorMessageColor = q.ErrorMessageColor;
            dto.DecorativeImage = q.DecorativeImage;
            dto.ContinueButtonText = q.ContinueButtonText;
            dto.IsAccessibilityAgreementAccepted = q.IsAccessibilityAgreementAccepted;
        }

        return dto;
    }
}