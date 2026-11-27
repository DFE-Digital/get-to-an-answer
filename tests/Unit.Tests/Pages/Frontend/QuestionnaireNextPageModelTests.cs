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

public class QuestionnaireNextPageModelTests
{
    private readonly Mock<IApiClient> _mockApiClient;
    private readonly Mock<ILogger<QuestionnaireNext>> _mockLogger;
    private readonly QuestionnaireNext _pageModel;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<IFormCollection> _mockFormCollection;

    public QuestionnaireNextPageModelTests()
    {
        _mockApiClient = new Mock<IApiClient>();
        _mockLogger = new Mock<ILogger<QuestionnaireNext>>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockFormCollection = new Mock<IFormCollection>();

        _pageModel = new QuestionnaireNext(_mockApiClient.Object, _mockLogger.Object)
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
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext(),
            Questionnaire = new QuestionnaireInfoDto { Id = Guid.NewGuid() },
            Destination = new DestinationDto()
        };

        _pageModel.PageContext.HttpContext = _mockHttpContext.Object;
        _mockHttpContext.Setup(x => x.Request.Form).Returns(_mockFormCollection.Object);
        _mockHttpContext.Setup(x => x.Features.Set(It.IsAny<QuestionnaireRunFeature>()));
    }

    #region OnPost - Continue Button Tests
    

    [Fact]
    public async Task OnPost_Continue_Does_Not_Redirect_When_Embedded()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var answerId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            Slug = "test-questionnaire"
        };

        var currentQuestion = new QuestionDto
        {
            Id = currentQuestionId,
            Order = 1,
            Type = QuestionType.SingleSelect
        };

        var nextQuestion = new QuestionDto
        {
            Id = nextQuestionId,
            Order = 2,
            Type = QuestionType.SingleSelect
        };

        var externalLinkDestination = new DestinationDto
        {
            Type = DestinationType.ExternalLink,
            Content = "https://example.com",
            Question = nextQuestion
        };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 1,
            SelectedAnswerIds = new List<Guid> { answerId },
            ValidateAnswers = true
        };
        _pageModel.Embed = true;

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, currentQuestionId))
            .ReturnsAsync(currentQuestion);

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, It.IsAny<GetNextStateRequest>()))
            .ReturnsAsync(externalLinkDestination);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
    }

    #endregion

    #region QuestionnaireSlug Validation Tests

    [Fact]
    public async Task OnPost_Continue_Throws_InvalidOperationException_When_QuestionnaireSlug_Null()
    {
        // Arrange
        _pageModel.QuestionnaireSlug = null;
        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _pageModel.OnPost());
        exception.Message.Should().Contain("QuestionnaireSlug is required");
    }

    [Fact]
    public async Task OnPost_Back_Throws_InvalidOperationException_When_QuestionnaireSlug_Null()
    {
        // Arrange
        _pageModel.QuestionnaireSlug = null;
        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("backButton");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _pageModel.OnPost());
        exception.Message.Should().Contain("QuestionnaireSlug is required");
    }

    #endregion

    #region OnPost - Custom Content Button Tests

    [Fact]
    public async Task OnPost_CustomContentBackButton_Returns_Page_With_Last_Question()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var question3Id = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            DisplayTitle = "Test Questionnaire",
            Slug = "test-questionnaire"
        };

        var question3 = new QuestionDto
        {
            Id = question3Id,
            Order = 3,
            Content = "Question 3",
            Type = QuestionType.SingleSelect,
            Answers = new List<AnswerDto>()
        };

        _pageModel.QuestionnaireSlug = "test-questionnaire";
        _pageModel.Questionnaire = questionnaire;
        _mockFormCollection.Setup(x => x["submitButton"]).Returns("customContentBackButton");

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetLastQuestion(questionnaireId, false))
            .ReturnsAsync(question3);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.Destination.Should().NotBeNull();
        _pageModel.Destination!.Type.Should().Be(DestinationType.Question);
        _pageModel.Destination!.Question.Should().Be(question3);
        _pageModel.Destination!.Question!.Order.Should().Be(3);
    }

    [Fact]
    public async Task OnPost_CustomContentBackButton_WithNoQuestions_Returns_NotFound()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            DisplayTitle = "Test Questionnaire",
            Slug = "test-questionnaire"
        };

        _pageModel.QuestionnaireSlug = "test-questionnaire";
        _pageModel.Questionnaire = questionnaire;
        _mockFormCollection.Setup(x => x["submitButton"]).Returns("customContentBackButton");

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, It.IsAny<Guid>(), false))
            .ReturnsAsync((QuestionDto?)null);

        _mockApiClient
            .Setup(x => x.GetLastQuestion(questionnaireId, false))
            .ReturnsAsync((QuestionDto?)null);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task OnPost_CustomContentBackButton_WithNullQuestions_Returns_NotFound()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            DisplayTitle = "Test Questionnaire",
            Slug = "test-questionnaire"
        };

        _pageModel.QuestionnaireSlug = "test-questionnaire";
        _pageModel.Questionnaire = questionnaire;
        _mockFormCollection.Setup(x => x["submitButton"]).Returns("customContentBackButton");

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, It.IsAny<Guid>(), false))
            .ReturnsAsync((QuestionDto?)null);

        _mockApiClient
            .Setup(x => x.GetLastQuestion(questionnaireId, false))
            .ReturnsAsync((QuestionDto?)null);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Continue, Back and AnswerHistory Happy Path Tests

    [Fact]
    public async Task OnPost_Continue_Returns_Page_With_Next_Question()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var nextQuestion = new QuestionDto { Id = Guid.NewGuid(), Order = 2, Type = QuestionType.SingleSelect };
        var questionnaire = new QuestionnaireInfoDto { Id = questionnaireId, Slug = "test-questionnaire" };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 1,
            SelectedAnswerIds = new List<Guid> { Guid.NewGuid() },
            ValidateAnswers = true
        };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, currentQuestionId, false))
            .ReturnsAsync(new QuestionDto { Id = currentQuestionId, Order = 1, Type = QuestionType.SingleSelect });

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, It.IsAny<GetNextStateRequest>(), false))
            .ReturnsAsync(new DestinationDto { Type = DestinationType.Question, Question = nextQuestion });

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.Destination.Type.Should().Be(DestinationType.Question);
        _pageModel.Destination.Question.Should().Be(nextQuestion);
    }

    [Fact]
    public async Task OnPost_Back_Returns_Page_With_Previous_Question()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var previousQuestion = new QuestionDto { Id = Guid.NewGuid(), Order = 1, Type = QuestionType.SingleSelect };
        var questionnaire = new QuestionnaireInfoDto { Id = questionnaireId, Slug = "test-questionnaire" };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("backButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 2,
            SelectedAnswerIds = new List<Guid>(),
            ValidateAnswers = true
        };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, currentQuestionId, false))
            .ReturnsAsync(new QuestionDto { Id = currentQuestionId, Order = 2, Type = QuestionType.SingleSelect });

        _mockApiClient
            .Setup(x => x.GetPreviousState(questionnaireId, 2, false, It.IsAny<string?>()))
            .ReturnsAsync(new DestinationDto { Type = DestinationType.Question, Question = previousQuestion });

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.Destination.Question.Should().Be(previousQuestion);
        _pageModel.NextStateRequest.CurrentQuestionId.Should().Be(previousQuestion.Id);
        _pageModel.NextStateRequest.CurrentQuestionOrder.Should().Be(1);
        _mockApiClient.Verify(x => x.GetNextState(It.IsAny<Guid>(), It.IsAny<GetNextStateRequest>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_Continue_Saves_Answer_To_History_And_Back_Restores_It()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();
        var answerId = Guid.NewGuid();
        var question1 = new QuestionDto { Id = question1Id, Order = 1, Type = QuestionType.SingleSelect };
        var question2 = new QuestionDto { Id = question2Id, Order = 2, Type = QuestionType.SingleSelect };
        var questionnaire = new QuestionnaireInfoDto { Id = questionnaireId, Slug = "test-questionnaire" };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, question1Id, false))
            .ReturnsAsync(question1);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, question2Id, false))
            .ReturnsAsync(question2);

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, It.IsAny<GetNextStateRequest>(), false))
            .ReturnsAsync(new DestinationDto { Type = DestinationType.Question, Question = question2 });

        _mockApiClient
            .Setup(x => x.GetPreviousState(questionnaireId, 2, false, It.IsAny<string?>()))
            .ReturnsAsync(new DestinationDto { Type = DestinationType.Question, Question = question1 });

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = question1Id,
            CurrentQuestionOrder = 1,
            SelectedAnswerIds = new List<Guid> { answerId },
            ValidateAnswers = true
        };

        // Act - continue from question 1 with an answer selected
        await _pageModel.OnPost();

        // Assert - answer is saved to history in the expected format
        _pageModel.AnswerHistory.Should().Be($"{question1Id:N}.{answerId:N}");
        AnswerHistoryHelper.GetAnswersFromHistory(_pageModel.AnswerHistory, question1Id).Should().BeEquivalentTo(new[] { answerId });

        // Arrange - back from question 2
        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("backButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = question2Id,
            CurrentQuestionOrder = 2,
            SelectedAnswerIds = new List<Guid>(),
            ValidateAnswers = true
        };

        // Act - back to question 1
        var result = await _pageModel.OnPost();

        // Assert - question 1 is restored with the previously selected answer read from history
        result.Should().BeOfType<PageResult>();
        _pageModel.Destination.Question.Should().Be(question1);
        _pageModel.Destination.PreviouslySelectedAnswerIds.Should().BeEquivalentTo(new[] { answerId });
        _pageModel.NextStateRequest.SelectedAnswerIds.Should().BeEquivalentTo(new[] { answerId });
    }

    #endregion
}
