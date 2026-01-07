using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Pages.Questionnaire;

[IgnoreAntiforgeryToken]
public class QuestionnaireStart(
    IApiClient apiClient, 
    IImageStorageClient imageStorageClient,
    ILogger<QuestionnaireStart> logger) : QuestionnairesPageModel
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
            HttpContext.Features.Set(new QuestionnaireRunFeature
            {
                IsEmbedded = Embed
            });
        
            if (QuestionnaireSlug == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireSlug);

            if (questionnaire == null)
                return NotFound();

            // if the display title is not defined, redirect to the first question
            if (!questionnaire.HasStartPage)
            {
                return Redirect($"/questionnaires/{QuestionnaireSlug}/next?embed={Embed}");
            }

            Questionnaire = questionnaire;

            if (!await imageStorageClient.CheckImageExistsAsync($"{questionnaire.Id}/published"))
            {
                logger.LogWarning("Questionnaire image not found: {QuestionnaireId}", questionnaire.Id);
                questionnaire.DecorativeImage = null;
            }

            IsEmbedded = Embed;
        }
        catch (GetToAnAnswerApiException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogError(e, "Questionnaire not found: {QuestionnaireSlug}", QuestionnaireSlug);
            return NotFound();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            throw;
        }
        return Page();
    }
}