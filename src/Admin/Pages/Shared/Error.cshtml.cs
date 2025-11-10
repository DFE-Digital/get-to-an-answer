using System.Diagnostics;
using Admin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Shared;

public class Error : PageModel
{
    public ErrorViewModel ViewModel { get; } = new();
    
    public void OnGet()
    {
        ViewModel.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}