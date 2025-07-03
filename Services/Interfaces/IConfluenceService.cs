using JiraIntegration.Models.Dto;

namespace JiraIntegration.Services.Interfaces;

/// <summary>
/// Interface for Confluence API operations
/// </summary>
public interface IConfluenceService
{
    /// <summary>
    /// Tests the connection to Confluence API
    /// </summary>
    /// <returns>True if connection is successful</returns>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// Gets all spaces the user has access to
    /// </summary>
    /// <returns>List of Confluence spaces</returns>
    Task<List<ConfluenceSpace>> GetSpacesAsync();

    /// <summary>
    /// Searches for pages across all spaces
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="spaceKey">Optional space key to limit search</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>Search results</returns>
    Task<ConfluenceSearchResults> SearchPagesAsync(string query, string? spaceKey = null, int limit = 25);

    /// <summary>
    /// Gets a specific page by ID
    /// </summary>
    /// <param name="pageId">Page ID</param>
    /// <returns>Page details</returns>
    Task<ConfluencePage?> GetPageAsync(string pageId);

    /// <summary>
    /// Creates a new page in a space
    /// </summary>
    /// <param name="spaceKey">Target space key</param>
    /// <param name="title">Page title</param>
    /// <param name="content">Page content in storage format</param>
    /// <param name="parentId">Optional parent page ID</param>
    /// <returns>Created page</returns>
    Task<ConfluencePage?> CreatePageAsync(string spaceKey, string title, string content, string? parentId = null);

    /// <summary>
    /// Gets pages in a specific space
    /// </summary>
    /// <param name="spaceKey">Space key</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>List of pages in the space</returns>
    Task<List<ConfluencePage>> GetPagesInSpaceAsync(string spaceKey, int limit = 25);
}
