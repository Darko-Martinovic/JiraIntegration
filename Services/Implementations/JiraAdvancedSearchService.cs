using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Interfaces;
using System.Text;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Implementation of Jira advanced search service
/// </summary>
public class JiraAdvancedSearchService : BaseJiraHttpService, IJiraAdvancedSearchService
{
    private readonly List<SavedSearch> _savedSearches = new();

    public JiraAdvancedSearchService(
        HttpClient httpClient,
        IOptions<JiraSettings> settings,
        ILogger<JiraAdvancedSearchService> logger)
        : base(httpClient, settings, logger)
    {
    }

    /// <summary>
    /// Builds JQL query using visual parameters
    /// </summary>
    public string BuildJqlQuery(JqlBuilderRequest request)
    {
        var jqlParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.ProjectKey))
            jqlParts.Add($"project = \"{request.ProjectKey}\"");

        if (!string.IsNullOrWhiteSpace(request.AssigneeId))
        {
            if (request.AssigneeId.Equals("currentUser", StringComparison.OrdinalIgnoreCase))
                jqlParts.Add("assignee = currentUser()");
            else if (request.AssigneeId.Equals("unassigned", StringComparison.OrdinalIgnoreCase))
                jqlParts.Add("assignee is EMPTY");
            else
                jqlParts.Add($"assignee = \"{request.AssigneeId}\"");
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            jqlParts.Add($"status = \"{request.Status}\"");

        if (!string.IsNullOrWhiteSpace(request.Priority))
            jqlParts.Add($"priority = \"{request.Priority}\"");

        if (!string.IsNullOrWhiteSpace(request.IssueType))
            jqlParts.Add($"issuetype = \"{request.IssueType}\"");

        if (request.CreatedAfter.HasValue)
            jqlParts.Add($"created >= \"{request.CreatedAfter.Value:yyyy-MM-dd}\"");

        if (request.CreatedBefore.HasValue)
            jqlParts.Add($"created <= \"{request.CreatedBefore.Value:yyyy-MM-dd}\"");

        if (request.UpdatedAfter.HasValue)
            jqlParts.Add($"updated >= \"{request.UpdatedAfter.Value:yyyy-MM-dd}\"");

        if (request.UpdatedBefore.HasValue)
            jqlParts.Add($"updated <= \"{request.UpdatedBefore.Value:yyyy-MM-dd}\"");

        if (request.Labels.Any())
            jqlParts.Add($"labels in ({string.Join(",", request.Labels.Select(l => $"\"{l}\""))})");

        if (!string.IsNullOrWhiteSpace(request.TextSearch))
            jqlParts.Add($"text ~ \"{request.TextSearch}\"");

        return string.Join(" AND ", jqlParts);
    }

    /// <summary>
    /// Executes advanced search with filters
    /// </summary>
    public async Task<JiraSearchResult> AdvancedSearchAsync(AdvancedSearchRequest request)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            _logger.LogDebug("Executing advanced search with JQL: {JQL}", request.JqlQuery);

            var queryParams = new List<string>
            {
                $"jql={Uri.EscapeDataString(request.JqlQuery)}",
                $"maxResults={request.MaxResults}",
                $"startAt={request.StartAt}"
            };

            if (request.Fields.Any())
                queryParams.Add($"fields={string.Join(",", request.Fields)}");

            var queryString = string.Join("&", queryParams);
            var response = await GetAsync<JiraSearchResponse>($"/rest/api/3/search?{queryString}");

            var searchTime = DateTime.UtcNow - startTime;

            if (response != null)
            {
                var result = new JiraSearchResult
                {
                    Issues = response.Issues,
                    Total = response.Total,
                    StartAt = response.StartAt,
                    MaxResults = response.MaxResults,
                    JqlQuery = request.JqlQuery,
                    SearchTime = searchTime
                };

                _logger.LogInformation("Advanced search completed. Found {Total} issues in {SearchTime}ms",
                    result.Total, searchTime.TotalMilliseconds);
                return result;
            }

            return new JiraSearchResult { JqlQuery = request.JqlQuery, SearchTime = searchTime };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing advanced search with JQL: {JQL}", request.JqlQuery);
            throw;
        }
    }

    /// <summary>
    /// Saves a search query for reuse
    /// </summary>
    public Task<SavedSearch> SaveSearchAsync(SaveSearchRequest request)
    {
        try
        {
            _logger.LogDebug("Saving search: {Name}", request.Name);

            var savedSearch = new SavedSearch
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                JqlQuery = request.JqlQuery,
                Created = DateTime.UtcNow,
                IsShared = request.IsShared
            };

            _savedSearches.Add(savedSearch);

            _logger.LogInformation("Successfully saved search: {Name} with ID: {Id}", request.Name, savedSearch.Id);
            return Task.FromResult(savedSearch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving search: {Name}", request.Name);
            throw;
        }
    }

    /// <summary>
    /// Gets all saved searches for the user
    /// </summary>
    public async Task<List<SavedSearch>> GetSavedSearchesAsync()
    {
        try
        {
            _logger.LogDebug("Getting saved searches");
            await Task.Delay(1); // Simulate async operation
            return _savedSearches.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting saved searches");
            throw;
        }
    }

    /// <summary>
    /// Executes a saved search
    /// </summary>
    public async Task<JiraSearchResult> ExecuteSavedSearchAsync(string searchId)
    {
        try
        {
            _logger.LogDebug("Executing saved search: {SearchId}", searchId);

            var savedSearch = _savedSearches.FirstOrDefault(s => s.Id == searchId);
            if (savedSearch == null)
            {
                throw new ArgumentException($"Saved search with ID {searchId} not found");
            }

            var searchRequest = new AdvancedSearchRequest
            {
                JqlQuery = savedSearch.JqlQuery,
                MaxResults = 50
            };

            return await AdvancedSearchAsync(searchRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing saved search: {SearchId}", searchId);
            throw;
        }
    }

    /// <summary>
    /// Gets predefined smart filters
    /// </summary>
    public List<SmartFilter> GetSmartFilters()
    {
        return new List<SmartFilter>
        {
            new() { Name = "My Open Tickets", Description = "All open tickets assigned to me", JqlQuery = "assignee = currentUser() AND status != Done", Category = "Personal" },
            new() { Name = "My In Progress", Description = "My tickets currently in progress", JqlQuery = "assignee = currentUser() AND status = \"In Progress\"", Category = "Personal" },
            new() { Name = "Overdue Items", Description = "All overdue tickets", JqlQuery = "duedate < now() AND status != Done", Category = "Priority" },
            new() { Name = "High Priority Open", Description = "High priority open tickets", JqlQuery = "priority = High AND status != Done", Category = "Priority" },
            new() { Name = "Recently Created", Description = "Tickets created in the last 7 days", JqlQuery = "created >= -7d", Category = "Recent" },
            new() { Name = "Recently Updated", Description = "Tickets updated in the last 24 hours", JqlQuery = "updated >= -1d", Category = "Recent" },
            new() { Name = "Unassigned Tickets", Description = "All unassigned open tickets", JqlQuery = "assignee is EMPTY AND status != Done", Category = "Management" },
            new() { Name = "This Week's Work", Description = "Tickets due this week", JqlQuery = "duedate >= startOfWeek() AND duedate <= endOfWeek()", Category = "Planning" }
        };
    }
}
