using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using JiraIntegration.Models.Jira;
using JiraIntegration.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Services.Implementations;

public sealed class JiraSearchService(HttpClient http, IOptions<AtlassianSettings> settings)
    : AtlassianHttpService(http, settings), IJiraSearchService
{
    private const string Fields = "summary,status,priority,issuetype,assignee,created,updated";

    public Task<JiraSearchPage> SearchAsync(string jql, int maxResults, string? nextPageToken, CancellationToken cancellationToken = default)
    {
        // The enhanced search: GET /rest/api/3/search was retired in 2025 (CHANGE-2046).
        // It returns no fields unless asked, and pages by token rather than by startAt.
        var path = $"rest/api/3/search/jql?jql={Escape(jql)}&maxResults={maxResults}&fields={Fields}";
        if (!string.IsNullOrEmpty(nextPageToken))
        {
            path += $"&nextPageToken={Escape(nextPageToken)}";
        }
        return GetJsonAsync<JiraSearchPage>(path, cancellationToken);
    }
}
