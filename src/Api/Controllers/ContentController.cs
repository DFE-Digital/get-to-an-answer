using System.Security.Claims;
using Api.Extensions;
using Api.Services;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Extensions;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ContentController(IContentService contentService) : Controller
{
    [HttpPost("contents")]
    public async Task<IActionResult> CreateContent(CreateContentRequestDto request)
    {
        var userId = User.GetIdClaim()!;

        return (await contentService.CreateContent(userId, request)).ToActionResult();
    }
    
    [HttpGet("contents/{id:guid}")]
    public IActionResult GetContent(Guid id)
    {
        var userId = User.GetIdClaim()!;

        return contentService.GetContent(userId, id).ToActionResult();
    }

    [HttpGet("questionnaires/{questionnaireId:guid}/contents")]
    public IActionResult GetContents(Guid questionnaireId)
    {
        var userId = User.GetIdClaim()!;

        return contentService.GetContents(userId, questionnaireId).ToActionResult();
    }

    [HttpPut("contents/{id:guid}")]
    public async Task<IActionResult> UpdateContent(Guid id, UpdateContentRequestDto request)
    {
        var userId = User.GetIdClaim()!;

        return (await contentService.UpdateContent(userId, id, request)).ToActionResult();
    }

    [HttpDelete("contents/{id:guid}")]
    public async Task<IActionResult> DeleteContent(Guid id)
    {
        var userId = User.GetIdClaim()!;

        return (await contentService.DeleteContent(userId, id)).ToActionResult();
    }
}