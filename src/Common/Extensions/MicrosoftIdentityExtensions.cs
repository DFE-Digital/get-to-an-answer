using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
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
        return builder.AddMicrosoftIdentityWebApp(options =>
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
    }
}