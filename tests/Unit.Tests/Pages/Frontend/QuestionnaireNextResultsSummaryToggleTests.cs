using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
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

public class QuestionnaireNextResultsSummaryToggleTests
{
    private readonly Mock<IApiClient> _mockApiClient = new();
    private readonly Mock<ILogger<QuestionnaireNext>> _mockLogger = new();
    private readonly Mock<HttpContext> _mockHttpContext = new();
    private readonly Mock<IFormCollection> _mockFormCollection = new();

    private QuestionnaireNext CreatePageModel(Guid questionnaireId)
    {
        var pageModel = new QuestionnaireNext(_mockApiClient.Object, _mockLogger.Object)
        {
            QuestionnaireSlug = "test-questionnaire",
            NextStateRequest = new GetNextStateRequest
            {
                CurrentQuestionId = Guid.NewGuid(),
                CurrentQuestionOrder = 1,
                SelectedAnswerIds = new List<Guid>(),
                ValidateAnswers = true
            },
            IsEmbedded = false,
            AnswerHistory = string.Empty,
            Embed = false,
            PageContext = new PageContext(),
            Questionnaire = new QuestionnaireInfoDto { Id = questionnaireId },
            Destination = new DestinationDto()
        };

        pageModel.PageContext.HttpContext = _mockHttpContext.Object;
        _mockHttpContext.Setup(x => x.Request.Form).Returns(_mockFormCollection.Object);
        _mockHttpContext.Setup(x => x.Features.Set(It.IsAny<QuestionnaireRunFeature>()));
        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");

        return pageModel;
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task OnPost_CustomContent_Shows_Results_Summary_Only_When_Toggle_Is_On(bool showResultsSummary, bool expectSummary)
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var pageModel = CreatePageModel(questionnaireId);
        var customContent = new DestinationDto
        {
            Type = DestinationType.CustomContent,
            Title = "Result",
            Content = "Done",
            ShowResultsSummary = showResultsSummary
        };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(new QuestionnaireInfoDto { Id = questionnaireId, Slug = "test-questionnaire" });

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, It.IsAny<GetNextStateRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(customContent);

        // Act
        var result = await pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        if (expectSummary)
            pageModel.ResultsSummaryRows.Should().NotBeNull();
        else
            pageModel.Destination.Should().BeSameAs(customContent);
    }
}
