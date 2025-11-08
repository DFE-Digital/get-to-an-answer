using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;

namespace Common.Client;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

public class BearerTokenHandler(
    IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var auth = await httpContextAccessor.HttpContext?.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme)!;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth?.Properties?.GetTokenValue("id_token"));
        return await base.SendAsync(request, cancellationToken);
    }
}