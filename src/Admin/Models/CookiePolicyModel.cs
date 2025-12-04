using Azure;
using Admin.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Models;

public class CookiePolicyModel
{
    public bool AcceptCookies { get; set; }
    
    public bool ShowSuccessBanner { get; set; }
    
    public Page? Page { get; set; }
}