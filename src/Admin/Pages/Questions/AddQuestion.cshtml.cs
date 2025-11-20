using System.ComponentModel.DataAnnotations;
using Common.Client;
using Common.Domain;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

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
    [Required(ErrorMessage = "Enter question hint text")]
    public string QuestionHintText { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Select question type")]
    public QuestionType QuestionType { get; set; }
    
    public List<QuestionDto> Questions { get; set; }

    // New navigation properties for the radio that reveals dropdowns / inputs
    public enum NavigationOptionType
    {
        NextQuestion,
        SpecificQuestion,
        ResultsPageInternal,
        ResultsPageExternal
    }

    [BindProperty]
    [Required(ErrorMessage = "Select where to go after this question")]
    public NavigationOptionType NavigationOption { get; set; } = NavigationOptionType.NextQuestion;

    // When SpecificQuestion is selected
    [BindProperty] public Guid? SelectedQuestionId { get; set; }

    // When ResultsPageInternal is selected
    [BindProperty] public Guid? SelectedResultsPageId { get; set; }

    // When ResultsPageExternal is selected
    [BindProperty]
    [Url(ErrorMessage = "Enter a valid URL")]
    public string ExternalResultsLink { get; set; } = "";

    // Select lists for the UI (populated in OnGet)
    public IEnumerable<SelectListItem> QuestionSelectList { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> ResultsPageSelectList { get; set; } = Enumerable.Empty<SelectListItem>();

    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);
        
        Questions = await apiClient.GetQuestionsAsync(QuestionnaireId);    
        
        QuestionSelectList = Questions.Select(q => new SelectListItem(q.Content, q.Id.ToString()));

        // ResultsPageSelectList = new[]
        // {
        //     new SelectListItem("Choose a results page", ""),
        //     // new SelectListItem("15 or under page", "guid-or-id-here") // replace
        // };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            // Create the question (core fields)
            var response = await apiClient.CreateQuestionAsync(new CreateQuestionRequestDto
            {
                QuestionnaireId = QuestionnaireId,
                Content = QuestionContent,
                Description = QuestionHintText,
                Type = QuestionType
            });

            // TODO: persist navigation settings:
            // NavigationOption, SelectedQuestionId, SelectedResultsPageId, ExternalResultsLink
            // You should call the appropriate API endpoints to save routing for the created question.
            // Below is a placeholder comment showing intent.

            // if (NavigationOption == NavigationOptionType.SpecificQuestion && SelectedQuestionId.HasValue)
            // {
            //     await apiClient.SetQuestionRedirectAsync(response.Id, SelectedQuestionId.Value);
            // }
            // else if (NavigationOption == NavigationOptionType.ResultsPageInternal && SelectedResultsPageId.HasValue)
            // {
            //     await apiClient.SetQuestionResultPageAsync(response.Id, SelectedResultsPageId.Value);
            // }
            // else if (NavigationOption == NavigationOptionType.ResultsPageExternal && !string.IsNullOrWhiteSpace(ExternalResultsLink))
            // {
            //     await apiClient.SetQuestionExternalResultLinkAsync(response.Id, ExternalResultsLink);
            // }

            return Redirect(string.Format(Routes.AddAnswerOptions, QuestionnaireId, response?.Id));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating question for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }
    }
}