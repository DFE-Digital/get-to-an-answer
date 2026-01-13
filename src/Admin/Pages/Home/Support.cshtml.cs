using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Home;

public class Support : PageModel
{
    public bool IsEmbedded { get; set; } = false;
    
    public void OnGet()
    {
        
    }
}