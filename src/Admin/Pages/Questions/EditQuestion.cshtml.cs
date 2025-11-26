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

    [BindProperty] public string QuestionContent { get; set; } = string.Empty;

    [BindProperty] public string? QuestionHintText { get; set; }

    [BindProperty] public QuestionType QuestionType { get; set; }

    public List<AnswerSummaryViewModel> Answers { get; } = [];

    public string QuestionNumber => "1";

    [TempData(Key = "QuestionSaved")] public bool QuestionSaved { get; set; }

    [TempData(Key = "CurrentQuestionHasNextOne")]
    public bool CurrentQuestionHasNextOne { get; set; }
    
    [TempData(Key = "NextQuestionId")] public Guid? NextQuestionId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);

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
            Destination = a.DestinationType.ToString() ?? string.Empty
        }));

        return Page();
    }

    public async Task<IActionResult> OnPostSaveQuestion()
    {
        try
        {
            if (!ModelState.IsValid)
                return Page();

            await apiClient.UpdateQuestionAsync(QuestionId, new UpdateQuestionRequestDto
            {
                Content = QuestionContent,
                Description = QuestionHintText,
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

    public async Task<IActionResult> OnPostDeleteQuestion()
    {
        await apiClient.DeleteQuestionAsync(QuestionId);
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
    
    public IActionResult OnPostAddAQuestion() => Redirect(string.Format(Routes.AddQuestion, QuestionnaireId));

    public string QuestionTypeFriendly(QuestionType type) =>
        type switch
        {
            QuestionType.SingleSelect => "One option only (radio button)",
            QuestionType.DropdownSelect => "One option only (drop-down)",
            QuestionType.MultiSelect => "One or more options (multi-select)",
            _ => type.ToString()
        };

    public string QuestionTypeHint(QuestionType type) =>
        type switch
        {
            QuestionType.SingleSelect => "Use this when you have a small list of options.",
            QuestionType.DropdownSelect => "Use this when you have a long list of options.",
            QuestionType.MultiSelect => "Use this when people can select more than one option.",
            _ => string.Empty
        };


    private async Task<QuestionDto?> GetQuestion() => await apiClient.GetQuestionAsync(QuestionId);

    private void PopulateFields(QuestionDto question)
    {
        QuestionContent = question.Content;
        QuestionHintText = question.Description;
        QuestionType = question.Type;
    }
}

public class AnswerSummaryViewModel
{
    public string Content { get; set; } = string.Empty;
    public string Priority { get; set; } = "0";
    public string Destination { get; set; } = string.Empty;
}