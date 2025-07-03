namespace JiraIntegration.Models.Requests;

/// <summary>
/// Request for building JQL queries visually
/// </summary>
public class JqlBuilderRequest
{
    public string? ProjectKey { get; set; }
    public string? AssigneeId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? IssueType { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }
    public List<string> Labels { get; set; } = new();
    public string? TextSearch { get; set; }
}
