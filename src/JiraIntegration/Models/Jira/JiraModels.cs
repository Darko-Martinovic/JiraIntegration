using System.Text.Json.Serialization;
using JiraIntegration.Atlassian;

namespace JiraIntegration.Models.Jira;

// The parts of Jira's REST API v3 answers this app reads. Jira sends more fields;
// System.Text.Json skips the ones not listed here.

public sealed record JiraUser(string AccountId, string DisplayName, string? EmailAddress, bool Active);

public sealed record JiraStatus(string Id, string Name);

public sealed record JiraPriority(string Id, string Name);

public sealed record JiraIssueType(string Id, string Name, bool Subtask);

/// <summary>GET /rest/api/3/project/{key}; issueTypes comes with it by default</summary>
public sealed record JiraProject(string Id, string Key, string Name, IReadOnlyList<JiraIssueType>? IssueTypes);

public sealed record JiraIssueFields
{
    public string Summary { get; init; } = "";

    /// <summary>ADF in v3, read as plain text</summary>
    [JsonConverter(typeof(AdfTextConverter))]
    public string? Description { get; init; }

    public JiraStatus? Status { get; init; }

    public JiraPriority? Priority { get; init; }

    [JsonPropertyName("issuetype")]
    public JiraIssueType? IssueType { get; init; }

    public JiraUser? Assignee { get; init; }

    public JiraUser? Reporter { get; init; }

    public DateTimeOffset? Created { get; init; }

    public DateTimeOffset? Updated { get; init; }
}

public sealed record JiraIssue(string Id, string Key, JiraIssueFields Fields);

/// <summary>POST /rest/api/3/issue answers with the new issue's id and key only</summary>
public sealed record JiraCreatedIssue(string Id, string Key);

/// <summary>
/// GET /rest/api/3/search/jql pages with a token: pass nextPageToken back for the next
/// page, until isLast. It doesn't count the matches.
/// </summary>
public sealed record JiraSearchPage(IReadOnlyList<JiraIssue> Issues, string? NextPageToken, bool IsLast);

public sealed record JiraTransition(string Id, string Name, JiraStatus To);

public sealed record JiraTransitions(IReadOnlyList<JiraTransition> Transitions);

public sealed record JiraComment(
    string Id,
    JiraUser? Author,
    [property: JsonConverter(typeof(AdfTextConverter))] string? Body,
    DateTimeOffset Created);

public sealed record JiraComments(IReadOnlyList<JiraComment> Comments, int Total);
