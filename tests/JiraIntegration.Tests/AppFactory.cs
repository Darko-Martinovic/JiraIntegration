using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JiraIntegration.Tests;

/// <summary>
/// The app with every Jira and Confluence call going to a <see cref="FakeAtlassian"/>
/// </summary>
public sealed class AppFactory(bool configured = true) : WebApplicationFactory<Program>
{
    public const string Site = "https://example.atlassian.net";
    public const string Email = "dev@example.com";
    public const string Token = "secret-token";

    public FakeAtlassian Atlassian { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JIRA_BASE_URL"] = configured ? Site : "",
            ["JIRA_USER_EMAIL"] = configured ? Email : "",
            ["JIRA_API_TOKEN"] = configured ? Token : "",
            ["JIRA_PROJECT_KEY"] = configured ? "KAN" : "",
        }));
        builder.ConfigureTestServices(services =>
            services.ConfigureHttpClientDefaults(client => client.ConfigurePrimaryHttpMessageHandler(() => Atlassian)));
    }
}
