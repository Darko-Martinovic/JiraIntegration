using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Interfaces;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Service for Confluence API operations
/// </summary>
public class ConfluenceService : BaseJiraHttpService, IConfluenceService
{
    public ConfluenceService(
        HttpClient httpClient,
        IOptions<JiraSettings> jiraSettings,
        ILogger<ConfluenceService> logger
    )
        : base(httpClient, jiraSettings, logger) { }

    /// <summary>
    /// Gets the Confluence base URL from JIRA base URL
    /// </summary>
    private string GetConfluenceBaseUrl()
    {
        var jiraBaseUrl = _settings.BaseUrl;
        // Convert from https://domain.atlassian.net to https://domain.atlassian.net/wiki
        return jiraBaseUrl.TrimEnd('/') + "/wiki";
    }

    /// <summary>
    /// Tests the connection to Confluence API
    /// </summary>
    public new async Task<bool> TestConnectionAsync()
    {
        try
        {
            _logger.LogDebug("Testing Confluence connection");

            var confluenceBaseUrl = GetConfluenceBaseUrl();
            var response = await _httpClient.GetAsync(
                $"{confluenceBaseUrl}/rest/api/space?limit=1"
            );

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Confluence connection test successful");
                return true;
            }

            _logger.LogWarning(
                "Confluence connection test failed with status: {StatusCode}",
                response.StatusCode
            );
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Confluence connection");
            return false;
        }
    }

    /// <summary>
    /// Gets all spaces the user has access to
    /// </summary>
    public async Task<List<ConfluenceSpace>> GetSpacesAsync()
    {
        try
        {
            _logger.LogDebug("Fetching Confluence spaces");

            var confluenceBaseUrl = GetConfluenceBaseUrl();
            var spaceResponse = await GetAsync<ConfluenceSpaceListResponse>(
                $"{confluenceBaseUrl}/rest/api/space?limit=50&expand=description.plain,homepage"
            );

            if (spaceResponse != null)
            {
                _logger.LogInformation(
                    "Retrieved {Count} Confluence spaces",
                    spaceResponse.Results.Count
                );
                return spaceResponse.Results;
            }

            _logger.LogWarning("Failed to get spaces - received null response");
            return new List<ConfluenceSpace>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Confluence spaces");
            return new List<ConfluenceSpace>();
        }
    }

    /// <summary>
    /// Searches for pages across all spaces
    /// </summary>
    public async Task<ConfluenceSearchResults> SearchPagesAsync(
        string query,
        string? spaceKey = null,
        int limit = 25
    )
    {
        try
        {
            _logger.LogDebug("Searching Confluence pages with query: {Query}", query);

            var confluenceBaseUrl = GetConfluenceBaseUrl();
            var searchQuery = Uri.EscapeDataString(query);
            var url = $"{confluenceBaseUrl}/rest/api/content/search?cql=text~\"{searchQuery}\"";

            if (!string.IsNullOrEmpty(spaceKey))
            {
                url += $"+and+space=\"{Uri.EscapeDataString(spaceKey)}\"";
            }

            url += $"&limit={limit}&expand=space,body.view,version";

            var searchResponse = await GetAsync<ConfluenceSearchResults>(url);

            if (searchResponse != null)
            {
                _logger.LogInformation(
                    "Found {Count} pages matching query",
                    searchResponse.Results.Count
                );
                return searchResponse;
            }

            _logger.LogWarning("Failed to search pages - received null response");
            return new ConfluenceSearchResults();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Confluence pages");
            return new ConfluenceSearchResults();
        }
    }

    /// <summary>
    /// Gets a specific page by ID
    /// </summary>
    public async Task<ConfluencePage?> GetPageAsync(string pageId)
    {
        try
        {
            _logger.LogDebug("Fetching Confluence page: {PageId}", pageId);

            var confluenceBaseUrl = GetConfluenceBaseUrl();
            var page = await GetAsync<ConfluencePage>(
                $"{confluenceBaseUrl}/rest/api/content/{pageId}?expand=space,body.storage,body.view,version,metadata"
            );

            if (page != null)
            {
                _logger.LogDebug("Retrieved page: {Title}", page.Title);
                return page;
            }

            _logger.LogWarning("Failed to get page {PageId} - received null response", pageId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Confluence page {PageId}", pageId);
            return null;
        }
    }

    /// <summary>
    /// Creates a new page in a space
    /// </summary>
    public async Task<ConfluencePage?> CreatePageAsync(
        string spaceKey,
        string title,
        string content,
        string? parentId = null
    )
    {
        try
        {
            _logger.LogInformation(
                "Creating new page in space {SpaceKey}: {Title}",
                spaceKey,
                title
            );

            var createRequest = new
            {
                type = "page",
                title = title,
                space = new { key = spaceKey },
                body = new { storage = new { value = content, representation = "storage" } },
                ancestors = parentId != null ? new[] { new { id = parentId } } : null
            };

            var confluenceBaseUrl = GetConfluenceBaseUrl();
            var page = await PostAsync<ConfluencePage>(
                $"{confluenceBaseUrl}/rest/api/content",
                createRequest
            );

            if (page != null)
            {
                _logger.LogInformation("Created page: {Title} with ID: {PageId}", title, page.Id);
                return page;
            }

            _logger.LogWarning("Failed to create page - received null response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Confluence page");
            return null;
        }
    }

    /// <summary>
    /// Gets pages in a specific space
    /// </summary>
    public async Task<List<ConfluencePage>> GetPagesInSpaceAsync(string spaceKey, int limit = 25)
    {
        try
        {
            _logger.LogDebug("Fetching pages in space: {SpaceKey}", spaceKey);

            var confluenceBaseUrl = GetConfluenceBaseUrl();
            var searchResults = await GetAsync<ConfluenceSearchResults>(
                $"{confluenceBaseUrl}/rest/api/content?spaceKey={spaceKey}&limit={limit}&expand=space,version"
            );

            if (searchResults?.Results != null)
            {
                var pages = searchResults.Results
                    .Where(r => r.Content != null)
                    .Select(r => r.Content!)
                    .ToList();

                _logger.LogInformation(
                    "Retrieved {Count} pages from space {SpaceKey}",
                    pages.Count,
                    spaceKey
                );
                return pages;
            }

            _logger.LogWarning(
                "Failed to get pages from space {SpaceKey} - received null response",
                spaceKey
            );
            return new List<ConfluencePage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pages from space {SpaceKey}", spaceKey);
            return new List<ConfluencePage>();
        }
    }
}
