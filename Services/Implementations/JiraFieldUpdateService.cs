using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Interfaces;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Implementation of Jira field update service
/// </summary>
public class JiraFieldUpdateService : BaseJiraHttpService, IJiraFieldUpdateService
{
    public JiraFieldUpdateService(
        HttpClient httpClient,
        IOptions<JiraSettings> settings,
        ILogger<JiraFieldUpdateService> logger)
        : base(httpClient, settings, logger)
    {
    }

    /// <summary>
    /// Updates specific fields on a ticket
    /// </summary>
    public async Task<bool> UpdateTicketFieldsAsync(string ticketKey, UpdateFieldsRequest request)
    {
        try
        {
            _logger.LogInformation("Updating fields for ticket: {TicketKey}", ticketKey);

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                throw new ArgumentException("Ticket key cannot be empty", nameof(ticketKey));
            }

            var payload = request.ToJiraFormat();
            var success = await PutAsync($"/rest/api/3/issue/{ticketKey}", payload);

            if (success)
            {
                _logger.LogInformation("Successfully updated fields for ticket: {TicketKey}", ticketKey);
            }
            else
            {
                _logger.LogWarning("Failed to update fields for ticket: {TicketKey}", ticketKey);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fields for ticket: {TicketKey}", ticketKey);
            throw;
        }
    }

    /// <summary>
    /// Updates multiple tickets with the same field values
    /// </summary>
    public async Task<BulkUpdateResult> BulkUpdateFieldsAsync(List<string> ticketKeys, UpdateFieldsRequest request)
    {
        try
        {
            _logger.LogInformation("Bulk updating {Count} tickets", ticketKeys.Count);

            var result = new BulkUpdateResult
            {
                TotalTickets = ticketKeys.Count
            };

            foreach (var ticketKey in ticketKeys)
            {
                try
                {
                    var success = await UpdateTicketFieldsAsync(ticketKey, request);
                    if (success)
                    {
                        result.SuccessfulUpdates++;
                    }
                    else
                    {
                        result.FailedUpdates++;
                        result.FailedTicketKeys.Add(ticketKey);
                        result.ErrorMessages.Add($"Failed to update {ticketKey}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedUpdates++;
                    result.FailedTicketKeys.Add(ticketKey);
                    result.ErrorMessages.Add($"Error updating {ticketKey}: {ex.Message}");
                    _logger.LogError(ex, "Error in bulk update for ticket: {TicketKey}", ticketKey);
                }
            }

            _logger.LogInformation("Bulk update completed. Success: {Success}, Failed: {Failed}",
                result.SuccessfulUpdates, result.FailedUpdates);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk update operation");
            throw;
        }
    }

    /// <summary>
    /// Gets available fields for a project
    /// </summary>
    public async Task<List<JiraField>> GetAvailableFieldsAsync(string projectKey)
    {
        try
        {
            _logger.LogInformation("Getting available fields for project: {ProjectKey}", projectKey);

            if (string.IsNullOrWhiteSpace(projectKey))
            {
                throw new ArgumentException("Project key cannot be empty", nameof(projectKey));
            }

            var response = await GetAsync<List<JiraField>>($"/rest/api/3/project/{projectKey}/fields");

            if (response != null)
            {
                _logger.LogDebug("Found {Count} fields for project: {ProjectKey}", response.Count, projectKey);
                return response;
            }

            _logger.LogWarning("No fields found for project: {ProjectKey}", projectKey);
            return new List<JiraField>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fields for project: {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <summary>
    /// Gets current field values for a ticket
    /// </summary>
    public async Task<Dictionary<string, object?>> GetTicketFieldValuesAsync(string ticketKey)
    {
        try
        {
            _logger.LogInformation("Getting field values for ticket: {TicketKey}", ticketKey);

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                throw new ArgumentException("Ticket key cannot be empty", nameof(ticketKey));
            }

            var ticket = await GetAsync<JiraIssue>($"/rest/api/3/issue/{ticketKey}");

            if (ticket?.Fields != null)
            {
                var fieldValues = new Dictionary<string, object?>
                {
                    ["summary"] = ticket.Fields.Summary,
                    ["description"] = ticket.Fields.Description,
                    ["assignee"] = ticket.Fields.Assignee?.DisplayName,
                    ["priority"] = ticket.Fields.Priority.Name,
                    ["status"] = ticket.Fields.Status.Name,
                    ["created"] = ticket.Fields.Created,
                    ["updated"] = ticket.Fields.Updated
                };

                _logger.LogDebug("Retrieved field values for ticket: {TicketKey}", ticketKey);
                return fieldValues;
            }

            _logger.LogWarning("No field values found for ticket: {TicketKey}", ticketKey);
            return new Dictionary<string, object?>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field values for ticket: {TicketKey}", ticketKey);
            throw;
        }
    }
}
