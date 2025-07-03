namespace JiraIntegration.Models.Requests;

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
