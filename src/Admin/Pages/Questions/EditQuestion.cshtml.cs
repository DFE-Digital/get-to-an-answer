using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Common.Client;
using Common.Domain;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Questions;

[Authorize]
public class EditQuestion(IApiClient apiClient, ILogger<EditQuestion> logger) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }
    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [Required(ErrorMessage = "The question is required")]
    [BindProperty] public string QuestionContent { get; set; } = string.Empty;

    [BindProperty] public string? QuestionHintText { get; set; }

    [Required(ErrorMessage = "Select question type")]
    [BindProperty] public QuestionType QuestionType { get; set; }

    public List<AnswerSummaryViewModel> Answers { get; } = [];

    [BindProperty] public string QuestionNumber { get; set; } = 1.ToString();

    [TempData(Key = "QuestionSaved")] public bool QuestionSaved { get; set; }

    [TempData(Key = "CurrentQuestionHasNextOne")]
    public bool CurrentQuestionHasNextOne { get; set; }
    
    [TempData(Key = "NextQuestionId")] public Guid? NextQuestionId { get; set; }
    
    public QuestionnaireState? QuestionnaireState { get; set;}

    public async Task<IActionResult> OnGetAsync()
    {
        BackLinkSlug = string.Format(Routes.AddAndEditQuestionsAndAnswers, QuestionnaireId);

        try
        {
            var question = await GetQuestion();

            if (question == null)
            {
                logger.LogWarning("Question {QuestionId} not found", QuestionId);
                throw new Exception($"Question {QuestionId} not found");
            }

            PopulateFields(question);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return RedirectToErrorPage();
        }

        var answers = await apiClient.GetAnswersAsync(QuestionId);
        Answers.Clear();
        Answers.AddRange(answers.Select(a => new AnswerSummaryViewModel
        {
            Content = a.Content,
            Priority = Convert.ToString(a.Priority, CultureInfo.CurrentCulture),
            Destination = ToDestDisplayName(a.DestinationType)
        }));

        return Page();
    }
    
    private static string ToDestDisplayName(DestinationType? type) => type switch
    {
        DestinationType.Question => "Specific question",
        DestinationType.CustomContent => "Results page",
        DestinationType.ExternalLink => "Link",
        _ => "Next question",
    };

    public async Task<IActionResult> OnPostSaveQuestion()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                QuestionSaved = false;
                return Page();
            }

            await apiClient.UpdateQuestionAsync(QuestionId, new UpdateQuestionRequestDto
            {
                Content = QuestionContent,
                Description = QuestionHintText ?? string.Empty,
                Type = QuestionType
            });

            var question = await GetQuestion();

            if (question == null)
            {
                logger.LogWarning("Question {QuestionId} not found", QuestionId);
                throw new Exception($"Question {QuestionId} not found");
            }

            PopulateFields(question);
            QuestionSaved = true;

            var questionForQuestionnaire = await apiClient.GetQuestionsAsync(QuestionnaireId);
            var nextQuestionInOrder = questionForQuestionnaire.SingleOrDefault(q => q.Order == question.Order + 1);

            if (nextQuestionInOrder != null)
            {
                CurrentQuestionHasNextOne = true;
                NextQuestionId = nextQuestionInOrder.Id;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return RedirectToErrorPage();
        }

        return Page();
    }

    public async Task <IActionResult> OnPostAnswerEditAsync()
    {
        await OnPostSaveQuestion();
        
        return Redirect(string.Format(Routes.EditAnswerOptions, QuestionnaireId, QuestionId));
    }

    public IActionResult OnPostDeleteQuestion()
    {
        TempData["TitleOfQuestionToBeDeleted"] = QuestionContent;
        TempData["NumberOfQuestionToBeDeleted"] = QuestionNumber;
        return Redirect(string.Format(Routes.ConfirmDeleteQuestion, QuestionnaireId, QuestionId));
    }
    
    public IActionResult OnPostAddQuestion() => Redirect(string.Format(Routes.AddQuestion, QuestionnaireId));

    private async Task<QuestionDto?> GetQuestion() => await apiClient.GetQuestionAsync(QuestionId);

    private void PopulateFields(QuestionDto question)
    {
        QuestionContent = question.Content;
        QuestionHintText = question.Description;
        QuestionType = question.Type;
        QuestionNumber = question.Order.ToString();
    }
}

public class AnswerSummaryViewModel
{
    public string Content { get; set; } = string.Empty;
    public string Priority { get; set; } = "0";
    public string Destination { get; set; } = string.Empty;
}