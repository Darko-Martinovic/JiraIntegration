using System.Net;
using System.Text.Json;

namespace JiraIntegration.Atlassian;

/// <summary>
/// Jira or Confluence answered with an error status. The message is the one Atlassian sent.
/// </summary>
public sealed class AtlassianApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public static AtlassianApiException From(HttpStatusCode statusCode, string body) =>
        new(statusCode, ReadMessage(body) ?? $"Atlassian answered {(int)statusCode} {statusCode}");

    /// <summary>
    /// Reads the three error shapes in use: Jira's errorMessages and errors by field,
    /// Confluence v2's errors list and Confluence v1's message
    /// </summary>
    internal static string? ReadMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var messages = new List<string>();
            if (root.TryGetProperty("errorMessages", out var errorMessages) && errorMessages.ValueKind == JsonValueKind.Array)
            {
                messages.AddRange(errorMessages.EnumerateArray()
                    .Where(m => m.ValueKind == JsonValueKind.String)
                    .Select(m => m.GetString()!));
            }
            if (root.TryGetProperty("errors", out var errors))
            {
                if (errors.ValueKind == JsonValueKind.Object)
                {
                    messages.AddRange(errors.EnumerateObject().Select(e => $"{e.Name}: {e.Value}"));
                }
                else if (errors.ValueKind == JsonValueKind.Array)
                {
                    messages.AddRange(errors.EnumerateArray().Select(ReadV2Error).OfType<string>());
                }
            }
            if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
            {
                messages.Add(message.GetString()!);
            }

            var text = string.Join("; ", messages.Where(m => m.Length > 0));
            return text.Length > 0 ? text : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ReadV2Error(JsonElement error)
    {
        if (error.ValueKind != JsonValueKind.Object)
        {
            return null;
        }
        var title = error.TryGetProperty("title", out var t) ? t.GetString() : null;
        var detail = error.TryGetProperty("detail", out var d) ? d.GetString() : null;
        return string.IsNullOrEmpty(detail) ? title : $"{title}: {detail}";
    }
}

/// <summary>
/// The site, the account or the API token is missing from .env
/// </summary>
public sealed class AtlassianNotConfiguredException(IReadOnlyList<string> missingSettings)
    : Exception($"Set {string.Join(", ", missingSettings)} in .env")
{
    public IReadOnlyList<string> MissingSettings { get; } = missingSettings;
}
