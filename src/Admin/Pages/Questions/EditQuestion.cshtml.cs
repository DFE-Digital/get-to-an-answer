using Common.Client;
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

    [BindProperty]
    public string QuestionContent { get; set; } = string.Empty;

    [BindProperty]
    public string? QuestionHintText { get; set; }

    [BindProperty]
    public QuestionType QuestionType { get; set; }

    public List<AnswerSummaryViewModel> Answers { get; } = new();

    public string QuestionNumber => "1";
    
    public async Task<IActionResult> OnGetAsync()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);

        var question = await apiClient.GetQuestionAsync(QuestionId);
        if (question == null)
        {
            logger.LogWarning("Question {QuestionId} not found", QuestionId);
            RedirectToErrorPage();
            return Page();
        }

        QuestionContent = question.Content;
        QuestionHintText = question.Description;
        QuestionType = question.Type;

        // var answers = await apiClient.GetAnswersAsync(QuestionId);
        // Answers.Clear();
        // Answers.AddRange(answers.Select(a => new AnswerSummaryViewModel
        // {
        //     Content = a.Content,
        //     Priority = a.Priority?.ToString() ?? "0",
        //     Destination = a.DestinationType.ToString()
        // }));
        //
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        // await apiClient.UpdateQuestionAsync(QuestionId, new UpdateQuestionRequestDto
        // {
        //     Title = QuestionContent,
        //     Description = QuestionHintText,
        //     Type = QuestionType
        // });

        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }

    public async Task<IActionResult> OnPostDelete()
    {
        await apiClient.DeleteQuestionAsync(QuestionId);
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }

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
}

public class AnswerSummaryViewModel
{
    public string Content { get; set; } = string.Empty;
    public string Priority { get; set; } = "0";
    public string Destination { get; set; } = string.Empty;
}