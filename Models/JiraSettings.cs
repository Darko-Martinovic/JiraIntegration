namespace JiraIntegration.Models;

/// <summary>
/// Configuration settings for Jira API connection
/// </summary>
public class JiraSettings
{
    public const string SectionName = "JiraSettings";

    /// <summary>
    /// Base URL of the Jira instance (e.g., https://your-domain.atlassian.net)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Email address associated with the Jira account
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// API token for authentication
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// Default project key to use
    /// </summary>
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of results to return in searches
    /// </summary>
    public int MaxResults { get; set; } = 50;

    /// <summary>
    /// Timeout in seconds for HTTP requests
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Validates that all required settings are present
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(BaseUrl) &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(ApiToken) &&
               !string.IsNullOrWhiteSpace(ProjectKey);
    }
}
