using Common.Domain;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Questionnaire;

public class Index : QuestionnairesPageModel
{
    [BindProperty] public required bool IsEmbedded { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }
    
    public void OnGet()
    {
        IsEmbedded = Embed;
    }
}