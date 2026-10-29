using JiraIntegration.Models.Jira;

namespace JiraIntegration.Services.Interfaces;

public interface IJiraCommentService
{
    Task<IReadOnlyList<JiraComment>> GetCommentsAsync(string key, CancellationToken cancellationToken = default);

    Task<JiraComment> AddCommentAsync(string key, string body, CancellationToken cancellationToken = default);

    Task DeleteCommentAsync(string key, string commentId, CancellationToken cancellationToken = default);
}
