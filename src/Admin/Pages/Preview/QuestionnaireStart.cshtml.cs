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
            
            var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);
            
            if (questionnaire == null)
                return NotFound();
            
            IsEmbedded = Embed;

            // if the display title is not defined, redirect to the first question
            if (!questionnaire.HasStartPage)
            {
                return Redirect($"/admin/questionnaires/{QuestionnaireId}/next-preview?embed={Embed}");
            }
            
            Questionnaire = questionnaire;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return RedirectToPage("/Error");
        }
        return Page();
    }
}