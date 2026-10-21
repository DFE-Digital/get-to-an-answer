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
using Microsoft.Extensions.Configuration;
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
    private readonly Mock<IConfiguration> _mockConfiguration;
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
            AnswerHistory = "{}",
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
    public async Task OnPost_Continue_Returns_Page_With_Next_Question()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var answerId = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            DisplayTitle = "Test Questionnaire",
            Slug = "test-questionnaire"
        };

        var currentQuestion = new QuestionDto
        {
            Id = currentQuestionId,
            Order = 1,
            Content = "Current Question",
            Type = QuestionType.SingleSelect,
            Answers = new List<AnswerDto>()
        };

        var nextQuestion = new QuestionDto
        {
            Id = nextQuestionId,
            Order = 2,
            Content = "Next Question",
            Type = QuestionType.SingleSelect,
            Answers = new List<AnswerDto>()
        };

        var nextDestination = new DestinationDto
        {
            Type = DestinationType.Question,
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

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, currentQuestionId))
            .ReturnsAsync(currentQuestion);

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, _pageModel.NextStateRequest))
            .ReturnsAsync(nextDestination);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.Destination.Should().Be(nextDestination);
        _pageModel.Questionnaire.Should().Be(questionnaire);
    }

    [Fact]
    public async Task OnPost_Continue_Returns_NotFound_When_Questionnaire_Not_Found()
    {
        // Arrange
        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync((QuestionnaireInfoDto)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _pageModel.OnPost());
        exception.Message.Should().Contain("Questionnaire not found");
    }

    [Fact]
    public async Task OnPost_Continue_Returns_NotFound_When_Next_State_Not_Found()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            DisplayTitle = "Test Questionnaire",
            Slug = "test-questionnaire"
        };

        var currentQuestion = new QuestionDto
        {
            Id = currentQuestionId,
            Order = 1,
            Content = "Current Question",
            Type = QuestionType.SingleSelect
        };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 1,
            SelectedAnswerIds = new List<Guid>(),
            ValidateAnswers = true
        };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, currentQuestionId))
            .ReturnsAsync(currentQuestion);

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, It.IsAny<GetNextStateRequest>()))
            .ReturnsAsync((DestinationDto)null);

        // Act & Assert
        var result = await _pageModel.OnPost();
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task OnPost_Continue_Handles_Validation_Error_For_SingleSelect()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            DisplayTitle = "Test Questionnaire",
            Slug = "test-questionnaire"
        };

        var currentQuestion = new QuestionDto
        {
            Id = currentQuestionId,
            Order = 1,
            Content = "Current Question",
            Type = QuestionType.SingleSelect
        };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 1,
            SelectedAnswerIds = new List<Guid>(), // No answers selected
            ValidateAnswers = true
        };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, currentQuestionId))
            .ReturnsAsync(currentQuestion);

        // Simulate invalid ModelState by adding an error before calling OnPost
        _pageModel.ModelState.AddModelError("test", "test error");

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.ModelState.ContainsKey("NextStateRequest.SelectedAnswerIds").Should().BeTrue();
        var errors = _pageModel.ModelState["NextStateRequest.SelectedAnswerIds"].Errors;
        errors.Should().HaveCount(1);
        errors[0].ErrorMessage.Should().Contain("Select an answer");
    }

    [Fact]
    public async Task OnPost_Continue_Handles_Validation_Error_For_MultiSelect()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            Slug = "test-questionnaire"
        };

        var currentQuestion = new QuestionDto
        {
            Id = currentQuestionId,
            Order = 1,
            Type = QuestionType.MultiSelect
        };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 1,
            SelectedAnswerIds = new List<Guid>(),
            ValidateAnswers = true
        };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, currentQuestionId))
            .ReturnsAsync(currentQuestion);

        // Simulate invalid ModelState
        _pageModel.ModelState.AddModelError("test", "test error");

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        var errors = _pageModel.ModelState["NextStateRequest.SelectedAnswerIds"].Errors;
        errors[0].ErrorMessage.Should().Contain("Select at least one answer");
    }

    [Fact]
    public async Task OnPost_Continue_Handles_Validation_Error_For_DropdownSelect()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            Slug = "test-questionnaire"
        };

        var currentQuestion = new QuestionDto
        {
            Id = currentQuestionId,
            Order = 1,
            Type = QuestionType.DropdownSelect
        };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 1,
            SelectedAnswerIds = new List<Guid>(),
            ValidateAnswers = true
        };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, currentQuestionId))
            .ReturnsAsync(currentQuestion);

        // Simulate invalid ModelState
        _pageModel.ModelState.AddModelError("test", "test error");

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        var errors = _pageModel.ModelState["NextStateRequest.SelectedAnswerIds"].Errors;
        errors[0].ErrorMessage.Should().Contain("Select an option");
    }

    [Fact]
    public async Task OnPost_Continue_Does_Not_Validate_When_ValidateAnswers_False()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
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

        var nextDestination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = nextQuestion
        };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 1,
            SelectedAnswerIds = new List<Guid>(), // No answers
            ValidateAnswers = false // Don't validate
        };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, currentQuestionId))
            .ReturnsAsync(currentQuestion);

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, It.IsAny<GetNextStateRequest>()))
            .ReturnsAsync(nextDestination);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.ModelState.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task OnPost_Continue_Redirects_To_External_Link_When_Not_Embedded()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var answerId = Guid.NewGuid();

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

        var externalLinkDestination = new DestinationDto
        {
            Type = DestinationType.ExternalLink,
            Content = "https://example.com/"
        };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 1,
            SelectedAnswerIds = new List<Guid> { answerId },
            ValidateAnswers = true
        };
        _pageModel.Embed = false;

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
        result.Should().BeOfType<RedirectResult>();
        var redirect = result as RedirectResult;
        redirect.Url.Should().Contain("example.com");
    }

    [Fact]
    public async Task OnPost_Continue_Adds_Https_To_External_Link_If_Missing()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var answerId = Guid.NewGuid();

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

        var externalLinkDestination = new DestinationDto
        {
            Type = DestinationType.ExternalLink,
            Content = "example.com"
        };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("continueButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 1,
            SelectedAnswerIds = new List<Guid> { answerId },
            ValidateAnswers = true
        };
        _pageModel.Embed = false;

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
        var redirect = result as RedirectResult;
        redirect.Url.Should().Be("https://example.com");
    }

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

    #region OnPost - Back Button Tests

    [Fact]
    public async Task OnPost_Back_Returns_Page_With_Previous_Question()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var previousQuestionId = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            Slug = "test-questionnaire"
        };

        var previousQuestion = new QuestionDto
        {
            Id = previousQuestionId,
            Order = 1,
            Type = QuestionType.SingleSelect
        };

        var previousDestination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = previousQuestion
        };

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
            .Setup(x => x.GetPreviousState(questionnaireId, 2))
            .ReturnsAsync(previousDestination);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.Destination.Should().Be(previousDestination);
        _pageModel.ModelState.IsValid.Should().BeTrue(); // ModelState should be cleared
    }

    [Fact]
    public async Task OnPost_Back_Returns_NotFound_When_Questionnaire_Not_Found()
    {
        // Arrange
        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("backButton");

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync((QuestionnaireInfoDto)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _pageModel.OnPost());
        exception.Message.Should().Contain("Questionnaire not found");
    }

    [Fact]
    public async Task OnPost_Back_Returns_NotFound_When_Previous_State_Not_Found()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            Slug = "test-questionnaire"
        };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("backButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionOrder = 2
        };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetPreviousState(questionnaireId, 2))
            .ReturnsAsync((DestinationDto)null);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task OnPost_Back_Restores_Previously_Selected_Answers()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var previousQuestionId = Guid.NewGuid();
        var answerId1 = Guid.NewGuid();
        var answerId2 = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            Slug = "test-questionnaire"
        };

        var previousQuestion = new QuestionDto
        {
            Id = previousQuestionId,
            Order = 1,
            Type = QuestionType.MultiSelect
        };

        var previousDestination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = previousQuestion
        };

        // Setup answer history JSON
        var answerHistory = $@"{{
            ""{previousQuestionId}"": [""{answerId1}"", ""{answerId2}""]
        }}";

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("backButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 2,
            SelectedAnswerIds = new List<Guid>(),
            ValidateAnswers = true
        };
        _pageModel.AnswerHistory = answerHistory;

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetPreviousState(questionnaireId, 2))
            .ReturnsAsync(previousDestination);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.NextStateRequest.SelectedAnswerIds.Should().HaveCount(2);
        _pageModel.NextStateRequest.SelectedAnswerIds.Should().Contain(answerId1);
        _pageModel.NextStateRequest.SelectedAnswerIds.Should().Contain(answerId2);
    }

    #endregion

    #region OnPost - Unknown Button Tests

    [Fact]
    public async Task OnPost_Unknown_Button_Returns_BadRequest()
    {
        // Arrange
        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("unknownButton");

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest.Value.Should().Be("Unknown action");
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

    #region Answer History Tests

    [Fact]
    public async Task OnPost_Continue_Saves_Answers_To_History()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var answerId = Guid.NewGuid();

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

        var nextDestination = new DestinationDto
        {
            Type = DestinationType.Question,
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
        _pageModel.AnswerHistory = "{}";

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, currentQuestionId))
            .ReturnsAsync(currentQuestion);

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, It.IsAny<GetNextStateRequest>()))
            .ReturnsAsync(nextDestination);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.AnswerHistory.Should().Contain(currentQuestionId.ToString());
        _pageModel.AnswerHistory.Should().Contain(answerId.ToString());
    }

    [Fact]
    public async Task OnPost_Back_Saves_Current_Answers_Before_Going_Back()
    {
        // Arrange
        var questionnaireId = Guid.NewGuid();
        var currentQuestionId = Guid.NewGuid();
        var previousQuestionId = Guid.NewGuid();
        var currentAnswerId = Guid.NewGuid();

        var questionnaire = new QuestionnaireInfoDto
        {
            Id = questionnaireId,
            Slug = "test-questionnaire"
        };

        var previousQuestion = new QuestionDto
        {
            Id = previousQuestionId,
            Order = 1,
            Type = QuestionType.SingleSelect
        };

        var previousDestination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = previousQuestion
        };

        _mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns("backButton");
        _pageModel.NextStateRequest = new GetNextStateRequest
        {
            CurrentQuestionId = currentQuestionId,
            CurrentQuestionOrder = 2,
            SelectedAnswerIds = new List<Guid> { currentAnswerId },
            ValidateAnswers = true
        };
        _pageModel.AnswerHistory = "{}";

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetPreviousState(questionnaireId, 2))
            .ReturnsAsync(previousDestination);

        // Act
        var result = await _pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        // Verify current answers were saved to history
        _pageModel.AnswerHistory.Should().Contain(currentQuestionId.ToString());
        _pageModel.AnswerHistory.Should().Contain(currentAnswerId.ToString());
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
        }
