using JiraIntegration.Models.Jira;

namespace JiraIntegration.Services.Interfaces;

public interface IJiraProjectService
{
    /// <summary>The project with its issue types</summary>
    Task<JiraProject> GetProjectAsync(string projectKey, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JiraPriority>> GetPrioritiesAsync(CancellationToken cancellationToken = default);
}
