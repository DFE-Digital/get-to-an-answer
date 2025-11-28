using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common.Domain.Graph;
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

    public MsGraphClient(HttpClient httpClient, ILogger<MsGraphClient> logger)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    public async Task<GraphUser?> GetGraphUserAsync(string contributorEmailAddress)
    {
        var response = await _httpClient.GetAsync($"/v1.0/users/{contributorEmailAddress}");
        
        response.EnsureSuccessStatusCode();
        var contributor = await response.Content.ReadFromJsonAsync<GraphUser>();

        return contributor;
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

            return await response.Content.ReadFromJsonAsync<GraphUsers>() ?? new GraphUsers();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting users from Graph");
            
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
    }
    
    private string AggregateConditions(string a, string b) => a + " or " + b;
}