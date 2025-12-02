using System.ComponentModel.DataAnnotations;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages.Questions;

[Authorize]
public class AddQuestion(ILogger<AddQuestion> logger, IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    public string QuestionNumber { get; set; } = 1.ToString();

    [BindProperty] 
    [Required(ErrorMessage = "Enter a question")] 
    public string QuestionContent { get; set; } = "";

    [BindProperty] 
    public string? QuestionHintText { get; set; } = "";

    [BindProperty] 
    [Required(ErrorMessage = "Select question type")]
    public QuestionType QuestionType { get; set; }

    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);

        try
        {
            var existingQuestions = await apiClient.GetQuestionsAsync(QuestionnaireId);

            if (existingQuestions.Count > 0)
                QuestionNumber = (existingQuestions.Max(q => q.Order) + 1).ToString();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Page();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();
        
        try
        {
            var response = await apiClient.CreateQuestionAsync(new CreateQuestionRequestDto
            {
                QuestionnaireId = QuestionnaireId,
                Content = QuestionContent,
                Description = QuestionHintText,
                Type = QuestionType
            });
            
            TempData["QuestionType"] = (int)QuestionType;
         
            return Redirect(string.Format(Routes.AddAnswerOptions, QuestionnaireId, response?.Id));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating question for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }
    }
}