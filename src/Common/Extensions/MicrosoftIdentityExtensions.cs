using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

namespace Common.Extensions;

public static class MicrosoftIdentityExtensions
{
    public static MicrosoftIdentityWebApiAuthenticationBuilder AddMicrosoftIdentityWebApi(
        this AuthenticationBuilder builder, IConfigurationSection configurationSection)
    {
        return builder.AddMicrosoftIdentityWebApi(options =>
            {
                var clientId = configurationSection.GetValue<string>("ClientId")!;
            
                options.TokenValidationParameters.ValidAudiences = new List<string>
                {
                    clientId, $"api://{clientId}"
                };
            }, 
            configurationSection.Bind);
    }
    
    public static MicrosoftIdentityWebAppAuthenticationBuilder AddMicrosoftIdentityWebApp(
        this AuthenticationBuilder builder, IConfigurationSection configurationSection)
    {
        var authBuilder = builder.AddMicrosoftIdentityWebApp(options =>
        {
            configurationSection.Bind(options);

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
        });;
        
        return authBuilder;
    }
}