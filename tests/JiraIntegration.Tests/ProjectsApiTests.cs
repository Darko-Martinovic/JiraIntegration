using System.Net;
using System.Net.Http.Json;
using JiraIntegration.Models.Jira;

namespace JiraIntegration.Tests;

public class ProjectsApiTests : IDisposable
{
    private readonly AppFactory _app = new();
    private readonly HttpClient _client;

    public ProjectsApiTests() => _client = _app.CreateClient();

    public void Dispose() => _app.Dispose();

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Issue_types_come_from_the_project_without_subtasks()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/project/KAN", """
            {"id":"10000","key":"KAN","name":"Kanban","issueTypes":[
              {"id":"10001","name":"Task","subtask":false},
              {"id":"10002","name":"Subtask","subtask":true},
              {"id":"10004","name":"Bug","subtask":false}]}
            """);

        var types = await _client.GetFromJsonAsync<List<JiraIssueType>>("/api/projects/KAN/issue-types", Cancel);

        Assert.Equal(["Task", "Bug"], types!.Select(t => t.Name));
    }

    [Fact]
    public async Task Priorities_come_from_the_priority_search()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/priority/search",
            """{"values":[{"id":"1","name":"Highest"},{"id":"3","name":"Medium"}],"isLast":true}""");

        var priorities = await _client.GetFromJsonAsync<List<JiraPriority>>("/api/priorities", Cancel);

        Assert.Equal(["Highest", "Medium"], priorities!.Select(p => p.Name));
    }

    [Fact]
    public async Task Assignable_users_are_asked_for_by_project()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/user/assignable/search",
            """[{"accountId":"acc-1","displayName":"Ana Lee","active":true}]""");

        var users = await _client.GetFromJsonAsync<List<JiraUser>>("/api/projects/KAN/assignable-users", Cancel);

        Assert.Equal("acc-1", Assert.Single(users!).AccountId);
        Assert.Equal("KAN", _app.Atlassian.Requests[0].QueryValue("project"));
    }

    [Fact]
    public async Task A_user_search_passes_the_query_on()
    {
        _app.Atlassian.On(HttpMethod.Get, "/rest/api/3/user/search",
            """[{"accountId":"acc-1","displayName":"Ana Lee","emailAddress":"ana@example.com","active":true}]""");

        var users = await _client.GetFromJsonAsync<List<JiraUser>>("/api/users?query=ana%20l", Cancel);

        Assert.Equal("ana@example.com", Assert.Single(users!).EmailAddress);
        Assert.Equal("ana l", _app.Atlassian.Requests[0].QueryValue("query"));
    }

    [Fact]
    public async Task A_user_search_needs_a_query()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.GetAsync("/api/users", Cancel)).StatusCode);
    }
}
