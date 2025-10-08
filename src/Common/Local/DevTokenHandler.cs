using System.Net.Http.Json;

namespace Common.Local;

public sealed class DevTokenHandler : DelegatingHandler
{
    private readonly Uri _apiBase;
    
    public DevTokenHandler(Uri apiBase) => _apiBase = apiBase;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        // Acquire mock token
        using var http = new HttpClient { BaseAddress = _apiBase };
        var res = await http.PostAsJsonAsync("/dev/token", new
        {
            sub = "alice", name = "Alice", roles = new[] { "User" }, scopes = new[] { "api.read" }
        }, ct);
        res.EnsureSuccessStatusCode();
        var payload = await res.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", payload!.access_token);
        return await base.SendAsync(request, ct);
    }

    private sealed class TokenResponse
    {
        public string access_token { get; set; } = "";
    }
}