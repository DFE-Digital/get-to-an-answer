using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

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
            
                // For development/local environments only - accept unsigned tokens
                if (environment == "Development")
                {
                    options.TokenValidationParameters.RequireSignedTokens = false;
                    options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                    options.TokenValidationParameters.SignatureValidator = (token, parameters) =>
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);

                        var jsonHeader = JsonSerializer.Serialize(jwtToken.Header);
                        var jsonPayload = JsonSerializer.Serialize(jwtToken.Payload);
                        
                        return new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(jsonHeader, jsonPayload);
                    };
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.TokenValidationParameters.ValidateLifetime = true; // Still validate expiration
                    options.TokenValidationParameters.TryAllIssuerSigningKeys = false;
                    options.TokenValidationParameters.IssuerSigningKeys = new List<SecurityKey>();
                    options.TokenValidationParameters.TokenReader = (token, parameters) =>
                    {
                        
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);
                        
                        var jsonHeader = JsonSerializer.Serialize(jwtToken.Header);
                        var jsonPayload = JsonSerializer.Serialize(jwtToken.Payload);
                        
                        return new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(jsonHeader, jsonPayload);
                    };
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