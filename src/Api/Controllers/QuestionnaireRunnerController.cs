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
    [HttpGet("questionnaires/{questionnaireSlug}/publishes/last/info")]
    public async Task<IActionResult> GetLastPublishedQuestionnaireInfo(string questionnaireSlug)
    {
        return (await questionnaireRunnerService.GetLastPublishedQuestionnaireInfo(questionnaireSlug)).ToActionResult();
    }
    
    [HttpGet("questionnaires/{questionnaireId:guid}/initial")]
    public async Task<IActionResult> GetInitialQuestion(Guid questionnaireId)
    {
        return (await questionnaireRunnerService.GetInitialQuestion(questionnaireId)).ToActionResult();
    }

    [HttpPost("questionnaires/{questionnaireId:guid}/next")]
    public async Task<IActionResult> GetNextState(Guid questionnaireId, GetNextStateRequest request)
    {
        return (await questionnaireRunnerService.GetNextState(questionnaireId, request)).ToActionResult();
    }
}