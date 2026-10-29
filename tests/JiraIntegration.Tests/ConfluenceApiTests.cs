using System.Net;
using System.Net.Http.Json;
using JiraIntegration.Models.Api;

namespace JiraIntegration.Tests;

public class ConfluenceApiTests : IDisposable
{
    private readonly AppFactory _app = new();
    private readonly HttpClient _client;

    public ConfluenceApiTests() => _client = _app.CreateClient();

    public void Dispose() => _app.Dispose();

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    private const string DevSpace = """
        {"results":[{"id":"98304","key":"DEV","name":"Development","type":"global","status":"current",
          "description":{"plain":{"representation":"plain","value":"Team docs"}},
          "_links":{"webui":"/spaces/DEV"}}],"_links":{}}
        """;

    [Fact]
    public async Task Spaces_come_from_v2_with_links_into_the_site()
    {
        _app.Atlassian.On(HttpMethod.Get, "/wiki/api/v2/spaces", DevSpace);

        var spaces = await _client.GetFromJsonAsync<List<SpaceView>>("/api/confluence/spaces", Cancel);

        Assert.Equal([new SpaceView("98304", "DEV", "Development", "Team docs", "https://example.atlassian.net/wiki/spaces/DEV")], spaces);
        Assert.Equal("plain", _app.Atlassian.Requests[0].QueryValue("description-format"));
    }

    [Fact]
    public async Task A_spaces_pages_are_found_by_its_id_after_looking_up_the_key()
    {
        _app.Atlassian
            .On(HttpMethod.Get, "/wiki/api/v2/spaces", DevSpace)
            .On(HttpMethod.Get, "/wiki/api/v2/spaces/98304/pages", """
                {"results":[{"id":"557057","title":"Release notes","spaceId":"98304","status":"current",
                  "version":{"number":3},"_links":{"webui":"/spaces/DEV/pages/557057/Release+notes"}}]}
                """);

        var pages = await _client.GetFromJsonAsync<List<PageView>>("/api/confluence/spaces/DEV/pages", Cancel);

        Assert.Equal("DEV", _app.Atlassian.Requests[0].QueryValue("keys"));
        Assert.Equal(
            [new PageView("557057", "Release notes", "98304", 3, "https://example.atlassian.net/wiki/spaces/DEV/pages/557057/Release+notes", null)],
            pages);
    }

    [Fact]
    public async Task An_unknown_space_is_a_404()
    {
        _app.Atlassian.On(HttpMethod.Get, "/wiki/api/v2/spaces", """{"results":[],"_links":{}}""");

        var response = await _client.GetAsync("/api/confluence/spaces/NOPE/pages", Cancel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Single(_app.Atlassian.Requests);
    }

    [Fact]
    public async Task A_page_is_read_with_its_storage_body()
    {
        _app.Atlassian.On(HttpMethod.Get, "/wiki/api/v2/pages/557057", """
            {"id":"557057","title":"Release notes","spaceId":"98304","status":"current","version":{"number":3},
             "body":{"storage":{"representation":"storage","value":"<p>Version 2.1</p>"}},"_links":{"webui":"/x"}}
            """);

        var page = await _client.GetFromJsonAsync<PageView>("/api/confluence/pages/557057", Cancel);

        Assert.Equal("<p>Version 2.1</p>", page!.Body);
        Assert.Equal("storage", _app.Atlassian.Requests[0].QueryValue("body-format"));
    }

    [Fact]
    public async Task A_new_page_goes_into_the_space_by_id_with_its_text_encoded()
    {
        _app.Atlassian
            .On(HttpMethod.Get, "/wiki/api/v2/spaces", DevSpace)
            .On(HttpMethod.Post, "/wiki/api/v2/pages", """
                {"id":"600001","title":"Notes","spaceId":"98304","status":"current","version":{"number":1},
                 "_links":{"webui":"/spaces/DEV/pages/600001/Notes"}}
                """);

        var response = await _client.PostAsJsonAsync("/api/confluence/pages",
            new { spaceKey = "DEV", title = "Notes", text = "Use <b> tags & such" }, Cancel);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var sent = _app.Atlassian.Single(HttpMethod.Post, "/wiki/api/v2/pages").Json;
        Assert.Equal("98304", (string?)sent["spaceId"]);
        Assert.Equal("current", (string?)sent["status"]);
        Assert.Equal("storage", (string?)sent["body"]!["representation"]);
        Assert.Equal("<p>Use &lt;b&gt; tags &amp; such</p>", (string?)sent["body"]!["value"]);
        Assert.False(sent.AsObject().ContainsKey("parentId"));
    }

    [Fact]
    public async Task The_search_quotes_the_text_inside_cql()
    {
        _app.Atlassian.On(HttpMethod.Get, "/wiki/rest/api/search", """
            {"results":[{"content":{"id":"557057","type":"page","title":"Release notes"},"title":"Release notes",
              "excerpt":"the @@@hl@@@release@@@endhl@@@ notes","url":"/spaces/DEV/pages/557057/Release+notes",
              "resultGlobalContainer":{"title":"Development","displayUrl":"/spaces/DEV"}}],"totalSize":1}
            """);

        var hits = await _client.GetFromJsonAsync<List<SearchHit>>(
            "/api/confluence/search?text=say%20%22release%22&spaceKey=DEV", Cancel);

        Assert.Equal("type = page AND text ~ \"say \\\"release\\\"\" AND space = \"DEV\"", _app.Atlassian.Requests[0].QueryValue("cql"));
        Assert.Equal(
            [new SearchHit("557057", "Release notes", "the release notes", "Development",
                "https://example.atlassian.net/wiki/spaces/DEV/pages/557057/Release+notes")],
            hits);
    }

    [Fact]
    public async Task A_search_without_text_is_refused()
    {
        var response = await _client.GetAsync("/api/confluence/search?text=", Cancel);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_app.Atlassian.Requests);
    }
}
