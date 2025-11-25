using System.Diagnostics;
using Common.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Shared;

public class Error2 : PageModel
{
    public ErrorViewModel ViewModel { get; } = new();
    
    public IActionResult OnGet()
    {
        ViewModel.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return Page();  
    }
}