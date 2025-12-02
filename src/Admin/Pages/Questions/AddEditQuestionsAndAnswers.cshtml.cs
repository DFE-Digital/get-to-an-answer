using System.Text.Json;
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
public class AddEditQuestionsAndAnswers(ILogger<AddEditQuestionsAndAnswers> logger, IApiClient apiClient)
    : BasePageModel
{
    [FromRoute] public Guid QuestionnaireId { get; set; }
    
    public string? QuestionnaireTitle { get; set; } 

    [BindProperty] public List<QuestionDto> Questions { get; set; } = [];
    
    [BindProperty]
    public bool FinishedEditing { get; set; }
    
    public EntityStatus Status { get; set; } = EntityStatus.Draft;
    
    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);

        if (TempData.Peek("QuestionnaireStatus") is int status)
        {
            Status = (EntityStatus) status;
        }
            
        if (TempData.Peek("QuestionnaireTitle") is string title)
        {
            QuestionnaireTitle = title;
        }

        try
        {
            logger.LogInformation("Getting questions for questionnaire {QuestionnaireId}", QuestionnaireId);
            var response = await apiClient.GetQuestionsAsync(QuestionnaireId);
            
            Questions = response.OrderBy(q => q.Order).ToList();
            
            TempData["QuestionCount"] = Questions.Count;

            // If a move error message was set during a previous POST, surface it in ModelState
            if (TempData.TryGetValue("MoveError", out var moveErrorObj) && moveErrorObj is string moveError &&
                !string.IsNullOrWhiteSpace(moveError))
            {
                ModelState.AddModelError("QuestionMoveError", moveError);
            }
            
            if (TempData.TryGetValue("CompletionTrackingMap", out var trackingMapObj) && 
                trackingMapObj is string trackingMapJson)
            {
                try
                {
                    var trackingMap = JsonSerializer
                        .Deserialize<Dictionary<CompletableTask, CompletionStatus>>(trackingMapJson);
                    FinishedEditing = trackingMap?[CompletableTask.AddQuestionsAndAnswers] ==
                                      CompletionStatus.Completed;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error deserializing completion tracking map");
                }
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning("No questions found for Questionniare {QuestionniareId}", QuestionnaireId);
            return Page();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting questions for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAndContinueAsync()
    {
        await apiClient.UpdateCompletionStateAsync(QuestionnaireId, new UpdateCompletionStateRequestDto
        {
            Task = CompletableTask.AddQuestionsAndAnswers,
            Status = FinishedEditing ? CompletionStatus.Completed : 
                TempData.TryGetValue("QuestionCount", out var countObj) && countObj is > 0 ? 
                    CompletionStatus.InProgress : CompletionStatus.NotStarted
        });
        
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
    
    public async Task<IActionResult> OnPostMoveUpAsync(Guid questionnaireId, Guid questionId)
    {
        try
        {
            await apiClient.MoveQuestionUpOneAsync(questionnaireId, questionId);

            // PRG - redirect back to GET for the same questionnaire so refresh won't resubmit
            return RedirectToPage(new { questionnaireId });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            // Set an error message in TempData and redirect back (PRG)
            TempData["MoveError"] = "You cannot move this question further up";
            return RedirectToPage(new { questionnaireId });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error moving question up for questionnaire {QuestionnaireId}", questionnaireId);
            return RedirectToErrorPage();
        }
    }

    public async Task<IActionResult> OnPostMoveDownAsync(Guid questionnaireId, Guid questionId)
    {
        try
        {
            await apiClient.MoveQuestionDownOneAsync(questionnaireId, questionId);

            // PRG - redirect back to GET for the same questionnaire so refresh won't resubmit
            return RedirectToPage(new { questionnaireId });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            // Set an error message in TempData and redirect back (PRG)
            TempData["MoveError"] = "You cannot move this question further down.";
            return RedirectToPage(new { questionnaireId });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error moving question down for questionnaire {QuestionnaireId}", questionnaireId);
            return RedirectToErrorPage();
        }
    }
}