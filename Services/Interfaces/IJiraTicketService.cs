using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;

namespace JiraIntegration.Services.Interfaces;

/// <summary>
/// Interface for Jira ticket/issue operations
/// </summary>
public interface IJiraTicketService
{
    /// <summary>
    /// Creates a new ticket in Jira
    /// </summary>
    /// <param name="request">Ticket creation request</param>
    /// <returns>Created ticket information</returns>
    Task<JiraIssue?> CreateTicketAsync(CreateIssueRequest request);

    /// <summary>
    /// Gets a ticket by its key
    /// </summary>
    /// <param name="ticketKey">Ticket key (e.g., "PROJECT-123")</param>
    /// <returns>Ticket information or null if not found</returns>
    Task<JiraIssue?> GetTicketAsync(string ticketKey);

    /// <summary>
    /// Updates an existing ticket
    /// </summary>
    /// <param name="ticketKey">Ticket key to update</param>
    /// <param name="request">Update request</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateTicketAsync(string ticketKey, UpdateIssueRequest request);

    /// <summary>
    /// Gets available transitions for a ticket
    /// </summary>
    /// <param name="ticketKey">Ticket key</param>
    /// <returns>List of available transitions</returns>
    Task<List<JiraTransition>> GetAvailableTransitionsAsync(string ticketKey);

    /// <summary>
    /// Transitions a ticket to a new status
    /// </summary>
    /// <param name="ticketKey">Ticket key</param>
    /// <param name="transitionId">Transition ID to execute</param>
    /// <param name="comment">Optional comment</param>
    /// <returns>True if transition was successful</returns>
    Task<bool> TransitionTicketAsync(string ticketKey, string transitionId, string? comment = null);
}
