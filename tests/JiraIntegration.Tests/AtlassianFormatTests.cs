using System.Net;
using System.Text.Json;
using JiraIntegration.Atlassian;
using JiraIntegration.Models.Jira;

namespace JiraIntegration.Tests;

public class AtlassianFormatTests
{
    [Fact]
    public void Text_becomes_one_paragraph_per_block_with_hard_breaks_inside()
    {
        var document = JsonSerializer.SerializeToElement(Adf.FromText("First line\r\nsecond line\n\nNext paragraph\n\n\n"));

        Assert.Equal("doc", document.GetProperty("type").GetString());
        Assert.Equal(1, document.GetProperty("version").GetInt32());
        var paragraphs = document.GetProperty("content").EnumerateArray().ToList();
        Assert.Equal(2, paragraphs.Count);
        Assert.Equal(
            ["text", "hardBreak", "text"],
            paragraphs[0].GetProperty("content").EnumerateArray().Select(n => n.GetProperty("type").GetString()));
        Assert.Equal("Next paragraph", paragraphs[1].GetProperty("content")[0].GetProperty("text").GetString());
    }

    [Fact]
    public void Text_survives_the_round_trip_through_adf()
    {
        var text = "Steps:\nopen the page\n\nExpected: it loads";

        var document = JsonSerializer.SerializeToElement(Adf.FromText(text));

        Assert.Equal(text, Adf.ToText(document));
    }

    [Fact]
    public void Adf_is_read_as_text_including_lists_and_mentions()
    {
        const string json = """
            {"type":"doc","version":1,"content":[
              {"type":"heading","attrs":{"level":2},"content":[{"type":"text","text":"Summary"}]},
              {"type":"paragraph","content":[{"type":"text","text":"Ask "},{"type":"mention","attrs":{"id":"1","text":"@Ana"}},{"type":"text","text":" first"}]},
              {"type":"bulletList","content":[
                {"type":"listItem","content":[{"type":"paragraph","content":[{"type":"text","text":"one"}]}]},
                {"type":"listItem","content":[{"type":"paragraph","content":[{"type":"text","text":"two"}]}]}]}]}
            """;

        Assert.Equal("Summary\n\nAsk @Ana first\n\none\ntwo",Adf.ToText(JsonDocument.Parse(json).RootElement));
    }

    [Fact]
    public void An_issue_reads_with_jira_dates_and_an_adf_description()
    {
        const string json = """
            {"id":"10001","key":"KAN-1","fields":{
              "summary":"Login fails","created":"2025-10-21T15:23:48.000+0200",
              "description":{"type":"doc","version":1,"content":[{"type":"paragraph","content":[{"type":"text","text":"After a reset"}]}]},
              "status":{"id":"3","name":"In Progress"},"issuetype":{"id":"10004","name":"Bug","subtask":false}}}
            """;

        var issue = JsonSerializer.Deserialize<JiraIssue>(json, AtlassianHttpService.Json)!;

        Assert.Equal("After a reset", issue.Fields.Description);
        Assert.Equal(new DateTimeOffset(2025, 10, 21, 15, 23, 48, TimeSpan.FromHours(2)), issue.Fields.Created);
        Assert.Equal("Bug", issue.Fields.IssueType!.Name);
        Assert.Null(issue.Fields.Assignee);
    }

    [Fact]
    public void A_plain_string_description_from_older_data_is_kept()
    {
        var issue = JsonSerializer.Deserialize<JiraIssue>(
            """{"id":"1","key":"KAN-2","fields":{"summary":"s","description":"plain"}}""",
            AtlassianHttpService.Json)!;

        Assert.Equal("plain", issue.Fields.Description);
    }

    [Theory]
    [InlineData("""{"errorMessages":["Issue does not exist"],"errors":{}}""", "Issue does not exist")]
    [InlineData("""{"errorMessages":[],"errors":{"priority":"Field cannot be set"}}""", "priority: Field cannot be set")]
    [InlineData("""{"errors":[{"status":400,"title":"INVALID_REQUEST","detail":"Title is required"}]}""", "INVALID_REQUEST: Title is required")]
    [InlineData("""{"statusCode":400,"message":"Could not parse cql"}""", "Could not parse cql")]
    public void Atlassian_error_answers_keep_their_message(string body, string message)
    {
        Assert.Equal(message, AtlassianApiException.From(HttpStatusCode.BadRequest, body).Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("<html>Unauthorized</html>")]
    [InlineData("""{"errorMessages":[]}""")]
    public void An_error_without_a_message_names_the_status(string body)
    {
        Assert.Equal("Atlassian answered 401 Unauthorized", AtlassianApiException.From(HttpStatusCode.Unauthorized, body).Message);
    }

    [Fact]
    public void Quoted_query_text_cannot_end_the_string()
    {
        Assert.Equal("\"say \\\"hi\\\" C:\\\\temp\"", QueryText.Quote("say \"hi\" C:\\temp"));
    }

    [Fact]
    public void Page_text_is_encoded_as_storage_paragraphs()
    {
        Assert.Equal("<p>a &lt;b&gt; &amp; c<br />d</p><p>e</p>", QueryText.ToStorage("a <b> & c\nd\n\ne"));
    }

    [Fact]
    public void Search_excerpts_lose_their_highlight_markers()
    {
        Assert.Equal("the release notes", QueryText.PlainExcerpt(" the @@@hl@@@release@@@endhl@@@ notes "));
    }
}
