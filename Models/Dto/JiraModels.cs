using Newtonsoft.Json;

namespace JiraIntegration.Models.Dto;

/// <summary>
/// Custom converter to handle JIRA description field which can be either string or ADF object
/// </summary>
public class JiraDescriptionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(string);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            return reader.Value?.ToString() ?? string.Empty;
        }
        else if (reader.TokenType == JsonToken.StartObject)
        {
            // Parse ADF object and extract text content
            var adfDocument = serializer.Deserialize<JiraAdfDocument>(reader);
            return ExtractTextFromAdf(adfDocument);
        }
        else if (reader.TokenType == JsonToken.Null)
        {
            return string.Empty;
        }

        return string.Empty;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
    }

    private static string ExtractTextFromAdf(JiraAdfDocument? document)
    {
        if (document?.Content == null)
            return string.Empty;

        var text = new System.Text.StringBuilder();

        foreach (var content in document.Content)
        {
            ExtractTextFromContent(content, text);
        }

        return text.ToString().Trim();
    }

    private static void ExtractTextFromContent(JiraAdfContent content, System.Text.StringBuilder text)
    {
        if (content.Type == "text" && !string.IsNullOrEmpty(content.Text))
        {
            text.Append(content.Text);
        }
        else if (content.Content != null)
        {
            foreach (var subContent in content.Content)
            {
                ExtractTextFromContent(subContent, text);
            }

            // Add line breaks for paragraphs
            if (content.Type == "paragraph")
            {
                text.AppendLine();
            }
        }
    }
}

/// <summary>
/// Represents JIRA Atlassian Document Format (ADF) document
/// </summary>
public class JiraAdfDocument
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("version")]
    public int Version { get; set; }

    [JsonProperty("content")]
    public List<JiraAdfContent>? Content { get; set; }
}

/// <summary>
/// Represents content within an ADF document
/// </summary>
public class JiraAdfContent
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("text")]
    public string? Text { get; set; }

    [JsonProperty("content")]
    public List<JiraAdfContent>? Content { get; set; }

    [JsonProperty("attrs")]
    public Dictionary<string, object>? Attrs { get; set; }
}

/// <summary>
/// Represents a Jira user
/// </summary>
public class JiraUser
{
    [JsonProperty("self")]
    public string Self { get; set; } = string.Empty;

    [JsonProperty("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonProperty("emailAddress")]
    public string EmailAddress { get; set; } = string.Empty;

    [JsonProperty("active")]
    public bool Active { get; set; }
}

/// <summary>
/// Represents a Jira project
/// </summary>
public class JiraProject
{
    [JsonProperty("self")]
    public string Self { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("projectTypeKey")]
    public string ProjectTypeKey { get; set; } = string.Empty;

    [JsonProperty("simplified")]
    public bool Simplified { get; set; }
}

/// <summary>
/// Represents a Jira issue type
/// </summary>
public class JiraIssueType
{
    [JsonProperty("self")]
    public string Self { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("subtask")]
    public bool Subtask { get; set; }

    [JsonProperty("iconUrl")]
    public string IconUrl { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Jira status
/// </summary>
public class JiraStatus
{
    [JsonProperty("self")]
    public string Self { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("statusCategory")]
    public JiraStatusCategory StatusCategory { get; set; } = new();
}

/// <summary>
/// Represents a Jira status category
/// </summary>
public class JiraStatusCategory
{
    [JsonProperty("self")]
    public string Self { get; set; } = string.Empty;

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("colorName")]
    public string ColorName { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Jira priority
/// </summary>
public class JiraPriority
{
    [JsonProperty("self")]
    public string Self { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("iconUrl")]
    public string IconUrl { get; set; } = string.Empty;
}

/// <summary>
/// Represents Jira issue fields
/// </summary>
public class JiraIssueFields
{
    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonProperty("description")]
    [JsonConverter(typeof(JiraDescriptionConverter))]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("status")]
    public JiraStatus Status { get; set; } = new();

    [JsonProperty("assignee")]
    public JiraUser? Assignee { get; set; }

    [JsonProperty("reporter")]
    public JiraUser? Reporter { get; set; }

    [JsonProperty("priority")]
    public JiraPriority Priority { get; set; } = new();

    [JsonProperty("issuetype")]
    public JiraIssueType IssueType { get; set; } = new();

    [JsonProperty("project")]
    public JiraProject Project { get; set; } = new();

    [JsonProperty("created")]
    public DateTime Created { get; set; }

    [JsonProperty("updated")]
    public DateTime Updated { get; set; }

    [JsonProperty("resolutiondate")]
    public DateTime? ResolutionDate { get; set; }
}

/// <summary>
/// Represents a Jira issue
/// </summary>
public class JiraIssue
{
    [JsonProperty("expand")]
    public string Expand { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("self")]
    public string Self { get; set; } = string.Empty;

    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("fields")]
    public JiraIssueFields Fields { get; set; } = new();
}

/// <summary>
/// Represents a Jira transition
/// </summary>
public class JiraTransition
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("to")]
    public JiraStatus To { get; set; } = new();

    [JsonProperty("hasScreen")]
    public bool HasScreen { get; set; }

    [JsonProperty("isGlobal")]
    public bool IsGlobal { get; set; }

    [JsonProperty("isInitial")]
    public bool IsInitial { get; set; }

    [JsonProperty("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonProperty("isConditional")]
    public bool IsConditional { get; set; }
}

/// <summary>
/// Response for transition requests
/// </summary>
public class JiraTransitionsResponse
{
    [JsonProperty("expand")]
    public string Expand { get; set; } = string.Empty;

    [JsonProperty("transitions")]
    public List<JiraTransition> Transitions { get; set; } = new();
}

/// <summary>
/// Search response from Jira
/// </summary>
public class JiraSearchResponse
{
    [JsonProperty("expand")]
    public string Expand { get; set; } = string.Empty;

    [JsonProperty("startAt")]
    public int StartAt { get; set; }

    [JsonProperty("maxResults")]
    public int MaxResults { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("issues")]
    public List<JiraIssue> Issues { get; set; } = new();
}
