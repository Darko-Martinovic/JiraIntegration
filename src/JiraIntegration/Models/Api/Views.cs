using JiraIntegration.Models.Jira;

namespace JiraIntegration.Models.Api;

// What the API sends the browser: flat records with the links to Jira and Confluence filled in.

public sealed record SiteSettings(string BaseUrl, string Email, string ProjectKey, bool HasApiToken);

/// <summary>
/// Jira and Confluence are checked separately: an account can have one without the other
/// </summary>
public sealed record ConnectionStatus(
    SiteSettings Settings,
    IReadOnlyList<string> MissingSettings,
    bool JiraConnected,
    string? JiraError,
    JiraUser? User,
    bool ConfluenceConnected,
    string? ConfluenceError);

public sealed record TicketView(
    string Key,
    string Summary,
    string? Description,
    string Status,
    string? Priority,
    string IssueType,
    string? Assignee,
    string? AssigneeAccountId,
    DateTimeOffset? Created,
    DateTimeOffset? Updated,
    string Url);

/// <param name="NextPageToken">Pass back as nextPageToken for the next page; null on the last page</param>
public sealed record TicketPage(IReadOnlyList<TicketView> Tickets, string? NextPageToken);

public sealed record CreatedTicket(string Key, string Url);

public sealed record TransitionView(string Id, string Name, string ToStatus);

public sealed record CommentView(string Id, string Author, string Body, DateTimeOffset Created);

public sealed record SpaceView(string Id, string Key, string Name, string? Description, string Url);

public sealed record PageView(string Id, string Title, string SpaceId, int? Version, string Url, string? Body);

public sealed record SearchHit(string Id, string Title, string Excerpt, string? Space, string Url);
