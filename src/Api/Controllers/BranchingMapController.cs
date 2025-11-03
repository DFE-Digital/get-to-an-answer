using System.Security.Claims;
using Api.Extensions;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public class BranchingMapController(IQuestionnaireService questionnaireService) : ControllerBase
{
    [HttpGet("questionnaires/{questionnaireId:guid}/branching-map")]
    public async Task<IActionResult> GetBranchingMap(Guid questionnaireId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        return (await questionnaireService.GetBranchingMap(email, questionnaireId)).ToActionResult();
    }
}
