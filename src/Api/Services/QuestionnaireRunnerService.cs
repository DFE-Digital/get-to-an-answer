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
    Task<ServiceResult> GetLastPublishedQuestionnaireInfo(string questionnaireSlug);
    Task<ServiceResult> GetInitialQuestion(Guid questionnaireId);
    Task<ServiceResult> GetNextState(Guid questionnaireId, GetNextStateRequest request);
}

public class QuestionnaireRunnerService(GetToAnAnswerDbContext db, ILogger<QuestionnaireRunnerService> logger) : AbstractService, IQuestionnaireRunnerService
{
    [HttpGet("questionnaires/{questionnaireSlug}/publishes/last/info")]
    public async Task<ServiceResult> GetLastPublishedQuestionnaireInfo(string questionnaireSlug)
    {
        try
        {
            logger.LogInformation("GetLastPublishedQuestionnaireInfo started Slug={Slug}", questionnaireSlug);

            var questionnaireVersionJson =  await db.QuestionnaireVersions
                .Where(qv => db.Questionnaires
                    .Any(q => q.Id == qv.QuestionnaireId && q.Slug == questionnaireSlug && 
                              !new[] {EntityStatus.Deleted, EntityStatus.Private}.Contains(q.Status)))
                .OrderByDescending(qv => qv.Version)
                .Select(qv => qv.QuestionnaireJson)
                .FirstOrDefaultAsync();

            if (questionnaireVersionJson == null)
                return BadRequest(ProblemTrace("No published version found for this questionnaire.", 400));

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };
            options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));

            var questionnaire = JsonSerializer.Deserialize<QuestionnaireEntity>(questionnaireVersionJson, options);

            if (questionnaire == null)
                return BadRequest(ProblemTrace("Failed to read questionnaire meta.", 400));

            logger.LogInformation("GetLastPublishedQuestionnaireInfo succeeded Slug={Slug} QuestionnaireId={QuestionnaireId}", questionnaireSlug, questionnaire.Id);
            return Ok(new QuestionnaireInfoDto
            {
                Id = questionnaire.Id,
                DisplayTitle = questionnaire.DisplayTitle,
                Description = questionnaire.Description,
                Slug = questionnaireSlug,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetLastPublishedQuestionnaireInfo failed Slug={Slug}", questionnaireSlug);
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }

    public async Task<ServiceResult> GetInitialQuestion(Guid questionnaireId)
    {
        try
        {
            logger.LogInformation("GetInitialQuestion started QuestionnaireId={QuestionnaireId}", questionnaireId);

            var initialQuestion = await db.Questions
                .Include(x => x.Answers.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync(x => x.QuestionnaireId == questionnaireId && x.Order == 1 && 
                                          !x.IsDeleted);

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
                    Score = a.Score
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

    public async Task<ServiceResult> GetNextState(Guid questionnaireId, GetNextStateRequest request)
    {
        try
        {
            logger.LogInformation("GetNextState started QuestionnaireId={QuestionnaireId}", questionnaireId);

            if (request.SelectedAnswerIds.Count == 0)
            {
                return BadRequest(ProblemTrace("No answer was selected.", 400));           
            }
            
            var selectedAnswerId = request.SelectedAnswerIds.First();

            var answer = await db.Answers.FirstOrDefaultAsync(x => x.Id == selectedAnswerId);

            if (answer == null)
                return BadRequest(ProblemTrace("The selected answer was not found.", 400));

            if (answer.DestinationType == null)
            {
                return await GetDestinationQuestion(x => 
                        x.QuestionnaireId == questionnaireId && 
                        x.Order == request.CurrentQuestionOrder+1);
            }

            if (answer.DestinationType == DestinationType.Question)
            {
                return await GetDestinationQuestion(x =>
                    x.Id == answer.DestinationQuestionId);
            }

            if (answer.DestinationType == DestinationType.CustomContent)
            {
                var content = db.Contents.First();
                
                return await GetDestinationContent(x =>
                    x.Id == answer.DestinationContentId);
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
    private async Task<ServiceResult> GetDestinationQuestion(Expression<Func<QuestionEntity,bool>> destination)
    {
        try
        {
            var questionEntity = await db.Questions.Where(destination)
                .Include(x => x.Answers.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync();

            if (questionEntity == null)
                return BadRequest(ProblemTrace("Next question could not be determined.", 400));

            return Ok(new DestinationDto
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
                        Score = a.Score,
                    }).ToList(),
                    Type = questionEntity.Type,
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetDestinationQuestion failed");
            return Problem(ProblemTrace("Something went wrong. Try again later.", 500));
        }
    }
    
    private async Task<ServiceResult> GetDestinationContent(Expression<Func<ContentEntity,bool>> destination)
    {
        var questionEntity = await db.Contents.Where(destination)
            .FirstOrDefaultAsync();
            
        if (questionEntity == null)
            return BadRequest();

        return Ok(new DestinationDto
        {
            Type = DestinationType.CustomContent,
            Content = questionEntity.Content,
            Title = questionEntity.Title,
        });
    }
}