using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// The QuestionnaireRunnerController class provides endpoints for the retrieval data for service users to navigate through a questionnaire.
/// </summary>
/// <remarks>
/// This controller exposes actions to retrieve information about questionnaires, fetch initial questions, and determine the next state based on user-selected answers.
/// </remarks>
[ApiController]
[Route("api")]
[AllowAnonymous]
public class QuestionnaireRunnerController(GetToAnAnswerDbContext db) : Controller
{
    [HttpGet("questionnaires/{questionnaireSlug}/info")]
    public async Task<IActionResult> GetQuestionnaireInfo(string questionnaireSlug)
    {
        var questionnaireVersionJson =  await db.QuestionnaireVersions
            .Where(qv => db.Questionnaires
                .Any(q => q.Id == qv.QuestionnaireId && q.Slug == questionnaireSlug))
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
            Title = questionnaire.Title,
            Description = questionnaire.Description,
            Slug = questionnaire.Slug,
        });
    }
    
    [HttpGet("questionnaires/{questionnaireId}/initial")]
    public async Task<IActionResult> GetInitialQuestion(Guid questionnaireId)
    {
        var initialQuestion = await db.Questions
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.QuestionnaireId == questionnaireId && x.Order == 1);
        
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

    [HttpPost("questionnaires/{questionnaireId}/next")]
    public async Task<IActionResult> GetNextState(Guid questionnaireId, GetNextStateRequest request)
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
    private async Task<IActionResult> GetDestinationQuestion(Expression<Func<QuestionEntity,bool>> destination)
    {
        var questionEntity = await db.Questions.Where(destination)
            .Include(x => x.Answers)
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