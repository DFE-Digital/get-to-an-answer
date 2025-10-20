using System.Net;
using System.Text.Json;
using Common.Domain;
using Common.Enum;
using FluentAssertions;
using Integration.Tests.Fake;
using Integration.Tests.Fixture;
using Integration.Tests.Util;

namespace Integration.Tests.Answer;

public class AnswerTests(ApiFixture factory) :
    ControllerTests(factory, "/api/answers")
{
    private static string ExtractId(string json)
        => JsonDocument.Parse(json).RootElement.GetProperty("id").GetString()!;

    private async Task<Guid> CreateQuestionnaire(string? bearerToken = null)
    {
        var questionnaire = await Create<QuestionnaireDto>(new { title = "Any" },
            routePrefixOverride: "/api/questionnaires", bearerToken: bearerToken);
        return questionnaire.Id;
    }

    private async Task<Guid> CreateQuestion(Guid questionnaireId, string? bearerToken = null)
    {
        var question = await Create<QuestionDto>(new
            {
                questionnaireId,
                content = "Q",
                type = QuestionType.MultiSelect
            },
            routePrefixOverride: "/api/questions",
            bearerToken: bearerToken);
        return question.Id;
    }

    #region Create Answer Endpoint Tests

    [Fact]
    public async Task Create_With_Minimal_Fields_Returns200_And_Dto()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        var payload = new
        {
            questionnaireId,
            questionId,
            content = "A1"
        };

        using var res = await Create(payload);

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadAsStringAsync();
        var dto = body.Deserialize<AnswerDto>()!;

        dto.Id.Should().NotBeEmpty();
        dto.QuestionnaireId.Should().Be(questionnaireId);
        dto.QuestionId.Should().Be(questionId);
        dto.Content.Should().Be("A1");
        dto.CreatedAt.Should().NotBe(default);
        dto.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Create_With_All_Fields_Returns200_And_Dto_Has_Inputted_Values()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        var payload = new
        {
            questionnaireId,
            questionId,
            content = "Answer X",
            description = "Desc X",
            score = 5,
            destinationUrl = "https://url.com",
            destinationType = DestinationType.ExternalLink,
            destinationQuestionId = (Guid?)null
        };

        using var res = await Create(payload);

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = (await res.Content.ReadAsStringAsync()).Deserialize<AnswerDto>()!;
        dto.Content.Should().Be("Answer X");
        dto.Description.Should().Be("Desc X");
        dto.Score.Should().Be(5);
        dto.DestinationUrl.Should().Be("https://url.com");
        dto.DestinationType.Should().Be(DestinationType.ExternalLink);
        dto.DestinationQuestionId.Should().BeNull();
    }

    [Fact]
    public async Task Create_Invalid_Token_Returns401_No_Creation()
    {
        var payload = new
        {
            questionnaireId = Guid.NewGuid(),
            questionId = Guid.NewGuid(),
            content = "A"
        };

        using var res = await Create(payload, JwtTestTokenGenerator.InvalidAudJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_Expired_Token_Returns401_No_Creation()
    {
        var payload = new
        {
            questionnaireId = Guid.NewGuid(),
            questionId = Guid.NewGuid(),
            content = "A"
        };

        using var res = await Create(payload, JwtTestTokenGenerator.ExpiredJwtToken);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_No_Access_To_Question_Returns_Forbidden_Or_NotFound()
    {
        var questionnaireId = await CreateQuestionnaire(JwtTestTokenGenerator.UnauthorizedJwtToken);
        var questionId = await CreateQuestion(questionnaireId, JwtTestTokenGenerator.UnauthorizedJwtToken);

        var payload = new
        {
            questionnaireId,
            questionId,
            content = "A"
        };

        using var res = await Create(payload);

        new[] { HttpStatusCode.Forbidden, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
    }

    [Fact]
    public async Task Create_Invalid_Payload_Returns400()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        var payload = new
        {
            questionnaireId,
            questionId,
            content = 123, // invalid type
            description = new { bad = "obj" }
        };

        using var res = await Create(payload);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Answers By Question Endpoint Tests

    [Fact]
    public async Task Get_All_By_Question_Returns200_And_Only_My_Accessible_Items()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        // Seed for current user
        using (var r1 = await Create(new { questionnaireId, questionId, content = "A1" })) { r1.StatusCode.Should().Be(HttpStatusCode.OK); }
        using (var r2 = await Create(new { questionnaireId, questionId, content = "A2" })) { r2.StatusCode.Should().Be(HttpStatusCode.OK); }

        // Seed for another user on a different question
        var qnOther = await CreateQuestionnaire(JwtTestTokenGenerator.UnauthorizedJwtToken);
        var qOther = await CreateQuestion(qnOther, JwtTestTokenGenerator.UnauthorizedJwtToken);
        using (var r3 = await Create(new { questionnaireId = qnOther, questionId = qOther, content = "Other" },
                   JwtTestTokenGenerator.UnauthorizedJwtToken))
        { r3.StatusCode.Should().Be(HttpStatusCode.OK); }

        using var getRes = await GetAll(routePrefixOverride: $"/api/questions/{questionId}/answers");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = (await getRes.Content.ReadAsStringAsync()).Deserialize<List<AnswerDto>>()!;
        list.Should().HaveCount(2);
        list.Should().OnlyContain(a => a.QuestionId == questionId);
    }

    [Fact]
    public async Task Get_All_By_Question_Invalid_JWT_Returns401()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        using var res = await GetAll(JwtTestTokenGenerator.InvalidAudJwtToken,
            routePrefixOverride: $"/api/questions/{questionId}/answers");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_All_By_Question_Expired_JWT_Returns401()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        using var res = await GetAll(JwtTestTokenGenerator.ExpiredJwtToken,
            routePrefixOverride: $"/api/questions/{questionId}/answers");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_All_By_Question_No_Access_Returns_Forbidden_Or_NotFound()
    {
        var questionnaireId = await CreateQuestionnaire(JwtTestTokenGenerator.UnauthorizedJwtToken);
        var questionId = await CreateQuestion(questionnaireId, JwtTestTokenGenerator.UnauthorizedJwtToken);

        using var res = await GetAll(routePrefixOverride: $"/api/questions/{questionId}/answers");

        new[] { HttpStatusCode.Forbidden, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
    }

    [Fact]
    public async Task Get_All_By_Question_Missing_Returns_NotFound()
    {
        using var res = await GetAll(routePrefixOverride: $"/api/questions/{Guid.NewGuid()}/answers");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Get Specific Answer Endpoint Tests

    [Fact]
    public async Task Get_By_Id_Succeeds_With_Dto()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new { questionnaireId, questionId, content = "AX" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var getRes = await GetById(id);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = (await getRes.Content.ReadAsStringAsync()).Deserialize<AnswerDto>()!;
        dto.Id.ToString().Should().Be(id);
        dto.Content.Should().Be("AX");
        dto.QuestionId.Should().Be(questionId);
        dto.QuestionnaireId.Should().Be(questionnaireId);
        dto.CreatedAt.Should().NotBe(default);
        dto.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Get_By_Id_Unauthorized_User_Fails()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new { questionnaireId, questionId, content = "Secret" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var res = await GetById(id, JwtTestTokenGenerator.UnauthorizedJwtToken);
        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden }.Should().Contain(res.StatusCode);
    }

    [Fact]
    public async Task Get_By_Id_Missing_Returns_404()
    {
        using var res = await GetById(Guid.NewGuid().ToString("N"));
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    public async Task Get_By_Id_Invalid_Id_Returns_400(string invalidId)
    {
        using var res = await GetById(invalidId);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_By_Id_Invalid_Token_Returns401()
    {
        using var res = await GetById(Guid.NewGuid().ToString(), JwtTestTokenGenerator.InvalidAudJwtToken);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_By_Id_Expired_Token_Returns401()
    {
        using var res = await GetById(Guid.NewGuid().ToString(), JwtTestTokenGenerator.ExpiredJwtToken);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Update Answer Endpoint Tests

    [Fact]
    public async Task Update_Content_Only_NoContent_And_Others_Unchanged()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new
               {
                   questionnaireId,
                   questionId,
                   content = "A0",
                   description = "D0",
                   score = 1,
                   destinationUrl = "https://url.com",
                   destinationType = DestinationType.Question,
               }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var res = await UpdateById(id, new { content = "A1" });
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var answer = await GetById<AnswerDto>(id);
        answer.Content.Should().Be("A1");
        answer.Description.Should().Be("D0");
        answer.Score.Should().Be(1);
        answer.DestinationUrl.Should().Be("https://url.com");
        answer.DestinationType.Should().Be(DestinationType.Question);
        answer.CreatedAt.Should().NotBe(default);
        answer.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Update_All_Fields_NoContent_Then_Get()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new
               {
                   questionnaireId, 
                   questionId, 
                   content = "A0",
                   destinationType = DestinationType.Question,
               }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var res = await UpdateById(id, new
        {
            content = "A2",
            description = "D2",
            score = 10,
            destinationUrl = "https://url.com",
            destinationType = DestinationType.ExternalLink,
            destinationQuestionId = (Guid?)null
        });
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dto = await GetById<AnswerDto>(id);
        dto.Content.Should().Be("A2");
        dto.Description.Should().Be("D2");
        dto.Score.Should().Be(10);
        dto.DestinationUrl.Should().Be("https://url.com");
        dto.DestinationType.Should().Be(DestinationType.ExternalLink);
        dto.DestinationQuestionId.Should().BeNull();
    }

    [Fact]
    public async Task Update_Invalid_JWT_401_No_Changes()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new { questionnaireId, questionId, content = "Seed" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var res = await UpdateById(id, new { content = "Changed" }, JwtTestTokenGenerator.InvalidAudJwtToken);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var answer = await GetById<AnswerDto>(id);
        answer.Content.Should().Be("Seed");
    }

    [Fact]
    public async Task Update_Expired_JWT_401_No_Changes()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new { questionnaireId, questionId, content = "Seed" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var res = await UpdateById(id, new { content = "Changed" }, JwtTestTokenGenerator.ExpiredJwtToken);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var answer = await GetById<AnswerDto>(id);
        answer.Content.Should().Be("Seed");
    }

    [Fact]
    public async Task Update_NotFound_Returns_404()
    {
        using var res = await UpdateById(Guid.NewGuid().ToString("N"), new { content = "X" });
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_Forbidden_No_Changes()
    {
        // Create as another user
        var questionnaireId = await CreateQuestionnaire(JwtTestTokenGenerator.UnauthorizedJwtToken);
        var questionId = await CreateQuestion(questionnaireId, JwtTestTokenGenerator.UnauthorizedJwtToken);

        string id;
        using (var r = await Create(new { questionnaireId, questionId, content = "Private" },
                   JwtTestTokenGenerator.UnauthorizedJwtToken))
        {
            r.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await r.Content.ReadAsStringAsync());
        }

        using var res = await UpdateById(id, new { content = "Hack" });
        new[] { HttpStatusCode.Forbidden, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    public async Task Update_Invalid_Id_400(string invalidId)
    {
        using var res = await UpdateById(invalidId, new { content = "X" });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_Invalid_Payload_400_No_Changes()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new { questionnaireId, questionId, content = "Seed", description = "D0" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var res = await UpdateById(id, new { content = true, description = 5 }); // invalid types
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var answer = await GetById<AnswerDto>(id);
        answer.Content.Should().Be("Seed");
        answer.Description.Should().Be("D0");
    }

    #endregion

    #region Delete Answer Endpoint Tests

    [Fact]
    public async Task Delete_Existing_Answer_Succeeds()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new { questionnaireId, questionId, content = "Del" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var delRes = await DeleteById(id);
        delRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var getRes = await GetById(id);
        new[] { HttpStatusCode.NotFound, HttpStatusCode.Forbidden }.Should().Contain(getRes.StatusCode);
    }

    [Fact]
    public async Task Delete_NotAuthorized_Returns_Forbidden_Or_NotFound_No_Deletion()
    {
        var questionnaireId = await CreateQuestionnaire(JwtTestTokenGenerator.UnauthorizedJwtToken);
        var questionId = await CreateQuestion(questionnaireId, JwtTestTokenGenerator.UnauthorizedJwtToken);

        string id;
        using (var postRes = await Create(new { questionnaireId, questionId, content = "Private" },
                   JwtTestTokenGenerator.UnauthorizedJwtToken))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var res = await DeleteById(id);
        new[] { HttpStatusCode.Forbidden, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);

        // Still accessible by owner
        using var getRes = await GetById(id, JwtTestTokenGenerator.UnauthorizedJwtToken);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Missing_Returns_404()
    {
        using var res = await DeleteById(Guid.NewGuid().ToString("N"));
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Invalid_JWT_Returns_Unauthorized_No_Deletion()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new { questionnaireId, questionId, content = "Seed" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var res = await DeleteById(id, JwtTestTokenGenerator.InvalidAudJwtToken);
        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);

        using var getRes = await GetById(id);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Expired_JWT_Returns_Unauthorized_No_Deletion()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new { questionnaireId, questionId, content = "Seed" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using var res = await DeleteById(id, JwtTestTokenGenerator.ExpiredJwtToken);
        new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);

        using var getRes = await GetById(id);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Already_Deleted_Returns_NotFound_Or_Conflict()
    {
        var questionnaireId = await CreateQuestionnaire();
        var questionId = await CreateQuestion(questionnaireId);

        string id;
        using (var postRes = await Create(new { questionnaireId, questionId, content = "Tmp" }))
        {
            postRes.StatusCode.Should().Be(HttpStatusCode.OK);
            id = ExtractId(await postRes.Content.ReadAsStringAsync());
        }

        using (var first = await DeleteById(id))
        {
            first.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        using var res = await DeleteById(id);
        new[] { HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.Conflict }.Should().Contain(res.StatusCode);
    }

    #endregion
}