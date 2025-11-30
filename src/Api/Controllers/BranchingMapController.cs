using System.Security.Claims;
using Api.Extensions;
using Api.Services;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api")]
[ApiController]
public class BranchingMapController(IQuestionnaireService questionnaireService) : ControllerBase
{
    [HttpGet("questionnaires/{questionnaireId:guid}/branching-map")]
    public async Task<IActionResult> GetBranchingMap(Guid questionnaireId)
    {
        var userId = User.GetIdClaim()!;

        return (await questionnaireService.GetBranchingMap(userId, questionnaireId)).ToActionResult();
    }
}
