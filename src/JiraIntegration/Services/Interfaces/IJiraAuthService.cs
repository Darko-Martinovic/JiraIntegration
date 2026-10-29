using JiraIntegration.Models.Jira;

namespace JiraIntegration.Services.Interfaces;

public interface IJiraAuthService
{
    /// <summary>The account the API token belongs to; a good first call to test the credentials</summary>
    Task<JiraUser> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
