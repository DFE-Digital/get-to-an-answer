using System.Security.Claims;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

public class WebController(CheckerDbContext db) : Controller
{
    [HttpGet("questionnaires/{questionnaireId}/initial")]
    public async Task<IActionResult> GetInitialQuestion(int questionnaireId)
    {
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID
        
        var initialQuestion = await db.Questions
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.QuestionnaireId == questionnaireId && x.Order == 0 && x.TenantId == tenantId);
        
        if (initialQuestion == null)
            return BadRequest();

        return Ok(new QuestionDto
        {
            Id = initialQuestion.Id,
            Content = initialQuestion.Content,
            Description = initialQuestion.Description,
            Order = initialQuestion.Order,
            Status = initialQuestion.Status,
            Answers = initialQuestion.Answers.Select(a => new AnswerDto
            {
                Id = a.Id,
                Content = a.Content,
                Description = a.Description,
                QuestionId = a.QuestionId
            }).ToList(),
            Type = initialQuestion.Type
        });
    }

    [HttpPost("questionnaires/{questionnaireId}/next")]
    public async Task<IActionResult> GetNextState(int questionnaireId, GetNextStateRequest request)
    {
        var tenantId = User.FindFirstValue("tid")!; // Tenant ID

        var selectedAnswerId = request.SelectedAnswerId;
        
        var topAnswer = await db.Answers.FirstOrDefaultAsync(x => x.Id == selectedAnswerId 
                                                                  && x.TenantId == tenantId);
        
        if (topAnswer == null)
            return BadRequest();

        if (topAnswer.DestinationType == null)
        {
            var questionEntity = await db.Questions.Where(x => x.QuestionnaireId == questionnaireId && x.Order == request.CurrentQuestionOrder+1 && x.TenantId == tenantId)
                .Include(x => x.Answers)
                .FirstOrDefaultAsync();
            
            if (questionEntity == null)
                return BadRequest();
        
            return Ok(new DestinationDto
            {
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
                        QuestionId = a.QuestionId
                    }).ToList(),
                    Status = questionEntity.Status,
                    Type = questionEntity.Type,
                }
            });
        }
        
        return Ok(new DestinationDto
        {
            Type = topAnswer.DestinationType,
            Content = topAnswer.Description
        });
    }
}