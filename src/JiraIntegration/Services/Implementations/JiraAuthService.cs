using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using JiraIntegration.Models.Jira;
using JiraIntegration.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Services.Implementations;

public sealed class JiraAuthService(HttpClient http, IOptions<AtlassianSettings> settings)
    : AtlassianHttpService(http, settings), IJiraAuthService
{
    public Task<JiraUser> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        GetJsonAsync<JiraUser>("rest/api/3/myself", cancellationToken);
}
