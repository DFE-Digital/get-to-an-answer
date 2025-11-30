using System.Xml.Linq;
using AngleSharp.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Common.Accessibility;

public class SitemapService(IActionDescriptorCollectionProvider actionDescriptorProvider)
{
    private static readonly string[] Excluded = ["MicrosoftIdentity", "Home", "Shared"];
    
    public XDocument GenerateSitemap(HttpContext httpContext)
    {
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        
        // Define the namespace
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        
        var pages = actionDescriptorProvider.ActionDescriptors.Items
            .OfType<PageActionDescriptor>()
            .Where(p => p.AttributeRouteInfo?.Template != null)
            .Select(p =>
            {
                var template = p.AttributeRouteInfo!.Template;

                if (template == null || template.Equals("/")) 
                    return string.Empty;
                    
                return template.Replace(":guid", "");
            })
            .Where(p => !Excluded.Any(p.Contains))
            .Distinct()
            .OrderBy(template => template)
            .ToList();

        var sitemap = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "urlset",
                pages.Select(page => new XElement(ns + "url",
                    new XElement(ns + "loc", $"{baseUrl}/{page}")
                ))
            )
        );

        return sitemap;
    }
}

