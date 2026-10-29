namespace JiraIntegration.Configuration;

/// <summary>
/// One Atlassian Cloud site. Jira and Confluence share the site, the account and the API token.
/// </summary>
public sealed class AtlassianSettings
{
    /// <summary>The site, such as https://your-site.atlassian.net</summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>The account the API token belongs to</summary>
    public string Email { get; set; } = "";

    public string ApiToken { get; set; } = "";

    /// <summary>The project the ticket list opens on</summary>
    public string ProjectKey { get; set; } = "";

    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// The settings that still need a value, by the name they have in .env
    /// </summary>
    public IReadOnlyList<string> MissingSettings()
    {
        var missing = new List<string>();
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var site) || site.Scheme != Uri.UriSchemeHttps)
        {
            missing.Add("JIRA_BASE_URL");
        }
        if (string.IsNullOrWhiteSpace(Email))
        {
            missing.Add("JIRA_USER_EMAIL");
        }
        if (string.IsNullOrWhiteSpace(ApiToken))
        {
            missing.Add("JIRA_API_TOKEN");
        }
        return missing;
    }

    public bool IsConfigured => MissingSettings().Count == 0;

    public string Site => BaseUrl.TrimEnd('/');

    /// <summary>
    /// Where a ticket opens in the browser
    /// </summary>
    public string TicketUrl(string key) => $"{Site}/browse/{key}";

    /// <summary>
    /// Confluence links are relative to the site's /wiki
    /// </summary>
    public string ConfluenceUrl(string? webUi) => string.IsNullOrEmpty(webUi) ? "" : $"{Site}/wiki{webUi}";
}
