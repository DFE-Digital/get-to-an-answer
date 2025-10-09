using System.Linq.Expressions;
using System.Security.Claims;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("/")]
public class WebController(CheckerDbContext db) : Controller
{
    [HttpGet("questionnaires/{questionnaireId}/initial")]
    public async Task<IActionResult> GetInitialQuestion(int questionnaireId)
    {
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        var initialQuestion = await db.Questions
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.QuestionnaireId == questionnaireId && x.Order == 1 && x.TenantId == tenantId);
        
        if (initialQuestion == null)
            return BadRequest();

        return Ok(new QuestionDto
        {
            Id = initialQuestion.Id,
            QuestionnaireId = initialQuestion.QuestionnaireId,
            Content = initialQuestion.Content,
            Description = initialQuestion.Description,
            Order = initialQuestion.Order,
            Status = initialQuestion.Status,
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
    public async Task<IActionResult> GetNextState(int questionnaireId, GetNextStateRequest request)
    {
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID

        var selectedAnswerId = request.SelectedAnswerId;
        
        var answer = await db.Answers.FirstOrDefaultAsync(x => x.Id == selectedAnswerId 
                                                                  && x.TenantId == tenantId);
        
        if (answer == null)
            return BadRequest();

        if (answer.DestinationType == null)
        {
            return await GetDestinationQuestion(x => 
                    x.QuestionnaireId == questionnaireId && 
                    x.Order == request.CurrentQuestionOrder+1 && 
                    x.TenantId == tenantId);
        }

        if (answer.DestinationType == DestinationType.Question)
        {
            return await GetDestinationQuestion(x =>
                x.Id == answer.DestinationQuestionId && x.TenantId == tenantId);
        }

        return Ok(new DestinationDto
        {
            Type = answer.DestinationType,
            Content = answer.Destination
        });
    }

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
                Status = questionEntity.Status,
                Type = questionEntity.Type,
            }
        });
    }
}