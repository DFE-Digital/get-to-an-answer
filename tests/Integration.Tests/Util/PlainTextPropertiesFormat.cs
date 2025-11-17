using Microsoft.AspNetCore.Authentication;

namespace Integration.Tests.Util;

public sealed class PlainTextPropertiesFormat : ISecureDataFormat<AuthenticationProperties>
{
    public string Protect(AuthenticationProperties data)
        => Convert.ToBase64String(
            PropertiesSerializer.Default.Serialize(data));

    public string Protect(AuthenticationProperties data, string? purpose) => Protect(data);

    public AuthenticationProperties? Unprotect(string? protectedText)
    {
        if (protectedText == null)
            return null;

        var plainText = Convert.FromBase64String(protectedText);
            
        return PropertiesSerializer.Default.Deserialize(plainText);
    }

    public AuthenticationProperties? Unprotect(string? protectedText, string? purpose) => Unprotect(protectedText);
}