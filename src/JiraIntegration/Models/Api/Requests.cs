using System.ComponentModel.DataAnnotations;

namespace JiraIntegration.Models.Api;

/// <summary>
/// The shapes of keys and ids the API accepts in a route, checked before any of them
/// reaches a Jira or Confluence URL
/// </summary>
public static class Patterns
{
    public const string IssueKey = "^[A-Za-z][A-Za-z0-9_]*-[0-9]+$";
    public const string ProjectKey = "^[A-Za-z][A-Za-z0-9_]*$";
    public const string SpaceKey = "^~?[A-Za-z0-9_]+$";
    public const string NumericId = "^[0-9]+$";
}

/// <param name="ProjectKey">Leave out for the project in JIRA_PROJECT_KEY</param>
/// <param name="IssueType">An issue type's name in the project, such as Task or Bug</param>
/// <param name="Priority">A priority's name, such as Medium; leave out for the project's default</param>
/// <param name="AssigneeAccountId">Jira Cloud assigns by account id, not by email</param>
public sealed record CreateTicketRequest(
    [Required, StringLength(255)] string Summary,
    string? Description = null,
    [RegularExpression(Patterns.ProjectKey)] string? ProjectKey = null,
    string IssueType = "Task",
    string? Priority = null,
    string? AssigneeAccountId = null);

/// <summary>
/// Only the fields with a value change. An empty description clears it; an empty
/// assignee unassigns the ticket.
/// </summary>
public sealed record UpdateTicketRequest(
    [StringLength(255, MinimumLength = 1)] string? Summary = null,
    string? Description = null,
    string? Priority = null,
    string? AssigneeAccountId = null);

/// <param name="TransitionId">One of the ids GET /api/tickets/{key}/transitions lists</param>
/// <param name="Comment">Added to the ticket with the transition</param>
public sealed record TransitionTicketRequest(
    [Required, RegularExpression(Patterns.NumericId)] string TransitionId,
    string? Comment = null);

public sealed record AddCommentRequest([Required] string Body);

/// <param name="Text">Plain text; each block of text becomes a paragraph</param>
/// <param name="ParentId">The page to create it under; leave out for the space's home page</param>
public sealed record CreatePageRequest(
    [Required, RegularExpression(Patterns.SpaceKey)] string SpaceKey,
    [Required, StringLength(255)] string Title,
    string Text = "",
    [RegularExpression(Patterns.NumericId)] string? ParentId = null);
