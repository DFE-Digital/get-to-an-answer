namespace Integration.Tests.Util;

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Ensure ContentRoot points to the web project directory so static assets manifest is found
        builder.UseContentRoot(GetProjectPath(typeof(TEntryPoint).Assembly.GetName().Name!));
        return base.CreateHost(builder);
    }

    private static string GetProjectPath(string projectName)
    {
        // Traverse up from test bin folder to find the web project folder by name
        var dir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(dir))
        {
            var candidate = Path.Combine(dir, projectName);
            if (Directory.Exists(candidate))
                return candidate;

            dir = Directory.GetParent(dir)?.FullName!;
        }

        // Fallback: current directory
        return Directory.GetCurrentDirectory();
    }
}