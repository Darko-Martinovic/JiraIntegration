using System.Net;
using System.Text.RegularExpressions;

namespace JiraIntegration.Atlassian;

/// <summary>
/// Text from a caller put inside JQL, CQL or Confluence storage format, so it stays text
/// </summary>
public static partial class QueryText
{
    /// <summary>
    /// A quoted string for JQL or CQL: backslashes and double quotes are escaped
    /// </summary>
    public static string Quote(string value) =>
        "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

    /// <summary>
    /// Confluence storage format is XHTML: one encoded paragraph per block of text
    /// </summary>
    public static string ToStorage(string text) =>
        string.Concat(text.ReplaceLineEndings("\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(block => "<p>" + WebUtility.HtmlEncode(block).Replace("\n", "<br />") + "</p>"));

    /// <summary>
    /// Confluence marks the matched words in a search excerpt with @@@hl@@@ and @@@endhl@@@
    /// </summary>
    public static string PlainExcerpt(string? excerpt) =>
        excerpt is null ? "" : HighlightMarker().Replace(excerpt, "").Trim();

    [GeneratedRegex("@@@(end)?hl@@@")]
    private static partial Regex HighlightMarker();
}
