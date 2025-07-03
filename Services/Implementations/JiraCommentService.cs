using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Interfaces;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Implementation of Jira comment service
/// </summary>
public class JiraCommentService : BaseJiraHttpService, IJiraCommentService
{
    public JiraCommentService(
        HttpClient httpClient,
        IOptions<JiraSettings> settings,
        ILogger<JiraCommentService> logger)
        : base(httpClient, settings, logger)
    {
    }

    /// <summary>
    /// Adds a comment to a ticket
    /// </summary>
    public async Task<JiraComment?> AddCommentAsync(string ticketKey, AddCommentRequest request)
    {
        try
        {
            _logger.LogInformation("Adding comment to ticket: {TicketKey}", ticketKey);

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                throw new ArgumentException("Ticket key cannot be empty", nameof(ticketKey));
            }

            var payload = request.ToJiraFormat();

            // Use PostAsyncNoResponse since we don't need the complex response structure for adding comments
            var success = await PostAsyncNoResponse($"/rest/api/3/issue/{ticketKey}/comment", payload);

            if (success)
            {
                _logger.LogInformation("Successfully added comment to ticket: {TicketKey}", ticketKey);
                // Return a basic comment object to indicate success
                return new JiraComment
                {
                    Id = "created",
                    Body = request.Body,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                };
            }
            else
            {
                _logger.LogWarning("Failed to add comment to ticket: {TicketKey}", ticketKey);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to ticket: {TicketKey}", ticketKey);
            throw;
        }
    }

    /// <summary>
    /// Gets all comments for a ticket
    /// </summary>
    public async Task<List<JiraComment>> GetCommentsAsync(string ticketKey)
    {
        try
        {
            _logger.LogInformation("Getting comments for ticket: {TicketKey}", ticketKey);

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                throw new ArgumentException("Ticket key cannot be empty", nameof(ticketKey));
            }

            var response = await GetAsync<dynamic>($"/rest/api/3/issue/{ticketKey}/comment");

            if (response?.comments != null)
            {
                var comments = new List<JiraComment>();
                foreach (var comment in response.comments)
                {
                    comments.Add(new JiraComment
                    {
                        Id = comment.id,
                        Body = comment.body?.ToString() ?? string.Empty,
                        Author = new JiraUser
                        {
                            DisplayName = comment.author?.displayName ?? "Unknown",
                            EmailAddress = comment.author?.emailAddress ?? string.Empty
                        },
                        Created = comment.created ?? DateTime.MinValue,
                        Updated = comment.updated ?? DateTime.MinValue
                    });
                }

                _logger.LogDebug("Found {Count} comments for ticket: {TicketKey}", comments.Count, ticketKey);
                return comments;
            }

            _logger.LogWarning("No comments found for ticket: {TicketKey}", ticketKey);
            return new List<JiraComment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for ticket: {TicketKey}", ticketKey);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing comment
    /// </summary>
    public async Task<bool> UpdateCommentAsync(string ticketKey, string commentId, string newText)
    {
        try
        {
            _logger.LogInformation("Updating comment {CommentId} for ticket: {TicketKey}", commentId, ticketKey);

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                throw new ArgumentException("Ticket key cannot be empty", nameof(ticketKey));
            }

            if (string.IsNullOrWhiteSpace(commentId))
            {
                throw new ArgumentException("Comment ID cannot be empty", nameof(commentId));
            }

            var payload = new
            {
                body = new
                {
                    type = "doc",
                    version = 1,
                    content = new[]
                    {
                        new
                        {
                            type = "paragraph",
                            content = new[]
                            {
                                new
                                {
                                    type = "text",
                                    text = newText
                                }
                            }
                        }
                    }
                }
            };

            var success = await PutAsync($"/rest/api/3/issue/{ticketKey}/comment/{commentId}", payload);

            if (success)
            {
                _logger.LogInformation("Successfully updated comment {CommentId} for ticket: {TicketKey}", commentId, ticketKey);
            }
            else
            {
                _logger.LogWarning("Failed to update comment {CommentId} for ticket: {TicketKey}", commentId, ticketKey);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId} for ticket: {TicketKey}", commentId, ticketKey);
            throw;
        }
    }

    /// <summary>
    /// Deletes a comment
    /// </summary>
    public async Task<bool> DeleteCommentAsync(string ticketKey, string commentId)
    {
        try
        {
            _logger.LogInformation("Deleting comment {CommentId} for ticket: {TicketKey}", commentId, ticketKey);

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                throw new ArgumentException("Ticket key cannot be empty", nameof(ticketKey));
            }

            if (string.IsNullOrWhiteSpace(commentId))
            {
                throw new ArgumentException("Comment ID cannot be empty", nameof(commentId));
            }

            var response = await _httpClient.DeleteAsync($"/rest/api/3/issue/{ticketKey}/comment/{commentId}");
            var success = response.IsSuccessStatusCode;

            if (success)
            {
                _logger.LogInformation("Successfully deleted comment {CommentId} for ticket: {TicketKey}", commentId, ticketKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete comment {CommentId} for ticket: {TicketKey}", commentId, ticketKey);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId} for ticket: {TicketKey}", commentId, ticketKey);
            throw;
        }
    }

    /// <summary>
    /// Gets predefined comment templates
    /// </summary>
    public List<CommentTemplate> GetCommentTemplates()
    {
        return new List<CommentTemplate>
        {
            new() { Name = "Testing Complete", Template = "✅ Testing completed successfully. All test cases passed.", Category = "QA" },
            new() { Name = "Code Review Done", Template = "👀 Code review completed. Changes look good to merge.", Category = "Development" },
            new() { Name = "Ready for Deployment", Template = "🚀 Feature is ready for deployment to production.", Category = "DevOps" },
            new() { Name = "Needs More Info", Template = "ℹ️ Need additional information to proceed. Please provide more details.", Category = "General" },
            new() { Name = "Blocked", Template = "🚫 This ticket is blocked. Waiting for dependencies to be resolved.", Category = "General" },
            new() { Name = "In Progress", Template = "🔄 Started working on this ticket. Will update progress regularly.", Category = "General" },
            new() { Name = "Ready for Review", Template = "👁️ Work completed. Ready for review and feedback.", Category = "General" },
            new() { Name = "Approved", Template = "✅ Approved. Great work!", Category = "Management" }
        };
    }
}
