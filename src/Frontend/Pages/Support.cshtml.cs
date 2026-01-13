using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class Support : QuestionnairesPageModel
{
    [BindProperty] public required bool IsEmbedded { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }
    
    public void OnGet()
    {
        IsEmbedded = Embed;
    }
}