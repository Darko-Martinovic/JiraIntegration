using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace JiraIntegration.Tests;

/// <summary>
/// Stands in for the Atlassian site: answers the requests it was told about and
/// records every request the app sends
/// </summary>
public sealed class FakeAtlassian : HttpMessageHandler
{
    private readonly List<(HttpMethod Method, string Path, HttpStatusCode Status, string Body)> _answers = [];

    public List<SentRequest> Requests { get; } = [];

    /// <summary>
    /// Answers requests whose path (without the query) is the one given
    /// </summary>
    public FakeAtlassian On(HttpMethod method, string path, string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        _answers.Add((method, path, status, body));
        return this;
    }

    public SentRequest Single(HttpMethod method, string path) =>
        Assert.Single(Requests, r => r.Method == method && r.Path == path);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var sent = new SentRequest(
            request.Method,
            request.RequestUri!.AbsolutePath,
            request.RequestUri.Query,
            body,
            request.Headers.Authorization?.ToString());
        lock (Requests)
        {
            Requests.Add(sent);
        }

        var answer = _answers.LastOrDefault(a => a.Method == request.Method && a.Path == sent.Path);
        if (answer.Path is null)
        {
            return Json(HttpStatusCode.NotFound, """{"errorMessages":["The fake site has no answer for this request"]}""");
        }
        return answer.Status == HttpStatusCode.NoContent
            ? new HttpResponseMessage(HttpStatusCode.NoContent)
            : Json(answer.Status, answer.Body);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
}

public sealed record SentRequest(HttpMethod Method, string Path, string Query, string? Body, string? Authorization)
{
    public JsonNode Json => JsonNode.Parse(Body!)!;

    /// <summary>A query value, decoded</summary>
    public string? QueryValue(string name) =>
        Query.TrimStart('?').Split('&')
            .Select(pair => pair.Split('=', 2))
            .Where(pair => pair[0] == name)
            .Select(pair => Uri.UnescapeDataString(pair[1]))
            .FirstOrDefault();
}
