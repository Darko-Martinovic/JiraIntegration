using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using JiraIntegration.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace JiraIntegration.Tests;

public class TicketsApiTests : IDisposable
{
    private readonly AppFactory _app = new();
    private readonly HttpClient _client;

    public TicketsApiTests() => _client = _app.CreateClient();

    public void Dispose() => _app.Dispose();

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    private const string SearchAnswer = """
        {"issues":[{"id":"10001","key":"KAN-7","fields":{
          "summary":"Login fails","status":{"id":"3","name":"In Progress"},"priority":{"id":"2","name":"High"},
          "issuetype":{"id":"10004","name":"Bug","subtask":false},
          "assignee":{"accountId":"acc-1","displayName":"Ana Lee","active":true},
          "created":"2026-10-01T09:00:00.000+0000","updated":"2026-10-02T10:30:00.000+0000"}}],
         "nextPageToken":"page-2","isLast":false}
        """;

    [Fact]
    public async Task The_list_opens_on_the_configured_project_through_the_enhanced_search()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/search/jql", SearchAnswer);

        var page = await _client.GetFromJsonAsync<TicketPage>("/api/tickets", Cancel);

        var sent = _app.Atlassian.Single(HttpMethod.Get, "/rest/api/3/search/jql");
        Assert.Equal("project = \"KAN\" ORDER BY created DESC", sent.QueryValue("jql"));
        Assert.Equal("20", sent.QueryValue("maxResults"));
        Assert.Contains("summary", sent.QueryValue("fields"));
        Assert.Null(sent.QueryValue("startAt"));

