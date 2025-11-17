using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Questions;

public class AddEditQuestionsAndAnswers : BasePageModel
{
    [FromRoute]
    public Guid QuestionnaireId { get; set; }

    public IActionResult OnGet()
    {
        BackLinkSlug = string.Format(Routes.AddQuestion, QuestionnaireId);
        return Page();
    }
}