using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Pages;

[AllowAnonymous]
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