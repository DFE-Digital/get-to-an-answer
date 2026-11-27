using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Common.Helpers;
using FluentAssertions;
using Frontend.Models;
using Frontend.Pages.Questionnaire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Unit.Tests.Pages.Frontend;

public class QuestionnaireNextBackButtonTests
{
    private readonly Mock<IApiClient> _mockApiClient = new();
    private readonly Mock<ILogger<QuestionnaireNext>> _mockLogger = new();
    private readonly Mock<HttpContext> _mockHttpContext = new();
    private readonly Mock<IFormCollection> _mockFormCollection = new();

    [Fact]
    public async Task OnPost_Back_From_Third_Question_Returns_To_First_Shown_Question_When_Second_Was_Skipped()
    {
        var questionnaireId = Guid.NewGuid();
        var q1 = new QuestionDto { Id = Guid.NewGuid(), Order = 1, Type = QuestionType.SingleSelect };
        var q2 = new QuestionDto { Id = Guid.NewGuid(), Order = 2, Type = QuestionType.SingleSelect };
        var q3 = new QuestionDto { Id = Guid.NewGuid(), Order = 3, Type = QuestionType.SingleSelect };
        var answerA = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto { Id = questionnaireId, Slug = "test-questionnaire" };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, q1.Id, It.IsAny<bool>()))
            .ReturnsAsync(q1);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, q3.Id, It.IsAny<bool>()))
            .ReturnsAsync(q3);

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, It.IsAny<GetNextStateRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new DestinationDto { Type = DestinationType.Question, Question = q3 });

        _mockApiClient
            .Setup(x => x.GetPreviousState(questionnaireId, q3.Order, It.IsAny<bool>(), It.IsAny<string?>()))
            .ReturnsAsync(new DestinationDto { Type = DestinationType.Question, Question = q1 });

        var page = CreatePage();

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        page.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = q1.Id,
            CurrentQuestionOrder = q1.Order,
            SelectedAnswerIds = new List<Guid> { answerA },
            ValidateAnswers = true
        };
        await page.OnPost();

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("backButton");
        page.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = q3.Id,
            CurrentQuestionOrder = q3.Order,
            SelectedAnswerIds = new List<Guid>(),
            ValidateAnswers = false
        };

        var result = await page.OnPost();

        result.Should().BeOfType<PageResult>();
        page.Destination.Question.Should().NotBeNull();
        page.Destination.Question!.Id.Should().Be(q1.Id);
        page.Destination.Question.Id.Should().NotBe(q2.Id);
        page.Destination.PreviouslySelectedAnswerIds.Should().ContainSingle().Which.Should().Be(answerA);
        _mockApiClient.Verify(x => x.GetPreviousState(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string?>()), Times.Never);
        _mockApiClient.Verify(x => x.GetCurrentQuestion(questionnaireId, q2.Id, It.IsAny<bool>()), Times.Never);
    }

    private QuestionnaireNext CreatePage()
    {
        var page = new QuestionnaireNext(_mockApiClient.Object, _mockLogger.Object)
        {
            QuestionnaireSlug = "test-questionnaire",
            NextStateRequest = new GetNextStateRequest(),
            IsEmbedded = false,
            AnswerHistory = string.Empty,
            Embed = false,
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext(),
            Questionnaire = new QuestionnaireInfoDto { Id = Guid.NewGuid() },
            Destination = new DestinationDto()
        };

        page.PageContext.HttpContext = _mockHttpContext.Object;
        _mockHttpContext.Setup(x => x.Request.Form).Returns(_mockFormCollection.Object);
        _mockHttpContext.Setup(x => x.Features.Set(It.IsAny<QuestionnaireRunFeature>()));

        return page;
    }
}
