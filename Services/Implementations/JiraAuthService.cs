using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Interfaces;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Implementation of Jira authentication service
/// </summary>
public class JiraAuthService : BaseJiraHttpService, IJiraAuthService
{
    public JiraAuthService(
        HttpClient httpClient,
        IOptions<JiraSettings> settings,
        ILogger<JiraAuthService> logger)
        : base(httpClient, settings, logger)
    {
    }

    /// <summary>
    /// Validates the connection to Jira and authentication credentials
    /// </summary>
    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Validating Jira connection...");

            if (!_settings.IsValid())
            {
                _logger.LogWarning("Jira settings are invalid or incomplete");
                return false;
            }

            var user = await GetCurrentUserAsync();
            var isValid = user != null && user.Active;

            if (isValid)
            {
                _logger.LogInformation("Connection validation successful for user: {DisplayName}", user!.DisplayName);
            }
            else
            {
                _logger.LogWarning("Connection validation failed");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Jira connection");
            return false;
        }
    }

    /// <summary>
    /// Gets information about the current authenticated user
    /// </summary>
    public async Task<JiraUser?> GetCurrentUserAsync()
    {
        try
        {
            _logger.LogDebug("Getting current user information");
            var user = await GetAsync<JiraUser>("/rest/api/3/myself");

            if (user != null)
            {
                _logger.LogDebug("Successfully retrieved user: {DisplayName} ({Email})",
                    user.DisplayName, user.EmailAddress);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user information");
            return null;
        }
    }
}
