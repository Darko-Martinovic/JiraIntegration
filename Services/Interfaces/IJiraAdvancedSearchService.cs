using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;

namespace JiraIntegration.Services.Interfaces;

/// <summary>
/// Service for advanced search and saved searches
/// </summary>
public interface IJiraAdvancedSearchService
{
    /// <summary>
    /// Builds JQL query using visual parameters
    /// </summary>
    string BuildJqlQuery(JqlBuilderRequest request);

    /// <summary>
    /// Executes advanced search with filters
    /// </summary>
    Task<JiraSearchResult> AdvancedSearchAsync(AdvancedSearchRequest request);

    /// <summary>
    /// Saves a search query for reuse
    /// </summary>
    Task<SavedSearch> SaveSearchAsync(SaveSearchRequest request);

    /// <summary>
    /// Gets all saved searches for the user
    /// </summary>
    Task<List<SavedSearch>> GetSavedSearchesAsync();

    /// <summary>
    /// Executes a saved search
    /// </summary>
    Task<JiraSearchResult> ExecuteSavedSearchAsync(string searchId);

    /// <summary>
    /// Gets predefined smart filters
    /// </summary>
    List<SmartFilter> GetSmartFilters();
}
