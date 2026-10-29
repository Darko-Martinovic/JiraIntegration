using System.ComponentModel.DataAnnotations;
using JiraIntegration.Models.Api;
using JiraIntegration.Models.Jira;
using JiraIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JiraIntegration.Controllers;

/// <summary>
/// What a ticket form offers: the project's issue types, the priorities and the people to assign
/// </summary>
[ApiController]
[Route("api")]
public sealed class ProjectsController(IJiraProjectService projects, IJiraUserService users) : ControllerBase
{
    /// <summary>The issue types a ticket in the project can have, without sub-tasks</summary>
    [HttpGet("projects/{projectKey}/issue-types")]
    public async Task<IEnumerable<JiraIssueType>> GetIssueTypes(
        [RegularExpression(Patterns.ProjectKey)] string projectKey,
        CancellationToken cancellationToken)
    {
        var project = await projects.GetProjectAsync(projectKey, cancellationToken);
        return (project.IssueTypes ?? []).Where(t => !t.Subtask);
    }

    [HttpGet("projects/{projectKey}/assignable-users")]
    public Task<IReadOnlyList<JiraUser>> GetAssignableUsers(
        [RegularExpression(Patterns.ProjectKey)] string projectKey,
        [Range(1, 100)] int maxResults = 50,
        CancellationToken cancellationToken = default) =>
        users.GetAssignableAsync(projectKey, maxResults, cancellationToken);

    [HttpGet("priorities")]
    public Task<IReadOnlyList<JiraPriority>> GetPriorities(CancellationToken cancellationToken) =>
        projects.GetPrioritiesAsync(cancellationToken);

    /// <summary>Users whose name or email starts with the query</summary>
    [HttpGet("users")]
    public Task<IReadOnlyList<JiraUser>> SearchUsers(
        [Required, MinLength(1)] string query,
        [Range(1, 100)] int maxResults = 20,
        CancellationToken cancellationToken = default) =>
        users.SearchAsync(query, maxResults, cancellationToken);
}
