using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Extensions;
using Api.Services;
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
public class QuestionnaireRunnerController(IQuestionnaireRunnerService questionnaireRunnerService) : Controller
{
    [HttpGet("questionnaires/{questionnaireIdOrSlug}/publishes/last/info")]
    public async Task<IActionResult> GetLastPublishedQuestionnaireInfo(string questionnaireIdOrSlug, [FromQuery] bool preview = false)
    {
        return (await questionnaireRunnerService.GetLastPublishedQuestionnaireInfo(questionnaireIdOrSlug, preview)).ToActionResult();
    }
    
    [HttpGet("questionnaires/{questionnaireId:guid}/initial-state")]
    public async Task<IActionResult> GetInitialQuestion(Guid questionnaireId, [FromQuery] bool preview = false)
    {
        return (await questionnaireRunnerService.GetInitialQuestion(questionnaireId, preview)).ToActionResult();
    }

    [HttpPost("questionnaires/{questionnaireId:guid}/next-state")]
    public async Task<IActionResult> GetNextState(Guid questionnaireId, GetNextStateRequest request, [FromQuery] bool preview = false)
    {
        return (await questionnaireRunnerService.GetNextState(questionnaireId, request, preview)).ToActionResult();
    }
}