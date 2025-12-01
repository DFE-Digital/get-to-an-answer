using System.Security.Claims;

namespace Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static readonly string IdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    public static readonly string OidClaimType = "oid";
    
    public static string? GetIdClaim(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(IdClaimType) ?? principal.FindFirstValue(OidClaimType);
    }
}