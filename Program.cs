using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JiraIntegration.Application;
using JiraIntegration.Configuration;
using JiraIntegration.Models;
using JiraIntegration.Services.Interfaces;
using JiraIntegration.Services.Implementations;

namespace JiraIntegration;

/// <summary>
/// Main program entry point for Jira Integration Console Application
/// </summary>
class Program
{
    /// <summary>
    /// Application entry point
    /// </summary>
    static async Task Main(string[] args)
    {
        try
        {
            // Load environment variables from .env file
            EnvironmentLoader.LoadFromFile();

            // Validate environment configuration
            if (!EnvironmentLoader.ValidateEnvironment())
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
                return;
            }

            // Build configuration from multiple sources
            var configuration = BuildConfiguration();

            // Setup dependency injection container
            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            // Build service provider and run application
            using var serviceProvider = services.BuildServiceProvider();
            var app = serviceProvider.GetRequiredService<ConsoleApplication>();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Builds configuration from appsettings.json and environment variables
    /// </summary>
    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>
    /// Configures all services for dependency injection
    /// </summary>
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Configure HTTP client with proper timeout and connection management
        services.AddHttpClient<IJiraAuthService, JiraAuthService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IJiraTicketService, JiraTicketService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IJiraSearchService, JiraSearchService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IJiraProjectService, JiraProjectService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IJiraFieldUpdateService, JiraFieldUpdateService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IJiraCommentService, JiraCommentService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IJiraAdvancedSearchService, JiraAdvancedSearchService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IJiraReportingService, JiraReportingService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IJiraUserService, JiraUserService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Configure Jira settings from configuration and environment variables
        services.Configure<JiraSettings>(options =>
        {
            // First try to get from configuration file
            configuration.GetSection(JiraSettings.SectionName).Bind(options);

            // Override with environment variables if they exist
            var baseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL");
            var email = Environment.GetEnvironmentVariable("JIRA_USER_EMAIL");
            var apiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN");
            var projectKey = Environment.GetEnvironmentVariable("JIRA_PROJECT_KEY");

            if (!string.IsNullOrWhiteSpace(baseUrl))
                options.BaseUrl = baseUrl;
            if (!string.IsNullOrWhiteSpace(email))
                options.Email = email;
            if (!string.IsNullOrWhiteSpace(apiToken))
                options.ApiToken = apiToken;
            if (!string.IsNullOrWhiteSpace(projectKey))
                options.ProjectKey = projectKey;
        });

        // Register application services
        services.AddTransient<IJiraAuthService, JiraAuthService>();
        services.AddTransient<IJiraTicketService, JiraTicketService>();
        services.AddTransient<IJiraSearchService, JiraSearchService>();
        services.AddTransient<IJiraProjectService, JiraProjectService>();
        services.AddTransient<IJiraFieldUpdateService, JiraFieldUpdateService>();
        services.AddTransient<IJiraCommentService, JiraCommentService>();
        services.AddTransient<IJiraAdvancedSearchService, JiraAdvancedSearchService>();
        services.AddTransient<IJiraReportingService, JiraReportingService>();
        services.AddTransient<IJiraUserService, JiraUserService>();

        // Register main application
        services.AddTransient<ConsoleApplication>();
    }
}
