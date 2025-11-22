using Common.Local;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace Common.Extensions;

public static class MicrosoftIdentityExtensions
{
    public static MicrosoftIdentityWebApiAuthenticationBuilder AddMicrosoftIdentityWebApi(
        this AuthenticationBuilder builder, WebApplicationBuilder appBuilder)
    {
        var environment = appBuilder.Environment.EnvironmentName;
        var configurationSection = appBuilder.Configuration.GetSection("AzureAd"); 
        
        var config = builder.AddMicrosoftIdentityWebApi(options =>
            {
                var clientId = configurationSection.GetValue<string>("ClientId")!;
            
                options.TokenValidationParameters.ValidAudiences = new List<string>
                {
                    clientId, $"api://{clientId}"
                };

                if (environment == "Development")
                {
                    options.AddDeveloperJwtOptions();
                }
            }, 
            configurationSection.Bind);

        return config;
    }
    
    public static MicrosoftIdentityWebAppAuthenticationBuilder AddMicrosoftIdentityWebApp(
        this AuthenticationBuilder builder, IConfigurationSection configurationSection)
    {
        var authBuilder = builder.AddMicrosoftIdentityWebApp(options =>
        {
            configurationSection.Bind(options);

            var scopes = configurationSection.GetValue<string>("Scope")?.Split(' ') ?? ["offline_access"];

            foreach (var scope in scopes) {
                options.Scope.Add(scope);
            }
            
            options.ResponseMode = "form_post";
            options.ResponseType = "id_token code";
            
            var forceConsentPrompt = configurationSection.GetValue<bool>("ForceConsentPrompt");

            if (forceConsentPrompt)
            {
                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    context.ProtocolMessage.Prompt = "consent"; // or "select_account", "login", etc.
                    return Task.CompletedTask;
                };
            }
        });
            
        authBuilder.Services
        .Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.UseTokenLifetime = true; // optional, if you want cookie to match token lifetime
            options.Events ??= new OpenIdConnectEvents();
            options.Events.OnTokenValidated = ctx =>
            {
                if (ctx.Properties != null)
                {
                    ctx.Properties.IsPersistent = true; // make auth cookie persistent
                }

                // ctx.Properties.AllowRefresh = false; // optional: no sliding
                return Task.CompletedTask;
            };
        });
        
        return authBuilder;
    }
}