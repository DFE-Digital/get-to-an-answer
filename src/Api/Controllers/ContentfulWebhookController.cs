using Api.Services;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

using System.Security.Cryptography;
using System.Text;
// ... existing code ...
[ApiController]
[Route("api/webhooks/contentful")]
public class ContentfulWebhookController(IConfiguration config, IContentfulSyncService sync) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken ct)
    {
        // Verify signature if provided
        var secret = config["Contentful:WebhookSigningSecret"];
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(ct);

        /*var signatureHeader = Request.Headers["x-contentful-signature"].FirstOrDefault();
        if (!string.IsNullOrEmpty(secret) && !string.IsNullOrEmpty(signatureHeader))
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            var expected = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            if (!string.Equals(expected, signatureHeader, StringComparison.OrdinalIgnoreCase))
                return Unauthorized();
        }*/

        var eventType = Request.Headers["X-Contentful-Topic"].FirstOrDefault() ?? "";
        // Examples:
        // ContentManagement.Entry.publish
        // ContentManagement.Entry.unpublish
        // ContentManagement.Entry.delete

        await sync.HandleWebhook(eventType, body, ct);
        return Ok();
    }
}
// ... existing code ...