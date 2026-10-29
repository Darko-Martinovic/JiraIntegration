using System.ComponentModel.DataAnnotations;
using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using JiraIntegration.Models.Api;
using JiraIntegration.Models.Jira;
using JiraIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController(
    IJiraTicketService tickets,
    IJiraSearchService search,
    IJiraCommentService comments,
    IOptions<AtlassianSettings> options) : ControllerBase
{
    private AtlassianSettings Settings => options.Value;

    /// <summary>
    /// One page of tickets: the ones the JQL finds, or the configured project's, newest first
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<TicketPage>> List(
        string? jql,
        string? nextPageToken,
        [Range(1, 100)] int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jql))
        {
            if (Settings.ProjectKey.Length == 0)
            {
                return Problem(statusCode: 400, title: "Give a JQL query or set JIRA_PROJECT_KEY in .env");
            }
            jql = $"project = {QueryText.Quote(Settings.ProjectKey)} ORDER BY created DESC";
        }

        var page = await search.SearchAsync(jql, maxResults, nextPageToken, cancellationToken);
        return new TicketPage(page.Issues.Select(ToView).ToList(), page.IsLast ? null : page.NextPageToken);
    }

    [HttpGet("{key}")]
    public async Task<TicketView> Get([RegularExpression(Patterns.IssueKey)] string key, CancellationToken cancellationToken) =>
        ToView(await tickets.GetAsync(key, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CreatedTicket>> Create(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var projectKey = request.ProjectKey ?? Settings.ProjectKey;
        if (projectKey.Length == 0)
        {
            return Problem(statusCode: 400, title: "Give a project key or set JIRA_PROJECT_KEY in .env");
        }

        var created = await tickets.CreateAsync(projectKey, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { key = created.Key }, new CreatedTicket(created.Key, Settings.TicketUrl(created.Key)));
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Update(
        [RegularExpression(Patterns.IssueKey)] string key,
        UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        await tickets.UpdateAsync(key, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete([RegularExpression(Patterns.IssueKey)] string key, CancellationToken cancellationToken)
    {
        await tickets.DeleteAsync(key, cancellationToken);
        return NoContent();
    }

    [HttpGet("{key}/transitions")]
    public async Task<IEnumerable<TransitionView>> GetTransitions(
        [RegularExpression(Patterns.IssueKey)] string key,
        CancellationToken cancellationToken)
    {
        var transitions = await tickets.GetTransitionsAsync(key, cancellationToken);
        return transitions.Select(t => new TransitionView(t.Id, t.Name, t.To.Name));
    }

    [HttpPost("{key}/transitions")]
    public async Task<IActionResult> Transition(
        [RegularExpression(Patterns.IssueKey)] string key,
        TransitionTicketRequest request,
        CancellationToken cancellationToken)
    {
        await tickets.TransitionAsync(key, request.TransitionId, request.Comment, cancellationToken);
        return NoContent();
    }

    [HttpGet("{key}/comments")]
    public async Task<IEnumerable<CommentView>> GetComments(
        [RegularExpression(Patterns.IssueKey)] string key,
        CancellationToken cancellationToken)
    {
        var found = await comments.GetCommentsAsync(key, cancellationToken);
        return found.Select(ToView);
    }

    [HttpPost("{key}/comments")]
    public async Task<ActionResult<CommentView>> AddComment(
        [RegularExpression(Patterns.IssueKey)] string key,
        AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var comment = await comments.AddCommentAsync(key, request.Body, cancellationToken);
        return CreatedAtAction(nameof(GetComments), new { key }, ToView(comment));
    }

    [HttpDelete("{key}/comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(
        [RegularExpression(Patterns.IssueKey)] string key,
        [RegularExpression(Patterns.NumericId)] string commentId,
        CancellationToken cancellationToken)
    {
        await comments.DeleteCommentAsync(key, commentId, cancellationToken);
        return NoContent();
    }

    private TicketView ToView(JiraIssue issue) => new(
        issue.Key,
        issue.Fields.Summary,
        issue.Fields.Description,
        issue.Fields.Status?.Name ?? "",
        issue.Fields.Priority?.Name,
        issue.Fields.IssueType?.Name ?? "",
        issue.Fields.Assignee?.DisplayName,
        issue.Fields.Assignee?.AccountId,
        issue.Fields.Created,
        issue.Fields.Updated,
        Settings.TicketUrl(issue.Key));

    private static CommentView ToView(JiraComment comment) =>
        new(comment.Id, comment.Author?.DisplayName ?? "", comment.Body ?? "", comment.Created);
}
