using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using JiraIntegration.Models.Jira;
using JiraIntegration.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Services.Implementations;

public sealed class JiraProjectService(HttpClient http, IOptions<AtlassianSettings> settings)
    : AtlassianHttpService(http, settings), IJiraProjectService
{
    public Task<JiraProject> GetProjectAsync(string projectKey, CancellationToken cancellationToken = default) =>
        GetJsonAsync<JiraProject>($"rest/api/3/project/{Escape(projectKey)}", cancellationToken);

    public async Task<IReadOnlyList<JiraPriority>> GetPrioritiesAsync(CancellationToken cancellationToken = default)
    {
        var page = await GetJsonAsync<JiraValues<JiraPriority>>("rest/api/3/priority/search?maxResults=100", cancellationToken);
        return page.Values;
    }

    private sealed record JiraValues<T>(IReadOnlyList<T> Values);
}
