using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ServiceResultExtensions
{
    public static IActionResult ToActionResult(this ServiceResult result) 
    {
        return result.Type switch
        {
            ServiceResultType.Ok => new OkObjectResult(result.Value),
            ServiceResultType.Created => new CreatedResult((string?) null, result.Value),
            ServiceResultType.BadRequest => new BadRequestObjectResult(result.Value),
            ServiceResultType.Unauthorized => new UnauthorizedResult(),
            ServiceResultType.NoContent => new NoContentResult(),
            ServiceResultType.Forbid => new ForbidResult(),
            ServiceResultType.NotFound => new NotFoundResult(),
            ServiceResultType.Forbidden => new ForbidResult(),
            ServiceResultType.Conflict => new ConflictResult(),
            ServiceResultType.Problem => new ObjectResult(result.Value) { StatusCode = 500 },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}