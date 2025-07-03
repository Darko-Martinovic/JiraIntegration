namespace JiraIntegration.Models.Requests;

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
