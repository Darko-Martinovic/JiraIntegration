using JiraIntegration.Models.Dto;

namespace JiraIntegration.Services.Interfaces;

/// <summary>
/// Interface for Jira authentication and connection validation
/// </summary>
public interface IJiraAuthService
{
    /// <summary>
    /// Validates the connection to Jira and authentication credentials
    /// </summary>
    /// <returns>True if connection is valid, false otherwise</returns>
    Task<bool> ValidateConnectionAsync();

    /// <summary>
    /// Gets information about the current authenticated user
    /// </summary>
    /// <returns>Current user information or null if not authenticated</returns>
    Task<JiraUser?> GetCurrentUserAsync();
}
