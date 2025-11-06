using System.ComponentModel.DataAnnotations;
using Admin.Models;
using Admin.Models.ViewModels;
using Common.Client;
using Common.Domain.Request.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Questionnaire;

[Authorize]
public class CreateQuestionnaire(IApiClient apiClient) : QuestionnairePageModel
{
    [BindProperty] [Required] public string Title { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var response = await apiClient.CreateQuestionnaireAsync(new CreateQuestionnaireRequestDto
            {
                Title = Title
            });

            TempData[nameof(QuestionnairesViewModel)] = JsonConvert.SerializeObject(new QuestionnairesViewModel { JustCreated = true });
            
            return Redirect($"/admin/questionnaires/{response?.Id}/track");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}