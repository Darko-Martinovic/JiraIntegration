using JiraIntegration.Models.Jira;

namespace JiraIntegration.Services.Interfaces;

public interface IJiraSearchService
{
    /// <summary>
    /// One page of the issues a JQL query finds; pass the page's NextPageToken back for the next one
    /// </summary>
    Task<JiraSearchPage> SearchAsync(string jql, int maxResults, string? nextPageToken, CancellationToken cancellationToken = default);
}
