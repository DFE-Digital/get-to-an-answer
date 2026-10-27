using System.Runtime.CompilerServices;

namespace Integration.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Docker environment configuration for Testcontainers
        // This module initializer runs before any static constructors

        var dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");

        // Check if DOCKER_HOST is set to an invalid/malformed value
        // The npipe:////pipe/dockerDesktopLinuxEngine (with 4 slashes) is invalid
        if (!string.IsNullOrEmpty(dockerHost))
        {
            // If it's the malformed dockerDesktopLinuxEngine URI, clear it
            if (dockerHost.Contains("dockerDesktopLinuxEngine") || dockerHost.Contains("////pipe"))
            {
                // Clear the invalid DOCKER_HOST to let Testcontainers auto-detect
                Environment.SetEnvironmentVariable("DOCKER_HOST", "");
            }
            // If it's already a valid 2-slash format, leave it alone
            else if (dockerHost.StartsWith("npipe://./pipe/") || dockerHost.StartsWith("unix://"))
            {
                // Valid format, keep it
            }
        }

        // If DOCKER_HOST was cleared or not set, Testcontainers will auto-detect 
        // Windows Docker Desktop using its own detection logic
    }
}
