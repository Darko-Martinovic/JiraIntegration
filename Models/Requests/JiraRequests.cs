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

/// <summary>
/// Request model for transitioning an issue
/// </summary>
public class TransitionIssueRequest
{
    /// <summary>
    /// Transition ID to execute
    /// </summary>
    public string TransitionId { get; set; } = string.Empty;

    /// <summary>
    /// Optional comment to add during transition
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Converts to Jira API format
    /// </summary>
    public object ToJiraFormat()
    {
        var request = new Dictionary<string, object>
        {
            ["transition"] = new { id = TransitionId }
        };

        if (!string.IsNullOrWhiteSpace(Comment))
        {
            request["update"] = new
            {
                comment = new[]
                {
                    new
                    {
                        add = new
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
                                                text = Comment
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        return request;
    }
}
