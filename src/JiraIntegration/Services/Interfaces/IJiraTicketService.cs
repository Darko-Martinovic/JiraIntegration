using JiraIntegration.Models.Api;
using JiraIntegration.Models.Jira;

namespace JiraIntegration.Services.Interfaces;

public interface IJiraTicketService
{
    Task<JiraCreatedIssue> CreateAsync(string projectKey, CreateTicketRequest request, CancellationToken cancellationToken = default);

    Task<JiraIssue> GetAsync(string key, CancellationToken cancellationToken = default);

    Task UpdateAsync(string key, UpdateTicketRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>The moves the ticket's workflow allows from its current status</summary>
    Task<IReadOnlyList<JiraTransition>> GetTransitionsAsync(string key, CancellationToken cancellationToken = default);

    Task TransitionAsync(string key, string transitionId, string? comment, CancellationToken cancellationToken = default);
}
