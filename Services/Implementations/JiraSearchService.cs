using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Interfaces;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Implementation of Jira search service
/// </summary>
public class JiraSearchService : BaseJiraHttpService, IJiraSearchService
{
    public JiraSearchService(
        HttpClient httpClient,
        IOptions<JiraSettings> settings,
        ILogger<JiraSearchService> logger
    )
        : base(httpClient, settings, logger) { }

    /// <summary>
    /// Searches for tickets using JQL (Jira Query Language)
    /// </summary>
    public async Task<JiraSearchResponse?> SearchTicketsAsync(
        string jql,
        int maxResults = 50,
        int startAt = 0
    )
    {
        try
        {
            _logger.LogDebug("Searching tickets with JQL: {JQL}", jql);

            if (string.IsNullOrWhiteSpace(jql))
            {
                throw new ArgumentException("JQL query cannot be empty", nameof(jql));
            }

            var encodedJql = Uri.EscapeDataString(jql);
            var endpoint =
                $"/rest/api/3/search?jql={encodedJql}&maxResults={maxResults}&startAt={startAt}";

            var response = await GetAsync<JiraSearchResponse>(endpoint);

            if (response != null)
            {
                _logger.LogDebug(
                    "Search completed. Found {Total} total tickets, returning {Count} tickets",
                    response.Total,
                    response.Issues.Count
                );
            }
            else
            {
                _logger.LogWarning("Search failed for JQL: {JQL}", jql);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tickets with JQL: {JQL}", jql);
            throw;
        }
    }

    /// <summary>
    /// Gets tickets for a specific project
    /// </summary>
    public async Task<List<JiraIssue>> GetProjectTicketsAsync(
        string projectKey,
        int maxResults = 50
    )
    {
        try
        {
            _logger.LogDebug("Getting tickets for project: {ProjectKey}", projectKey);

            if (string.IsNullOrWhiteSpace(projectKey))
            {
                throw new ArgumentException("Project key cannot be empty", nameof(projectKey));
            }

            var jql = $"project = {projectKey} ORDER BY created DESC";
            var response = await SearchTicketsAsync(jql, maxResults);

            return response?.Issues ?? new List<JiraIssue>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tickets for project: {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <summary>
    /// Gets tickets assigned to current user
    /// </summary>
    public async Task<List<JiraIssue>> GetMyTicketsAsync(int maxResults = 50)
    {
        try
        {
            _logger.LogDebug("Getting tickets assigned to current user");

            var jql = "assignee = currentUser() ORDER BY updated DESC";
            var response = await SearchTicketsAsync(jql, maxResults);

            return response?.Issues ?? new List<JiraIssue>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tickets for current user");
            throw;
        }
    }

    /// <summary>
    /// Gets open tickets for a project
    /// </summary>
    public async Task<List<JiraIssue>> GetOpenTicketsAsync(string projectKey, int maxResults = 50)
    {
        try
        {
            _logger.LogDebug("Getting open tickets for project: {ProjectKey}", projectKey);

            if (string.IsNullOrWhiteSpace(projectKey))
            {
                throw new ArgumentException("Project key cannot be empty", nameof(projectKey));
            }

            var jql = $"project = {projectKey} AND statusCategory != Done ORDER BY created DESC";
            var response = await SearchTicketsAsync(jql, maxResults);

            return response?.Issues ?? new List<JiraIssue>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting open tickets for project: {ProjectKey}",
                projectKey
            );
            throw;
        }
    }
}
