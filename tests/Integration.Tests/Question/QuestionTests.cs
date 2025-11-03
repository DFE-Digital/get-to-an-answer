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

namespace Integration.Tests.Question;

public class QuestionTests(ApiFixture factory) : 
    ControllerTests(factory, "/api/questions")
{
    private static void AssertNoSensitiveUserData(string responseBody)
    {
        // Ensure no sensitive fields are present
        responseBody.Should().NotContain("createdBy");
        responseBody.Should().NotContain("contributors");
    }
    
    #region Create Question Endpoint Tests

    [Fact]
    public async Task Create_With_Content_Only_Returns201_And_Dto_Without_Sensitive_Data()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var payload = new
        {
            questionnaireId,
            content = "My Question",
            type = QuestionType.DropdownSelect,
        };

        using var res = await Create(payload);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var responseBody = await res.Content.ReadAsStringAsync();
        var dto = responseBody.Deserialize<QuestionDto>()!;

        dto.QuestionnaireId.Should().Be(questionnaireId);
        dto.Id.Should().NotBeEmpty(because: "id should be auto-generated");
        dto.Content.Should().Be("My Question");
        dto.CreatedAt.Should().NotBe(default);
        dto.UpdatedAt.Should().NotBe(default);

        AssertNoSensitiveUserData(responseBody);
    }

    [Fact]
    public async Task Create_With_Content_And_Description_Returns201_And_Dto_Has_Inputted_Values()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var payload = new
        {
            questionnaireId,
            content = "T1",
            description = "D1",
            type = QuestionType.SingleSelect
        };

        using var res = await Create(payload);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var responseBody = await res.Content.ReadAsStringAsync();
        var dto = responseBody.Deserialize<QuestionDto>()!;
        
        dto.QuestionnaireId.Should().Be(questionnaireId);
        dto.Content.Should().Be("T1");
        dto.Description.Should().Be("D1");
        AssertNoSensitiveUserData(responseBody);
    }

    [Fact]
    public async Task Create_With_Duplicate_Content_Still_Creates_New_Id()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var payload = new
        {
            questionnaireId,
            content = "Duplicate Content",
            type = QuestionType.MultiSelect
        };

        using var res1 = await Create(payload);
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);
        
        var res1Body = await res1.Content.ReadAsStringAsync();
        var dto1 = res1Body.Deserialize<QuestionDto>()!;

        using var res2 = await Create(payload);
        Assert.Equal(HttpStatusCode.Created, res2.StatusCode);
        
        var res2Body = await res2.Content.ReadAsStringAsync();
        var dto2 = res2Body.Deserialize<QuestionDto>()!;

        dto1.QuestionnaireId.Should().Be(questionnaireId);
        dto1.QuestionnaireId.Should().Be(questionnaireId);
        dto1.Id.Should().NotBeEmpty();
        dto2.Id.Should().NotBeEmpty();
        dto2.Id.Should().NotBe(dto1.Id);
    }

    [Fact]
    public async Task Create_Missing_Content_Returns400_With_Validation_Error_No_Creation()
    {
        var payload = new
        {
            // content missing
            description = "Some description"
        };

        using var res = await Create(payload);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        text.Should().Contain("content");
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
    public async Task Create_Multiple_Question_Questionnaire_Returns_Ordered_List_Of_Questions()
    {
        var questionnaireId = await CreateQuestionnaire();

        var typeList = GeneralExtensions.ToList<QuestionType>(2);

        foreach (var type in typeList) 
        {
            var payload = new
            {
                questionnaireId,
                content = "Any",
                type
            };

            await Create(payload);
        }
        
        var questions = await GetAll<List<QuestionDto>>(routePrefixOverride: 
            $"/api/questionnaires/{questionnaireId}/questions");
        
        questions.Should().NotBeEmpty();
        questions.Should().HaveCount(typeList.Count);

        for (var i = 0; i < typeList.Count; i++)
        {
            questions[i].Type.Should().Be(typeList[i]);
            questions[i].Order.Should().Be(i+1);
        }
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
            content = 12345,
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
    public async Task Content_Business_Rules(string content, bool expectedSuccess, string scenario)
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var payload = new { questionnaireId, content, type = QuestionType.MultiSelect };

        using var res = await Create(payload);

        res.StatusCode.Should().Be(expectedSuccess ? HttpStatusCode.Created : HttpStatusCode.BadRequest,
            because: $"Failed for {scenario}");
    }

    [Theory]
    [InlineData("   ", true, "description may allow whitespace?")]
    [InlineData("😀✨💥", true, "emojis")]
    [InlineData("\t\r\n", true, "control chars?")]
    [InlineData("A", true, "min")]
    public async Task Description_Business_Rules(string description, bool expectedSuccess, string scenario)
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var payload = new
        {
            questionnaireId, 
            content = "Valid Content", 
            description,
            type = QuestionType.MultiSelect,
        };

        using var res = await Create(payload);

        res.StatusCode.Should().Be(expectedSuccess ? HttpStatusCode.Created : HttpStatusCode.BadRequest,
            because: $"Failed for {scenario}");
    }
    
    #endregion
    
    // === Tests for the Get questions endpoint ===
    
    #region Get All Questions Endpoint Tests

    [Fact(DisplayName = "Get all questions for me returns 200 and only my accessible items")]
    public async Task Get_All_For_Current_User_Only()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        // Seed: create two questions for current user
        using (var res = await Create(new { questionnaireId, content = "Q1", description = "D1", type = QuestionType.MultiSelect })) 
        { Assert.Equal(HttpStatusCode.Created, res.StatusCode); }
        using (var res = await Create(new { questionnaireId, content = "Q2", type = QuestionType.SingleSelect }))
        { Assert.Equal(HttpStatusCode.Created, res.StatusCode); }

        // Seed: create a question as a different user (unauthorised for current user)
        using (var res = await Create(new
               {
                   questionnaireId = await CreateQuestionnaire(JwtTestTokenGenerator.UnauthorizedJwtToken), 
                   content = "OtherUser-NotVisible", 
                   type = QuestionType.DropdownSelect
               }, JwtTestTokenGenerator.UnauthorizedJwtToken))
        { Assert.Equal(HttpStatusCode.Created, res.StatusCode); }

        // Act
        using var getRes = await GetAll(routePrefixOverride: 
            $"/api/questionnaires/{questionnaireId}/questions");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var responseBody = await getRes.Content.ReadAsStringAsync();
        var questions = responseBody.Deserialize<List<QuestionDto>>();

        Assert.NotNull(questions);
        
        Assert.Equal(2, questions.Count);

        foreach (var q in questions)
        {
            Assert.NotEqual("OtherUser-NotVisible", q.Content); // shouldn't include others
        }
    }

    [Fact(DisplayName = "Invalid JWT returns 401 and no data leakage")]
    public async Task Invalid_Jwt()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        using var res = await GetAll(JwtTestTokenGenerator.InvalidAudJwtToken, routePrefixOverride: 
            $"/api/questionnaires/{questionnaireId}/questions");

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
            $"/api/questionnaires/{questionnaireId}/questions");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("createdBy", text);
        Assert.DoesNotContain("contributors", text);
    }

    [Fact(DisplayName = "Get returns empty list when none exist for user")]
    public async Task Get_Empty_When_None_Exist()
    {
        var questionnaireId = await CreateQuestionnaire(JwtTestTokenGenerator.NewUserJwtToken);
        
        // Use a token that has no created questions
        using var res = await GetAll(JwtTestTokenGenerator.NewUserJwtToken, routePrefixOverride: 
            $"/api/questionnaires/{questionnaireId}/questions");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var responseBody = await res.Content.ReadAsStringAsync();
        var list = responseBody.Deserialize<List<QuestionDto>>();
        
        list.Should().NotBeNull();
        list.Should().BeEmpty();
    }
    
    #endregion
    
    // === Tests for the Get specific question endpoint ===
    
    #region Get Specific Question Endpoint Tests

    private static string ExtractId(string json)
        => JsonDocument.Parse(json).RootElement.GetProperty("id").GetString()!;

    // Scenario: Retrieve an existing question
    [Fact(DisplayName = "GET /questions/{id} returns 200 with DTO and system-managed fields; no sensitive user data")]
    public async Task Get_By_Id_Succeeds_With_Dto_And_No_Sensitive_Data()
    {
        // Arrange: create a question as current user
        var questionnaireId = await CreateQuestionnaire();;
        
        var createPayload = new { 
            questionnaireId, 
            content = "My Q", 
            description = "Desc",
            type = QuestionType.MultiSelect, 
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
            var question = responseBody.Deserialize<QuestionDto>();

            question.Should().NotBeNull();
            
            question.QuestionnaireId.Should().Be(questionnaireId);
            question.Id.ToString().Should().Be(id);
            question.Content.Should().Be("My Q");
            question.Description.Should().Be("Desc");

            question.CreatedAt.Should().NotBe(default);
            question.UpdatedAt.Should().NotBe(default);
            
            // No sensitive user data
            AssertNoSensitiveUserData(responseBody);
        }
    }

    // Scenario: Access to question not permitted
    [Fact(DisplayName = "GET /questions/{id} unauthorized user gets 401/403 without existence leak")]
    public async Task Get_By_Id_Unauthorized_User_Fails_Without_Leak()
    {
        // Arrange: create as authorized user
        var questionnaireId = await CreateQuestionnaire();;
        
        var createPayload = new { 
            questionnaireId, 
            content = "Private Q",
            type = QuestionType.MultiSelect, 
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

    // Scenario: Question does not exist or is deleted
    [Fact(DisplayName = "GET missing/deleted question returns 404 Not Found")]
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

    // Scenario: Get the specific question you created or were invited to contribute
    [Fact(DisplayName = "GET /questions/{id} returns only the requested question with expected fields")]
    public async Task Get_By_Id_Returns_Correct_Fields_Only()
    {
        // Arrange: create two questions as current user; we will fetch only one
        var questionnaireId = await CreateQuestionnaire();;
        
        string idToFetch;
        {
            using var res1 = await Create(new
            {
                questionnaireId, 
                content = "Q1", 
                description = "D1",
                type = QuestionType.MultiSelect,
            });
            res1.StatusCode.Should().Be(HttpStatusCode.Created);
            idToFetch = ExtractId(await res1.Content.ReadAsStringAsync());
        }
        {
            using var res2 = await Create(new
            {
                questionnaireId, 
                content = "Q2", 
                description = "D2",
                type = QuestionType.SingleSelect,
            });
            res2.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        using var res = await GetById(idToFetch);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await res.Content.ReadAsStringAsync();
        var question = responseBody.Deserialize<QuestionDto>();

        question.Should().NotBeNull();
        
        question.QuestionnaireId.Should().Be(questionnaireId);
        question.Id.ToString().Should().Be(idToFetch);
        question.Content.Should().Be("Q1");
        question.Description.Should().Be("D1");

        AssertNoSensitiveUserData(responseBody);
    }

    // Scenario: Error when expired JWT bearer token
    [Fact(DisplayName = "GET with expired JWT returns 401 and no sensitive data")]
    public async Task Get_By_Id_Expired_Token_Returns_401_No_Leak()
    {
        // Arrange: create a question to target
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new
               {
                   questionnaireId,
                   content = "Any",
                   type = QuestionType.MultiSelect,
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
        // Arrange: create a question to target
        var questionnaireId = await CreateQuestionnaire();;
        
        string id;
        using (var postRes = await Create(new
               {
                   questionnaireId,
                   content = "Any",
                   type = QuestionType.MultiSelect,
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

    // Scenario: Error when invalid question id is passed
    [Theory(DisplayName = "GET with invalid question id returns 400 Bad Request")]
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
    
    // === Tests for the Update questions endpoint ===
    
    #region Update Question Endpoint Tests

    [Fact]
    public async Task Update_Content_Only_NoContent_And_Description_Unchanged()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();

        string id;
        const string originalDesc = "Original desc";
        using (var postRes = await Create(new
               {
                   questionnaireId, 
                   content = "T0", 
                   description = originalDesc, 
                   type = QuestionType.MultiSelect
               }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { content = "T1" });

        // Assert 204, then GET to verify
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var question = await GetById<QuestionDto>(id);
        question.Content.Should().Be("T1");
        question.Description.Should().Be(originalDesc);
        question.CreatedAt.Should().NotBe(default);
        question.UpdatedAt.Should().NotBe(default);
    }

    [Fact(DisplayName = "Update with missing content returns 400 Bad Request")]
    public async Task Update_Missing_Content_BadRequest_NoContent_Not_Used()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new { questionnaireId, content = "T0", description = "D0", type = QuestionType.MultiSelect }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { /* missing content */ });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().Contain("content");
        text.Should().NotContain("userId");
        text.Should().NotContain("userEmail");

        // Verify nothing changed
        var question = await GetById<QuestionDto>(id);
        question.Content.Should().Be("T0");
        question.Description.Should().Be("D0");
    }
    
    [Fact(DisplayName = "Update with invalid question type returns 400 Bad Request")]
    public async Task Update_Invalid_Question_Type_BadRequest()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new { questionnaireId, content = "T0", description = "D0", type = QuestionType.MultiSelect }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { content = "T0", type = (QuestionType)123 });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().NotContain("userId");
        text.Should().NotContain("userEmail");

        // Verify nothing changed
        var question = await GetById<QuestionDto>(id);
        question.Content.Should().Be("T0");
        question.Description.Should().Be("D0");
    }

    [Fact(DisplayName = "Update content and description returns 204 and updates values")]
    public async Task Update_Content_And_Description_NoContent_Then_Get()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();;
        
        string id;
        using (var postRes = await Create(new { questionnaireId, content = "T0", description = "D0", type = QuestionType.MultiSelect }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { content = "T1", description = "D1", type = QuestionType.MultiSelect });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var question = await GetById<QuestionDto>(id);
        question.Content.Should().Be("T1");
        question.Description.Should().Be("D1");
    }

    // Scenario: Error when invalid JWT bearer token (unauthorized access)
    [Fact]
    public async Task Update_Invalid_JWT_401_No_Changes()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();;
        
        string id;
        using (var postRes = await Create(new { questionnaireId, content = "Seed", description = "D0", type = QuestionType.MultiSelect }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { questionnaireId, content = "Changed", type = QuestionType.MultiSelect }, JwtTestTokenGenerator.InvalidAudJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var text = await res.Content.ReadAsStringAsync();

        // Verify unchanged
        var question = await GetById<QuestionDto>(id);
        question.Content.Should().Be("Seed");
        question.Description.Should().Be("D0");
    }

    // Scenario: Error when expired JWT bearer token (unauthorized access)
    [Fact]
    public async Task Update_Expired_JWT_401_No_Changes()
    {
        // Arrange
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new { questionnaireId, content = "Seed", description = "D0", type = QuestionType.MultiSelect }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act
        using var res = await UpdateById(id, new { content = "Changed" }, JwtTestTokenGenerator.ExpiredJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Verify unchanged
        var question = await GetById<QuestionDto>(id);
        question.Content.Should().Be("Seed");
        question.Description.Should().Be("D0");
    }

    // Scenario: Question not found
    [Fact]
    public async Task Update_NotFound_NoContent_Not_Returned()
    {
        var missingId = Guid.NewGuid().ToString("N");

        using var res = await UpdateById(missingId, new { content = "T1" });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Scenario: Access to question not permitted
    [Fact]
    public async Task Update_Forbidden_No_Changes()
    {
        // Arrange: create as authorized user
        var questionnaireId = await CreateQuestionnaire();;
        
        string id;
        using (var postRes = await Create(new { questionnaireId, content = "Private Q", description = "D0", type = QuestionType.MultiSelect }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act as unauthorized user
        using var res = await UpdateById(id, new { content = "Hack" }, JwtTestTokenGenerator.UnauthorizedJwtToken);

        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden }.Should().Contain(res.StatusCode);

        // Verify unchanged with authorized token
        var question = await GetById<QuestionDto>(id);
        question.Content.Should().Be("Private Q");
        question.Description.Should().Be("D0");
    }

    // Scenario: Error when invalid question id is passed
    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    public async Task Update_Invalid_Id_400_No_Changes(string invalidId)
    {
        using var res = await UpdateById(invalidId, new { content = "T1" });

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
        using (var postRes = await Create(new { questionnaireId, content = "Seed", description = "D0", type = QuestionType.MultiSelect }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        // Act: invalid type
        using var res = await UpdateById(id, new { content = "T1", description = true });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify unchanged
        var question = await GetById<QuestionDto>(id);
        question.Content.Should().Be("Seed");
        question.Description.Should().Be("D0");
    }

    // Scenario: Update a question with a matching Content to an existing question
    [Fact]
    public async Task Update_With_Duplicate_Content_Still_Distinct_Ids()
    {
        // Seed two questions
        var questionnaireId = await CreateQuestionnaire();;
        
        string id1, id2;
        using (var r1 = await Create(new { questionnaireId, content = "A", description = "D1", type = QuestionType.MultiSelect }))
        {
            r1.StatusCode.Should().Be(HttpStatusCode.Created);
            id1 = ExtractId(await r1.Content.ReadAsStringAsync());
        }
        using (var r2 = await Create(new { questionnaireId, content = "B", description = "D2", type = QuestionType.MultiSelect }))
        {
            r2.StatusCode.Should().Be(HttpStatusCode.Created);
            id2 = ExtractId(await r2.Content.ReadAsStringAsync());
        }

        // Act: update second to duplicate content of first
        using var res = await UpdateById(id2, new { content = "A" });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify via GETs: different IDs remain, contents reflect update
        var q1 = await GetById<QuestionDto>(id1);
        var q2 = await GetById<QuestionDto>(id2);

        q1.Id.Should().Be(id1);
        q2.Id.Should().Be(id2);
        q2.Content.Should().Be("A");
    }
    
    #endregion
    
    // === Tests for the Move question order up endpoint ===
    
    #region Move Question Order Up Tests

    [Fact]
    public async Task MoveUp_Question_In_Middle_Position_Decrements_Order()
    {
        // Arrange: Create questionnaire with 3 questions
        var questionnaireId = await CreateQuestionnaire();
        var questions = new List<QuestionDto>();

        for (var i = 0; i < 3; i++)
        {
            var payload = new
            {
                questionnaireId,
                content = $"Q{i}",
                type = QuestionType.MultiSelect
            };

            var question = await Create<QuestionDto>(payload);
            questions.Add(question);
        }

        // Act: Move middle question up
        using var moveRes = await Patch(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions/{questions[1].Id}/move-up");

        // Assert
        moveRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify updated orders via GET
        var updatedQuestions = await GetAll<List<QuestionDto>>(routePrefixOverride: 
            $"/api/questionnaires/{questionnaireId}/questions");

        updatedQuestions.Should().BeInAscendingOrder(q => q.Order);
        updatedQuestions.Single(q => q.Id == questions[1].Id).Order.Should().Be(1);
        updatedQuestions.Single(q => q.Id == questions[0].Id).Order.Should().Be(2);
    }

    [Fact] 
    public async Task MoveUp_First_Question_Returns_BadRequest()
    {
        var questionnaireId = await CreateQuestionnaire();
        var payload = new
        {
            questionnaireId,
            content = "Q1",
            type = QuestionType.MultiSelect
        };

        var question = await Create<QuestionDto>(payload);

        using var moveRes = await Patch(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions/{question.Id}/move-up");

        moveRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MoveUp_Invalid_Id_Returns_NotFound()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var invalidId = Guid.NewGuid();
        using var res = await Patch(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions/{invalidId}/move-up");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MoveUp_Invalid_Token_Returns_Unauthorized()
    {
        var questionnaireId = await CreateQuestionnaire();
        var payload = new
        {
            questionnaireId,
            content = "Q1",
            type = QuestionType.MultiSelect
        };

        var question = await Create<QuestionDto>(payload);

        using var res = await Patch(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions/{question.Id}/move-up",
            bearerToken: JwtTestTokenGenerator.InvalidAudJwtToken);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MoveUp_Unauthorized_User_Returns_Forbidden()
    {
        var questionnaireId = await CreateQuestionnaire();
        var payload = new
        {
            questionnaireId,
            content = "Q1", 
            type = QuestionType.MultiSelect
        };

        using var createRes = await Create(payload);
        var question = (await createRes.Content.ReadAsStringAsync()).Deserialize<QuestionDto>()!;

        using var res = await Patch(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions/{question.Id}/move-up",
            bearerToken: JwtTestTokenGenerator.UnauthorizedJwtToken);
        new[] { HttpStatusCode.Forbidden, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
    }
    
    #endregion
    
    // === Tests for the Move question order down endpoint ===
    
    #region Move Question Order Down Tests

    [Fact]
    public async Task MoveDown_Question_In_Middle_Position_Decrements_Order()
    {
        // Arrange: Create questionnaire with 3 questions
        var questionnaireId = await CreateQuestionnaire();
        var questions = new List<QuestionDto>();

        for (var i = 0; i < 3; i++)
        {
            var payload = new
            {
                questionnaireId,
                content = $"Q{i}",
                type = QuestionType.MultiSelect
            };

            var question = await Create<QuestionDto>(payload);
            questions.Add(question);
        }

        // Act: Move middle question up
        using var moveRes = await Patch(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions/{questions[1].Id}/move-down");

        // Assert
        moveRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify updated orders via GET
        var updatedQuestions = await GetAll<List<QuestionDto>>(routePrefixOverride: 
            $"/api/questionnaires/{questionnaireId}/questions");

        updatedQuestions.Should().BeInAscendingOrder(q => q.Order);
        updatedQuestions.Single(q => q.Id == questions[1].Id).Order.Should().Be(3);
        updatedQuestions.Single(q => q.Id == questions[2].Id).Order.Should().Be(2);
    }

    [Fact] 
    public async Task MoveDown_Last_Question_Returns_BadRequest()
    {
        var questionnaireId = await CreateQuestionnaire();
        var payload = new
        {
            questionnaireId,
            content = "Q1",
            type = QuestionType.MultiSelect
        };

        var question = await Create<QuestionDto>(payload);

        using var moveRes = await Patch(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions/{question.Id}/move-down");

        moveRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MoveDown_Invalid_Id_Returns_NotFound()
    {
        var questionnaireId = await CreateQuestionnaire();
        
        var invalidId = Guid.NewGuid();
        using var res = await Patch(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions/{invalidId}/move-down");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MoveDown_Invalid_Token_Returns_Unauthorized()
    {
        var questionnaireId = await CreateQuestionnaire();
        var payload = new
        {
            questionnaireId,
            content = "Q1",
            type = QuestionType.MultiSelect
        };

        var question = await Create<QuestionDto>(payload);

        using var res = await Patch(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions/{question.Id}/move-down",
            bearerToken: JwtTestTokenGenerator.InvalidAudJwtToken);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MoveDown_Unauthorized_User_Returns_Forbidden()
    {
        var questionnaireId = await CreateQuestionnaire();
        var payload = new
        {
            questionnaireId,
            content = "Q1", 
            type = QuestionType.MultiSelect
        };

        using var createRes = await Create(payload);
        var question = (await createRes.Content.ReadAsStringAsync()).Deserialize<QuestionDto>()!;

        using var res = await Patch(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions/{question.Id}/move-down",
            bearerToken: JwtTestTokenGenerator.UnauthorizedJwtToken);
        new[] { HttpStatusCode.Forbidden, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
    }

    #endregion

    // === Tests for the Delete question endpoint ===
    
    #region Delete Question Endpoint Tests

    [Fact]
    public async Task Delete_Existing_Question_Succeeds_And_Is_SoftDeleted()
    {
        // Arrange: create then ensure appears in GET list
        var questionnaireId = await CreateQuestionnaire();
        
        string id;
        using (var postRes = await Create(new { 
                   questionnaireId,
                   content = "ToDelete", 
                   description = "D0",
                   type = QuestionType.MultiSelect, 
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
        var list = await GetAll<List<QuestionDto>>(routePrefixOverride: $"/api/questionnaires/{questionnaireId}/questions");
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
                   content = "Private Q", 
                   description = "D0",
                   type = QuestionType.MultiSelect,
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
                   content = "Seed", 
                   description = "D0",
                   type = QuestionType.MultiSelect, 
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
                   content = "Seed", 
                   description = "D0",
                   type = QuestionType.MultiSelect,
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
                   content = "SoftDelete", 
                   description = "D0",
                   type = QuestionType.MultiSelect,
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