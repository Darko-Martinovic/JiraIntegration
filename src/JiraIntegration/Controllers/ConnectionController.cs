using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using JiraIntegration.Models.Api;
using JiraIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Controllers;

[ApiController]
[Route("api/connection")]
public sealed class ConnectionController(
    IOptions<AtlassianSettings> options,
    IJiraAuthService auth,
    IConfluenceService confluence) : ControllerBase
{
    /// <summary>
    /// The settings in use (without the token) and whether Jira and Confluence accept them
    /// </summary>
    [HttpGet]
    public async Task<ConnectionStatus> Get(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var shown = GetSettings();
        var missing = settings.MissingSettings();
        if (missing.Count > 0)
        {
            return new ConnectionStatus(shown, missing, false, null, null, false, null);
        }

        var jira = TryAsync(() => auth.GetCurrentUserAsync(cancellationToken));
        var spaces = TryAsync(() => confluence.GetSpacesAsync(1, cancellationToken));
        var (user, jiraError) = await jira;
        var (_, confluenceError) = await spaces;

        return new ConnectionStatus(shown, missing, user is not null, jiraError, user, confluenceError is null, confluenceError);
    }

    /// <summary>
    /// The settings in use, without the token and without calling Atlassian
    /// </summary>
    [HttpGet("settings")]
    public SiteSettings GetSettings()
    {
        var settings = options.Value;
        return new SiteSettings(settings.Site, settings.Email, settings.ProjectKey, settings.ApiToken.Length > 0);
    }

    private static async Task<(T? Value, string? Error)> TryAsync<T>(Func<Task<T>> call) where T : class
    {
        try
        {
            return (await call(), null);
        }
        catch (AtlassianApiException error)
        {
            return (null, $"{(int)error.StatusCode}: {error.Message}");
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException)
        {
            return (null, error.Message);
        }
    }
}
