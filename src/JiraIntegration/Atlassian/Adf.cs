using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiraIntegration.Atlassian;

/// <summary>
/// Atlassian Document Format, the JSON tree Jira's REST API v3 uses for descriptions
/// and comments. This app reads and writes plain text, so only the text is kept.
/// </summary>
public static class Adf
{
    private static readonly HashSet<string> Blocks =
        ["paragraph", "heading", "blockquote", "codeBlock", "listItem", "rule", "panel", "tableRow"];

    /// <summary>
    /// A document of one paragraph per block of text; single line breaks become hard breaks
    /// </summary>
    public static object FromText(string text)
    {
        var paragraphs = text.ReplaceLineEndings("\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Paragraph)
            .ToArray();

        return new { type = "doc", version = 1, content = paragraphs };
    }

    private static object Paragraph(string block)
    {
        var inline = new List<object>();
        foreach (var line in block.Split('\n'))
        {
            if (inline.Count > 0)
            {
                inline.Add(new { type = "hardBreak" });
            }
            if (line.Length > 0)
            {
                inline.Add(new { type = "text", text = line });
            }
        }
        return new { type = "paragraph", content = inline };
    }

    /// <summary>
    /// The text of a document: its top-level blocks apart by a blank line, as
    /// <see cref="FromText"/> reads them. Older Jira data can hold a plain string.
    /// </summary>
    public static string ToText(JsonElement document)
    {
        if (document.ValueKind == JsonValueKind.String)
        {
            return document.GetString()!;
        }
        if (document.ValueKind != JsonValueKind.Object
            || !document.TryGetProperty("content", out var blocks)
            || blocks.ValueKind != JsonValueKind.Array)
        {
            return "";
        }

        var texts = blocks.EnumerateArray().Select(block =>
        {
            var text = new StringBuilder();
            Append(block, text);
            return text.ToString().Trim('\n');
        });
        return string.Join("\n\n", texts.Where(t => t.Length > 0));
    }

    private static void Append(JsonElement node, StringBuilder text)
    {
        var type = node.TryGetProperty("type", out var t) ? t.GetString() : null;
        switch (type)
        {
            case "text" when node.TryGetProperty("text", out var value):
                text.Append(value.GetString());
                return;
            case "hardBreak":
                text.Append('\n');
                return;
            case "mention" or "emoji" or "status" when node.TryGetProperty("attrs", out var attrs)
                                                        && attrs.TryGetProperty("text", out var label):
                text.Append(label.GetString());
                return;
        }

        if (node.TryGetProperty("content", out var children) && children.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in children.EnumerateArray())
            {
                Append(child, text);
            }
        }

        if (type is not null && Blocks.Contains(type) && text.Length > 0 && text[^1] != '\n')
        {
            text.Append('\n');
        }
    }
}

/// <summary>
/// Reads a description or a comment body, ADF or plain string, as text
/// </summary>
public sealed class AdfTextConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        using var document = JsonDocument.ParseValue(ref reader);
        return Adf.ToText(document.RootElement);
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
