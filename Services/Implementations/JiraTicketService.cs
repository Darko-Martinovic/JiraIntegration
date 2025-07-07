using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Interfaces;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Implementation of Jira ticket service
/// </summary>
public class JiraTicketService : BaseJiraHttpService, IJiraTicketService
{
    public JiraTicketService(
        HttpClient httpClient,
        IOptions<JiraSettings> settings,
        ILogger<JiraTicketService> logger
    )
        : base(httpClient, settings, logger) { }

    /// <summary>
    /// Creates a new ticket in Jira
    /// </summary>
    public async Task<JiraIssue?> CreateTicketAsync(CreateIssueRequest request)
    {
        try
        {
            _logger.LogDebug(
                "Creating new ticket: {Summary} in project {ProjectKey}",
                request.Summary,
                request.ProjectKey
            );

            var payload = request.ToJiraFormat();
            var result = await PostAsync<JiraIssue>("/rest/api/3/issue", payload);

            if (result != null)
            {
                _logger.LogInformation("Successfully created ticket: {Key}", result.Key);
            }
            else
            {
                _logger.LogWarning("Failed to create ticket");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ticket");
            throw;
        }
    }

    /// <summary>
    /// Gets a ticket by its key
    /// </summary>
    public async Task<JiraIssue?> GetTicketAsync(string ticketKey)
    {
        try
        {
            _logger.LogDebug("Getting ticket: {TicketKey}", ticketKey);

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                throw new ArgumentException("Ticket key cannot be empty", nameof(ticketKey));
            }

            var ticket = await GetAsync<JiraIssue>($"/rest/api/3/issue/{ticketKey}");

            if (ticket != null)
            {
                _logger.LogDebug(
                    "Successfully retrieved ticket: {Key} - {Summary}",
                    ticket.Key,
                    ticket.Fields.Summary
                );
            }
            else
            {
                _logger.LogWarning("Ticket not found: {TicketKey}", ticketKey);
            }

            return ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket: {TicketKey}", ticketKey);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing ticket
    /// </summary>
    public async Task<bool> UpdateTicketAsync(string ticketKey, UpdateIssueRequest request)
    {
        try
        {
            _logger.LogDebug("Updating ticket: {TicketKey}", ticketKey);

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                throw new ArgumentException("Ticket key cannot be empty", nameof(ticketKey));
            }

            var payload = request.ToJiraFormat();
            var success = await PutAsync($"/rest/api/3/issue/{ticketKey}", payload);

            if (success)
            {
                _logger.LogInformation("Successfully updated ticket: {TicketKey}", ticketKey);
            }
            else
            {
                _logger.LogWarning("Failed to update ticket: {TicketKey}", ticketKey);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket: {TicketKey}", ticketKey);
            throw;
        }
    }

    /// <summary>
    /// Gets available transitions for a ticket
    /// </summary>
    public async Task<List<JiraTransition>> GetAvailableTransitionsAsync(string ticketKey)
    {
        try
        {
            _logger.LogInformation(
                "Getting available transitions for ticket: {TicketKey}",
                ticketKey
            );

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                throw new ArgumentException("Ticket key cannot be empty", nameof(ticketKey));
            }

            var response = await GetAsync<JiraTransitionsResponse>(
                $"/rest/api/3/issue/{ticketKey}/transitions"
            );

            if (response?.Transitions != null)
            {
                _logger.LogDebug(
                    "Found {Count} transitions for ticket: {TicketKey}",
                    response.Transitions.Count,
                    ticketKey
                );
                return response.Transitions;
            }

            _logger.LogWarning("No transitions found for ticket: {TicketKey}", ticketKey);
            return new List<JiraTransition>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transitions for ticket: {TicketKey}", ticketKey);
            throw;
        }
    }

    /// <summary>
    /// Transitions a ticket to a new status
    /// </summary>
    public async Task<bool> TransitionTicketAsync(
        string ticketKey,
        string transitionId,
        string? comment = null
    )
    {
        try
        {
            _logger.LogInformation(
                "Transitioning ticket {TicketKey} with transition {TransitionId}",
                ticketKey,
                transitionId
            );

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                throw new ArgumentException("Ticket key cannot be empty", nameof(ticketKey));
            }

            if (string.IsNullOrWhiteSpace(transitionId))
            {
                throw new ArgumentException("Transition ID cannot be empty", nameof(transitionId));
            }

            var request = new TransitionIssueRequest
            {
                TransitionId = transitionId,
                Comment = comment
            };

            var payload = request.ToJiraFormat();
            _logger.LogDebug(
                "About to call PostAsyncNoResponse for ticket: {TicketKey}",
                ticketKey
            );

            bool success;
            try
            {
                success = await PostAsyncNoResponse(
                    $"/rest/api/3/issue/{ticketKey}/transitions",
                    payload
                );
                _logger.LogDebug(
                    "PostAsyncNoResponse completed successfully and returned: {Success} for ticket: {TicketKey}",
                    success,
                    ticketKey
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception thrown by PostAsyncNoResponse for ticket: {TicketKey}",
                    ticketKey
                );
                throw;
            }

            if (success)
            {
                _logger.LogInformation("Successfully transitioned ticket: {TicketKey}", ticketKey);
            }
            else
            {
                _logger.LogWarning("Failed to transition ticket: {TicketKey}", ticketKey);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning ticket: {TicketKey}", ticketKey);
            throw;
        }
    }
}
