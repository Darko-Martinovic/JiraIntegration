using System.Net.Http.Headers;
using System.Text;
using JiraIntegration.Services.Implementations;
using JiraIntegration.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Configuration;

public static class AtlassianServices
{
    /// <summary>
    /// Reads the site's settings from the JIRA_* values in .env or the environment
    /// </summary>
    public static IServiceCollection AddAtlassianSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AtlassianSettings>().Configure(settings =>
        {
            settings.BaseUrl = configuration["JIRA_BASE_URL"] ?? "";
            settings.Email = configuration["JIRA_USER_EMAIL"] ?? "";
            settings.ApiToken = configuration["JIRA_API_TOKEN"] ?? "";
            settings.ProjectKey = configuration["JIRA_PROJECT_KEY"] ?? "";
            if (int.TryParse(configuration["JIRA_TIMEOUT_SECONDS"], out var seconds) && seconds > 0)
            {
                settings.TimeoutSeconds = seconds;
            }
        });
        return services;
    }

    /// <summary>
    /// Registers each service as a typed HttpClient on the site, signed in with basic
    /// authentication: the account's email and its API token
    /// </summary>
    public static IServiceCollection AddAtlassianClients(this IServiceCollection services)
    {
        services.AddHttpClient<IJiraAuthService, JiraAuthService>(ConfigureSite);
        services.AddHttpClient<IJiraTicketService, JiraTicketService>(ConfigureSite);
        services.AddHttpClient<IJiraSearchService, JiraSearchService>(ConfigureSite);
        services.AddHttpClient<IJiraCommentService, JiraCommentService>(ConfigureSite);
        services.AddHttpClient<IJiraProjectService, JiraProjectService>(ConfigureSite);
        services.AddHttpClient<IJiraUserService, JiraUserService>(ConfigureSite);
        services.AddHttpClient<IConfluenceService, ConfluenceService>(ConfigureSite);
        return services;
    }

    private static void ConfigureSite(IServiceProvider provider, HttpClient client)
    {
        var settings = provider.GetRequiredService<IOptions<AtlassianSettings>>().Value;
        if (!settings.IsConfigured)
        {
            // The services refuse to call out until .env is filled in
            return;
        }

        client.BaseAddress = new Uri(settings.Site + "/");
        client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.Email}:{settings.ApiToken}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
