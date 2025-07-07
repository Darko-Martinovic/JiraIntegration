using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Interfaces;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Implementation of Jira project service
/// </summary>
public class JiraProjectService : BaseJiraHttpService, IJiraProjectService
{
    public JiraProjectService(
        HttpClient httpClient,
        IOptions<JiraSettings> settings,
        ILogger<JiraProjectService> logger)
        : base(httpClient, settings, logger)
    {
    }

    /// <summary>
    /// Gets all projects accessible to the current user
    /// </summary>
    public async Task<List<JiraProject>> GetAccessibleProjectsAsync()
    {
        try
        {
            _logger.LogDebug("Getting accessible projects");

            var projects = await GetAsync<List<JiraProject>>("/rest/api/3/project");

            if (projects != null)
            {
                _logger.LogDebug("Found {Count} accessible projects", projects.Count);
                return projects;
            }

            _logger.LogWarning("No projects found or access denied");
            return new List<JiraProject>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accessible projects");
            throw;
        }
    }

    /// <summary>
    /// Gets issue types for a specific project
    /// </summary>
    public async Task<List<JiraIssueType>> GetIssueTypesAsync(string projectKey)
    {
        try
        {
            _logger.LogDebug("Getting issue types for project: {ProjectKey}", projectKey);

            if (string.IsNullOrWhiteSpace(projectKey))
            {
                throw new ArgumentException("Project key cannot be empty", nameof(projectKey));
            }

            var project = await GetProjectAsync(projectKey);
            if (project == null)
            {
                _logger.LogWarning("Project not found: {ProjectKey}", projectKey);
                return new List<JiraIssueType>();
            }

            // Get issue types for the project
            var issueTypes = await GetAsync<List<JiraIssueType>>($"/rest/api/3/project/{projectKey}/statuses");

            if (issueTypes != null)
            {
                _logger.LogDebug("Found {Count} issue types for project {ProjectKey}", issueTypes.Count, projectKey);
                return issueTypes;
            }

            // Fallback to global issue types
            var globalIssueTypes = await GetAsync<List<JiraIssueType>>("/rest/api/3/issuetype");
            return globalIssueTypes ?? new List<JiraIssueType>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting issue types for project: {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <summary>
    /// Gets priorities available in the Jira instance
    /// </summary>
    public async Task<List<JiraPriority>> GetPrioritiesAsync()
    {
        try
        {
            _logger.LogDebug("Getting available priorities");

            var priorities = await GetAsync<List<JiraPriority>>("/rest/api/3/priority");

            if (priorities != null)
            {
                _logger.LogDebug("Found {Count} priorities", priorities.Count);
                return priorities;
            }

            _logger.LogWarning("No priorities found");
            return new List<JiraPriority>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting priorities");
            throw;
        }
    }

    /// <summary>
    /// Gets project information by key
    /// </summary>
    public async Task<JiraProject?> GetProjectAsync(string projectKey)
    {
        try
        {
            _logger.LogDebug("Getting project: {ProjectKey}", projectKey);

            if (string.IsNullOrWhiteSpace(projectKey))
            {
                throw new ArgumentException("Project key cannot be empty", nameof(projectKey));
            }

            var project = await GetAsync<JiraProject>($"/rest/api/3/project/{projectKey}");

            if (project != null)
            {
                _logger.LogDebug("Successfully retrieved project: {Key} - {Name}", project.Key, project.Name);
            }
            else
            {
                _logger.LogWarning("Project not found: {ProjectKey}", projectKey);
            }

            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project: {ProjectKey}", projectKey);
            throw;
        }
    }
}
