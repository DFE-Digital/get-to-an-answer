using System.Diagnostics;
using AngleSharp.Common;
using Common.Client;
using Common.Domain;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Integration;

[Authorize]
public class IntegrationGuide(IApiClient apiClient, IMsGraphClient graphClient, IConfiguration configuration)
    : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public required Guid QuestionnaireId { get; set; }

    public string FrontEndUrl { get; set; } = "";

    [BindProperty] public required QuestionnaireDto? Questionnaire { get; set; }

    public async Task OnGet()
    {
        Questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);

        FrontEndUrl = GenerateFrontendUrl();
    }

    private string GenerateFrontendUrl()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        // get scheme and host from request headers
        var scheme = Request.Scheme;
        var host = Request.Host;

        if (host.Host == "localhost" || env == null || env == "Local")
        {
            // get appsetting for GtaaFrontendLocalPort

            var port = configuration.GetValue<int>("GtaaFrontendLocalPort");

            return $"{scheme}://{host.Host}:{port}";
        }
        var adminSubdomain = ToEnvAdminSubdomain(env);
        var frontendSubdomain = ToEnvFrontendSubdomain(env);

        return $"{scheme}://{host.Value?.Replace(adminSubdomain, frontendSubdomain)}";
    }

private string ToEnvAdminSubdomain(string env) => env switch
    {
        "Development" => "dev-admin", 
        "Test" => "staging-admin", 
        "Production" => "admin", 
        _ => String.Empty
    };
    
    private string ToEnvFrontendSubdomain(string env) => env switch
    {
        "Development" => "dev", 
        "Test" => "staging", 
        "Production" => "www", 
        _ => String.Empty
    };
}