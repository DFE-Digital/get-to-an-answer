using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Extensions;
using Api.Services;
using Common.Domain;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// The QuestionnaireVersionController class provides API endpoints for managing and retrieving versions of questionnaires.
/// It includes functionality to fetch the latest questionnaire version, fetch a specific version by version number,
/// and retrieve a list of all versions for a particular questionnaire.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public class QuestionnaireVersionController(IQuestionnaireVersionService questionnaireVersionService) : ControllerBase
{
    
   [HttpGet("questionnaires/{questionnaireId:guid}/versions")]
    public async Task<IActionResult> GetQuestionnaireVersions(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await questionnaireVersionService.GetQuestionnaireVersions(email, questionnaireId)).ToActionResult();
    }
}