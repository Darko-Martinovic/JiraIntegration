namespace JiraIntegration.Models.Requests;

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
