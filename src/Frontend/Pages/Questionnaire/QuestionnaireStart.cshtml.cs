using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Pages.Questionnaire;

[IgnoreAntiforgeryToken]
public class QuestionnaireStart(IApiClient apiClient, ILogger<QuestionnaireStart> logger) : QuestionnairesPageModel
{
    [BindProperty] public required bool IsEmbedded { get; set; }
    [BindProperty] public required QuestionnaireInfoDto Questionnaire { get; set; }
    
    [FromRoute(Name = "questionnaireSlug")] 
    public new string? QuestionnaireSlug { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            if (QuestionnaireSlug == null)
                return NotFound();
            
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireSlug);
            
            if (questionnaire == null)
                return NotFound();
            
            Questionnaire = questionnaire;
            
            IsEmbedded = Embed;

            // if the display title is not defined, redirect to the first question
            if (string.IsNullOrWhiteSpace(questionnaire.DisplayTitle))
            {
                return Redirect($"/questionnaires/{QuestionnaireSlug}/next?embed={Embed}");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return RedirectToPage("/Error");
        }
        return Page();
    }
}