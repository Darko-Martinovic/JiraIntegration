using System.Text.Json.Serialization;

namespace JiraIntegration.Models.Dto;

/// <summary>
/// DTO for creating ticket requests from frontend
/// </summary>
public class CreateTicketRequestDto
{
    [JsonPropertyName("projectKey")]
    public string ProjectKey { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("issueType")]
    public string IssueType { get; set; } = "Task";

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "Medium";

    [JsonPropertyName("assignee")]
    public string Assignee { get; set; } = string.Empty;
}