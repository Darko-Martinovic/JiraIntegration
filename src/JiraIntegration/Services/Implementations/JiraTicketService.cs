using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using JiraIntegration.Models.Api;
using JiraIntegration.Models.Jira;
using JiraIntegration.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Services.Implementations;

public sealed class JiraTicketService(HttpClient http, IOptions<AtlassianSettings> settings)
    : AtlassianHttpService(http, settings), IJiraTicketService
{
    private const string Fields = "summary,description,status,priority,issuetype,assignee,reporter,created,updated";

    public Task<JiraCreatedIssue> CreateAsync(string projectKey, CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        // Jira takes the project, the issue type and the priority by name or by id;
        // names travel between sites, ids don't
        var fields = new Dictionary<string, object?>
        {
            ["project"] = new { key = projectKey },
            ["issuetype"] = new { name = request.IssueType },
            ["summary"] = request.Summary,
        };
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            fields["description"] = Adf.FromText(request.Description);
        }
        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            fields["priority"] = new { name = request.Priority };
        }
        if (!string.IsNullOrWhiteSpace(request.AssigneeAccountId))
        {
            fields["assignee"] = new { accountId = request.AssigneeAccountId };
        }

        return PostJsonAsync<JiraCreatedIssue>("rest/api/3/issue", new { fields }, cancellationToken);
    }

    public Task<JiraIssue> GetAsync(string key, CancellationToken cancellationToken = default) =>
        GetJsonAsync<JiraIssue>($"rest/api/3/issue/{Escape(key)}?fields={Fields}", cancellationToken);

    public Task UpdateAsync(string key, UpdateTicketRequest request, CancellationToken cancellationToken = default)
    {
        // A null value clears a field in Jira, so the dictionary keeps them
        var fields = new Dictionary<string, object?>();
        if (request.Summary is not null)
        {
            fields["summary"] = request.Summary;
        }
        if (request.Description is not null)
        {
            fields["description"] = request.Description.Length == 0 ? null : Adf.FromText(request.Description);
        }
        if (!string.IsNullOrEmpty(request.Priority))
        {
            fields["priority"] = new { name = request.Priority };
        }
        if (request.AssigneeAccountId is not null)
        {
            fields["assignee"] = request.AssigneeAccountId.Length == 0 ? null : new { accountId = request.AssigneeAccountId };
        }

        return fields.Count == 0
            ? Task.CompletedTask
            : PutJsonAsync($"rest/api/3/issue/{Escape(key)}", new { fields }, cancellationToken);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
        DeleteResourceAsync($"rest/api/3/issue/{Escape(key)}", cancellationToken);

    public async Task<IReadOnlyList<JiraTransition>> GetTransitionsAsync(string key, CancellationToken cancellationToken = default)
    {
        var answer = await GetJsonAsync<JiraTransitions>($"rest/api/3/issue/{Escape(key)}/transitions", cancellationToken);
        return answer.Transitions;
    }

    public Task TransitionAsync(string key, string transitionId, string? comment, CancellationToken cancellationToken = default)
    {
        var update = string.IsNullOrWhiteSpace(comment)
            ? null
            : new { comment = new[] { new { add = new { body = Adf.FromText(comment) } } } };

        return PostJsonAsync(
            $"rest/api/3/issue/{Escape(key)}/transitions",
            new { transition = new { id = transitionId }, update },
            cancellationToken);
    }
}
