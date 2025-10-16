using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Integration.Tests.Fake;
using Integration.Tests.Fixture;

namespace Integration.Tests.Questionnaire;

public class QuestionnaireCreationTests(ApiFixture factory) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    private static HttpRequestMessage CreatePost(object payload, string? bearerToken = null)
    {
        bearerToken ??= JwtTestTokenGenerator.ValidJwtToken;
        
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/questionnaires")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json")
        };
        if (!string.IsNullOrEmpty(bearerToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        return req;
    }

    private static void AssertNoSensitiveUserData(JsonElement root)
    {
        // Ensure no userId or other sensitive fields are present
        Assert.False(root.TryGetProperty("userId", out _));
        Assert.False(root.TryGetProperty("userEmail", out _));
        Assert.False(root.TryGetProperty("userName", out _));
        // Adjust with any additional sensitive fields your DTO might exclude
    }

    [Fact]
    public async Task Create_With_Title_Only_Returns201_And_Dto_Without_Sensitive_Data()
    {
        var payload = new { title = "My Questionnaire" };

        using var req = CreatePost(payload, JwtTestTokenGenerator.ValidJwtToken);
        using var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var root = json.RootElement;

        Assert.True(root.TryGetProperty("id", out var idProp));
        Assert.False(string.IsNullOrWhiteSpace(idProp.GetString()), "id should be auto-generated");

        Assert.Equal("My Questionnaire", root.GetProperty("title").GetString());
        Assert.True(root.TryGetProperty("createdAt", out _));

        AssertNoSensitiveUserData(root);
    }

    [Fact]
    public async Task Create_With_Title_And_Description_Returns201_And_Dto_Has_Inputted_Values()
    {
        var payload = new
        {
            title = "T1",
            description = "D1"
        };

        using var req = CreatePost(payload);
        using var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var root = JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("T1", root.GetProperty("title").GetString());
        Assert.Equal("D1", root.GetProperty("description").GetString());
        AssertNoSensitiveUserData(root);
    }

    [Fact]
    public async Task Create_With_Title_Slug_Description_Returns201_And_Dto_Has_Inputted_Values()
    {
        var payload = new
        {
            title = "With Slug",
            slug = "with-slug",
            description = "Desc"
        };

        using var req = CreatePost(payload);
        using var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var root = JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("With Slug", root.GetProperty("title").GetString());
        Assert.Equal("with-slug", root.GetProperty("slug").GetString());
        Assert.Equal("Desc", root.GetProperty("description").GetString());
        AssertNoSensitiveUserData(root);
    }

    [Fact]
    public async Task Create_With_Duplicate_Title_Still_Creates_New_Id()
    {
        var payload = new { title = "Duplicate Title" };

        using var req1 = CreatePost(payload);
        using var res1 = await _client.SendAsync(req1);
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);
        var id1 = JsonDocument.Parse(await res1.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetString();

        using var req2 = CreatePost(payload);
        using var res2 = await _client.SendAsync(req2);
        Assert.Equal(HttpStatusCode.Created, res2.StatusCode);
        var id2 = JsonDocument.Parse(await res2.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetString();

        Assert.False(string.IsNullOrWhiteSpace(id1));
        Assert.False(string.IsNullOrWhiteSpace(id2));
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task Create_Missing_Title_Returns400_With_Validation_Error_No_Creation()
    {
        var payload = new
        {
            // title missing
            description = "Some description"
        };

        using var req = CreatePost(payload);
        using var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        Assert.Contains("title", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CreatedBy", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_Invalid_Token_Returns401_No_Leak_No_Creation()
    {
        var payload = new { title = "Any" };

        using var req = CreatePost(payload, JwtTestTokenGenerator.InvalidAudJwtToken);
        using var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("CreatedBy", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_Expired_Token_Returns401_No_Leak_No_Creation()
    {
        var payload = new { title = "Any" };

        using var req = CreatePost(payload, bearerToken: JwtTestTokenGenerator.ExpiredJwtToken);
        using var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("userId", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Post_NotAuthorized_For_Specific_Resource_Returns_403_Or_401_No_Creation()
    {
        // Adjust endpoint or headers to simulate authenticated but unauthorized user
        var payload = new { title = "Any" };

        using var req = CreatePost(payload, JwtTestTokenGenerator.UnauthorizedJwtToken);
        using var res = await _client.SendAsync(req);

        Assert.Contains(res.StatusCode, new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden });

        var text = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("userId", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exists", text, StringComparison.OrdinalIgnoreCase); // avoid leaking existence
    }

    [Fact]
    public async Task Create_Invalid_Payload_Field_Types_Returns400_No_Leak()
    {
        // e.g., title as number, description as object
        var payload = new
        {
            title = 12345,
            description = new { inner = "value" }
        };

        using var req = CreatePost(payload);
        using var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("userId", text, StringComparison.OrdinalIgnoreCase);
    }

    // Title business rules - adjust expected outcomes per confirmed rules

    [Theory]
    [InlineData("   ", false, "only whitespace")]
    [InlineData("😀✨💥", true, "emojis allowed?")]
    [InlineData("\t\r\n", false, "control characters")]
    [InlineData("A", true, "lower length")]
    //[InlineData(RandomData, true, "min boundary example")]
    //[InlineData(RandomData2, false, "exceeds max?")]
    public async Task Title_Business_Rules(string title, bool expectedSuccess, string _case)
    {
        var payload = new { title };

        using var req = CreatePost(payload);
        using var res = await _client.SendAsync(req);

        if (expectedSuccess)
        {
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            var text = await res.Content.ReadAsStringAsync();
            Assert.Contains("title", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    // Description business rules - adjust expected outcomes per confirmed rules
    [Theory]
    [InlineData("   ", true, "description may allow whitespace?")]
    [InlineData("😀✨💥", true, "emojis")]
    [InlineData("\t\r\n", true, "control chars?")]
    [InlineData("A", true, "min")]
    //[InlineData("".PadLeft(1024, 'D'), true, "upper limit within range?")]
    //[InlineData("".PadLeft(100_000, 'X'), false, "exceeds max?")]
    public async Task Description_Business_Rules(string description, bool expectedSuccess, string _case)
    {
        var payload = new { title = "Valid Title", description };

        using var req = CreatePost(payload);
        using var res = await _client.SendAsync(req);

        if (expectedSuccess)
        {
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            var text = await res.Content.ReadAsStringAsync();
            Assert.Contains("description", text, StringComparison.OrdinalIgnoreCase);
        }
    }
}