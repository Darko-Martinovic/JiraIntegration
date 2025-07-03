using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;

namespace JiraIntegration.Services.Interfaces;

/// <summary>
/// Service for updating ticket fields and handling bulk operations
/// </summary>
public interface IJiraFieldUpdateService
{
    /// <summary>
    /// Updates specific fields on a ticket
    /// </summary>
    Task<bool> UpdateTicketFieldsAsync(string ticketKey, UpdateFieldsRequest request);

    /// <summary>
    /// Updates multiple tickets with the same field values
    /// </summary>
    Task<BulkUpdateResult> BulkUpdateFieldsAsync(List<string> ticketKeys, UpdateFieldsRequest request);

    /// <summary>
    /// Gets available fields for a project
    /// </summary>
    Task<List<JiraField>> GetAvailableFieldsAsync(string projectKey);

    /// <summary>
    /// Gets current field values for a ticket
    /// </summary>
    Task<Dictionary<string, object?>> GetTicketFieldValuesAsync(string ticketKey);
}
