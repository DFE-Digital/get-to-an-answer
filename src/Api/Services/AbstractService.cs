using System.Diagnostics;

namespace Api.Services;

public abstract class AbstractService
{
    protected ServiceResult Ok(object? value = null) => new (ServiceResultType.Ok, value);
    protected ServiceResult Created(object? value = null) => new (ServiceResultType.Created, value);
    protected ServiceResult NoContent(object? value = null) => new (ServiceResultType.NoContent, value);
    protected ServiceResult Unauthorized(object? value = null) => new (ServiceResultType.Unauthorized, value);
    protected ServiceResult Forbid(object? value = null) => new (ServiceResultType.Forbid, value);
    protected ServiceResult BadRequest(object? value = null) => new (ServiceResultType.BadRequest, value);
    protected ServiceResult NotFound(object? value = null) => new (ServiceResultType.NotFound, value);
    protected ServiceResult Forbidden(object? value = null) => new (ServiceResultType.Forbidden, value);
    protected ServiceResult Conflict(object? value = null) => new (ServiceResultType.Conflict, value);
    protected ServiceResult Problem(object? value = null) => new (ServiceResultType.Problem, value);
    
    // Build RFC7807 ProblemDetails-shaped object with ASP.NET Core's trace id extension
    protected static object ProblemTrace(string detail, int status, string? type = null, string? title = null, string? instance = null)
        => new
        {
            type = type ?? "about:blank",
            title = title ?? (status == 400 ? "Bad Request" :
                status == 403 ? "Forbidden" :
                status == 404 ? "Not Found" :
                status == 500 ? "Internal Server Error" : "Error"),
            status,
            detail,
            instance,
            traceId = Activity.Current?.TraceId.ToString() ?? System.Diagnostics.ActivityTraceId.CreateRandom().ToString()
        };

    // Build ValidationProblemDetails-shaped payload
    protected static object ValidationProblemTrace(IDictionary<string, string[]> errors, string? instance = null)
        => new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title = "One or more validation errors occurred.",
            status = 400,
            errors,
            instance,
            traceId = Activity.Current?.TraceId.ToString() ?? System.Diagnostics.ActivityTraceId.CreateRandom().ToString()
        };
}

public enum ServiceResultType
{
    Ok = 0,
    Created = 1,
    NoContent = 2,
    Unauthorized = 3,
    Forbid = 4,
    BadRequest = 5,
    NotFound = 6,
    Forbidden = 7,
    Conflict = 8,
    Problem = 9
}

public sealed class ServiceResult(ServiceResultType resultType, object? value)
{
    public ServiceResultType Type { get; } = resultType;
    public object? Value { get; } = value;
}