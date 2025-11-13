using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Integration.Tests.Fake;
using Integration.Tests.Fixture;
using Integration.Tests.Util;

namespace Integration.Tests;

public class ControllerTests(ApiFixture factory, string routePrefix) : IClassFixture<ApiFixture>
{
    protected readonly HttpClient Client = factory.CreateClient();
    
    public async Task<HttpResponseMessage> Create(object? payload = null, string? bearerToken = null, string? routePrefixOverride = null)
    {
        bearerToken ??= JwtTestTokenGenerator.ValidJwtToken;
        
        var req = new HttpRequestMessage(HttpMethod.Post, routePrefixOverride ?? routePrefix)
        {
            Content = payload == null ? null : new StringContent(JsonSerializer.Serialize(payload, 
                new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json")
        };
        if (!string.IsNullOrEmpty(bearerToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        return await Client.SendAsync(req);
    }
    
    public async Task<TGetToAnAnswerDto> Create<TGetToAnAnswerDto>(object payload, string? bearerToken = null, string? routePrefixOverride = null)
    {
        var res = await Create(payload, bearerToken, routePrefixOverride);
        var responseBody = await res.Content.ReadAsStringAsync();
        return responseBody.Deserialize<TGetToAnAnswerDto>()!;
    }
    
    public async Task<HttpResponseMessage> GetAll(string? bearerToken = null, string? routePrefixOverride = null)
    {
        bearerToken ??= JwtTestTokenGenerator.ValidJwtToken;

        var req = new HttpRequestMessage(HttpMethod.Get, routePrefixOverride ?? routePrefix);
        if (!string.IsNullOrEmpty(bearerToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        return await Client.SendAsync(req);
    }
    
    public async Task<TGetToAnAnswerDto> GetAll<TGetToAnAnswerDto>(string? bearerToken = null, string? routePrefixOverride = null)
    {
        var res = await GetAll(bearerToken, routePrefixOverride);
        var responseBody = await res.Content.ReadAsStringAsync();
        return responseBody.Deserialize<TGetToAnAnswerDto>()!;
    }
    
    public async Task<HttpResponseMessage> GetById(string id, string? bearerToken = null, string? routePrefixOverride = null)
    {
        bearerToken ??= JwtTestTokenGenerator.ValidJwtToken;

        var req = new HttpRequestMessage(HttpMethod.Get, $"{routePrefixOverride ?? routePrefix}/{id}");
        if (!string.IsNullOrEmpty(bearerToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        return await Client.SendAsync(req);
    }
    
    public async Task<TGetToAnAnswerDto> GetById<TGetToAnAnswerDto>(string id, string? bearerToken = null, string? routePrefixOverride = null)
    {
        var res = await GetById(id, bearerToken, routePrefixOverride);
        var responseBody = await res.Content.ReadAsStringAsync();
        return responseBody.Deserialize<TGetToAnAnswerDto>()!;
    }
    
    public async Task<HttpResponseMessage> Update(object? payload = null, string? bearerToken = null, string? routePrefixOverride = null, HttpMethod? method = null)
    {
        bearerToken ??= JwtTestTokenGenerator.ValidJwtToken;

        var req = new HttpRequestMessage(method ?? HttpMethod.Put, routePrefixOverride ?? routePrefix)
        {
            Content = payload == null ? null : new StringContent(JsonSerializer.Serialize(payload, 
                new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json")
        };
        if (!string.IsNullOrEmpty(bearerToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        return await Client.SendAsync(req);
    }
    
    public async Task<HttpResponseMessage> Patch(object? payload = null, string? bearerToken = null, string? routePrefixOverride = null)
    {
        bearerToken ??= JwtTestTokenGenerator.ValidJwtToken;

        var req = new HttpRequestMessage(HttpMethod.Patch, routePrefixOverride ?? routePrefix)
        {
            Content = payload == null ? null : new StringContent(JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json")
        };
        if (!string.IsNullOrEmpty(bearerToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        return await Client.SendAsync(req);
    }
    
    public async Task<HttpResponseMessage> UpdateById(string id, object payload, string? bearerToken = null, string? routePrefixOverride = null)
    {
        return await Update(payload, bearerToken, $"{routePrefixOverride ?? routePrefix}/{id}");
    }
    
    public async Task<HttpResponseMessage> DeleteById(string id, string? bearerToken = null, string? routePrefixOverride = null)
    {
        bearerToken ??= JwtTestTokenGenerator.ValidJwtToken;

        var req = new HttpRequestMessage(HttpMethod.Delete, $"{routePrefixOverride ?? routePrefix}/{id}");
        if (!string.IsNullOrEmpty(bearerToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        return await Client.SendAsync(req);
    }
}