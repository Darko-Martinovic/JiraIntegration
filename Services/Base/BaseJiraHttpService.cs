using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JiraIntegration.Models;

namespace JiraIntegration.Services.Base;

/// <summary>
/// Base HTTP service for Jira API operations with authentication and error handling
/// </summary>
public class BaseJiraHttpService
{
    protected readonly HttpClient _httpClient;
    protected readonly JiraSettings _settings;
    protected readonly ILogger _logger;

    public BaseJiraHttpService(
        HttpClient httpClient,
        IOptions<JiraSettings> settings,
        ILogger logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        ConfigureHttpClient();
    }

    /// <summary>
    /// Configures the HTTP client with base settings and authentication
    /// </summary>
    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        // Configure basic authentication
        var authToken = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_settings.Email}:{_settings.ApiToken}"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", authToken);

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Performs a GET request and returns the deserialized response
    /// </summary>
    protected async Task<T?> GetAsync<T>(string endpoint) where T : class
    {
        string content = string.Empty;

        try
        {
            _logger.LogDebug("Making GET request to: {Endpoint}", endpoint);

            var response = await _httpClient.GetAsync(endpoint);
            content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Raw JSON response from {Endpoint}: {Content}", endpoint, content);

                var result = JsonConvert.DeserializeObject<T>(content);
                _logger.LogDebug("GET request successful for: {Endpoint}", endpoint);
                return result;
            }

            _logger.LogWarning("GET request failed for {Endpoint}. Status: {StatusCode}, Content: {Content}",
                endpoint, response.StatusCode, content);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during GET request to: {Endpoint}", endpoint);
            throw new InvalidOperationException($"Failed to execute GET request to {endpoint}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout during GET request to: {Endpoint}", endpoint);
            throw new TimeoutException($"Request to {endpoint} timed out", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for GET request to: {Endpoint}. Raw response: {Content}", endpoint, content);
            throw new InvalidOperationException($"Failed to parse response from {endpoint}", ex);
        }
    }

    /// <summary>
    /// Performs a POST request with JSON payload
    /// </summary>
    protected async Task<T?> PostAsync<T>(string endpoint, object payload) where T : class
    {
        try
        {
            var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
            _logger.LogDebug("Making POST request to: {Endpoint} with payload: {Payload}", endpoint, json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<T>(responseContent);
                _logger.LogDebug("POST request successful for: {Endpoint}", endpoint);
                return result;
            }

            _logger.LogWarning("POST request failed for {Endpoint}. Status: {StatusCode}, Content: {Content}",
                endpoint, response.StatusCode, responseContent);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during POST request to: {Endpoint}", endpoint);
            throw new InvalidOperationException($"Failed to execute POST request to {endpoint}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout during POST request to: {Endpoint}", endpoint);
            throw new TimeoutException($"Request to {endpoint} timed out", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error during POST request to: {Endpoint}", endpoint);
            throw new InvalidOperationException($"Failed to process JSON for {endpoint}", ex);
        }
    }

    /// <summary>
    /// Performs a POST request and returns only success status (for operations that return 204 No Content)
    /// </summary>
    protected async Task<bool> PostAsyncNoResponse(string endpoint, object payload)
    {
        string responseContent = string.Empty;

        try
        {
            var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
            _logger.LogDebug("Making POST request to: {Endpoint} with payload: {Payload}", endpoint, json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("POST response for {Endpoint}: Status={StatusCode}, IsSuccess={IsSuccess}",
                endpoint, response.StatusCode, response.IsSuccessStatusCode);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("POST request successful for: {Endpoint}, Status: {StatusCode}", endpoint, response.StatusCode);
                return true;
            }

            _logger.LogWarning("POST request failed for {Endpoint}. Status: {StatusCode}, Content: {Content}",
                endpoint, response.StatusCode, responseContent);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during POST request to: {Endpoint}", endpoint);
            throw new InvalidOperationException($"Failed to execute POST request to {endpoint}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout during POST request to: {Endpoint}", endpoint);
            throw new TimeoutException($"Request to {endpoint} timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during POST request to: {Endpoint}. Response: {Content}", endpoint, responseContent);
            throw new InvalidOperationException($"Failed to process POST request to {endpoint}", ex);
        }
    }

    /// <summary>
    /// Performs a PUT request with JSON payload
    /// </summary>
    protected async Task<bool> PutAsync(string endpoint, object payload)
    {
        try
        {
            var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
            _logger.LogDebug("Making PUT request to: {Endpoint} with payload: {Payload}", endpoint, json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("PUT request successful for: {Endpoint}", endpoint);
                return true;
            }

            _logger.LogWarning("PUT request failed for {Endpoint}. Status: {StatusCode}, Content: {Content}",
                endpoint, response.StatusCode, responseContent);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during PUT request to: {Endpoint}", endpoint);
            throw new InvalidOperationException($"Failed to execute PUT request to {endpoint}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout during PUT request to: {Endpoint}", endpoint);
            throw new TimeoutException($"Request to {endpoint} timed out", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error during PUT request to: {Endpoint}", endpoint);
            throw new InvalidOperationException($"Failed to process JSON for {endpoint}", ex);
        }
    }

    /// <summary>
    /// Tests the connection to Jira by attempting to get current user info
    /// </summary>
    protected async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/rest/api/3/myself");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
