using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Extensions;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Common.Local;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
public class BranchingMapController(GetToAnAnswerDbContext db) : ControllerBase
{
    [HttpGet("questionnaires/{questionnaireId}/branching-map")]
    public async Task<IActionResult> GetBranchingMap(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        if (!await db.HasAccessToEntity<QuestionnaireEntity>(email, questionnaireId))
            return Unauthorized();

        var questionnaire = await db.Questionnaires
            .AsNoTracking()    
            .Where(q => q.Id == questionnaireId)
            .Include(q => q.Questions.Where(a => !a.IsDeleted))
            .ThenInclude(qq => qq.Answers.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync();

        if (questionnaire == null)
            return NotFound();

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
}
