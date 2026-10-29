using System.Net;
using System.Net.Http.Json;
using JiraIntegration.Models.Api;

namespace JiraIntegration.Tests;

public class ConnectionApiTests
{
    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Jira_and_confluence_are_checked_separately()
    {
        using var app = new AppFactory();
        app.Atlassian
            .On(HttpMethod.Get, "/rest/api/3/myself",
                """{"accountId":"acc-1","displayName":"Dev","emailAddress":"dev@example.com","active":true}""")
            .On(HttpMethod.Get, "/wiki/api/v2/spaces",
                """{"errors":[{"status":403,"code":"FORBIDDEN","title":"No Confluence on this site"}]}""",
                HttpStatusCode.Forbidden);

        var status = await app.CreateClient().GetFromJsonAsync<ConnectionStatus>("/api/connection", Cancel);

        Assert.True(status!.JiraConnected);
        Assert.Equal("Dev", status.User!.DisplayName);
        Assert.False(status.ConfluenceConnected);
        Assert.Equal("403: No Confluence on this site", status.ConfluenceError);
        Assert.Equal(new SiteSettings("https://example.atlassian.net", "dev@example.com", "KAN", true), status.Settings);
        Assert.Empty(status.MissingSettings);
    }

    [Fact]
    public async Task Missing_settings_are_named_and_nothing_is_called()
    {
        using var app = new AppFactory(configured: false);

        var status = await app.CreateClient().GetFromJsonAsync<ConnectionStatus>("/api/connection", Cancel);

        Assert.Equal(["JIRA_BASE_URL", "JIRA_USER_EMAIL", "JIRA_API_TOKEN"], status!.MissingSettings);
        Assert.False(status.JiraConnected);
        Assert.False(status.Settings.HasApiToken);
        Assert.Empty(app.Atlassian.Requests);
    }

    [Theory]
    [InlineData("/api/connection")]
    [InlineData("/api/connection/settings")]
    public async Task The_token_is_never_sent_to_the_browser(string url)
    {
        using var app = new AppFactory();

        var json = await app.CreateClient().GetStringAsync(url, Cancel);

        Assert.DoesNotContain(AppFactory.Token, json);
    }

    [Fact]
    public async Task The_settings_are_read_without_calling_atlassian()
    {
        using var app = new AppFactory();

        var settings = await app.CreateClient().GetFromJsonAsync<SiteSettings>("/api/connection/settings", Cancel);

        Assert.Equal("KAN", settings!.ProjectKey);
        Assert.Empty(app.Atlassian.Requests);
    }
}
