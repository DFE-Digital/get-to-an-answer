using System.ComponentModel.DataAnnotations;
using Admin.Models;
using Common.Client;
using Common.Domain.Request.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Questionnaire;

[Authorize]
public class CreateQuestionnaire(IApiClient apiClient) : QuestionnaireViewModel
{
    [BindProperty]
    [Required]
    public string Title { get; set; } = string.Empty;

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