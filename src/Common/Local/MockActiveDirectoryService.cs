using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Common.Local;

public static class MockActiveDirectoryService
{
    public static void CreateMockAzureAdClient(this WebApplicationBuilder builder)
    {
        // Mock issuer settings
        const string mockIssuer = "https://mock.local.ad";
        const string mockAudience = "mock-api";
        const string mockSigningKey = "super-secret-dev-signing-key-change-me"; // dev only

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mockSigningKey));

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = mockIssuer,
                    ValidateAudience = true,
                    ValidAudience = mockAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(10)
                };
            });

        // Expose a simple in-app token factory for local testing
        builder.Services.AddSingleton<Func<string, string, IEnumerable<Claim>?, string>>(_ => (subject, name, extraClaims) =>
        {
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: mockIssuer,
                audience: mockAudience,
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, subject),
                    new Claim(ClaimTypes.Name, name ?? subject)
                }.Concat(extraClaims ?? Enumerable.Empty<Claim>()),
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        });
    }
}