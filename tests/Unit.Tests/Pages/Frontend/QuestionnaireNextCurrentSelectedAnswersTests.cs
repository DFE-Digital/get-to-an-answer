using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Common.Helpers;
using FluentAssertions;
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

public class QuestionnaireNextCurrentSelectedAnswersTests
{
    private readonly Mock<IApiClient> _mockApiClient = new();
    private readonly Mock<ILogger<QuestionnaireNext>> _mockLogger = new();

    [Fact]
    public async Task Continue_And_Back_Keep_Answers_In_CurrentSelectedAnswers_And_Initialise_Question_One()
    {
        var questionnaireId = Guid.NewGuid();
        var questionnaire = new QuestionnaireInfoDto { Id = questionnaireId, Slug = "test-questionnaire" };
        var q1 = new QuestionDto { Id = Guid.NewGuid(), Order = 1, Type = QuestionType.SingleSelect };
        var q2 = new QuestionDto { Id = Guid.NewGuid(), Order = 2, Type = QuestionType.SingleSelect };
        var q1Answer = Guid.NewGuid();
        var q2Answer = Guid.NewGuid();

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire"))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, q1.Id, It.IsAny<bool>()))
            .ReturnsAsync(q1);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, q2.Id, It.IsAny<bool>()))
            .ReturnsAsync(q2);

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, It.IsAny<GetNextStateRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new DestinationDto { Type = DestinationType.Question, Question = q2 });

        _mockApiClient
            .Setup(x => x.GetPreviousState(questionnaireId, q2.Order, It.IsAny<bool>(), It.IsAny<string?>()))
            .ReturnsAsync(new DestinationDto { Type = DestinationType.Question, Question = q1 });

        var questionOnePage = CreatePostPage(
            "continueButton",
            new GetNextStateRequest
            {
                CurrentQuestionId = q1.Id,
                CurrentQuestionOrder = q1.Order,
                SelectedAnswerIds = new List<Guid> { q1Answer },
                ValidateAnswers = true
            },
            answerHistory: string.Empty,
            currentSelectedAnswers: string.Empty);

        var continueResult = await questionOnePage.OnPost();

        continueResult.Should().BeOfType<PageResult>();
        questionOnePage.Destination.Question!.Id.Should().Be(q2.Id);
        questionOnePage.Destination.PreviouslySelectedAnswerIds.Should().BeEmpty();
        AnswerHistoryHelper.GetSelectedAnswers(questionOnePage.CurrentSelectedAnswers, q1.Id)
            .Should().BeEquivalentTo(new[] { q1Answer });

        var questionTwoPage = CreatePostPage(
            "backButton",
            new GetNextStateRequest
            {
                CurrentQuestionId = q2.Id,
                CurrentQuestionOrder = q2.Order,
                SelectedAnswerIds = new List<Guid> { q2Answer },
                ValidateAnswers = false
            },
            answerHistory: string.Empty,
            currentSelectedAnswers: questionOnePage.CurrentSelectedAnswers);

        var backResult = await questionTwoPage.OnPost();

        backResult.Should().BeOfType<PageResult>();
        questionTwoPage.Destination.Question!.Id.Should().Be(q1.Id);
        questionTwoPage.Destination.PreviouslySelectedAnswerIds.Should().BeEquivalentTo(new[] { q1Answer });
        AnswerHistoryHelper.GetSelectedAnswers(questionTwoPage.CurrentSelectedAnswers, q1.Id)
            .Should().BeEquivalentTo(new[] { q1Answer });
        AnswerHistoryHelper.GetSelectedAnswers(questionTwoPage.CurrentSelectedAnswers, q2.Id)
            .Should().BeEquivalentTo(new[] { q2Answer });
    }

    [Fact]
    public async Task Back_After_Changing_Branch_Follows_Taken_Path_And_Restores_Selected_Answers()
    {
        var questionnaireId = Guid.NewGuid();
        var questionnaire = new QuestionnaireInfoDto { Id = questionnaireId, Slug = "test-questionnaire" };
        var q1 = new QuestionDto { Id = Guid.NewGuid(), Order = 1, Type = QuestionType.SingleSelect };
        var q2 = new QuestionDto { Id = Guid.NewGuid(), Order = 2, Type = QuestionType.SingleSelect };
        var q3 = new QuestionDto { Id = Guid.NewGuid(), Order = 3, Type = QuestionType.SingleSelect };
        var q4 = new QuestionDto { Id = Guid.NewGuid(), Order = 4, Type = QuestionType.SingleSelect };
        var q5 = new QuestionDto { Id = Guid.NewGuid(), Order = 5, Type = QuestionType.SingleSelect };
        var questions = new[] { q1, q2, q3, q4, q5 }.ToDictionary(q => q.Id);

        var q1Answer = Guid.NewGuid();
        var q2AnswerToQ3 = Guid.NewGuid();
        var q2AnswerToQ4 = Guid.NewGuid();
        var q3Answer = Guid.NewGuid();
        var q4Answer = Guid.NewGuid();
        var q5Answer = Guid.NewGuid();

        var routes = new Dictionary<Guid, QuestionDto>
        {
            [q1Answer] = q2,
            [q2AnswerToQ3] = q3,
            [q2AnswerToQ4] = q4,
            [q4Answer] = q5
        };

        _mockApiClient
            .Setup(x => x.GetLastPublishedQuestionnaireInfoAsync("test-questionnaire", It.IsAny<bool>()))
            .ReturnsAsync(questionnaire);

        _mockApiClient
            .Setup(x => x.GetCurrentQuestion(questionnaireId, It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync((Guid _, Guid questionId, bool _) => questions[questionId]);

        _mockApiClient
            .Setup(x => x.GetNextState(questionnaireId, It.IsAny<GetNextStateRequest>(), It.IsAny<bool>()))
            .ReturnsAsync((Guid _, GetNextStateRequest request, bool _) =>
                new DestinationDto { Type = DestinationType.Question, Question = routes[request.SelectedAnswerIds[0]] });

        var history = string.Empty;
        var selections = string.Empty;

        async Task<QuestionnaireNext> Post(string button, QuestionDto current, Guid answer)
        {
            var page = CreatePostPage(
                button,
                new GetNextStateRequest
                {
                    CurrentQuestionId = current.Id,
                    CurrentQuestionOrder = current.Order,
                    SelectedAnswerIds = new List<Guid> { answer },
                    ValidateAnswers = button == "continueButton"
                },
                history,
                selections);

            var result = await page.OnPost();
            result.Should().BeOfType<PageResult>();

            history = page.AnswerHistory ?? string.Empty;
            selections = page.CurrentSelectedAnswers ?? string.Empty;
            return page;
        }

        // Q1 -> Q2 -> Q3
        (await Post("continueButton", q1, q1Answer)).Destination.Question!.Id.Should().Be(q2.Id);
        var atQ3 = await Post("continueButton", q2, q2AnswerToQ3);
        atQ3.Destination.Question!.Id.Should().Be(q3.Id);
        atQ3.Destination.PreviouslySelectedAnswerIds.Should().BeEmpty();

        // Back from Q3 -> Q2 restores Q2 answer and keeps Q3 answer
        var backToQ2 = await Post("backButton", q3, q3Answer);
        backToQ2.Destination.Question!.Id.Should().Be(q2.Id);
        backToQ2.Destination.PreviouslySelectedAnswerIds.Should().BeEquivalentTo(new[] { q2AnswerToQ3 });
        AnswerHistoryHelper.GetSelectedAnswers(selections, q3.Id).Should().BeEquivalentTo(new[] { q3Answer });

        // Change branch: Q2 -> Q4 -> Q5
        var atQ4 = await Post("continueButton", q2, q2AnswerToQ4);
        atQ4.Destination.Question!.Id.Should().Be(q4.Id);
        atQ4.Destination.PreviouslySelectedAnswerIds.Should().BeEmpty();
        AnswerHistoryHelper.GetSelectedAnswers(selections, q2.Id).Should().BeEquivalentTo(new[] { q2AnswerToQ4 });
        AnswerHistoryHelper.GetAnsweredQuestionIds(history).Should().Equal(q1.Id, q2.Id);

        (await Post("continueButton", q4, q4Answer)).Destination.Question!.Id.Should().Be(q5.Id);

        // Back from Q5 -> Q4
        var backToQ4 = await Post("backButton", q5, q5Answer);
        backToQ4.Destination.Question!.Id.Should().Be(q4.Id);
        backToQ4.Destination.PreviouslySelectedAnswerIds.Should().BeEquivalentTo(new[] { q4Answer });
        backToQ4.NextStateRequest.SelectedAnswerIds.Should().BeEquivalentTo(new[] { q4Answer });

        // Back from Q4 -> Q2 (not Q3)
        var backToQ2Again = await Post("backButton", q4, q4Answer);
        backToQ2Again.Destination.Question!.Id.Should().Be(q2.Id);
        backToQ2Again.Destination.PreviouslySelectedAnswerIds.Should().BeEquivalentTo(new[] { q2AnswerToQ4 });

        // Back from Q2 -> Q1
        var backToQ1 = await Post("backButton", q2, q2AnswerToQ4);
        backToQ1.Destination.Question!.Id.Should().Be(q1.Id);
        backToQ1.Destination.PreviouslySelectedAnswerIds.Should().BeEquivalentTo(new[] { q1Answer });

        // Continue forward again: Q1 -> Q2 -> Q4 preselects stored answers
        var forwardToQ2 = await Post("continueButton", q1, q1Answer);
        forwardToQ2.Destination.PreviouslySelectedAnswerIds.Should().BeEquivalentTo(new[] { q2AnswerToQ4 });
        var forwardToQ4 = await Post("continueButton", q2, q2AnswerToQ4);
        forwardToQ4.Destination.Question!.Id.Should().Be(q4.Id);
        forwardToQ4.Destination.PreviouslySelectedAnswerIds.Should().BeEquivalentTo(new[] { q4Answer });

        // All selections are retained
        AnswerHistoryHelper.GetSelectedAnswers(selections, q1.Id).Should().BeEquivalentTo(new[] { q1Answer });
        AnswerHistoryHelper.GetSelectedAnswers(selections, q2.Id).Should().BeEquivalentTo(new[] { q2AnswerToQ4 });
        AnswerHistoryHelper.GetSelectedAnswers(selections, q3.Id).Should().BeEquivalentTo(new[] { q3Answer });
        AnswerHistoryHelper.GetSelectedAnswers(selections, q4.Id).Should().BeEquivalentTo(new[] { q4Answer });
        AnswerHistoryHelper.GetSelectedAnswers(selections, q5.Id).Should().BeEquivalentTo(new[] { q5Answer });

        _mockApiClient.Verify(x => x.GetPreviousState(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string?>()), Times.Never);
    }    

    private QuestionnaireNext CreatePostPage(
        string submitButton,
        GetNextStateRequest request,
        string answerHistory,
        string currentSelectedAnswers)
    {
        var mockFormCollection = new Mock<IFormCollection>();
        mockFormCollection.Setup(x => x[It.IsAny<string>()]).Returns(submitButton);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.Form).Returns(mockFormCollection.Object);

        return new QuestionnaireNext(_mockApiClient.Object, _mockLogger.Object)
        {
            QuestionnaireSlug = "test-questionnaire",
            NextStateRequest = request,
            IsEmbedded = false,
            AnswerHistory = answerHistory,
            CurrentSelectedAnswers = currentSelectedAnswers,
            Embed = false,
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext { HttpContext = mockHttpContext.Object },
            Questionnaire = new QuestionnaireInfoDto { Id = Guid.NewGuid() },
            Destination = new DestinationDto()
        };
    }
}
