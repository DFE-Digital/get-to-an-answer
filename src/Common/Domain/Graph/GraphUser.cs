

using Newtonsoft.Json;

namespace Common.Domain.Graph;

public class GraphUser
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public required string GivenName { get; set; }
    public required string Surname { get; set; }
    public string? Mail { get; set; }
    public required string UserPrincipalName { get; set; }
    
}

public class GraphUsers
{
    public List<GraphUser> Value { get; set; } = new();
}