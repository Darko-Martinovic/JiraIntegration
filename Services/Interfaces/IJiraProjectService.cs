using JiraIntegration.Models.Dto;

namespace JiraIntegration.Services.Interfaces;

/// <summary>
/// Interface for Jira project and metadata operations
/// </summary>
public interface IJiraProjectService
{
    /// <summary>
    /// Gets all projects accessible to the current user
    /// </summary>
    /// <returns>List of accessible projects</returns>
    Task<List<JiraProject>> GetAccessibleProjectsAsync();

    /// <summary>
    /// Gets issue types for a specific project
    /// </summary>
    /// <param name="projectKey">Project key</param>
    /// <returns>List of issue types</returns>
    Task<List<JiraIssueType>> GetIssueTypesAsync(string projectKey);

    /// <summary>
    /// Gets priorities available in the Jira instance
    /// </summary>
    /// <returns>List of priorities</returns>
    Task<List<JiraPriority>> GetPrioritiesAsync();

    /// <summary>
    /// Gets project information by key
    /// </summary>
    /// <param name="projectKey">Project key</param>
    /// <returns>Project information or null if not found</returns>
    Task<JiraProject?> GetProjectAsync(string projectKey);
}
