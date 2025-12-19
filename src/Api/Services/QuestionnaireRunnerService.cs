using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Services;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Services;

public interface IQuestionnaireRunnerService
{
    Task<ServiceResult> GetLastPublishedQuestionnaireInfo(string questionnaireIdOrSlug, bool isPreview = false);
    Task<ServiceResult> GetInitialQuestion(Guid questionnaireId, bool isPreview = false);
    Task<ServiceResult> GetNextState(Guid questionnaireId, GetNextStateRequest request, bool isPreview = false);
}

public class QuestionnaireRunnerService(GetToAnAnswerDbContext db, ILogger<QuestionnaireRunnerService> logger) : AbstractService, IQuestionnaireRunnerService
{
    public async Task<ServiceResult> GetLastPublishedQuestionnaireInfo(string questionnaireIdOrSlug, bool isPreview = false)
    {
        try
        {
            logger.LogInformation("GetLastPublishedQuestionnaireInfo started SlugOrId={Slug}", questionnaireIdOrSlug);

            QuestionnaireEntity? questionnaire; 

            if (isPreview && Guid.TryParse(questionnaireIdOrSlug, out var questionnaireId))
            {
                questionnaire = await db.Questionnaires.FirstOrDefaultAsync(q => q.Id == questionnaireId);
                
                if (questionnaire == null)
                    return NotFound(ProblemTrace("Failed to read questionnaire meta.", 404));
            }
            else
            {
                var questionnaireVersionJson =  await GetDraftOrLatestPublishedVersion(questionnaireSlug: questionnaireIdOrSlug);

                if (questionnaireVersionJson == null)
                    return NotFound(ProblemTrace("No published version found for this questionnaire.", 404));

                questionnaire = ToQuestionnaire(questionnaireVersionJson);
            
                if (questionnaire == null)
                    return NotFound(ProblemTrace("Failed to read questionnaire meta.", 404));

                logger.LogInformation("GetLastPublishedQuestionnaireInfo succeeded Slug={Slug} QuestionnaireId={QuestionnaireId}", questionnaireIdOrSlug, questionnaire.Id);
            }
            
            return Ok(new QuestionnaireInfoDto
            {
                Id = questionnaire.Id,
                DisplayTitle = questionnaire.DisplayTitle,
                Description = questionnaire.Description,
                Slug = questionnaire.Slug,
                HasStartPage = !string.IsNullOrWhiteSpace(questionnaire.DisplayTitle),
                
                TextColor = questionnaire.TextColor,
                BackgroundColor = questionnaire.BackgroundColor,
                PrimaryButtonColor = questionnaire.PrimaryButtonColor,
                SecondaryButtonColor = questionnaire.SecondaryButtonColor,
                StateColor = questionnaire.StateColor,
                ErrorMessageColor = questionnaire.ErrorMessageColor,
                DecorativeImage = questionnaire.DecorativeImage,
                ContinueButtonText = questionnaire.ContinueButtonText,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetLastPublishedQuestionnaireInfo failed SlugOrId={Slug}", questionnaireIdOrSlug);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> GetInitialQuestion(Guid questionnaireId, bool isPreview = false)
    {
        try
        {
            logger.LogInformation("GetInitialQuestion started QuestionnaireId={QuestionnaireId}", questionnaireId);

            var isExisting = await db.Questionnaires.AnyAsync(q => q.Id == questionnaireId);
            
            if (!isExisting)
                return NotFound(ProblemTrace("The questionnaire does not exist.", 400));
            
            QuestionEntity? initialQuestion;
            
            if (isPreview)
            {
                initialQuestion = await db.Questions
                    .Include(x => x.Answers.Where(a => !a.IsDeleted))
                    .FirstOrDefaultAsync(x => x.QuestionnaireId == questionnaireId && x.Order == 1 && 
                                              !x.IsDeleted);
            }
            else
            {
                var questionnaireVersionJson = await GetDraftOrLatestPublishedVersion(questionnaireId);

                if (questionnaireVersionJson == null)
                    return BadRequest(ProblemTrace("No published version found for this questionnaire.", 400));
                
                var questionnaire = ToQuestionnaire(questionnaireVersionJson);

                initialQuestion = questionnaire?.Questions.FirstOrDefault(q => q is { Order: 1, IsDeleted: false });
            }

            if (initialQuestion == null)
                return BadRequest(ProblemTrace("The questionnaire does not have a starting question.", 400));

            logger.LogInformation("GetInitialQuestion succeeded QuestionnaireId={QuestionnaireId} QuestionId={QuestionId}", questionnaireId, initialQuestion.Id);
            return Ok(new QuestionDto
            {
                Id = initialQuestion.Id,
                QuestionnaireId = initialQuestion.QuestionnaireId,
                Content = initialQuestion.Content,
                Description = initialQuestion.Description,
                Order = initialQuestion.Order,
                Answers = initialQuestion.Answers.Select(a => new AnswerDto
                {
                    Id = a.Id,
                    Content = a.Content,
                    Description = a.Description,
                    QuestionId = a.QuestionId,
                    Priority = a.Priority
                }).ToList(),
                Type = initialQuestion.Type
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetInitialQuestion failed QuestionnaireId={QuestionnaireId}", questionnaireId);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> GetNextState(Guid questionnaireId, GetNextStateRequest request, bool isPreview = false)
    {
        try
        {
            logger.LogInformation("GetNextState started QuestionnaireId={QuestionnaireId}", questionnaireId);

            var isExisting = await db.Questionnaires.AnyAsync(q => q.Id == questionnaireId);
            
            if (!isExisting)
                return NotFound(ProblemTrace("The questionnaire does not exist.", 400));
            
            if (request.SelectedAnswerIds.Count == 0)
            {
                return BadRequest(ProblemTrace("No answer was selected.", 400));           
            }

            var selectedAnswerId = request.SelectedAnswerIds.First();

            QuestionnaireEntity? questionnaire = null;
            AnswerEntity? answer;

            if (isPreview)
            {
                answer = await db.Answers.FirstOrDefaultAsync(x => x.Id == selectedAnswerId);
            }
            else
            {
                var questionnaireVersionJson = isPreview ? null : await GetDraftOrLatestPublishedVersion(questionnaireId);
                
                if (questionnaireVersionJson == null)
                    return BadRequest(ProblemTrace("No published version found for this questionnaire.", 400));
                
                questionnaire = ToQuestionnaire(questionnaireVersionJson);
                
                answer = questionnaire?.Questions.SelectMany(q => q.Answers).FirstOrDefault(a => a.Id == selectedAnswerId);
            }

            if (answer == null)
                return BadRequest(ProblemTrace("The selected answer was not found.", 400));
            
            if (answer.DestinationType == null)
            {
                return await GetDestinationQuestion(x => 
                    x.QuestionnaireId == questionnaireId && 
                    x.Order == request.CurrentQuestionOrder+1 && !x.IsDeleted, isPreview, questionnaire?.Questions);
            }

            if (answer.DestinationType == DestinationType.Question)
            {
                return await GetDestinationQuestion(x =>
                    x.Id == answer.DestinationQuestionId && !x.IsDeleted, isPreview, questionnaire?.Questions);
            }

            if (answer.DestinationType == DestinationType.CustomContent)
            {
                return await GetDestinationContent(x =>
                    x.Id == answer.DestinationContentId && !x.IsDeleted, isPreview, questionnaire?.Contents);
            }

            logger.LogInformation("GetNextState resolved to external destination for AnswerId={AnswerId}", selectedAnswerId);
            return Ok(new DestinationDto
            {
                Type = answer.DestinationType,
                Content = answer.DestinationUrl
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetNextState failed QuestionnaireId={QuestionnaireId}", questionnaireId);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    private async Task<ServiceResult> GetDestinationQuestion(Expression<Func<QuestionEntity,bool>> destination, bool isPreview = false, ICollection<QuestionEntity>? questions = null)
    {
        try
        {
            QuestionEntity? questionEntity = null;

            if (isPreview)
            {
                questionEntity = await db.Questions.Where(destination)
                    .Include(x => x.Answers.Where(a => !a.IsDeleted))
                    .FirstOrDefaultAsync();
                
            }
            else if (questions is { Count: > 0 })
            {
                questionEntity = questions.Where(destination.Compile())
                    .FirstOrDefault();
            }

            if (questionEntity == null)
                return BadRequest(ProblemTrace("Next question could not be determined.", 400));

            return Ok(ToDestinationDto(questionEntity));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetDestinationQuestion failed");
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    private DestinationDto? ToDestinationDto(QuestionEntity questionEntity)
    {
        return new DestinationDto
        {
            Type = DestinationType.Question,
            Question = new QuestionDto
            {
                Id = questionEntity.Id,
                Content = questionEntity.Content,
                Description = questionEntity.Description,
                Order = questionEntity.Order,
                Answers = questionEntity.Answers.Select(a => new AnswerDto
                {
                    Id = a.Id,
                    Content = a.Content,
                    Description = a.Description,
                    QuestionId = a.QuestionId,
                    Priority = a.Priority,
                }).ToList(),
                Type = questionEntity.Type,
            }
        };
    }
    
    private async Task<ServiceResult> GetDestinationContent(Expression<Func<ContentEntity,bool>> destination, bool isPreview = false, ICollection<ContentEntity>? contents = null)
    {
        ContentEntity? contentEntity = null;

        if (isPreview)
        {
            contentEntity = await db.Contents.Where(destination)
                .FirstOrDefaultAsync();
        }
        else if (contents is { Count: > 0 })
        {
            contentEntity = contents.Where(destination.Compile())
                .FirstOrDefault();
        }
        
        if (contentEntity == null)
            return BadRequest();

        return Ok(new DestinationDto
        {
            Type = DestinationType.CustomContent,
            Content = contentEntity.Content,
            Title = contentEntity.Title,
        });
    }

    private async Task<string?> GetDraftOrLatestPublishedVersion(Guid? questionnaireId = null, string? questionnaireSlug = null)
    {
        var questionnaireVersionJson =  await db.QuestionnaireVersions
            .Where(qv => db.Questionnaires
                .Any(q => (q.Id == qv.QuestionnaireId && (questionnaireId == null || q.Id == questionnaireId)) &&
                          (questionnaireSlug == null || q.Slug == questionnaireSlug) &&
                          q.Status != EntityStatus.Deleted && !q.IsUnpublished))
            .OrderByDescending(qv => qv.Version)
            .Select(qv => qv.QuestionnaireJson)
            .FirstOrDefaultAsync();
        
        return questionnaireVersionJson;
    }

    private QuestionnaireEntity? ToQuestionnaire(string questionnaireJson)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));

        return JsonSerializer.Deserialize<QuestionnaireEntity>(questionnaireJson, options);
    }
}