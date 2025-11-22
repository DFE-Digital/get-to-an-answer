using Api.Extensions;
using Api.Services;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        var userId = User.GetIdClaim()!;

        return (await questionnaireVersionService.GetQuestionnaireVersions(userId, questionnaireId)).ToActionResult();
    }
}