namespace JiraIntegration.Models.Requests;

/// <summary>
/// Request for advanced search with filters
/// </summary>
public class AdvancedSearchRequest
{
    public string JqlQuery { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 50;
    public int StartAt { get; set; } = 0;
    public List<string> Fields { get; set; } = new();
    public string? OrderBy { get; set; }
    public bool ValidateQuery { get; set; } = true;
}
