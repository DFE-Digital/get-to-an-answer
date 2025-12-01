using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Domain.Graph;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Common.Client;

public interface IMsGraphClient
{
    Task<GraphUser?> GetGraphUserAsync(string contributorEmailAddress);
    Task<GraphUsers> GetGraphUsersAsync(params string?[] contributorEmailAddresses);
}

public class MsGraphClient : IMsGraphClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MsGraphClient> _logger;
    private static readonly string[] EnvArray = ["Local", "Development"];

    public MsGraphClient(ILogger<MsGraphClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    public async Task<GraphUser?> GetGraphUserAsync(string contributorEmailAddress)
    {
        var response = await _httpClient.GetAsync($"/v1.0/users/{contributorEmailAddress}");
        return await GetResponse<GraphUser>(response);
    }

    public async Task<GraphUsers> GetGraphUsersAsync(params string?[] contributorUserIds)
    {
        var contributorQuery = contributorUserIds
            .Select(id => $"id eq '{id}'").Aggregate(AggregateConditions);
        
        if (string.IsNullOrWhiteSpace(contributorQuery)) 
            return new GraphUsers();
        
        try
        {
            var response = await _httpClient.GetAsync($"/v1.0/users?$filter={contributorQuery}");
            
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<GraphUsers>() ?? new GraphUsers();
            
            throw new MsGraphException("Error getting users from Graph", response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting users from Graph");

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (env == null || EnvArray.Contains(env))
            {
                return new GraphUsers
                {
                    Value = contributorUserIds.Select(id => new GraphUser
                    {
                        Id = id!,
                        UserPrincipalName = id!,
                        Mail = id!,
                        DisplayName = id!,
                        GivenName = string.Empty,
                        Surname = string.Empty
                    }).ToList()
                };
            }
            
            throw;
        }
    }
    
    

    private async Task<TResponse?> GetResponse<TResponse>(HttpResponseMessage response) where TResponse : class
    {
        if (response.IsSuccessStatusCode)
        {
            if (typeof(TResponse) == typeof(string))
            {
                return await response.Content.ReadAsStringAsync() as TResponse;
            }
            
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        // Unsuccessful response: Handle the error gracefully.
        var errorBody = await response.Content.ReadAsStringAsync();

        try
        {
            // Deserialize a ProblemDetails response for specific error handling.
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorBody);

            _logger.LogError(problemDetails?.Detail);

            // Throw a custom exception or return an alternative response.
            throw new MsGraphException(
                $"Microsoft Graph request failed with status code {response.StatusCode} and message: {problemDetails?.Detail}", 
                response.StatusCode);
        }
        catch (Exception)
        {
            _logger.LogError($"Microsoft Graph request '{response.RequestMessage?.RequestUri?.AbsolutePath}' failed with status code '{response.StatusCode}', error: '{errorBody}'");
            throw;
        }
    }
    
    private string AggregateConditions(string a, string b) => a + " or " + b;
}

public class MsGraphException : Exception
{
    public MsGraphException()
    { }
    
    public MsGraphException(string? message, HttpStatusCode? statusCode)
        : base(message, null)
    {
        StatusCode = statusCode;
    }
    
    /// <value>
    /// An HTTP status code if the exception represents a non-successful result, otherwise <c>null</c>.
    /// </value>
    public HttpStatusCode? StatusCode { get; }
}