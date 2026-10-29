using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using JiraIntegration.Models.Jira;
using JiraIntegration.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Services.Implementations;

public sealed class JiraUserService(HttpClient http, IOptions<AtlassianSettings> settings)
    : AtlassianHttpService(http, settings), IJiraUserService
{
    public Task<IReadOnlyList<JiraUser>> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default) =>
        GetJsonAsync<IReadOnlyList<JiraUser>>($"rest/api/3/user/search?query={Escape(query)}&maxResults={maxResults}", cancellationToken);

    public Task<IReadOnlyList<JiraUser>> GetAssignableAsync(string projectKey, int maxResults, CancellationToken cancellationToken = default) =>
        GetJsonAsync<IReadOnlyList<JiraUser>>(
            $"rest/api/3/user/assignable/search?project={Escape(projectKey)}&maxResults={maxResults}",
            cancellationToken);
}
