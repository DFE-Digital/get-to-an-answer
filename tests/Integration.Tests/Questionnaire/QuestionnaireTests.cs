using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Enum;
using FluentAssertions;
using Integration.Tests.Fake;
using Integration.Tests.Fixture;
using Integration.Tests.Util;

namespace Integration.Tests.Questionnaire;

public class QuestionnaireTests(ApiFixture factory) : 
    ControllerTests(factory, "/api/questionnaires")
{
    private static void AssertNoSensitiveUserData(string responseBody)
    {
        // Ensure no sensitive fields are present
        responseBody.Should().NotContain("createdBy");
        responseBody.Should().NotContain("contributors");
    }
    
    #region Create Questionnaire Endpoint Tests

    [Fact]
    public async Task Create_With_Title_Only_Returns201_And_Dto_Without_Sensitive_Data()
    {
        var payload = new { title = "My Questionnaire" };

        using var res = await Create(payload, JwtTestTokenGenerator.ValidJwtToken);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var responseBody = await res.Content.ReadAsStringAsync();
        var dto = responseBody.Deserialize<QuestionnaireDto>()!;

        dto.Id.Should().NotBeEmpty(because: "id should be auto-generated");
        dto.Title.Should().Be("My Questionnaire");
        dto.CreatedAt.Should().NotBe(default);

        AssertNoSensitiveUserData(responseBody);
    }

    [Fact]
    public async Task Create_With_Title_And_Description_Returns201_And_Dto_Has_Inputted_Values()
    {
        var payload = new
        {
            title = "T1",
            description = "D1"
        };

        using var res = await Create(payload);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var responseBody = await res.Content.ReadAsStringAsync();
        var dto = responseBody.Deserialize<QuestionnaireDto>()!;
        dto.Title.Should().Be("T1");
        dto.Description.Should().Be("D1");
        AssertNoSensitiveUserData(responseBody);
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

        using var res = await Create(payload);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var responseBody = await res.Content.ReadAsStringAsync();
        var dto = responseBody.Deserialize<QuestionnaireDto>()!;
        dto.Title.Should().Be("With Slug");
        dto.Slug.Should().Be("with-slug");
        dto.Description.Should().Be("Desc");
        AssertNoSensitiveUserData(responseBody);
    }
    
    [Fact]
    public async Task Create_With_Really_Long_Slug_Description_Returns400()
    {
        var payload = new
        {
            title = "With Slug",
            slug = new string('a', 700),
            description = "Desc"
        };

        using var res = await Create(payload);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        var responseBody = await res.Content.ReadAsStringAsync();

        var error = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        // Assert response contains validation error for slug length
        error.GetProperty("title").GetString().Should().Contain("One or more validation errors occurred.");
        error.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.TryGetProperty("Slug", out var slugErrors).Should().BeTrue();
        slugErrors[0].GetString().Should().Be("The field Slug must be a string or array type with a maximum length of '500'.");
        
        AssertNoSensitiveUserData(responseBody);
    }

    [Fact]
    public async Task Create_With_Duplicate_Title_Still_Creates_New_Id()
    {
        var payload = new { title = "Duplicate Title" };

        using var res1 = await Create(payload);
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);
        
        var res1Body = await res1.Content.ReadAsStringAsync();
        var dto1 = res1Body.Deserialize<QuestionnaireDto>()!;

        using var res2 = await Create(payload);
        Assert.Equal(HttpStatusCode.Created, res2.StatusCode);
        
        var res2Body = await res2.Content.ReadAsStringAsync();
        var dto2 = res2Body.Deserialize<QuestionnaireDto>()!;

        dto1.Id.Should().NotBeEmpty();
        dto2.Id.Should().NotBeEmpty();
        dto2.Id.Should().NotBe(dto1.Id);
    }

    [Fact]
    public async Task Create_Missing_Title_Returns400_With_Validation_Error_No_Creation()
    {
        var payload = new
        {
            // title missing
            description = "Some description"
        };

        using var res = await Create(payload);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        text.Should().Contain("title");
        text.Should().NotContain("createdBy");
    }

    [Fact]
    public async Task Create_Invalid_Token_Returns401_No_Leak_No_Creation()
    {
        var payload = new { title = "Any" };

        using var res = await Create(payload, JwtTestTokenGenerator.InvalidAudJwtToken);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("createdBy");
    }

    [Fact]
    public async Task Create_Expired_Token_Returns401_No_Leak_No_Creation()
    {
        var payload = new { title = "Any" };

        using var res = await Create(payload, bearerToken: JwtTestTokenGenerator.ExpiredJwtToken);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
    }

    [Fact]
    public async Task Post_NotAuthorized_For_Specific_Resource_Returns_403_Or_401_No_Creation()
    {
        var payload = new { title = "Any" };

        using var res = await Create(payload, JwtTestTokenGenerator.NonDfeJwtToken);

        Assert.Contains(res.StatusCode, new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden });

        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
        text.Should().NotContain("exists");
    }

    [Fact]
    public async Task Create_Invalid_Payload_Field_Types_Returns400_No_Leak()
    {
        var payload = new
        {
            title = 12345,
            description = new { inner = "value" }
        };

        using var res = await Create(payload);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
    }

    [Theory]
    [InlineData("   ", false, "only whitespace")]
    [InlineData("😀✨💥", true, "emojis allowed?")]
    [InlineData("\t\r\n", false, "control characters")]
    [InlineData("A", true, "lower length")]
    public async Task Title_Business_Rules(string title, bool expectedSuccess, string _case)
    {
        var payload = new { title };

        using var res = await Create(payload);

        if (expectedSuccess)
        {
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            var text = await res.Content.ReadAsStringAsync();
            text.Should().Contain("title");
        }
    }

    [Theory]
    [InlineData("   ", true, "description may allow whitespace?")]
    [InlineData("😀✨💥", true, "emojis")]
    [InlineData("\t\r\n", true, "control chars?")]
    [InlineData("A", true, "min")]
    public async Task Description_Business_Rules(string description, bool expectedSuccess, string _case)
    {
        var payload = new { title = "Valid Title", description };

        using var res = await Create(payload);

        if (expectedSuccess)
        {
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            var text = await res.Content.ReadAsStringAsync();
            text.Should().Contain("description");
        }
    }
    
    #endregion
    
    // === Tests for the Get questionnaires endpoint ===
    
    #region Get Questionnaires Endpoint Tests

    [Fact(DisplayName = "Get all questionnaires for me returns 200 and only my accessible items")]
    public async Task Get_All_For_Current_User_Only()
    {
        // Seed: create two questionnaires for current user
        using (var res = await Create(new { title = "Q1", description = "D1" }, JwtTestTokenGenerator.ValidJwtToken)) 
        { Assert.Equal(HttpStatusCode.Created, res.StatusCode); }
        using (var res = await Create(new { title = "Q2" }, JwtTestTokenGenerator.ValidJwtToken))
        { Assert.Equal(HttpStatusCode.Created, res.StatusCode); }

        // Seed: create a questionnaire as a different user (unauthorised for current user)
        using (var res = await Create(new { title = "OtherUser-NotVisible" }, JwtTestTokenGenerator.UnauthorizedJwtToken))
        { Assert.Equal(HttpStatusCode.Created, res.StatusCode); }

        // Act
        using var getRes = await GetAll(JwtTestTokenGenerator.ValidJwtToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var responseBody = await getRes.Content.ReadAsStringAsync();
        var questionnaires = responseBody.Deserialize<List<QuestionnaireDto>>();

        Assert.NotNull(questionnaires);
        
        Assert.Equal(2, questionnaires.Count);

        foreach (var q in questionnaires)
        {
            Assert.NotEqual("OtherUser-NotVisible", q.Title); // shouldn't include others
        }
    }

    [Fact(DisplayName = "Invalid JWT returns 401 and no data leakage")]
    public async Task Invalid_Jwt()
    {
        using var res = await GetAll(JwtTestTokenGenerator.InvalidAudJwtToken);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("createdBy", text);
        Assert.DoesNotContain("contributors", text);
    }

    [Fact(DisplayName = "Expired JWT returns 401 and no data leakage")]
    public async Task Expired_Jwt()
    {
        using var res = await GetAll(JwtTestTokenGenerator.ExpiredJwtToken);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("createdBy", text);
        Assert.DoesNotContain("contributors", text);
    }

    [Fact(DisplayName = "Get returns empty list when none exist for user")]
    public async Task Get_Empty_When_None_Exist()
    {
        // Use a token that has no created questionnaires
        using var res = await GetAll(JwtTestTokenGenerator.NewUserJwtToken);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var responseBody = await res.Content.ReadAsStringAsync();
        var list = responseBody.Deserialize<List<QuestionnaireDto>>();
        
        list.Should().NotBeNull();
        list.Should().BeEmpty();
    }
    
    // === Tests for the Get specific questionnaire endpoint ===
    
    private async Task<HttpResponseMessage> CreateCreateGet(string id, string? bearerToken = null)
    {
        bearerToken ??= JwtTestTokenGenerator.ValidJwtToken;

        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/questionnaires/{id}");
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        return await Client.SendAsync(req);
    }

    private static string ExtractId(string json)
        => JsonDocument.Parse(json).RootElement.GetProperty("id").GetString()!;

    // Scenario: Retrieve an existing questionnaire
    [Fact(DisplayName = "GET /questionnaires/{id} returns 200 with DTO and system-managed fields; no sensitive user data")]
    public async Task Get_By_Id_Succeeds_With_Dto_And_No_Sensitive_Data()
    {
        // Arrange: create a questionnaire as current user
        var createPayload = new { title = "My Q", description = "Desc" };
        using (var postRes = await Create(createPayload, JwtTestTokenGenerator.ValidJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = ExtractId(await postRes.Content.ReadAsStringAsync());

            // Act
            using var getRes = await CreateCreateGet(id, JwtTestTokenGenerator.ValidJwtToken);
            
            // Assert
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseBody = await getRes.Content.ReadAsStringAsync();
            var questionnaire = responseBody.Deserialize<QuestionnaireDto>();

            questionnaire.Should().NotBeNull();
            
            questionnaire.Id.ToString().Should().Be(id);
            questionnaire.Title.Should().Be("My Q");
            questionnaire.Description.Should().Be("Desc");

            questionnaire.CreatedAt.Should().NotBe(default);
            questionnaire.UpdatedAt.Should().NotBe(default);
            
            // No sensitive user data
            AssertNoSensitiveUserData(responseBody);
        }
    }

    // Scenario: Access to questionnaire not permitted
    [Fact(DisplayName = "GET /questionnaires/{id} unauthorized user gets 401/403 without existence leak")]
    public async Task Get_By_Id_Unauthorized_User_Fails_Without_Leak()
    {
        // Arrange: create as authorized user
        var createPayload = new { title = "Private Q" };
        using (var postRes = await Create(createPayload, JwtTestTokenGenerator.ValidJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = ExtractId(await postRes.Content.ReadAsStringAsync());

            // Act: fetch as a different/unauthorized user
            using var res = await CreateCreateGet(id, JwtTestTokenGenerator.UnauthorizedJwtToken);
            
            // Assert: either 401 or 403 and no existence-sensitive details
            new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden }.Should().Contain(res.StatusCode);

            var text = await res.Content.ReadAsStringAsync();
            
            AssertNoSensitiveUserData(text);
        }
    }

    // Scenario: Questionnaire does not exist or is deleted
    [Fact(DisplayName = "GET missing/deleted questionnaire returns 404 Not Found")]
    public async Task Get_By_Id_Missing_Or_Deleted_Returns_404()
    {
        // Arrange: random id that won't exist
        var missingId = Guid.NewGuid().ToString("N");

        // Act
        using var res = await CreateCreateGet(missingId, JwtTestTokenGenerator.ValidJwtToken);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
        text.Should().NotContain("contributors");
    }

    // Scenario: Get the specific questionnaire you created or were invited to contribute
    [Fact(DisplayName = "GET /questionnaires/{id} returns only the requested questionnaire with expected fields")]
    public async Task Get_By_Id_Returns_Correct_Fields_Only()
    {
        // Arrange: create two questionnaires as current user; we will fetch only one
        string idToFetch;
        {
            using var res1 = await Create(new { title = "Q1", description = "D1" }, JwtTestTokenGenerator.ValidJwtToken);
            res1.StatusCode.Should().Be(HttpStatusCode.Created);
            idToFetch = ExtractId(await res1.Content.ReadAsStringAsync());
        }
        {
            using var res2 = await Create(new { title = "Q2", description = "D2" }, JwtTestTokenGenerator.ValidJwtToken);
            res2.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        using var res = await CreateCreateGet(idToFetch, JwtTestTokenGenerator.ValidJwtToken);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await res.Content.ReadAsStringAsync();
        var questionnaire = responseBody.Deserialize<QuestionnaireDto>();

        questionnaire.Should().NotBeNull();
        
        questionnaire.Id.ToString().Should().Be(idToFetch);
        questionnaire.Title.Should().Be("Q1");
        questionnaire.Description.Should().Be("D1");

        AssertNoSensitiveUserData(responseBody);
    }

    // Scenario: Error when expired JWT bearer token
    [Fact(DisplayName = "GET with expired JWT returns 401 and no sensitive data")]
    public async Task Get_By_Id_Expired_Token_Returns_401_No_Leak()
    {
        // Arrange: create a questionnaire to target
        string id;
        using (var postRes = await Create(new { title = "Any" }, JwtTestTokenGenerator.ValidJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await CreateCreateGet(id, JwtTestTokenGenerator.ExpiredJwtToken);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var responseBody = await res.Content.ReadAsStringAsync();
        
        AssertNoSensitiveUserData(responseBody);
    }

    // Scenario: Error when invalid JWT bearer token
    [Fact(DisplayName = "GET with invalid JWT returns 401 and no sensitive data")]
    public async Task Get_By_Id_Invalid_Token_Returns_401_No_Leak()
    {
        // Arrange: create a questionnaire to target
        string id;
        using (var postRes = await Create(new { title = "Any" }, JwtTestTokenGenerator.ValidJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await CreateCreateGet(id, JwtTestTokenGenerator.InvalidAudJwtToken);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var text = await res.Content.ReadAsStringAsync();
        
        AssertNoSensitiveUserData(text);
    }

    // Scenario: Error when invalid questionnaire id is passed
    [Theory(DisplayName = "GET with invalid questionnaire id returns 400 Bad Request")]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    public async Task Get_By_Id_Invalid_Id_Returns_400(string invalidId)
    {
        using var res = await CreateCreateGet(invalidId, JwtTestTokenGenerator.ValidJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotBeNullOrWhiteSpace();
    }
    
    #endregion

    // Scenario: Update a questionnaire with only the Title
    
    #region Update Questionnaire Endpoint Tests
    
    [Fact]
    public async Task Update_Title_Only_NoContent_And_Description_Unchanged()
    {
        // Arrange
        string id;
        const string originalDesc = "Original desc";
        using (var postRes = await Create(new { title = "T0", description = originalDesc }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { title = "T1" });
        
        // Assert 204, then GET to verify
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var questionnaire = await GetById<QuestionnaireDto>(id);
        questionnaire.Title.Should().Be("T1");
        questionnaire.Description.Should().Be(originalDesc);
        questionnaire.CreatedAt.Should().NotBe(default);
        questionnaire.UpdatedAt.Should().NotBe(default);
    }

    // Scenario: Update a questionnaire with missing Title
    [Fact]
    public async Task Update_Missing_Title_BadRequest_NoContent_Not_Used()
    {
        // Arrange
        string id;
        using (var postRes = await Create(new { title = "T0", description = "D0" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { /* missing title */ });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().Contain("title");
        text.Should().NotContain("userId");
        text.Should().NotContain("userEmail");

        // Verify nothing changed
        var questionnaire = await GetById<QuestionnaireDto>(id);
        questionnaire.Title.Should().Be("T0");
        questionnaire.Description.Should().Be("D0");
    }

    // Scenario: Update with Title and Description
    [Fact]
    public async Task Update_Title_And_Description_NoContent_Then_Get()
    {
        // Arrange
        string id;
        using (var postRes = await Create(new { title = "T0", description = "D0" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { title = "T1", description = "D1" });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var questionnaire = await GetById<QuestionnaireDto>(id);
        questionnaire.Title.Should().Be("T1");
        questionnaire.Description.Should().Be("D1");
    }

    // Scenario: Update with Title, Slug and Description
    [Fact]
    public async Task Update_Title_Slug_Description_NoContent_Then_Get()
    {
        // Arrange
        string id;
        using (var postRes = await Create(new { title = "T0", description = "D0" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { title = "T2", slug = "t2-slug", description = "D2" });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var questionnaire = await GetById<QuestionnaireDto>(id);
        questionnaire.Title.Should().Be("T2");
        questionnaire.Slug.Should().Be("t2-slug");
        questionnaire.Description.Should().Be("D2");
    }

    // Scenario: Error when invalid JWT bearer token (unauthorized access)
    [Fact]
    public async Task Update_Invalid_JWT_401_No_Changes()
    {
        // Arrange
        string id;
        using (var postRes = await Create(new { title = "Seed", description = "D0" }, JwtTestTokenGenerator.ValidJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { title = "Changed" }, JwtTestTokenGenerator.InvalidAudJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var text = await res.Content.ReadAsStringAsync();

        // Verify unchanged
        var questionnaire = await GetById<QuestionnaireDto>(id, JwtTestTokenGenerator.ValidJwtToken);
        questionnaire.Title.Should().Be("Seed");
        questionnaire.Description.Should().Be("D0");
    }

    // Scenario: Error when expired JWT bearer token (unauthorized access)
    [Fact]
    public async Task Update_Expired_JWT_401_No_Changes()
    {
        // Arrange
        string id;
        using (var postRes = await Create(new { title = "Seed", description = "D0" }, JwtTestTokenGenerator.ValidJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { title = "Changed" }, JwtTestTokenGenerator.ExpiredJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Verify unchanged
        var questionnaire = await GetById<QuestionnaireDto>(id, JwtTestTokenGenerator.ValidJwtToken);
        questionnaire.Title.Should().Be("Seed");
        questionnaire.Description.Should().Be("D0");
    }

    // Scenario: Questionnaire not found
    [Fact]
    public async Task Update_NotFound_NoContent_Not_Returned()
    {
        var missingId = Guid.NewGuid().ToString("N");

        using var res = await UpdateById(missingId, new { title = "T1" });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Scenario: Access to questionnaire not permitted
    [Fact]
    public async Task Update_Forbidden_No_Changes()
    {
        // Arrange: create as authorized user
        string id;
        using (var postRes = await Create(new { title = "Private Q", description = "D0" }, JwtTestTokenGenerator.ValidJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act as unauthorized user
        using var res = await UpdateById(id, new { title = "Hack" }, JwtTestTokenGenerator.UnauthorizedJwtToken);

        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden }.Should().Contain(res.StatusCode);

        // Verify unchanged with authorized token
        var questionnaire = await GetById<QuestionnaireDto>(id, JwtTestTokenGenerator.ValidJwtToken);
        questionnaire.Title.Should().Be("Private Q");
        questionnaire.Description.Should().Be("D0");
    }

    // Scenario: Error when invalid questionnaire id is passed
    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    public async Task Update_Invalid_Id_400_No_Changes(string invalidId)
    {
        using var res = await UpdateById(invalidId, new { title = "T1" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotBeNullOrWhiteSpace();
    }

    // Scenario: Update with invalid payload data e.g. description as Boolean
    [Fact]
    public async Task Update_Invalid_Payload_400_No_Changes()
    {
        // Arrange
        string id;
        using (var postRes = await Create(new { title = "Seed", description = "D0" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act: invalid type
        using var res = await UpdateById(id, new { title = "T1", description = true });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify unchanged
        var questionnaire = await GetById<QuestionnaireDto>(id);
        questionnaire.Title.Should().Be("Seed");
        questionnaire.Description.Should().Be("D0");
    }

    // Scenario: Update a questionnaire with a matching Title to an existing questionnaire
    [Fact]
    public async Task Update_With_Duplicate_Title_Still_Distinct_Ids()
    {
        // Seed two questionnaires
        string id1, id2;
        using (var r1 = await Create(new { title = "A", description = "D1" }))
        {
            r1.StatusCode.Should().Be(HttpStatusCode.Created);
            id1 = ExtractId(await r1.Content.ReadAsStringAsync());
        }
        using (var r2 = await Create(new { title = "B", description = "D2" }))
        {
            r2.StatusCode.Should().Be(HttpStatusCode.Created);
            id2 = ExtractId(await r2.Content.ReadAsStringAsync());
        }

        // Act: update second to duplicate title of first
        using var res = await UpdateById(id2, new { title = "A" });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify via GETs: different IDs remain, titles reflect update
        var q1 = await GetById<QuestionnaireDto>(id1);
        var q2 = await GetById<QuestionnaireDto>(id2);

        q1.Id.Should().Be(id1);
        q2.Id.Should().Be(id2);
        q2.Title.Should().Be("A");
    }
    
    #endregion

    // === Tests for the Delete questionnaire endpoint ===
    
    #region Delete Questionnaire Endpoint Tests

    [Fact]
    public async Task Delete_Existing_Questionnaire_Succeeds_And_Is_SoftDeleted()
    {
        // Arrange: create then ensure appears in GET list
        string id;
        using (var postRes = await Create(new { title = "ToDelete", description = "D0" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act: delete
        using var delRes = await DeleteById(id);

        // Assert 200 OK or 204 NoContent
        new[] { HttpStatusCode.OK, HttpStatusCode.NoContent }.Should().Contain(delRes.StatusCode);

        // Verify via GET by id -> 404 (soft-deleted not retrievable)
        using (var getRes = await GetById(id, JwtTestTokenGenerator.ValidJwtToken))
        {
            getRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // Verify it no longer appears in list
        using (var listReq = new HttpRequestMessage(HttpMethod.Get, "/api/questionnaires"))
        {
            listReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JwtTestTokenGenerator.ValidJwtToken);
            using var listRes = await Client.SendAsync(listReq);
            listRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await listRes.Content.ReadAsStringAsync();
            body.Should().NotContain(id);
        }
    }

    [Fact]
    public async Task Delete_NotAuthorized_Questionnaire_Returns_Forbidden_Or_NotFound_No_Leak_No_Deletion()
    {
        // Arrange: create as authorized user
        string id;
        using (var postRes = await Create(new { title = "Private Q", description = "D0" }, JwtTestTokenGenerator.ValidJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act: delete as unauthorized user
        using var res = await DeleteById(id, JwtTestTokenGenerator.UnauthorizedJwtToken);

        new[] { HttpStatusCode.Forbidden, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("contributors");
        text.Should().NotContain("createdBy");

        // Verify still retrievable by owner
        using var getRes = await GetById(id, JwtTestTokenGenerator.ValidJwtToken);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Missing_Questionnaire_Returns_404()
    {
        var missingId = Guid.NewGuid().ToString("N");

        using var res = await DeleteById(missingId);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotBeNullOrWhiteSpace();
        text.Should().NotContain("userId");
        text.Should().NotContain("contributors");
    }

    [Fact]
    public async Task Delete_Invalid_JWT_Returns_Unauthorized_No_Deletion()
    {
        // Arrange
        string id;
        using (var postRes = await Create(new { title = "Seed", description = "D0" }, JwtTestTokenGenerator.ValidJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await DeleteById(id, JwtTestTokenGenerator.InvalidAudJwtToken);

        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
        
        // Verify still retrievable with valid token
        using var getRes = await GetById(id, JwtTestTokenGenerator.ValidJwtToken);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Expired_JWT_Returns_Unauthorized_No_Deletion()
    {
        // Arrange
        string id;
        using (var postRes = await Create(new { title = "Seed", description = "D0" }, JwtTestTokenGenerator.ValidJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await DeleteById(id, JwtTestTokenGenerator.ExpiredJwtToken);

        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
        var text = await res.Content.ReadAsStringAsync();

        // Verify still retrievable with valid token
        using var getRes = await GetById(id, JwtTestTokenGenerator.ValidJwtToken);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Already_Deleted_Returns_NotFound_Or_Conflict_No_Duplication()
    {
        // Arrange: create and delete
        string id;
        using (var postRes = await Create(new { title = "SoftDelete", description = "D0" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }
        using (var firstRes = await DeleteById(id))
        {
            new[] { HttpStatusCode.OK, HttpStatusCode.NoContent }.Should().Contain(firstRes.StatusCode);
        }

        // Act: delete again
        using var res = await DeleteById(id);

        new[]
        {
            HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.Conflict
        }.Should().Contain(res.StatusCode);
    }
    
    #endregion
    
    // === Tests for the Publish questionnaire endpoint ===
    
    #region Publish Questionnaire Endpoint Tests
    
    [Fact(DisplayName = "Publish questionnaire returns NoContent when successful")]
    public async Task Publish_Returns_NoContent_When_Successful()
    {
        // Arrange: create an unpublished questionnaire
        string id;
        using (var createRes = await Create(new { title = "To Publish" }))
        {
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await createRes.Content.ReadAsStringAsync());
        }
    
        // Act
        using var res = await Update(routePrefixOverride: $"/api/questionnaires/{id}/publish");
    
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    
        // Verify via GET that it's published
        var questionnaire = await GetById<QuestionnaireDto>(id);
        questionnaire.Status.Should().Be(EntityStatus.Published);
        questionnaire.Version.Should().Be(1);
        
        // Update the questionnaire so it returns to Draft status
        await UpdateById(id, new { title = "Changed after publish" });
        
        // Verify via GET that it's draft again
        questionnaire = await GetById<QuestionnaireDto>(id);
        questionnaire.Status.Should().Be(EntityStatus.Draft);
        questionnaire.Version.Should().Be(1);
        
        // Re-publish the questionnaire
        await Update(routePrefixOverride: $"/api/questionnaires/{id}/publish");

        // Verify via GET that it's draft again
        questionnaire = await GetById<QuestionnaireDto>(id);
        questionnaire.Status.Should().Be(EntityStatus.Published);
        questionnaire.Version.Should().Be(2);
    }
    
    [Fact(DisplayName = "Publish missing questionnaire returns NotFound")]
    public async Task Publish_Missing_Questionnaire_Returns_NotFound() 
    {
        var missingId = Guid.NewGuid().ToString("N");
    
        using var res = await Update(routePrefixOverride: $"/api/questionnaires/{missingId}/publish");
    
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact(DisplayName = "Publish with invalid JWT returns Unauthorized")]
    public async Task Publish_Invalid_JWT_Returns_Unauthorized()
    {
        // Arrange
        string id;
        using (var createRes = await Create(new { title = "Test" }))
        {
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await createRes.Content.ReadAsStringAsync());
        }
    
        // Act with invalid token
        using var res = await Update(routePrefixOverride: $"/api/questionnaires/{id}/publish", 
            bearerToken: JwtTestTokenGenerator.InvalidAudJwtToken);
    
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact(DisplayName = "Publish with expired JWT returns Unauthorized")] 
    public async Task Publish_Expired_JWT_Returns_Unauthorized()
    {
        // Arrange
        string id;
        using (var createRes = await Create(new { title = "Test" }))
        {
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await createRes.Content.ReadAsStringAsync());
        }
    
        // Act with expired token
        using var res = await Update(routePrefixOverride: $"/api/questionnaires/{id}/publish", 
            bearerToken: JwtTestTokenGenerator.ExpiredJwtToken);
    
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact(DisplayName = "Unauthorized user cannot publish questionnaire")] 
    public async Task Publish_Unauthorized_User_Cannot_Publish()
    {
        // Arrange: create as authorized user
        string id;
        using (var createRes = await Create(new { title = "Private" }, JwtTestTokenGenerator.ValidJwtToken))
        {
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await createRes.Content.ReadAsStringAsync());
        }
    
        // Act as unauthorized user
        using var res = await Update(routePrefixOverride: $"/api/questionnaires/{id}/publish", 
            bearerToken: JwtTestTokenGenerator.UnauthorizedJwtToken);
    
        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden }.Should().Contain(res.StatusCode);
    
        // Verify not published
        var questionnaire = await GetById<QuestionnaireDto>(id, JwtTestTokenGenerator.ValidJwtToken);
        questionnaire.Status.Should().Be(EntityStatus.Draft);
    }
    
    [Theory(DisplayName = "Publish with invalid questionnaire id returns BadRequest")]
    [InlineData("not-a-guid")] 
    [InlineData("123")]
    public async Task Publish_Invalid_Id_Returns_BadRequest(string invalidId)
    {
        using var res = await Update(routePrefixOverride: $"/api/questionnaires/{invalidId}/publish", 
            bearerToken: JwtTestTokenGenerator.UnauthorizedJwtToken);
    
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    #endregion
    
    // === Tests for the clone questionnaire endpoint ===
    
    #region Clone Questionnaire Endpoint Tests

    [Fact(DisplayName = "Clone questionnaire returns Created with new ID")]
    public async Task Clone_Returns_Created_With_New_Id()
    {
        // Arrange: create original questionnaire
        string originalId;
        using (var createRes = await Create(new { title = "Original Q", description = "D1" }))
        {
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            originalId = ExtractId(await createRes.Content.ReadAsStringAsync());
        }

        // Act: clone it
        using var res = await Create(new { title = "Original Q", description = "D1" }, 
            routePrefixOverride: $"/api/questionnaires/{originalId}/clones");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await res.Content.ReadAsStringAsync();
        var cloned = responseBody.Deserialize<QuestionnaireDto>();

        cloned.Should().NotBeNull();
        cloned.Id.Should().NotBe(originalId);
        cloned.Title.Should().Be("Original Q");
        cloned.Description.Should().Be("D1");
        cloned.Status.Should().Be(EntityStatus.Draft);

        AssertNoSensitiveUserData(responseBody);
    }

    [Fact(DisplayName = "Unauthorized user cannot clone questionnaire")]
    public async Task Clone_Unauthorized_User_Cannot_Clone()
    {
        // Arrange: create as authorized user
        string originalId;
        using (var createRes = await Create(new { title = "Original" }, JwtTestTokenGenerator.ValidJwtToken))
        {
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            originalId = ExtractId(await createRes.Content.ReadAsStringAsync());
        }

        // Act: clone as unauthorized user
        using var res = await Create(new { title = "Title"}, routePrefixOverride: $"/api/questionnaires/{originalId}/clones",
            bearerToken: JwtTestTokenGenerator.UnauthorizedJwtToken);

        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden }.Should().Contain(res.StatusCode);
        var text = await res.Content.ReadAsStringAsync();
        AssertNoSensitiveUserData(text);
    }

    [Fact(DisplayName = "Clone non-existent questionnaire returns NotFound")]
    public async Task Clone_NonExistent_Returns_NotFound()
    {
        var missingId = Guid.NewGuid().ToString("N");

        using var res = await Create(new { title = "Title" }, routePrefixOverride: $"/api/questionnaires/{missingId}/clones");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
    }

    [Theory(DisplayName = "Clone with invalid questionnaire id returns BadRequest")]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    public async Task Clone_Invalid_Id_Returns_BadRequest(string invalidId)
    {
        using var res = await Create(new { title = "Original Q", description = "D1" }, 
            routePrefixOverride: $"/api/questionnaires/{invalidId}/clones");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Clone with expired JWT returns Unauthorized")]
    public async Task Clone_Expired_JWT_Returns_Unauthorized()
    {
        // Arrange: create questionnaire
        string originalId;
        using (var createRes = await Create(new { title = "Original" }))
        {
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            originalId = ExtractId(await createRes.Content.ReadAsStringAsync());
        }

        // Act with expired token
        using var res = await Create(new { title = "Original Q", description = "D1" }, 
            routePrefixOverride: $"/api/questionnaires/{originalId}/clones",
            bearerToken: JwtTestTokenGenerator.ExpiredJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var text = await res.Content.ReadAsStringAsync();
        AssertNoSensitiveUserData(text);
    }

    [Fact(DisplayName = "Clone questionnaire clones questions and their answers")]
    public async Task Clone_Questionnaire_Clones_Questions_And_Answers()
    {
        // Arrange: create questionnaire with 3 questions of different types and 2 answers each
        var questionnaire = await Create<QuestionnaireDto>(new { title = "Original Q", description = "D1" });
        var questionnaireId = questionnaire.Id;

        var qTypes = new[] { QuestionType.SingleSelect, QuestionType.MultiSelect, QuestionType.DropdownSelect };
        var createdQuestions = new List<QuestionDto>();
        foreach (var qt in qTypes)
        {
            var q = await Create<QuestionDto>(new { questionnaireId, content = $"Q-{qt}", type = qt },
                routePrefixOverride: "/api/questions");
            createdQuestions.Add(q);

            // two answers per question
            await Create<AnswerDto>(new { questionnaireId, questionId = q.Id, content = $"A1-{qt}", description = $"D1-{qt}", score = 1 },
                routePrefixOverride: "/api/answers");
            await Create<AnswerDto>(new { questionnaireId, questionId = q.Id, content = $"A2-{qt}", description = $"D2-{qt}", score = 2 },
                routePrefixOverride: "/api/answers");
        }

        // Act: clone questionnaire
        var cloneReq = new { title = "Cloned Title", description = "Cloned Desc", questionnaireId };
        var cloneRes = await Create(cloneReq, routePrefixOverride: $"/api/questionnaires/{questionnaireId}/clones");
        cloneRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var cloneBody = await cloneRes.Content.ReadAsStringAsync();
        var clonedQn = cloneBody.Deserialize<QuestionnaireDto>()!;
        clonedQn.Id.Should().NotBe(questionnaireId);
        clonedQn.Title.Should().Be("Cloned Title");
        clonedQn.Description.Should().Be("Cloned Desc");

        // Assert: cloned questions
        var clonedQuestions = await GetAll<List<QuestionDto>>(routePrefixOverride: $"/api/questionnaires/{clonedQn.Id}/questions");
        clonedQuestions.Should().HaveCount(3);

        // Map original question content/type -> original answers
        var originalAnswersByQuestion = new Dictionary<string, List<AnswerDto>>();
        foreach (var q in createdQuestions)
        {
            var answers = await GetAll<List<AnswerDto>>(routePrefixOverride: $"/api/questions/{q.Id}/answers");
            answers.Should().HaveCount(2);
            originalAnswersByQuestion.Add(q.Content, answers.OrderBy(a => a.Score).ToList());
        }

        // For each cloned question, validate content/type/order, and answers cloned with same fields but new ids and linked to cloned questionnaire/question
        foreach (var (qt, idx) in qTypes.Select((t, i) => (t, i)))
        {
            var expectedContent = $"Q-{qt}";
            var cq = clonedQuestions.Single(q => q.Content == expectedContent);
            cq.Type.Should().Be(qt);
            cq.Order.Should().Be(idx + 1);
            cq.QuestionnaireId.Should().Be(clonedQn.Id);

            var clonedAnswers = await GetAll<List<AnswerDto>>(routePrefixOverride: $"/api/questions/{cq.Id}/answers");
            clonedAnswers.Should().HaveCount(2);

            var originalAnswers = originalAnswersByQuestion[expectedContent];
            clonedAnswers = clonedAnswers.OrderBy(a => a.Score).ToList();

            for (var i = 0; i < 2; i++)
            {
                var oa = originalAnswers[i];
                var ca = clonedAnswers[i];

                ca.Id.Should().NotBeEmpty();
                ca.Id.Should().NotBe(oa.Id);
                ca.QuestionnaireId.Should().Be(clonedQn.Id);
                ca.QuestionId.Should().Be(cq.Id);

                ca.Content.Should().Be(oa.Content);
                ca.Description.Should().Be(oa.Description);
                ca.Score.Should().Be(oa.Score);
                ca.DestinationUrl.Should().Be(oa.DestinationUrl);
                ca.DestinationType.Should().Be(oa.DestinationType);
                ca.DestinationQuestionId.Should().Be(oa.DestinationQuestionId);

                ca.CreatedAt.Should().NotBe(default);
                ca.UpdatedAt.Should().NotBe(default);
            }
        }

        // Sanity: original questionnaire data remains unchanged
        var origQuestions = await GetAll<List<QuestionDto>>(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions");
        origQuestions.Should().HaveCount(3);
        foreach (var oq in origQuestions)
        {
            var ans = await GetAll<List<AnswerDto>>(routePrefixOverride: $"/api/questions/{oq.Id}/answers");
            ans.Should().HaveCount(2);
            ans.Should().OnlyContain(a => a.QuestionnaireId == questionnaireId && a.QuestionId == oq.Id);
        }
    }

    #endregion
    
    // === Tests for the invite questionnaire contributor endpoint ===
    
    #region Invite Contributor Endpoint Tests

    [Fact(DisplayName = "Invite contributor returns NoContent when successful")]
    public async Task Invite_Contributor_Returns_NoContent_When_Successful()
    {
        // Arrange: create questionnaire
        string id;
        using (var createRes = await Create(new { title = "Team Q" }))
        {
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await createRes.Content.ReadAsStringAsync());
        }

        // Act: invite contributor
        using var res = await Update( 
            routePrefixOverride: $"/api/questionnaires/{id}/contributors",
            bearerToken: JwtTestTokenGenerator.UnauthorizedJwtToken);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Invite to non-existent questionnaire returns NotFound")]
    public async Task Invite_NonExistent_Returns_NotFound()
    {
        var missingId = Guid.NewGuid().ToString("N");

        using var res = await Update( 
            routePrefixOverride: $"/api/questionnaires/{missingId}/contributors");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
    }

    [Fact(DisplayName = "Invite with expired JWT returns Unauthorized")]
    public async Task Invite_Expired_JWT_Returns_Unauthorized()
    {
        // Arrange: create questionnaire
        string id;
        using (var createRes = await Create(new { title = "Team Q" }))
        {
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await createRes.Content.ReadAsStringAsync());
        }

        // Act with expired token
        using var res = await Update(
            routePrefixOverride: $"/api/questionnaires/{id}/contributors",
            bearerToken: JwtTestTokenGenerator.ExpiredJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var text = await res.Content.ReadAsStringAsync();
        AssertNoSensitiveUserData(text);
    }

    [Fact(DisplayName = "Duplicate contributor invitation returns Conflict")]
    public async Task Invite_Duplicate_Returns_Conflict()
    {
        // Arrange: create questionnaire
        string id;
        using (var createRes = await Create(new { title = "Team Q" }))
        {
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await createRes.Content.ReadAsStringAsync());
        }

        // First invitation
        using (var res1 = await Update(
                   routePrefixOverride: $"/api/questionnaires/{id}/contributors", 
                   bearerToken: JwtTestTokenGenerator.UnauthorizedJwtToken))
        {
            res1.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // Act: duplicate invitation
        using var res2 = await Update(
            routePrefixOverride: $"/api/questionnaires/{id}/contributors", 
            bearerToken: JwtTestTokenGenerator.UnauthorizedJwtToken);

        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var text = await res2.Content.ReadAsStringAsync();
        AssertNoSensitiveUserData(text);
    }

    #endregion
}



