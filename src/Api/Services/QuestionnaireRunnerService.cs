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

namespace Api.Services;

public interface IQuestionnaireRunnerService
{
    Task<ServiceResult> GetLastPublishedQuestionnaireInfo(string questionnaireSlug);
    Task<ServiceResult> GetInitialQuestion(Guid questionnaireId);
    Task<ServiceResult> GetNextState(Guid questionnaireId, GetNextStateRequest request);
}

public class QuestionnaireRunnerService(GetToAnAnswerDbContext db) : AbstractService, IQuestionnaireRunnerService
{
    [HttpGet("questionnaires/{questionnaireSlug}/publishes/last/info")]
    public async Task<ServiceResult> GetLastPublishedQuestionnaireInfo(string questionnaireSlug)
    {
        var questionnaireVersionJson =  await db.QuestionnaireVersions
            .Where(qv => db.Questionnaires
                .Any(q => q.Id == qv.QuestionnaireId && q.Slug == questionnaireSlug && 
                          q.Status != EntityStatus.Deleted))
            .OrderByDescending(qv => qv.Version)
            .Select(qv => qv.QuestionnaireJson)
            .FirstOrDefaultAsync();
        // Not published version exists
        if (questionnaireVersionJson == null)
            return BadRequest();
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        
        var questionnaire = JsonSerializer.Deserialize<QuestionnaireEntity>(questionnaireVersionJson, options);
        
        if (questionnaire == null)
            return BadRequest();
        
        return Ok(new QuestionnaireInfoDto
        {
            Id = questionnaire.Id,
            DisplayTitle = questionnaire.DisplayTitle,
            Description = questionnaire.Description,
            Slug = questionnaire.Slug,
        });
    }

    public async Task<ServiceResult> GetInitialQuestion(Guid questionnaireId)
    {
        var initialQuestion = await db.Questions
            .Include(x => x.Answers.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(x => x.QuestionnaireId == questionnaireId && x.Order == 1 && 
                                      !x.IsDeleted);
        
        if (initialQuestion == null)
            return BadRequest();

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

    public async Task<ServiceResult> GetNextState(Guid questionnaireId, GetNextStateRequest request)
    {
        var selectedAnswerId = request.SelectedAnswerId;
        
        var answer = await db.Answers.FirstOrDefaultAsync(x => x.Id == selectedAnswerId);
        
        if (answer == null)
            return BadRequest();

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

        return Ok(new DestinationDto
        {
            Type = answer.DestinationType,
            Content = answer.DestinationUrl
        });
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    private async Task<ServiceResult> GetDestinationQuestion(Expression<Func<QuestionEntity,bool>> destination)
    {
        var questionEntity = await db.Questions.Where(destination)
            .Include(x => x.Answers.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync();
            
        if (questionEntity == null)
            return BadRequest();
        
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
}