using Api.Services;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Enum;
using FluentAssertions;
using Unit.Tests.Util;

namespace Unit.Tests;

public class BranchingHealthTests
{
    private const string UserEmail = "john.doe@test.com";

    [Fact]
    public async Task Questionnaire_Has_Cyclic_Branching_Health()
    {
        await using var db = TestUtils.CreateInMemoryDb(nameof(Questionnaire_Has_Cyclic_Branching_Health));
        var questionnaireService = new QuestionnaireService(db);
        var questionService = new QuestionService(db);
        var answerService = new AnswerService(db);
        
        var questionnaire = (await questionnaireService.CreateQuestionnaire(UserEmail, new CreateQuestionnaireRequestDto
        {
            Title = "Test",
        })).Value as QuestionnaireDto;
        
        questionnaire.Should().NotBeNull();
        
        var question1 = (await questionService.CreateQuestion(UserEmail, new CreateQuestionRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            Content = "Test",
            Type = QuestionType.SingleSelect,
        })).Value as QuestionDto;
        
        question1.Should().NotBeNull();
        
        var question2 = (await questionService.CreateQuestion(UserEmail, new CreateQuestionRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            Content = "Test",
            Type = QuestionType.MultiSelect,
        })).Value as QuestionDto;
        
        question2.Should().NotBeNull();
        
        var question1Answer = (await answerService.CreateAnswer(UserEmail, new CreateAnswerRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            QuestionId = question1.Id,
            Content = "Test",
            DestinationType = DestinationType.Question,
            DestinationQuestionId = question2.Id,
        })).Value as AnswerDto;
        
        var question2Answer = (await answerService.CreateAnswer(UserEmail, new CreateAnswerRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            QuestionId = question2.Id,
            Content = "Test",
            DestinationType = DestinationType.Question,
            DestinationQuestionId = question1.Id,
        })).Value as AnswerDto;

        var result = QuestionnaireService.IsBranchingHealthy(db.Questionnaires.First());
        result.Should().Be(BranchingHealthType.Cyclic);
    }
    
    [Fact]
    public async Task Questionnaire_Has_Broken_Branching_Health()
    {
        await using var db = TestUtils.CreateInMemoryDb(nameof(Questionnaire_Has_Cyclic_Branching_Health));
        var questionnaireService = new QuestionnaireService(db);
        var questionService = new QuestionService(db);
        var answerService = new AnswerService(db);
        
        var questionnaire = (await questionnaireService.CreateQuestionnaire(UserEmail, new CreateQuestionnaireRequestDto
        {
            Title = "Test",
        })).Value as QuestionnaireDto;
        
        questionnaire.Should().NotBeNull();
        
        var question1 = (await questionService.CreateQuestion(UserEmail, new CreateQuestionRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            Content = "Test",
            Type = QuestionType.SingleSelect,
        })).Value as QuestionDto;
        
        question1.Should().NotBeNull();
        
        var question2 = (await questionService.CreateQuestion(UserEmail, new CreateQuestionRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            Content = "Test",
            Type = QuestionType.MultiSelect,
        })).Value as QuestionDto;
        
        question2.Should().NotBeNull();
        
        var question1Answer = (await answerService.CreateAnswer(UserEmail, new CreateAnswerRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            QuestionId = question1.Id,
            Content = "Test",
            DestinationType = DestinationType.Question,
            DestinationQuestionId = question2.Id,
        })).Value as AnswerDto;
        
        var question2Answer = (await answerService.CreateAnswer(UserEmail, new CreateAnswerRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            QuestionId = question2.Id,
            Content = "Test",
        })).Value as AnswerDto;

        var result = QuestionnaireService.IsBranchingHealthy(db.Questionnaires.First());
        result.Should().Be(BranchingHealthType.Broken);
    }
    
    [Fact]
    public async Task Questionnaire_Has_Ok_Branching_Health()
    {
        await using var db = TestUtils.CreateInMemoryDb(nameof(Questionnaire_Has_Cyclic_Branching_Health));
        var questionnaireService = new QuestionnaireService(db);
        var questionService = new QuestionService(db);
        var answerService = new AnswerService(db);
        
        var questionnaire = (await questionnaireService.CreateQuestionnaire(UserEmail, new CreateQuestionnaireRequestDto
        {
            Title = "Test",
        })).Value as QuestionnaireDto;
        
        questionnaire.Should().NotBeNull();
        
        var question1 = (await questionService.CreateQuestion(UserEmail, new CreateQuestionRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            Content = "Test",
            Type = QuestionType.SingleSelect,
        })).Value as QuestionDto;
        
        question1.Should().NotBeNull();
        
        var question2 = (await questionService.CreateQuestion(UserEmail, new CreateQuestionRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            Content = "Test",
            Type = QuestionType.MultiSelect,
        })).Value as QuestionDto;
        
        question2.Should().NotBeNull();
        
        var question1Answer = (await answerService.CreateAnswer(UserEmail, new CreateAnswerRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            QuestionId = question1.Id,
            Content = "Test",
            DestinationType = DestinationType.Question,
            DestinationQuestionId = question2.Id,
        })).Value as AnswerDto;
        
        var question2Answer = (await answerService.CreateAnswer(UserEmail, new CreateAnswerRequestDto
        {
            QuestionnaireId = questionnaire.Id,
            QuestionId = question2.Id,
            Content = "Test",
            DestinationType = DestinationType.ExternalLink,
            DestinationUrl = "https://test.com",
        })).Value as AnswerDto;

        var result = QuestionnaireService.IsBranchingHealthy(db.Questionnaires.First());
        result.Should().Be(BranchingHealthType.Ok);
    }
}