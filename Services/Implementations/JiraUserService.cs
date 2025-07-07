using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Interfaces;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Service for user management operations in Jira
/// </summary>
public class JiraUserService : BaseJiraHttpService, IJiraUserService
{
    public JiraUserService(
        HttpClient httpClient,
        IOptions<JiraSettings> settings,
        ILogger<JiraUserService> logger
    )
        : base(httpClient, settings, logger) { }

    /// <summary>
    /// Searches for users by email, display name, or username
    /// </summary>
    public async Task<List<JiraUser>> SearchUsersAsync(string query, int maxResults = 50)
    {
        try
        {
            _logger.LogDebug("Searching for users with query: {Query}", query);

            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"user/search?query={encodedQuery}&maxResults={maxResults}";

            var response = await GetAsync<List<JiraUser>>(url);

            _logger.LogInformation(
                "Found {Count} users matching query: {Query}",
                response?.Count ?? 0,
                query
            );
            return response ?? new List<JiraUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for users with query: {Query}", query);
            throw;
        }
    }

    /// <summary>
    /// Gets user details by account ID
    /// </summary>
    public async Task<JiraUser?> GetUserByAccountIdAsync(string accountId)
    {
        try
        {
            _logger.LogDebug("Getting user details for account ID: {AccountId}", accountId);

            var url = $"user?accountId={Uri.EscapeDataString(accountId)}";
            var response = await GetAsync<JiraUser>(url);

            _logger.LogInformation("Retrieved user details for account ID: {AccountId}", accountId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting user details for account ID: {AccountId}",
                accountId
            );
            throw;
        }
    }

    /// <summary>
    /// Gets assignable users for a specific project
    /// </summary>
    public async Task<List<JiraUser>> GetAssignableUsersAsync(
        string projectKey,
        int maxResults = 50
    )
    {
        try
        {
            _logger.LogInformation(
                "Getting assignable users for project: {ProjectKey}",
                projectKey
            );

            var url =
                $"user/assignable/search?project={Uri.EscapeDataString(projectKey)}&maxResults={maxResults}";
            var response = await GetAsync<List<JiraUser>>(url);

            _logger.LogInformation(
                "Found {Count} assignable users for project: {ProjectKey}",
                response?.Count ?? 0,
                projectKey
            );
            return response ?? new List<JiraUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting assignable users for project: {ProjectKey}",
                projectKey
            );
            throw;
        }
    }

    /// <summary>
    /// Gets assignable users for a specific issue
    /// </summary>
    public async Task<List<JiraUser>> GetAssignableUsersForIssueAsync(
        string issueKey,
        int maxResults = 50
    )
    {
        try
        {
            _logger.LogDebug("Getting assignable users for issue: {IssueKey}", issueKey);

            var url =
                $"user/assignable/search?issueKey={Uri.EscapeDataString(issueKey)}&maxResults={maxResults}";
            var response = await GetAsync<List<JiraUser>>(url);

            _logger.LogInformation(
                "Found {Count} assignable users for issue: {IssueKey}",
                response?.Count ?? 0,
                issueKey
            );
            return response ?? new List<JiraUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignable users for issue: {IssueKey}", issueKey);
            throw;
        }
    }
}
