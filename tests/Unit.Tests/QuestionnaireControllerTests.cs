using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Api.Controllers;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using FluentAssertions;
using Integration.Tests.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Unit.Tests;

public class QuestionnaireControllerTests
{
    private static GetToAnAnswerDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<GetToAnAnswerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new GetToAnAnswerDbContext(options);
    }

    private static QuestionnaireController CreateController(GetToAnAnswerDbContext db, ClaimsPrincipal? user = null)
    {
        var controller = new QuestionnaireController(db);

        var httpContext = new DefaultHttpContext();
        if (user is not null)
        {
            httpContext.User = user;
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private static ClaimsPrincipal CreateUser(string email = "user@contoso.com")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim("tid", "mock-tenant"),
            new Claim("aud", "your-client-id")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task CreateQuestionnaire_Minimal_Succeeds_Persists_And_Returns201()
    {
        using var db = CreateInMemoryDb(nameof(CreateQuestionnaire_Minimal_Succeeds_Persists_And_Returns201));
        var controller = CreateController(db, CreateUser());

        var req = new CreateQuestionnaireRequestDto
        {
            Title = "My Questionnaire"
        };

        var result = await controller.CreateQuestionnaire(req);

        result.Should().BeOfType<CreatedResult>();
        var created = (CreatedResult)result;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);

        // Validate returned DTO shape
        created.Value.Should().NotBeNull();
        var questionnaire = created.Value as QuestionnaireDto;

        questionnaire?.Title.Should().BeEquivalentTo("My Questionnaire");
        questionnaire?.Id.Should().NotBeEmpty();

        // Validate persistence
        var entity = await db.Questionnaires.SingleAsync();
        entity.Title.Should().Be("My Questionnaire");
        entity.Contributors.Should().Contain("user@contoso.com");
        entity.CreatedBy.Should().Be("user@contoso.com");
        entity.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(1));
        entity.UpdatedAt.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task CreateQuestionnaire_Duplicate_Title_Produces_Different_Ids()
    {
        using var db = CreateInMemoryDb(nameof(CreateQuestionnaire_Duplicate_Title_Produces_Different_Ids));
        var controller = CreateController(db, CreateUser());

        var req = new CreateQuestionnaireRequestDto { Title = "Same Title" };

        var r1 = (CreatedResult)await controller.CreateQuestionnaire(req);
        var r2 = (CreatedResult)await controller.CreateQuestionnaire(req);
        
        var questionnaire1 = r1.Value as QuestionnaireDto;
        var questionnaire2 = r2.Value as QuestionnaireDto;

        var id1 = questionnaire1?.Id.ToString();
        var id2 = questionnaire2?.Id.ToString();

        id1.Should().NotBeNullOrWhiteSpace();
        id2.Should().NotBeNullOrWhiteSpace();
        id1.Should().NotBe(id2);
    }

    [Fact]
    public async Task CreateQuestionnaire_Requires_Authenticated_User_Email()
    {
        using var db = CreateInMemoryDb(nameof(CreateQuestionnaire_Requires_Authenticated_User_Email));
        // User without email
        var claims = new[] { new Claim("sub", "abc") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var noEmailUser = new ClaimsPrincipal(identity);

        var controller = CreateController(db, noEmailUser);

        var req = new CreateQuestionnaireRequestDto { Title = "Any" };

        Func<Task> act = async () => await controller.CreateQuestionnaire(req);
        await act.Should().ThrowAsync<Exception>(); // controller uses ClaimTypes.Email directly; expecting failure path
    }

    [Theory]
    [InlineData("   ", false)]
    [InlineData("\t\r\n", false)]
    [InlineData("A", true)]
    [InlineData("😀✨💥", true)]
    public async Task Title_Rules(string title, bool expectedSuccess)
    {
        using var db = CreateInMemoryDb(nameof(Title_Rules) + "_" + Guid.NewGuid());
        var controller = CreateController(db, CreateUser());

        var req = new CreateQuestionnaireRequestDto { Title = title };

        if (expectedSuccess)
        {
            var result = await controller.CreateQuestionnaire(req);
            result.Should().BeOfType<CreatedResult>();
        }
        else
        {
            controller.Validate(req);
            
            var result = await controller.CreateQuestionnaire(req);
            result.Should().BeOfType<BadRequestObjectResult>();
            var bad = (BadRequestObjectResult)result;
            bad.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            
            var errors = controller.ModelState.Values.SelectMany(v => v.Errors);
            errors.Should().Contain(e => e.ErrorMessage == "Enter a questionnaire title");
            
        }
    }

    [Fact]
    public async Task CreateQuestionnaire_Invalid_Model_Returns400()
    {
        using var db = CreateInMemoryDb(nameof(CreateQuestionnaire_Invalid_Model_Returns400));
        var controller = CreateController(db, CreateUser());

        var req = new CreateQuestionnaireRequestDto
        {
            Title = null! // force invalid
        };

        controller.Validate(req);

        var result = await controller.CreateQuestionnaire(req);

        result.Should().BeOfType<BadRequestObjectResult>();
        var bad = (BadRequestObjectResult)result;
        bad.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            
        var errors = controller.ModelState.Values.SelectMany(v => v.Errors);
        errors.Should().Contain(e => e.ErrorMessage == "Enter a questionnaire title");
    }

    [Fact]
    public async Task CreateQuestionnaire_Sets_Timestamps_And_Audit()
    {
        using var db = CreateInMemoryDb(nameof(CreateQuestionnaire_Sets_Timestamps_And_Audit));
        var controller = CreateController(db, CreateUser("owner@contoso.com"));

        var req = new CreateQuestionnaireRequestDto { Title = "Audit" };
        var result = (CreatedResult)await controller.CreateQuestionnaire(req);

        result.StatusCode.Should().Be(StatusCodes.Status201Created);

        var entity = await db.Questionnaires.SingleAsync(q => q.Title == "Audit");
        entity.CreatedBy.Should().Be("owner@contoso.com");
        entity.Contributors.Should().Contain("owner@contoso.com");
        (entity.UpdatedAt - entity.CreatedAt).Should().BeCloseTo(TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
    
    
}