        var ticket = Assert.Single(page!.Tickets);
        Assert.Equal(
            new TicketView("KAN-7", "Login fails", null, "In Progress", "High", "Bug", "Ana Lee", "acc-1",
                new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero),
                "https://example.atlassian.net/browse/KAN-7"),
            ticket);
        Assert.Equal("page-2", page.NextPageToken);
    }

    [Fact]
    public async Task The_next_page_is_asked_for_with_the_token_and_the_last_page_has_none()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/search/jql", """{"issues":[],"isLast":true}""");

        var page = await _client.GetFromJsonAsync<TicketPage>(
            "/api/tickets?jql=assignee%20%3D%20currentUser()&nextPageToken=page-2&maxResults=5", Cancel);

        var sent = _app.Atlassian.Single(HttpMethod.Get, "/rest/api/3/search/jql");
        Assert.Equal("assignee = currentUser()", sent.QueryValue("jql"));
        Assert.Equal("page-2", sent.QueryValue("nextPageToken"));
        Assert.Equal("5", sent.QueryValue("maxResults"));
        Assert.Empty(page!.Tickets);
        Assert.Null(page.NextPageToken);
    }

    [Fact]
    public async Task Every_call_signs_in_with_the_email_and_the_api_token()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/search/jql", """{"issues":[],"isLast":true}""");

        await _client.GetAsync("/api/tickets", Cancel);

        var expected = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("dev@example.com:secret-token"));
        Assert.Equal(expected, Assert.Single(_app.Atlassian.Requests).Authorization);
    }

    [Fact]
    public async Task A_ticket_is_read_with_its_description_as_text()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/issue/KAN-7", """
            {"id":"10001","key":"KAN-7","fields":{"summary":"Login fails",
             "description":{"type":"doc","version":1,"content":[{"type":"paragraph","content":[{"type":"text","text":"After a password reset"}]}]},
             "status":{"id":"1","name":"To Do"},"issuetype":{"id":"10001","name":"Task","subtask":false}}}
            """);

        var ticket = await _client.GetFromJsonAsync<TicketView>("/api/tickets/KAN-7", Cancel);

        Assert.Equal("After a password reset", ticket!.Description);
        Assert.Equal("To Do", ticket.Status);
        Assert.Null(ticket.Assignee);
    }

    [Fact]
    public async Task A_new_ticket_names_its_issue_type_and_priority_and_sends_the_description_as_adf()
    {
        _app.Atlassian.On(HttpMethod.Post, "/rest/api/3/issue", """{"id":"10010","key":"KAN-8","self":"x"}""", HttpStatusCode.Created);

        var response = await _client.PostAsJsonAsync("/api/tickets", new
        {
            summary = "Export to CSV",
            description = "As a user\nI want a CSV",
            issueType = "Story",
            priority = "Medium",
            assigneeAccountId = "acc-1",
        }, Cancel);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/tickets/KAN-8", response.Headers.Location!.AbsolutePath);
        Assert.Equal(
            new CreatedTicket("KAN-8", "https://example.atlassian.net/browse/KAN-8"),
            await response.Content.ReadFromJsonAsync<CreatedTicket>(Cancel));

        var fields = _app.Atlassian.Single(HttpMethod.Post, "/rest/api/3/issue").Json["fields"]!;
        Assert.Equal("KAN", (string?)fields["project"]!["key"]);
        Assert.Equal("Story", (string?)fields["issuetype"]!["name"]);
        Assert.Equal("Medium", (string?)fields["priority"]!["name"]);
        Assert.Equal("acc-1", (string?)fields["assignee"]!["accountId"]);
        Assert.Equal("doc", (string?)fields["description"]!["type"]);
    }

    [Fact]
    public async Task A_new_ticket_without_a_summary_is_refused_before_jira_is_called()
    {
        var response = await _client.PostAsJsonAsync("/api/tickets", new { description = "no summary" }, Cancel);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_app.Atlassian.Requests);
    }

    [Fact]
    public async Task An_update_sends_only_the_fields_given_and_an_empty_assignee_unassigns()
    {
        _app.Atlassian.On(HttpMethod.Put, "/rest/api/3/issue/KAN-7", "", HttpStatusCode.NoContent);

        var response = await _client.PutAsJsonAsync("/api/tickets/KAN-7", new { summary = "Renamed", assigneeAccountId = "" }, Cancel);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fields = _app.Atlassian.Single(HttpMethod.Put, "/rest/api/3/issue/KAN-7").Json["fields"]!.AsObject();
        Assert.Equal(["summary", "assignee"], fields.Select(f => f.Key));
        Assert.Null(fields["assignee"]);
    }

    [Fact]
    public async Task A_transition_carries_its_comment()
    {
        _app.Atlassian.On(HttpMethod.Post, "/rest/api/3/issue/KAN-7/transitions", "", HttpStatusCode.NoContent);

        var response = await _client.PostAsJsonAsync("/api/tickets/KAN-7/transitions", new { transitionId = "31", comment = "Done" }, Cancel);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var body = _app.Atlassian.Single(HttpMethod.Post, "/rest/api/3/issue/KAN-7/transitions").Json;
        Assert.Equal("31", (string?)body["transition"]!["id"]);
        Assert.Equal("Done", (string?)body["update"]!["comment"]![0]!["add"]!["body"]!["content"]![0]!["content"]![0]!["text"]);
    }

    [Fact]
    public async Task Transitions_are_listed_with_the_status_they_lead_to()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/issue/KAN-7/transitions",
            """{"transitions":[{"id":"31","name":"Done","to":{"id":"10002","name":"Done"},"hasScreen":false}]}""");

        var transitions = await _client.GetFromJsonAsync<List<TransitionView>>("/api/tickets/KAN-7/transitions", Cancel);

        Assert.Equal([new TransitionView("31", "Done", "Done")], transitions);
    }

    [Fact]
    public async Task Comments_are_read_as_text_and_added_as_adf()
    {
        _app.Atlassian
            .On(HttpMethod.Get, "/rest/api/3/issue/KAN-7/comment", """
                {"comments":[{"id":"100","author":{"accountId":"acc-1","displayName":"Ana Lee","active":true},
                 "body":{"type":"doc","version":1,"content":[{"type":"paragraph","content":[{"type":"text","text":"Looks good"}]}]},
                 "created":"2026-10-03T08:00:00.000+0000"}],"total":1}
                """)
            .On(HttpMethod.Post, "/rest/api/3/issue/KAN-7/comment", """
                {"id":"101","author":{"accountId":"acc-2","displayName":"Dev","active":true},
                 "body":{"type":"doc","version":1,"content":[{"type":"paragraph","content":[{"type":"text","text":"Thanks"}]}]},
                 "created":"2026-10-03T09:00:00.000+0000"}
                """, HttpStatusCode.Created);

        var comments = await _client.GetFromJsonAsync<List<CommentView>>("/api/tickets/KAN-7/comments", Cancel);
        var added = await _client.PostAsJsonAsync("/api/tickets/KAN-7/comments", new { body = "Thanks" }, Cancel);

        Assert.Equal("Looks good", Assert.Single(comments!).Body);
        Assert.Equal("Ana Lee", comments![0].Author);
        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        Assert.Equal("101", (await added.Content.ReadFromJsonAsync<CommentView>(Cancel))!.Id);
        var sent = _app.Atlassian.Single(HttpMethod.Post, "/rest/api/3/issue/KAN-7/comment").Json;
        Assert.Equal("doc", (string?)sent["body"]!["type"]);
    }

    [Fact]
    public async Task Deleting_a_ticket_and_a_comment_reaches_jira()
    {
        _app.Atlassian
            .On(HttpMethod.Delete, "/rest/api/3/issue/KAN-7", "", HttpStatusCode.NoContent)
            .On(HttpMethod.Delete, "/rest/api/3/issue/KAN-7/comment/100", "", HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync("/api/tickets/KAN-7/comments/100", Cancel)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync("/api/tickets/KAN-7", Cancel)).StatusCode);
        Assert.Equal(2, _app.Atlassian.Requests.Count);
    }

    [Theory]
    [InlineData("/api/tickets/not-a-key")]
    [InlineData("/api/tickets/KAN-7%2F..%2F..%2Fmyself")]
    [InlineData("/api/tickets/KAN-7/comments/abc")]
    public async Task Keys_and_ids_of_the_wrong_shape_never_reach_jira(string url)
    {
        var response = await _client.DeleteAsync(url, Cancel);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_app.Atlassian.Requests);
    }

    [Fact]
    public async Task A_ticket_jira_cannot_find_is_a_404_with_jiras_message()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/issue/KAN-99",
            """{"errorMessages":["Issue does not exist or you do not have permission to see it."],"errors":{}}""",
            HttpStatusCode.NotFound);

        var response = await _client.GetAsync("/api/tickets/KAN-99", Cancel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Cancel);
        Assert.Equal("Issue does not exist or you do not have permission to see it.", problem!.Detail);
    }

    [Fact]
    public async Task Refused_credentials_are_a_bad_gateway_not_a_401_for_the_browser()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/search/jql", "", HttpStatusCode.Unauthorized);

        var response = await _client.GetAsync("/api/tickets", Cancel);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Cancel);
        Assert.Equal("Atlassian refused the credentials or the permission", problem!.Title);
    }

    [Fact]
    public async Task Without_settings_every_call_is_a_503_naming_what_is_missing()
    {
        using var app = new AppFactory(configured: false);
        var client = app.CreateClient();

        var response = await client.GetAsync("/api/tickets?jql=project%20%3D%20KAN", Cancel);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Cancel);
        Assert.Equal("Set JIRA_BASE_URL, JIRA_USER_EMAIL, JIRA_API_TOKEN in .env", problem!.Detail);
        Assert.Empty(app.Atlassian.Requests);
    }
}
