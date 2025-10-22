import React, { useState, useEffect } from "react";
import {
  CheckCircle,
  XCircle,
  AlertCircle,
  RefreshCw,
  Code,
} from "lucide-react";

interface ConnectionStatus {
  isConnected: boolean;
  message: string;
  user?: {
    accountId: string;
    displayName: string;
    emailAddress: string;
  };
  settings?: {
    baseUrl: string;
    projectKey: string;
    email: string;
    maxResults: number;
    timeoutSeconds: number;
  };
}

const Dashboard: React.FC = () => {
  const [connectionStatus, setConnectionStatus] =
    useState<ConnectionStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCSharpCode, setShowCSharpCode] = useState(false);

  const checkConnection = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(
        "http://localhost:5000/api/jira/test-connection"
      );
      if (response.ok) {
        const data = await response.json();
        setConnectionStatus(data);
      } else {
        throw new Error(`HTTP ${response.status}`);
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to test connection"
      );
      setConnectionStatus(null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    checkConnection();
  }, []);

  const getStatusIcon = () => {
    if (loading)
      return <RefreshCw className="w-5 h-5 animate-spin text-blue-600" />;
    if (error) return <XCircle className="w-5 h-5 text-red-600" />;
    if (connectionStatus?.isConnected)
      return <CheckCircle className="w-5 h-5 text-green-600" />;
    return <XCircle className="w-5 h-5 text-red-600" />;
  };

  const getStatusColor = () => {
    if (loading) return "bg-blue-50 border-blue-200";
    if (error) return "bg-red-50 border-red-200";
    if (connectionStatus?.isConnected) return "bg-green-50 border-green-200";
    return "bg-red-50 border-red-200";
  };

  const getStatusText = () => {
    if (loading) return "Testing connection...";
    if (error) return `Connection Error: ${error}`;
    return connectionStatus?.message || "Unknown status";
  };

  return (
    <div className="p-8">
      <div className="max-w-7xl mx-auto">
        {/* Page Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600 mt-1">
            Monitor your JIRA integration status and system health
          </p>
        </div>

        {/* Connection Status Card */}
        <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6 mb-8">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold text-gray-900">
              JIRA Connection Status
            </h2>
            <button
              onClick={checkConnection}
              disabled={loading}
              className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
            >
              <RefreshCw
                className={`w-4 h-4 mr-2 ${loading ? "animate-spin" : ""}`}
              />
              Refresh
            </button>
          </div>

          <div className={`rounded-lg border p-4 ${getStatusColor()}`}>
            <div className="flex items-center">
              {getStatusIcon()}
              <span className="ml-3 text-sm font-medium text-gray-900">
                {getStatusText()}
              </span>
            </div>
          </div>

          {/* User Information */}
          {connectionStatus?.user && (
            <div className="mt-4 p-4 bg-gray-50 rounded-lg">
              <h3 className="text-sm font-medium text-gray-900 mb-2">
                Connected User
              </h3>
              <div className="space-y-1">
                <p className="text-sm text-gray-600">
                  <span className="font-medium">Name:</span>{" "}
                  {connectionStatus.user.displayName}
                </p>
                <p className="text-sm text-gray-600">
                  <span className="font-medium">Email:</span>{" "}
                  {connectionStatus.user.emailAddress}
                </p>
                <p className="text-sm text-gray-600">
                  <span className="font-medium">Account ID:</span>{" "}
                  {connectionStatus.user.accountId}
                </p>
              </div>
            </div>
          )}
        </div>

        {/* System Information */}
        {connectionStatus?.settings && (
          <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6 mb-8">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              System Configuration
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="p-4 bg-gray-50 rounded-lg">
                <h3 className="text-sm font-medium text-gray-900 mb-2">
                  JIRA Instance
                </h3>
                <p className="text-sm text-gray-600 break-words">
                  {connectionStatus.settings.baseUrl}
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg">
                <h3 className="text-sm font-medium text-gray-900 mb-2">
                  Project Key
                </h3>
                <p className="text-sm text-gray-600">
                  {connectionStatus.settings.projectKey}
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg">
                <h3 className="text-sm font-medium text-gray-900 mb-2">
                  Configured Email
                </h3>
                <p className="text-sm text-gray-600">
                  {connectionStatus.settings.email}
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg">
                <h3 className="text-sm font-medium text-gray-900 mb-2">
                  Query Limit
                </h3>
                <p className="text-sm text-gray-600">
                  {connectionStatus.settings.maxResults} results
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Quick Actions */}
        <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-semibold text-gray-900">
              Quick Actions
            </h2>
            <button
              onClick={() => setShowCSharpCode(!showCSharpCode)}
              className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              <Code className="w-4 h-4 mr-2" />
              {showCSharpCode ? "Hide" : "Show"} C# Code
            </button>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <a
              href="/configuration"
              className="block p-4 border border-gray-200 rounded-lg hover:border-blue-300 hover:shadow-md transition-all duration-200"
            >
              <div className="flex items-center">
                <div className="p-2 bg-blue-100 rounded-lg">
                  <AlertCircle className="w-5 h-5 text-blue-600" />
                </div>
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-gray-900">
                    Configuration
                  </h3>
                  <p className="text-xs text-gray-500">Manage JIRA settings</p>
                </div>
              </div>
            </a>

            <a
              href="/tickets"
              className="block p-4 border border-gray-200 rounded-lg hover:border-blue-300 hover:shadow-md transition-all duration-200"
            >
              <div className="flex items-center">
                <div className="p-2 bg-green-100 rounded-lg">
                  <CheckCircle className="w-5 h-5 text-green-600" />
                </div>
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-gray-900">
                    Manage Tickets
                  </h3>
                  <p className="text-xs text-gray-500">
                    Create, update, and view tickets
                  </p>
                </div>
              </div>
            </a>

            <div className="block p-4 border border-gray-200 rounded-lg opacity-50">
              <div className="flex items-center">
                <div className="p-2 bg-gray-100 rounded-lg">
                  <RefreshCw className="w-5 h-5 text-gray-400" />
                </div>
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-gray-500">Reports</h3>
                  <p className="text-xs text-gray-400">Coming soon</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* C# Code Examples Section */}
        {showCSharpCode && (
          <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6 mt-8">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              C# JIRA Connection & Authentication Code
            </h2>

            <div className="space-y-6">
              {/* Connection Test Example */}
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-3">
                  1. Test JIRA Connection
                </h3>
                <pre className="bg-gray-900 text-gray-100 p-4 rounded-lg overflow-x-auto text-sm">
                  <code>{`// C# - Test JIRA Connection
using System.Text;
using System.Text.Json;

public class JiraAuthService
{
    private readonly HttpClient _httpClient;
    private readonly JiraSettings _settings;

    public JiraAuthService(HttpClient httpClient, IOptions<JiraSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            // Set up Basic Authentication
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_settings.Email}:{_settings.ApiToken}")
            );
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            // Test connection by getting current user info
            var response = await _httpClient.GetAsync("/rest/api/3/myself");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            // Log error details
            Console.WriteLine($"Connection failed: {ex.Message}");
            return false;
        }
    }

    public async Task<JiraUser?> GetCurrentUserAsync()
    {
        try
        {
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_settings.Email}:{_settings.ApiToken}")
            );
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.GetAsync("/rest/api/3/myself");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<JiraUser>(json);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get current user: {ex.Message}");
            return null;
        }
    }
}`}</code>
                </pre>
              </div>

              {/* Configuration Example */}
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-3">
                  2. JIRA Configuration Setup
                </h3>
                <pre className="bg-gray-900 text-gray-100 p-4 rounded-lg overflow-x-auto text-sm">
                  <code>{`// C# - JIRA Configuration
public class JiraSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 50;
    public int TimeoutSeconds { get; set; } = 30;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(BaseUrl) &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(ApiToken) &&
               !string.IsNullOrWhiteSpace(ProjectKey);
    }
}

// Program.cs - Dependency Injection Setup
var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotNetEnv.Env.Load(".env");

// Configure JIRA settings from environment variables
builder.Services.Configure<JiraSettings>(options =>
{
    options.BaseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL") ?? "";
    options.Email = Environment.GetEnvironmentVariable("JIRA_USER_EMAIL") ?? "";
    options.ApiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN") ?? "";
    options.ProjectKey = Environment.GetEnvironmentVariable("JIRA_PROJECT_KEY") ?? "";
});

// Configure HTTP client with timeout and headers
builder.Services.AddHttpClient<JiraAuthService>(client =>
{
    var baseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL");
    if (!string.IsNullOrEmpty(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Register services
builder.Services.AddScoped<JiraAuthService>();

var app = builder.Build();`}</code>
                </pre>
              </div>

              {/* Environment Variables */}
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-3">
                  3. Environment Variables (.env file)
                </h3>
                <pre className="bg-gray-900 text-gray-100 p-4 rounded-lg overflow-x-auto text-sm">
                  <code>{`# .env file - JIRA Configuration
JIRA_BASE_URL=https://your-domain.atlassian.net
JIRA_USER_EMAIL=your-email@domain.com
JIRA_API_TOKEN=your-api-token-here
JIRA_PROJECT_KEY=YOUR-PROJECT

# How to get JIRA API Token:
# 1. Go to https://id.atlassian.com/manage-profile/security/api-tokens
# 2. Click "Create API token"
# 3. Give it a label (e.g., "C# Integration")
# 4. Click "Create"
# 5. Copy the token immediately (you won't see it again)
# 6. Store it securely in your .env file

# Security Best Practices:
# - Never commit .env files to version control
# - Add .env to your .gitignore file
# - Use different tokens for different environments
# - Rotate tokens regularly
# - Use minimal required permissions`}</code>
                </pre>
              </div>

              {/* Error Handling */}
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-3">
                  4. Error Handling & Logging
                </h3>
                <pre className="bg-gray-900 text-gray-100 p-4 rounded-lg overflow-x-auto text-sm">
                  <code>{`// C# - Error Handling for JIRA API
public class JiraService
{
    private readonly ILogger<JiraService> _logger;
    private readonly HttpClient _httpClient;

    public async Task<T?> GetAsync<T>(string endpoint) where T : class
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json);
            }
            
            // Log HTTP errors
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("JIRA API Error: {StatusCode} - {Content}", 
                response.StatusCode, errorContent);
            
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while calling JIRA API: {Endpoint}", endpoint);
            throw new Exception($"Network error: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while calling JIRA API: {Endpoint}", endpoint);
            throw new Exception("Request timed out", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JIRA API response: {Endpoint}", endpoint);
            throw new Exception("Invalid response format", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while calling JIRA API: {Endpoint}", endpoint);
            throw;
        }
    }
}`}</code>
                </pre>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Dashboard;
