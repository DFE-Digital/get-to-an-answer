using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Client;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

public class BearerTokenHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<BearerTokenHandler> logger)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var auth = await httpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);

        logger.LogInformation($"Auth Cookie expiring at {auth?.Properties?.ExpiresUtc}");
        
        var idToken = auth?.Properties?.GetTokenValue("id_token");

        var shouldChallenge = string.IsNullOrEmpty(idToken);

        if (!shouldChallenge)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (tokenHandler.CanReadToken(idToken!))
            {
                var jwtToken = tokenHandler.ReadJwtToken(idToken);
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                if (long.TryParse(expClaim, out var expSeconds))
                {
                    var expiry = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
                    
                    logger.LogInformation($"Token expires at {expiry}");

                    if (expiry <= DateTimeOffset.UtcNow)
                    {
                        shouldChallenge = true;
                    }
                }
            }
            else
            {
                shouldChallenge = true;
            }
        }

        if (shouldChallenge)
        {
            logger.LogWarning("Token expired or invalid, forcing re-authentication");
            
            await httpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
        return await base.SendAsync(request, cancellationToken);
    }
}