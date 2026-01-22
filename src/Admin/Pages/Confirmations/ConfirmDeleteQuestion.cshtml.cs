using System.ComponentModel.DataAnnotations;
using Admin.Models;
using Common.Client;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Confirmations;

[Authorize]
public class ConfirmDeleteQuestion(ILogger<ConfirmDeleteQuestion> logger, IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }
    
    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [BindProperty] public string? QuestionNumber { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Confirm you want to delete this question")]
    public bool ConfirmDelete { get; set; }

    public IActionResult OnGet()
    {
        BackLinkSlug = string.Format(Routes.EditQuestion, QuestionnaireId, QuestionId);
        QuestionNumber = TempData.Peek("NumberOfQuestionToBeDeleted") as string;
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (ConfirmDelete)
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                logger.LogInformation($"Deleting question {QuestionId}");
                await apiClient.DeleteQuestionAsync(QuestionId);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error deleting question {QuestionId}", QuestionId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var questionTitle = TempData.Peek("TitleOfQuestionToBeDeleted") as string;

            TempData["DeletedQuestion"] =
                JsonConvert.SerializeObject(new QuestionNotificationSummary(IsDeleted: true,
                    QuestionTitle: questionTitle));

            await UpdateStatusIfNotQuestionsLeft();

            return Redirect(string.Format(Routes.AddAndEditQuestionsAndAnswers, QuestionnaireId));
        }
        
        return Redirect(string.Format(Routes.EditQuestion, QuestionnaireId, QuestionId));
    }
    
    private async Task UpdateStatusIfNotQuestionsLeft()
    {
        // if one before deletion, then will be 0 after
        // so, if so, 0 after deletion, mark as not started
        if (TempData.Peek("QuestionCount") is 1)
        {
            await apiClient.UpdateCompletionStateAsync(QuestionnaireId, new UpdateCompletionStateRequestDto
            {
                Task = CompletableTask.AddQuestionsAndAnswers,
                Status = CompletionStatus.NotStarted
            });
        }
    }
}