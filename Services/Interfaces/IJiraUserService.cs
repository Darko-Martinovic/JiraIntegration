using JiraIntegration.Models.Dto;

namespace JiraIntegration.Services.Interfaces;

/// <summary>
/// Interface for user management operations in Jira
/// </summary>
public interface IJiraUserService
{
    /// <summary>
    /// Searches for users by email, display name, or username
    /// </summary>
    /// <param name="query">Search query (email, display name, or username)</param>
    /// <param name="maxResults">Maximum number of results to return (default: 50)</param>
    /// <returns>List of matching users</returns>
    Task<List<JiraUser>> SearchUsersAsync(string query, int maxResults = 50);

    /// <summary>
    /// Gets user details by account ID
    /// </summary>
    /// <param name="accountId">User's account ID</param>
    /// <returns>User details if found</returns>
    Task<JiraUser?> GetUserByAccountIdAsync(string accountId);

    /// <summary>
    /// Gets assignable users for a specific project
    /// </summary>
    /// <param name="projectKey">Project key (e.g., "PROJ")</param>
    /// <param name="maxResults">Maximum number of results to return (default: 50)</param>
    /// <returns>List of assignable users for the project</returns>
    Task<List<JiraUser>> GetAssignableUsersAsync(string projectKey, int maxResults = 50);

    /// <summary>
    /// Gets assignable users for a specific issue
    /// </summary>
    /// <param name="issueKey">Issue key (e.g., "PROJ-123")</param>
    /// <param name="maxResults">Maximum number of results to return (default: 50)</param>
    /// <returns>List of assignable users for the issue</returns>
    Task<List<JiraUser>> GetAssignableUsersForIssueAsync(string issueKey, int maxResults = 50);
}
