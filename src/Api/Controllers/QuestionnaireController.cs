using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class QuestionnaireController(GetToAnAnswerDbContext db) : ControllerBase
{
    [HttpPost("questionnaires")]
    [Authorize]
    public async Task<IActionResult> CreateQuestionnaire(CreateQuestionnaireRequestDto request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);
        
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var entity = new QuestionnaireEntity
        {
            Title = request.Title,
            Description = request.Description,
            Slug = request.Slug,
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
            Description = entity.Description,
            Slug = entity.Slug
        };
        
        return Created($"api/questionnaires/{dto.Id}", dto);
    }
}