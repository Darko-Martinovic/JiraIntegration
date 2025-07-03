using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;

namespace JiraIntegration.Services.Interfaces;

/// <summary>
/// Service for managing ticket comments
/// </summary>
public interface IJiraCommentService
{
    /// <summary>
    /// Adds a comment to a ticket
    /// </summary>
    Task<JiraComment?> AddCommentAsync(string ticketKey, AddCommentRequest request);

    /// <summary>
    /// Gets all comments for a ticket
    /// </summary>
    Task<List<JiraComment>> GetCommentsAsync(string ticketKey);

    /// <summary>
    /// Updates an existing comment
    /// </summary>
    Task<bool> UpdateCommentAsync(string ticketKey, string commentId, string newText);

    /// <summary>
    /// Deletes a comment
    /// </summary>
    Task<bool> DeleteCommentAsync(string ticketKey, string commentId);

    /// <summary>
    /// Gets predefined comment templates
    /// </summary>
    List<CommentTemplate> GetCommentTemplates();
}
