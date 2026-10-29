using JiraIntegration.Models.Api;
using JiraIntegration.Models.Confluence;

namespace JiraIntegration.Services.Interfaces;

public interface IConfluenceService
{
    Task<IReadOnlyList<ConfluenceSpace>> GetSpacesAsync(int limit, CancellationToken cancellationToken = default);

    /// <summary>The space with this key, or null when there is none the account can see</summary>
    Task<ConfluenceSpace?> FindSpaceAsync(string spaceKey, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConfluencePage>> GetPagesAsync(string spaceId, int limit, CancellationToken cancellationToken = default);

    /// <summary>The page with its body in storage format</summary>
    Task<ConfluencePage> GetPageAsync(string pageId, CancellationToken cancellationToken = default);

    Task<ConfluencePage> CreatePageAsync(string spaceId, CreatePageRequest request, CancellationToken cancellationToken = default);

    /// <summary>Pages whose text matches, optionally in one space</summary>
    Task<ConfluenceSearchResults> SearchAsync(string text, string? spaceKey, int limit, CancellationToken cancellationToken = default);
}
