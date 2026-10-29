using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using JiraIntegration.Models.Jira;
using JiraIntegration.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Services.Implementations;

public sealed class JiraCommentService(HttpClient http, IOptions<AtlassianSettings> settings)
    : AtlassianHttpService(http, settings), IJiraCommentService
{
    public async Task<IReadOnlyList<JiraComment>> GetCommentsAsync(string key, CancellationToken cancellationToken = default)
    {
        var answer = await GetJsonAsync<JiraComments>($"rest/api/3/issue/{Escape(key)}/comment?orderBy=created", cancellationToken);
        return answer.Comments;
    }

    public Task<JiraComment> AddCommentAsync(string key, string body, CancellationToken cancellationToken = default) =>
        PostJsonAsync<JiraComment>($"rest/api/3/issue/{Escape(key)}/comment", new { body = Adf.FromText(body) }, cancellationToken);

    public Task DeleteCommentAsync(string key, string commentId, CancellationToken cancellationToken = default) =>
        DeleteResourceAsync($"rest/api/3/issue/{Escape(key)}/comment/{Escape(commentId)}", cancellationToken);
}
