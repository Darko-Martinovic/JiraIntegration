namespace JiraIntegration.Models.Requests;

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
