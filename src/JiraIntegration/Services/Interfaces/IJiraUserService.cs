using JiraIntegration.Models.Jira;

namespace JiraIntegration.Services.Interfaces;

public interface IJiraUserService
{
    /// <summary>Users whose name or email starts with the query</summary>
    Task<IReadOnlyList<JiraUser>> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default);

    /// <summary>The users a ticket in the project can be assigned to</summary>
    Task<IReadOnlyList<JiraUser>> GetAssignableAsync(string projectKey, int maxResults, CancellationToken cancellationToken = default);
}
