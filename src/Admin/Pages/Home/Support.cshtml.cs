using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Home;

[AllowAnonymous]
public class Support : PageModel
{
    public void OnGet()
    {
        
    }
}