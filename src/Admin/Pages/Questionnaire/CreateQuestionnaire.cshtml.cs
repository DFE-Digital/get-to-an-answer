using Admin.Models;
using Common.Client;
using Common.Domain.Request.Create;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages.Questionnaire;

public class CreateQuestionnaire(IApiClient apiClient) : QuestionnaireViewModel
{
    [BindProperty] public string Title { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            await apiClient.CreateQuestionnaireAsync(new CreateQuestionnaireRequestDto
            {
                Title = Title
            });

            return Page();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}