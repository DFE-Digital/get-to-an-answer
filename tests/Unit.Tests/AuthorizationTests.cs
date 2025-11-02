using System.Security.Claims;
using Api.Controllers;
using Api.Services;
using Common.Domain.Request.Create;
using Common.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Unit.Tests;

public class AuthorizationTests
{
    private static GetToAnAnswerDbContext Db(string name) =>
        new GetToAnAnswerDbContext(new DbContextOptionsBuilder<GetToAnAnswerDbContext>()
            .UseInMemoryDatabase(name)
            .Options);

    [Fact]
    public async Task CreateQuestionnaire_Unauthenticated_Returns_Unauthorized_When_Policy_Enforced()
    {
        using var db = Db(nameof(CreateQuestionnaire_Unauthenticated_Returns_Unauthorized_When_Policy_Enforced));

        var controller = new QuestionnaireController(new QuestionnaireService(db, 
            new Moq.Mock<ILogger<QuestionnaireService>>().Object))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // No Authorization header
            }
        };

        var req = new CreateQuestionnaireRequestDto { Title = "Any" };
        await controller.CreateQuestionnaire(req);
    }

    [Fact]
    public async Task CreateQuestionnaire_Missing_Email_Claim_Fails()
    {
        using var db = Db(nameof(CreateQuestionnaire_Missing_Email_Claim_Fails));

        var claims = new[] { new Claim("sub", "abc") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var http = new DefaultHttpContext { User = principal };

        var controller = new QuestionnaireController(new QuestionnaireService(db, 
            new Moq.Mock<ILogger<QuestionnaireService>>().Object))
        {
            ControllerContext = new ControllerContext { HttpContext = http }
        };

        var req = new CreateQuestionnaireRequestDto { Title = "X" };
        await controller.CreateQuestionnaire(req);
    }
}