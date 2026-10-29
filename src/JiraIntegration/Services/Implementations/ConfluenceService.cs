using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using JiraIntegration.Models.Api;
using JiraIntegration.Models.Confluence;
using JiraIntegration.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Confluence on the same site and with the same API token as Jira, under /wiki.
/// Spaces and pages use REST API v2, which names spaces by id; the search uses v1's CQL search.
/// </summary>
public sealed class ConfluenceService(HttpClient http, IOptions<AtlassianSettings> settings)
    : AtlassianHttpService(http, settings), IConfluenceService
{
    public async Task<IReadOnlyList<ConfluenceSpace>> GetSpacesAsync(int limit, CancellationToken cancellationToken = default)
    {
        var page = await GetJsonAsync<ConfluenceList<ConfluenceSpace>>(
            $"wiki/api/v2/spaces?limit={limit}&status=current&description-format=plain&sort=name",
            cancellationToken);
        return page.Results;
    }

    public async Task<ConfluenceSpace?> FindSpaceAsync(string spaceKey, CancellationToken cancellationToken = default)
    {
        var page = await GetJsonAsync<ConfluenceList<ConfluenceSpace>>(
            $"wiki/api/v2/spaces?keys={Escape(spaceKey)}&description-format=plain",
            cancellationToken);
        return page.Results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<ConfluencePage>> GetPagesAsync(string spaceId, int limit, CancellationToken cancellationToken = default)
    {
        var page = await GetJsonAsync<ConfluenceList<ConfluencePage>>(
            $"wiki/api/v2/spaces/{Escape(spaceId)}/pages?limit={limit}&status=current&sort=title",
            cancellationToken);
        return page.Results;
    }

    public Task<ConfluencePage> GetPageAsync(string pageId, CancellationToken cancellationToken = default) =>
        GetJsonAsync<ConfluencePage>($"wiki/api/v2/pages/{Escape(pageId)}?body-format=storage", cancellationToken);

    public Task<ConfluencePage> CreatePageAsync(string spaceId, CreatePageRequest request, CancellationToken cancellationToken = default) =>
        PostJsonAsync<ConfluencePage>(
            "wiki/api/v2/pages",
            new
            {
                spaceId,
                status = "current",
                title = request.Title,
                parentId = request.ParentId,
                body = new { representation = "storage", value = QueryText.ToStorage(request.Text) },
            },
            cancellationToken);

    public Task<ConfluenceSearchResults> SearchAsync(string text, string? spaceKey, int limit, CancellationToken cancellationToken = default)
    {
        var cql = $"type = page AND text ~ {QueryText.Quote(text)}";
        if (!string.IsNullOrEmpty(spaceKey))
        {
            cql += $" AND space = {QueryText.Quote(spaceKey)}";
        }
        return GetJsonAsync<ConfluenceSearchResults>($"wiki/rest/api/search?cql={Escape(cql)}&limit={limit}", cancellationToken);
    }
}
