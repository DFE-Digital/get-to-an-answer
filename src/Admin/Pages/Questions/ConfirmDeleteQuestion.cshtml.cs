using System.ComponentModel.DataAnnotations;
using Admin.Models;
using Common.Client;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Questions;

public class ConfirmDeleteQuestion(ILogger<ConfirmDeleteQuestion> logger, IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [BindProperty] public string? QuestionNumber { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Confirm you want to delete this question")]
    public bool? ConfirmDelete { get; set; }

    public IActionResult OnGet()
    {
        BackLinkSlug = string.Format(Routes.AddAndEditQuestionsAndAnswers, QuestionId);
        QuestionNumber = TempData.Peek("QuestionNumber") as string;
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
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
            JsonConvert.SerializeObject(new QuestionNotificationSummary(IsDeleted: true, QuestionTitle: questionTitle));
        
        return Redirect(string.Format(Routes.AddAndEditQuestionsAndAnswers, QuestionId));
    }
}