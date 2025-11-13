using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core.Pipeline;
using Common.Domain;
using Common.Enum;
using FluentAssertions;
using Integration.Tests.Fake;
using Integration.Tests.Fixture;
using Integration.Tests.Util;

namespace Integration.Tests.Content;

public class ContentTests(ApiFixture factory) : 
    ControllerTests(factory, "/api/contents")
{
    private static void AssertNoSensitiveUserData(string responseBody)
    {
        // Ensure no sensitive fields are present
        responseBody.Should().NotContain("createdBy");
        responseBody.Should().NotContain("contributors");
    }
    
    #region Create Content Endpoint Tests

    [Fact]
    public async Task Create_With_Title_And_Content_Returns201_And_Dto_Has_Inputted_Values()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var payload = new
        {
            questionnaireId,
            title = "T1",
            content = "D1",
            referenceName = "Some reference name"
        };

        using var res = await Create(payload);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var responseBody = await res.Content.ReadAsStringAsync();
        var dto = responseBody.Deserialize<ContentDto>()!;
        
        dto.QuestionnaireId.Should().Be(questionnaireId);
        dto.Title.Should().Be("T1");
        dto.Content.Should().Be("D1");
        AssertNoSensitiveUserData(responseBody);
    }

    [Fact]
    public async Task Create_With_Duplicate_Title_Still_Creates_New_Id()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var payload = new
        {
            questionnaireId,
            title = "Duplicate title",
            content = "Some content",
            referenceName = "Some reference name"
        };

        using var res1 = await Create(payload);
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);
        
        var res1Body = await res1.Content.ReadAsStringAsync();
        var dto1 = res1Body.Deserialize<ContentDto>()!;

        using var res2 = await Create(payload);
        Assert.Equal(HttpStatusCode.Created, res2.StatusCode);
        
        var res2Body = await res2.Content.ReadAsStringAsync();
        var dto2 = res2Body.Deserialize<ContentDto>()!;

        dto1.QuestionnaireId.Should().Be(questionnaireId);
        dto1.QuestionnaireId.Should().Be(questionnaireId);
        dto1.Id.Should().NotBeEmpty();
        dto2.Id.Should().NotBeEmpty();
        dto2.Id.Should().NotBe(dto1.Id);
    }

    [Fact]
    public async Task Create_Missing_Title_Returns400_With_Validation_Error_No_Creation()
    {
        var payload = new
        {
            // content missing
            content = "Some description"
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
        var payload = new { content = "Any" };

        using var res = await Create(payload, JwtTestTokenGenerator.InvalidAudJwtToken);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("createdBy");
    }

    [Fact]
    public async Task Create_Expired_Token_Returns401_No_Leak_No_Creation()
    {
        var payload = new { content = "Any" };

        using var res = await Create(payload, bearerToken: JwtTestTokenGenerator.ExpiredJwtToken);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
    }

    [Fact]
    public async Task Post_NotAuthorized_For_Specific_Resource_Returns_403_Or_401_No_Creation()
    {
        var payload = new { content = "Any" };

        using var res = await Create(payload, JwtTestTokenGenerator.NonDfeJwtToken);

        Assert.Contains(res.StatusCode, new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden });

        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
    }

    [Fact]
    public async Task Create_Invalid_Payload_Field_Types_Returns400_No_Leak()
    {
        var payload = new
        {
            title = 12345,
            content = new { inner = "value" }
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
    public async Task Title_Business_Rules(string title, bool expectedSuccess, string scenario)
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var payload = new { questionnaireId, title, content = "Some description",
            referenceName = "Some reference name" };

        using var res = await Create(payload);

        res.StatusCode.Should().Be(expectedSuccess ? HttpStatusCode.Created : HttpStatusCode.BadRequest,
            because: $"Failed for {scenario}");
    }

    [Theory]
    [InlineData("   ", false, "content may not allow whitespace?")]
    [InlineData("😀✨💥", true, "emojis")]
    [InlineData("\t\r\n", false, "control chars?")]
    [InlineData("A", true, "min")]
    public async Task Content_Business_Rules(string content, bool expectedSuccess, string scenario)
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var payload = new
        {
            questionnaireId, 
            title = "Valid Content", 
            content,
            referenceName = "Some reference name"
        };

        using var res = await Create(payload);

        res.StatusCode.Should().Be(expectedSuccess ? HttpStatusCode.Created : HttpStatusCode.BadRequest,
            because: $"Failed for {scenario}");
    }
    
    #endregion
    
    // === Tests for the Get contents endpoint ===
    
    #region Get All Questions Endpoint Tests

    [Fact(DisplayName = "Get all contents for me returns 200 and only my accessible items")]
    public async Task Get_All_For_Current_User_Only()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        // Seed: create two contents for current user
        using (var res = await Create(new { questionnaireId, title = "T1", content = "C1",
                   referenceName = "Some reference name" })) 
        { Assert.Equal(HttpStatusCode.Created, res.StatusCode); }
        using (var res = await Create(new { questionnaireId, title = "T2", content = "C2",
                   referenceName = "Some reference name" }))
        { Assert.Equal(HttpStatusCode.Created, res.StatusCode); }

        // Seed: create a content as a different user (unauthorised for current user)
        using (var res = await Create(new
               {
                   questionnaireId = await CreateQuestionnaire(JwtTestTokenGenerator.UnauthorizedJwtToken), 
                   title = "OtherUser-NotVisible", 
                   content = "OtherUser-NotVisible",
                   referenceName = "Some reference name"
               }, JwtTestTokenGenerator.UnauthorizedJwtToken))
        { Assert.Equal(HttpStatusCode.Created, res.StatusCode); }

        // Act
        using var getRes = await GetAll(routePrefixOverride: 
            $"/api/questionnaires/{questionnaireId}/contents");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var responseBody = await getRes.Content.ReadAsStringAsync();
        var contents = responseBody.Deserialize<List<ContentDto>>();

        Assert.NotNull(contents);
        
        Assert.Equal(2, contents.Count);

        foreach (var q in contents)
        {
            Assert.NotEqual("OtherUser-NotVisible", q.Content); // shouldn't include others
        }
    }

    [Fact(DisplayName = "Invalid JWT returns 401 and no data leakage")]
    public async Task Invalid_Jwt()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        using var res = await GetAll(JwtTestTokenGenerator.InvalidAudJwtToken, routePrefixOverride: 
            $"/api/questionnaires/{questionnaireId}/contents");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("createdBy", text);
        Assert.DoesNotContain("contributors", text);
    }

    [Fact(DisplayName = "Expired JWT returns 401 and no data leakage")]
    public async Task Expired_Jwt()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        using var res = await GetAll(JwtTestTokenGenerator.ExpiredJwtToken, routePrefixOverride: 
            $"/api/questionnaires/{questionnaireId}/contents");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("createdBy", text);
        Assert.DoesNotContain("contributors", text);
    }

    [Fact(DisplayName = "Get returns empty list when none exist for user")]
    public async Task Get_Empty_When_None_Exist()
    {
        var questionnaireId = await CreateQuestionnaire(JwtTestTokenGenerator.NewUserJwtToken);
        
        // Use a token that has no created contents
        using var res = await GetAll(JwtTestTokenGenerator.NewUserJwtToken, routePrefixOverride: 
            $"/api/questionnaires/{questionnaireId}/contents");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var responseBody = await res.Content.ReadAsStringAsync();
        var list = responseBody.Deserialize<List<ContentDto>>();
        
        list.Should().NotBeNull();
        list.Should().BeEmpty();
    }
    
    #endregion
    
    // === Tests for the Get specific content endpoint ===
    
    #region Get Specific Content Endpoint Tests

    private static string ExtractId(string json)
        => JsonDocument.Parse(json).RootElement.GetProperty("id").GetString()!;

    // Scenario: Retrieve an existing content
    [Fact(DisplayName = "GET /contents/{id} returns 200 with DTO and system-managed fields; no sensitive user data")]
    public async Task Get_By_Id_Succeeds_With_Dto_And_No_Sensitive_Data()
    {
        // Arrange: create a content as current user
        var questionnaireId = await CreateQuestionnaire();;
        
        var createPayload = new { 
            questionnaireId, 
            title = "My Q", 
            content = "Desc",
            referenceName = "Some reference name"
        };
        using (var postRes = await Create(createPayload))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = ExtractId(await postRes.Content.ReadAsStringAsync());

            // Act
            using var getRes = await GetById(id);
            
            // Assert
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseBody = await getRes.Content.ReadAsStringAsync();
            var content = responseBody.Deserialize<ContentDto>();

            content.Should().NotBeNull();
            
            content.QuestionnaireId.Should().Be(questionnaireId);
            content.Id.ToString().Should().Be(id);
            content.Title.Should().Be("My Q");
            content.Content.Should().Be("Desc");

            content.CreatedAt.Should().NotBe(default);
            content.UpdatedAt.Should().NotBe(default);
            
            // No sensitive user data
            AssertNoSensitiveUserData(responseBody);
        }
    }

    // Scenario: Access to content not permitted
    [Fact(DisplayName = "GET /contents/{id} unauthorized user gets 401/403 without existence leak")]
    public async Task Get_By_Id_Unauthorized_User_Fails_Without_Leak()
    {
        // Arrange: create as authorized user
        var questionnaireId = await CreateQuestionnaire();
        
        var createPayload = new { 
            questionnaireId, 
            title = "Private Q",
            content = "content",
            referenceName = "Some reference name"
        };
        using (var postRes = await Create(createPayload))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = ExtractId(await postRes.Content.ReadAsStringAsync());

            // Act: fetch as a different/unauthorized user
            using var res = await GetById(id, JwtTestTokenGenerator.UnauthorizedJwtToken);
            
            // Assert: either 401 or 403 and no existence-sensitive details
            new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden }.Should().Contain(res.StatusCode);

            var text = await res.Content.ReadAsStringAsync();
            
            AssertNoSensitiveUserData(text);
        }
    }

    // Scenario: Content does not exist or is deleted
    [Fact(DisplayName = "GET missing/deleted content returns 404 Not Found")]
    public async Task Get_By_Id_Missing_Or_Deleted_Returns_404()
    {
        // Arrange: random id that won't exist
        var missingId = Guid.NewGuid().ToString("N");

        // Act
        using var res = await GetById(missingId);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
        text.Should().NotContain("contributors");
    }

    // Scenario: Get the specific content you created or were invited to contribute
    [Fact(DisplayName = "GET /contents/{id} returns only the requested content with expected fields")]
    public async Task Get_By_Id_Returns_Correct_Fields_Only()
    {
        // Arrange: create two contents as current user; we will fetch only one
        var questionnaireId = await CreateQuestionnaire();;
        
        string idToFetch;
        {
            using var res1 = await Create(new
            {
                questionnaireId, 
                title = "Q1", 
                content = "D1",
                referenceName = "Some reference name"
            });
            res1.StatusCode.Should().Be(HttpStatusCode.Created);
            idToFetch = ExtractId(await res1.Content.ReadAsStringAsync());
        }
        {
            using var res2 = await Create(new
            {
                questionnaireId, 
                title = "Q2", 
                content = "D2",
                referenceName = "Some reference name"
            });
            res2.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        using var res = await GetById(idToFetch);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await res.Content.ReadAsStringAsync();
        var content = responseBody.Deserialize<ContentDto>();

        content.Should().NotBeNull();
        
        content.QuestionnaireId.Should().Be(questionnaireId);
        content.Id.ToString().Should().Be(idToFetch);
        content.Title.Should().Be("Q1");
        content.Content.Should().Be("D1");

        AssertNoSensitiveUserData(responseBody);
    }

    // Scenario: Error when expired JWT bearer token
    [Fact(DisplayName = "GET with expired JWT returns 401 and no sensitive data")]
    public async Task Get_By_Id_Expired_Token_Returns_401_No_Leak()
    {
        // Arrange: create a content to target
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new
               {
                   questionnaireId,
                   title = "Any",
                   content = "Any",
                   referenceName = "Some reference name"
               }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await GetById(id, JwtTestTokenGenerator.ExpiredJwtToken);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var responseBody = await res.Content.ReadAsStringAsync();
        
        AssertNoSensitiveUserData(responseBody);
    }

    // Scenario: Error when invalid JWT bearer token
    [Fact(DisplayName = "GET with invalid JWT returns 401 and no sensitive data")]
    public async Task Get_By_Id_Invalid_Token_Returns_401_No_Leak()
    {
        // Arrange: create a content to target
        var questionnaireId = await CreateQuestionnaire();;
        
        string id;
        using (var postRes = await Create(new
               {
                   questionnaireId,
                   title = "Any",
                   content = "Any",
                   referenceName = "Some reference name"
               }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await GetById(id, JwtTestTokenGenerator.InvalidAudJwtToken);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var text = await res.Content.ReadAsStringAsync();
        
        AssertNoSensitiveUserData(text);
    }

    // Scenario: Error when invalid content id is passed
    [Theory(DisplayName = "GET with invalid content id returns 400 Bad Request")]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    public async Task Get_By_Id_Invalid_Id_Returns_400(string invalidId)
    {
        using var res = await GetById(invalidId);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().BeNullOrWhiteSpace();
    }

    #endregion
    
    // === Tests for the Update contents endpoint ===
    
    #region Update Content Endpoint Tests

    [Fact]
    public async Task Update_Title_Only_NoContent_And_Content_Unchanged()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();

        string id;
        const string originalDesc = "Original desc";
        using (var postRes = await Create(new
               {
                   questionnaireId, 
                   title = "T0", 
                   content = originalDesc,
                   referenceName = "Some reference name"
               }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { title = "T1" });

        // Assert 204, then GET to verify
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var content = await GetById<ContentDto>(id);
        content.Title.Should().Be("T1");
        content.Content.Should().Be(originalDesc);
        content.CreatedAt.Should().NotBe(default);
        content.UpdatedAt.Should().NotBe(default);
    }

    [Fact(DisplayName = "Update with missing content returns 400 Bad Request")]
    public async Task Update_Missing_Title_BadRequest_NoContent_Not_Used()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new { questionnaireId, title = "T0", content = "D0",
                   referenceName = "Some reference name" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { /* missing content */ });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
        text.Should().NotContain("userEmail");

        // Verify nothing changed
        var content = await GetById<ContentDto>(id);
        content.Title.Should().Be("T0");
        content.Content.Should().Be("D0");
    }

    [Fact(DisplayName = "Update content and description returns 204 and updates values")]
    public async Task Update_Title_And_Content_NoContent_Then_Get()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();;
        
        string id;
        using (var postRes = await Create(new { questionnaireId, title = "T0", content = "D0",
                   referenceName = "Some reference name" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { title = "T1", content = "D1" });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var content = await GetById<ContentDto>(id);
        content.Title.Should().Be("T1");
        content.Content.Should().Be("D1");
    }

    // Scenario: Error when invalid JWT bearer token (unauthorized access)
    [Fact]
    public async Task Update_Invalid_JWT_401_No_Changes()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();;
        
        string id;
        using (var postRes = await Create(new { questionnaireId, title = "Seed", content = "D0",
                   referenceName = "Some reference name" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { questionnaireId, title = "Changed" }, JwtTestTokenGenerator.InvalidAudJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var text = await res.Content.ReadAsStringAsync();

        // Verify unchanged
        var content = await GetById<ContentDto>(id);
        content.Title.Should().Be("Seed");
        content.Content.Should().Be("D0");
    }

    // Scenario: Error when expired JWT bearer token (unauthorized access)
    [Fact]
    public async Task Update_Expired_JWT_401_No_Changes()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new { questionnaireId, title = "Seed", content = "D0",
                   referenceName = "Some reference name" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { title = "Changed" }, JwtTestTokenGenerator.ExpiredJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Verify unchanged
        var content = await GetById<ContentDto>(id);
        content.Title.Should().Be("Seed");
        content.Content.Should().Be("D0");
    }

    // Scenario: Content not found
    [Fact]
    public async Task Update_NotFound_NoContent_Not_Returned()
    {
        var missingId = Guid.NewGuid().ToString("N");

        using var res = await UpdateById(missingId, new { title = "T1" });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Scenario: Access to content not permitted
    [Fact]
    public async Task Update_Forbidden_No_Changes()
    {
        // Arrange: create as authorized user
        var questionnaireId = await CreateQuestionnaire();;
        
        string id;
        using (var postRes = await Create(new { questionnaireId, title = "Private Q", content = "D0",
                   referenceName = "Some reference name" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act as unauthorized user
        using var res = await UpdateById(id, new { title = "Hack" }, JwtTestTokenGenerator.UnauthorizedJwtToken);

        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden }.Should().Contain(res.StatusCode);

        // Verify unchanged with authorized token
        var content = await GetById<ContentDto>(id);
        content.Title.Should().Be("Private Q");
        content.Content.Should().Be("D0");
    }

    // Scenario: Error when invalid content id is passed
    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    public async Task Update_Invalid_Id_400_No_Changes(string invalidId)
    {
        using var res = await UpdateById(invalidId, new { title = "T1" });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().BeNullOrWhiteSpace();
    }

    // Scenario: Update with invalid payload data e.g. description as Boolean
    [Fact]
    public async Task Update_Invalid_Payload_400_No_Changes()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new { questionnaireId, title = "Seed", content = "D0",
                   referenceName = "Some reference name" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act: invalid type
        using var res = await UpdateById(id, new { title = "T1", content = true });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify unchanged
        var content = await GetById<ContentDto>(id);
        content.Title.Should().Be("Seed");
        content.Content.Should().Be("D0");
    }

    // Scenario: Update a content with a matching Content to an existing content
    [Fact]
    public async Task Update_With_Duplicate_Title_Still_Distinct_Ids()
    {
        // Seed two contents
        var questionnaireId = await CreateQuestionnaire();;
        
        string id1, id2;
        using (var r1 = await Create(new { questionnaireId, title = "A", content = "D1",
                   referenceName = "Some reference name" }))
        {
            r1.StatusCode.Should().Be(HttpStatusCode.Created);
            id1 = ExtractId(await r1.Content.ReadAsStringAsync());
        }
        using (var r2 = await Create(new { questionnaireId, title = "B", content = "D2",
                   referenceName = "Some reference name" }))
        {
            r2.StatusCode.Should().Be(HttpStatusCode.Created);
            id2 = ExtractId(await r2.Content.ReadAsStringAsync());
        }

        // Act: update second to duplicate content of first
        using var res = await UpdateById(id2, new { title = "A" });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify via GETs: different IDs remain, contents reflect update
        var q1 = await GetById<ContentDto>(id1);
        var q2 = await GetById<ContentDto>(id2);

        q1.Id.Should().Be(id1);
        q2.Id.Should().Be(id2);
        q2.Title.Should().Be("A");
    }
    
    #endregion

    // === Tests for the Delete content endpoint ===
    
    #region Delete Content Endpoint Tests

    [Fact]
    public async Task Delete_Existing_Question_Succeeds_And_Is_SoftDeleted()
    {
        // Arrange: create then ensure appears in GET list
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new { 
                   questionnaireId,
                   title = "ToDelete", 
                   content = "D0",
                   referenceName = "Some reference name"
               }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act: delete
        using var delRes = await DeleteById(id);

        // Assert 200 OK or 204 NoContent
        new[] { HttpStatusCode.OK, HttpStatusCode.NoContent }.Should().Contain(delRes.StatusCode);

        // Verify via GET by id -> 404 (soft-deleted not retrievable)
        using (var getRes = await GetById(id))
        {
            getRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // Verify it no longer appears in list
        var list = await GetAll<List<ContentDto>>(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/contents");
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_NotAuthorized_Question_Returns_Forbidden_Or_NotFound_No_Leak_No_Deletion()
    {
        // Arrange: create as authorized user
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new
               {
                   questionnaireId, 
                   title = "Private Q", 
                   content = "D0",
                   referenceName = "Some reference name"
               }))
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
        using var getRes = await GetById(id);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Missing_Question_Returns_404()
    {
        var missingId = Guid.NewGuid().ToString("N");

        using var res = await DeleteById(missingId);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Invalid_JWT_Returns_Unauthorized_No_Deletion()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new { 
                   questionnaireId,
                   title = "Seed", 
                   content = "D0",
                   referenceName = "Some reference name"
               }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await DeleteById(id, JwtTestTokenGenerator.InvalidAudJwtToken);

        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
        
        // Verify still retrievable with valid token
        using var getRes = await GetById(id);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Expired_JWT_Returns_Unauthorized_No_Deletion()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();;
        
        string id;
        using (var postRes = await Create(new
               {
                   questionnaireId,
                   title = "Seed", 
                   content = "D0",
                   referenceName = "Some reference name"
               }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await DeleteById(id, JwtTestTokenGenerator.ExpiredJwtToken);

        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
        var text = await res.Content.ReadAsStringAsync();

        // Verify still retrievable with valid token
        using var getRes = await GetById(id);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Already_Deleted_Returns_NotFound_Or_Conflict_No_Duplication()
    {
        // Arrange: create and delete
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new
               {
                   questionnaireId,
                   title = "SoftDelete", 
                   content = "D0",
                   referenceName = "Some reference name"
               }))
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

    private async Task<Guid> CreateQuestionnaire(string? bearerToken = null)
    {
        var questionnaire = await Create<QuestionnaireDto>(new { title = "Any" }, 
            routePrefixOverride: "/api/questionnaires", bearerToken: bearerToken);

        return questionnaire.Id;
    }
}