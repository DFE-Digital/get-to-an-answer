using Common.Accessibility;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Home;

[AllowAnonymous]
public class SiteMap(SitemapService sitemapService) : BasePageModel
{
    public IActionResult OnGet()
    {
        var sitemap = sitemapService.GenerateSitemap(HttpContext);
        Response.Headers.CacheControl = "public, max-age=3600";
        return Content(sitemap.ToString(), "application/xml");
    }
}