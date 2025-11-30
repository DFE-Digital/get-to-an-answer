using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Answers;

public class BulkAnswerOptions(ILogger<AddAnswerOptions> logger) : BasePageModel
{
    
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }
    
    [BindProperty]
    public string? QuestionNumber { get; set; } = "1";

    [BindProperty]
    public string? BulkAnswerOptionsRawText { get; set; }

    public void OnGet()
    {
        
    }
}