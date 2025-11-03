using System.Security.Claims;
using Api.Extensions;
using Api.Services;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
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
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await contentService.CreateContent(email, request)).ToActionResult();
    }
    
    [HttpGet("contents/{id:guid}")]
    public IActionResult GetContent(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return contentService.GetContent(email, id).ToActionResult();
    }

    [HttpGet("questionnaires/{questionnaireId:guid}/contents")]
    public IActionResult GetContents(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return contentService.GetContents(email, questionnaireId).ToActionResult();
    }

    [HttpPut("contents/{id:guid}")]
    public async Task<IActionResult> UpdateContent(Guid id, UpdateContentRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await contentService.UpdateContent(email, id, request)).ToActionResult();
    }

    [HttpDelete("contents/{id:guid}")]
    public async Task<IActionResult> DeleteContent(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await contentService.DeleteContent(email, id)).ToActionResult();
    }
}