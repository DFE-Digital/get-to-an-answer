using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Preview;

[IgnoreAntiforgeryToken]
public class QuestionnaireStart(IApiClient apiClient, ILogger<QuestionnaireStart> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    [BindProperty] public required bool IsEmbedded { get; set; }
    [BindProperty] public required QuestionnaireInfoDto Questionnaire { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            var questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
            
            if (questionnaire == null)
                return NotFound();
            
            IsEmbedded = Embed;

            // if the display title is not defined, redirect to the first question
            if (string.IsNullOrWhiteSpace(questionnaire.DisplayTitle))
            {
                return Redirect($"/admin/questionnaires/{QuestionnaireId}/next-preview?embed={Embed}");
            }
            
            Questionnaire = new QuestionnaireInfoDto()
            {
                Id = questionnaire.Id,
                DisplayTitle = questionnaire.DisplayTitle ?? questionnaire.Title,
                Slug = questionnaire.Slug,
            
                DecorativeImage = questionnaire.DecorativeImage,
            
                TextColor = questionnaire.TextColor,
                BackgroundColor = questionnaire.BackgroundColor,
                PrimaryButtonColor = questionnaire.PrimaryButtonColor,
                SecondaryButtonColor = questionnaire.SecondaryButtonColor,
                StateColor = questionnaire.StateColor,
                ErrorMessageColor = questionnaire.ErrorMessageColor,
            
                ContinueButtonText = questionnaire.ContinueButtonText
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return RedirectToPage("/Error");
        }
        return Page();
    }
}