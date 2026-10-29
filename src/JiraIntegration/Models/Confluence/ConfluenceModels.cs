using System.Text.Json.Serialization;

namespace JiraIntegration.Models.Confluence;

// Spaces and pages come from Confluence's REST API v2 (/wiki/api/v2); the search
// comes from v1 (/wiki/rest/api/search), since v2 has no CQL search.

public sealed record ConfluenceLinks(string? WebUi, string? Next);

/// <summary>A page of results; _links.next holds the URL of the next page, with its cursor</summary>
public sealed record ConfluenceList<T>(
    IReadOnlyList<T> Results,
    [property: JsonPropertyName("_links")] ConfluenceLinks? Links);

public sealed record ConfluenceBodyValue(string Representation, string Value);

public sealed record ConfluenceSpaceDescription(ConfluenceBodyValue? Plain);

public sealed record ConfluenceSpace(
    string Id,
    string Key,
    string Name,
    string Type,
    string Status,
    ConfluenceSpaceDescription? Description,
    [property: JsonPropertyName("_links")] ConfluenceLinks? Links);

public sealed record ConfluenceVersion(int Number, DateTimeOffset? CreatedAt);

public sealed record ConfluencePageBody(ConfluenceBodyValue? Storage);

public sealed record ConfluencePage(
    string Id,
    string Title,
    string SpaceId,
    string Status,
    string? ParentId,
    ConfluenceVersion? Version,
    ConfluencePageBody? Body,
    [property: JsonPropertyName("_links")] ConfluenceLinks? Links);

public sealed record ConfluenceSearchContent(string Id, string Type, string Title);

public sealed record ConfluenceSearchContainer(string? Title);

/// <summary>One hit of GET /wiki/rest/api/search; url is relative to the site's /wiki</summary>
public sealed record ConfluenceSearchResult(
    ConfluenceSearchContent? Content,
    string Title,
    string? Excerpt,
    string? Url,
    ConfluenceSearchContainer? ResultGlobalContainer);

public sealed record ConfluenceSearchResults(IReadOnlyList<ConfluenceSearchResult> Results, int TotalSize);
