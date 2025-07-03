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

// === Advanced Features Request Models ===

/// <summary>
/// Request for updating ticket fields
/// </summary>
public class UpdateFieldsRequest
{
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? AssigneeId { get; set; }
    public string? PriorityId { get; set; }
    public DateTime? DueDate { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();

    public object ToJiraFormat()
    {
        var fields = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(Summary))
            fields["summary"] = Summary;

        if (!string.IsNullOrWhiteSpace(Description))
            fields["description"] = Description;

        if (!string.IsNullOrWhiteSpace(AssigneeId))
            fields["assignee"] = new { id = AssigneeId };

        if (!string.IsNullOrWhiteSpace(PriorityId))
            fields["priority"] = new { id = PriorityId };

        if (DueDate.HasValue)
            fields["duedate"] = DueDate.Value.ToString("yyyy-MM-dd");

        foreach (var customField in CustomFields)
        {
            fields[customField.Key] = customField.Value;
        }

        return new { fields };
    }
}

/// <summary>
/// Request for adding a comment
/// </summary>
public class AddCommentRequest
{
    public string Body { get; set; } = string.Empty;
    public bool NotifyUsers { get; set; } = true;
    public List<string> MentionedUsers { get; set; } = new();

    public object ToJiraFormat()
    {
        var bodyContent = Body;

        // Add mentions if any
        foreach (var user in MentionedUsers)
        {
            if (!bodyContent.Contains($"@{user}"))
            {
                bodyContent = $"@{user} {bodyContent}";
            }
        }

        return new
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
                                text = bodyContent
                            }
                        }
                    }
                }
            }
        };
    }
}

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

/// <summary>
/// Request for saving a search
/// </summary>
public class SaveSearchRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string JqlQuery { get; set; } = string.Empty;
    public bool IsShared { get; set; } = false;
}
