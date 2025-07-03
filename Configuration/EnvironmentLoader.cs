using Microsoft.Extensions.Configuration;

namespace JiraIntegration.Configuration;

/// <summary>
/// Helper class to load environment variables from .env file
/// </summary>
public static class EnvironmentLoader
{
    /// <summary>
    /// Loads environment variables from a .env file
    /// </summary>
    /// <param name="filePath">Path to the .env file</param>
    public static void LoadFromFile(string filePath = ".env")
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"⚠️  Warning: .env file not found at {filePath}");
            return;
        }

        try
        {
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                // Parse key=value pairs
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    // Remove quotes if present
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    Environment.SetEnvironmentVariable(key, value);
                }
            }

            Console.WriteLine("✅ Successfully loaded environment variables from .env file");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading .env file: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates that required environment variables are set
    /// </summary>
    /// <returns>True if all required variables are present</returns>
    public static bool ValidateEnvironment()
    {
        var requiredVars = new[]
        {
            "JIRA_BASE_URL",
            "JIRA_USER_EMAIL",
            "JIRA_API_TOKEN",
            "JIRA_PROJECT_KEY"
        };

        var missing = new List<string>();

        foreach (var varName in requiredVars)
        {
            var value = Environment.GetEnvironmentVariable(varName);
            if (string.IsNullOrWhiteSpace(value))
            {
                missing.Add(varName);
            }
        }

        if (missing.Any())
        {
            Console.WriteLine("❌ Missing required environment variables:");
            foreach (var varName in missing)
            {
                Console.WriteLine($"   - {varName}");
            }
            Console.WriteLine("\nPlease ensure these variables are set in your .env file or system environment.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Displays current environment configuration (without sensitive data)
    /// </summary>
    public static void DisplayConfiguration()
    {
        Console.WriteLine("📋 Current Configuration:");
        Console.WriteLine($"   JIRA_BASE_URL: {Environment.GetEnvironmentVariable("JIRA_BASE_URL")}");
        Console.WriteLine($"   JIRA_USER_EMAIL: {Environment.GetEnvironmentVariable("JIRA_USER_EMAIL")}");
        Console.WriteLine($"   JIRA_PROJECT_KEY: {Environment.GetEnvironmentVariable("JIRA_PROJECT_KEY")}");

        var apiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN");
        if (!string.IsNullOrWhiteSpace(apiToken))
        {
            var maskedToken = apiToken.Length > 8
                ? apiToken.Substring(0, 4) + "****" + apiToken.Substring(apiToken.Length - 4)
                : "****";
            Console.WriteLine($"   JIRA_API_TOKEN: {maskedToken}");
        }
        else
        {
            Console.WriteLine("   JIRA_API_TOKEN: Not set");
        }
    }
}
