using Newtonsoft.Json;

namespace JiraIntegration.Models.Requests;

/// <summary>
/// Request model for creating a new Jira issue
/// </summary>
public class CreateIssueRequest
{
    /// <summary>
    /// Project key where the issue will be created
    /// </summary>
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Summary/title of the issue
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Description of the issue
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Issue type ID (e.g., "10001" for Task)
    /// </summary>
    public string IssueTypeId { get; set; } = string.Empty;

    /// <summary>
    /// Priority name (e.g., "High", "Medium", "Low")
    /// </summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Optional assignee account ID
    /// </summary>
    public string? AssigneeId { get; set; }

    /// <summary>
    /// Converts to Jira API format
    /// </summary>
    public object ToJiraFormat()
    {
        var fields = new Dictionary<string, object>
        {
            ["project"] = new { key = ProjectKey },
            ["summary"] = Summary,
            ["description"] = new
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
                                text = Description
                            }
                        }
                    }
                }
            },
            ["issuetype"] = new { id = IssueTypeId },
            ["priority"] = new { name = Priority }
        };

        if (!string.IsNullOrWhiteSpace(AssigneeId))
        {
            fields["assignee"] = new { id = AssigneeId };
        }

        return new { fields };
    }
}
