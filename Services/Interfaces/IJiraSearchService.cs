using JiraIntegration.Models.Dto;

namespace JiraIntegration.Services.Interfaces;

/// <summary>
/// Interface for Jira search operations
/// </summary>
public interface IJiraSearchService
{
    /// <summary>
    /// Searches for tickets using JQL (Jira Query Language)
    /// </summary>
    /// <param name="jql">JQL query string</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="startAt">Starting index for pagination</param>
    /// <returns>Search results</returns>
    Task<JiraSearchResponse?> SearchTicketsAsync(string jql, int maxResults = 50, int startAt = 0);

    /// <summary>
    /// Gets tickets for a specific project
    /// </summary>
    /// <param name="projectKey">Project key</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <returns>List of tickets in the project</returns>
    Task<List<JiraIssue>> GetProjectTicketsAsync(string projectKey, int maxResults = 50);

    /// <summary>
    /// Gets tickets assigned to current user
    /// </summary>
    /// <param name="maxResults">Maximum number of results</param>
    /// <returns>List of assigned tickets</returns>
    Task<List<JiraIssue>> GetMyTicketsAsync(int maxResults = 50);

    /// <summary>
    /// Gets open tickets for a project
    /// </summary>
    /// <param name="projectKey">Project key</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <returns>List of open tickets</returns>
    Task<List<JiraIssue>> GetOpenTicketsAsync(string projectKey, int maxResults = 50);
}
