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
    Conflict = 8
}

public sealed class ServiceResult(ServiceResultType resultType, object? value)
{
    public ServiceResultType Type { get; } = resultType;
    public object? Value { get; } = value;
}