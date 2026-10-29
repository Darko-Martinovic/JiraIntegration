using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JiraIntegration.Configuration;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Atlassian;

/// <summary>
/// What every Jira and Confluence service shares: JSON in and out, and Atlassian's
/// error answers turned into <see cref="AtlassianApiException"/>
/// </summary>
public abstract class AtlassianHttpService(HttpClient http, IOptions<AtlassianSettings> settings)
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new AtlassianDateConverter() },
    };

    protected AtlassianSettings Settings { get; } = settings.Value;

    protected async Task<T> GetJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Get, path, null, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    protected async Task<T> PostJsonAsync<T>(string path, object body, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Post, path, body, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    /// <summary>
    /// For the calls Jira answers with 204 No Content, such as a transition
    /// </summary>
    protected async Task PostJsonAsync(string path, object body, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Post, path, body, cancellationToken);
    }

    protected async Task PutJsonAsync(string path, object body, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Put, path, body, cancellationToken);
    }

    protected async Task DeleteResourceAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Delete, path, null, cancellationToken);
    }

    /// <summary>
    /// A path segment or query value taken from a caller, escaped so it can't change the URL
    /// </summary>
    protected static string Escape(string value) => Uri.EscapeDataString(value);

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        if (!Settings.IsConfigured)
        {
            throw new AtlassianNotConfiguredException(Settings.MissingSettings());
        }

        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: Json);
        }

        var response = await http.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        using (response)
        {
            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            throw AtlassianApiException.From(response.StatusCode, text);
        }
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken) =>
        await response.Content.ReadFromJsonAsync<T>(Json, cancellationToken)
        ?? throw new JsonException($"Atlassian sent an empty answer where a {typeof(T).Name} was expected");
}

/// <summary>
/// Jira writes dates as 2025-10-21T15:23:48.000+0200, an offset without the colon
/// System.Text.Json expects; Confluence writes ISO 8601. Both parse here.
/// </summary>
internal sealed class AtlassianDateConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateTimeOffset.Parse(reader.GetString()!, CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
