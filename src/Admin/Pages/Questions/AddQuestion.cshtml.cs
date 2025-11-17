using System.ComponentModel.DataAnnotations;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages.Questions;

public class AddQuestion : BasePageModel
{
    [FromQuery(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    public string QuestionNumber { get; set; } = 1.ToString();

    [BindProperty] [Required] public string QuestionContent { get; set; } = "";

    [BindProperty] [Required] public string QuestionHintText { get; set; } = "";

    [BindProperty] [Required] public QuestionType QuestionType { get; set; }

    public IActionResult OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        return Page();
    }
}