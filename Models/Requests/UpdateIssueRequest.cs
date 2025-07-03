namespace JiraIntegration.Models.Requests;

/// <summary>
/// Request model for updating a Jira issue
/// </summary>
public class UpdateIssueRequest
{
    /// <summary>
    /// Updated summary/title
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Updated description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Updated priority name
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Updated assignee account ID
    /// </summary>
    public string? AssigneeId { get; set; }

    /// <summary>
    /// Converts to Jira API format
    /// </summary>
    public object ToJiraFormat()
    {
        var fields = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(Summary))
        {
            fields["summary"] = Summary;
        }

        if (!string.IsNullOrWhiteSpace(Description))
        {
            fields["description"] = new
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
            };
        }

        if (!string.IsNullOrWhiteSpace(Priority))
        {
            fields["priority"] = new { name = Priority };
        }

        if (!string.IsNullOrWhiteSpace(AssigneeId))
        {
            fields["assignee"] = new { id = AssigneeId };
        }

        return new { fields };
    }
}
