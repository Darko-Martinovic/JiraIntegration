namespace JiraIntegration.Models.Dto;

/// <summary>
/// Confluence space model
/// </summary>
public class ConfluenceSpace
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public ConfluenceSpaceDescription? Description { get; set; }
    public ConfluenceHomepage? Homepage { get; set; }
    public ConfluenceLinks? Links { get; set; }
}

/// <summary>
/// Confluence homepage information
/// </summary>
public class ConfluenceHomepage
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ConfluenceLinks? Links { get; set; }
}

/// <summary>
/// Confluence space description
/// </summary>
public class ConfluenceSpaceDescription
{
    public ConfluencePlainDescription? Plain { get; set; }
    public ConfluenceExpandableProperty? _Expandable { get; set; }
}

/// <summary>
/// Confluence plain description content
/// </summary>
public class ConfluencePlainDescription
{
    public string Value { get; set; } = string.Empty;
    public string Representation { get; set; } = string.Empty;
    public List<object> EmbeddedContent { get; set; } = new();
}

/// <summary>
/// Confluence expandable property
/// </summary>
public class ConfluenceExpandableProperty
{
    public string View { get; set; } = string.Empty;
}

/// <summary>
/// Confluence page model
/// </summary>
public class ConfluencePage
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ConfluenceSpace? Space { get; set; }
    public ConfluencePageContent? Body { get; set; }
    public ConfluenceVersion? Version { get; set; }
    public ConfluenceLinks? Links { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Confluence page content
/// </summary>
public class ConfluencePageContent
{
    public ConfluenceContentValue? Storage { get; set; }
    public ConfluenceContentValue? View { get; set; }
}

/// <summary>
/// Confluence content value
/// </summary>
public class ConfluenceContentValue
{
    public string Value { get; set; } = string.Empty;
    public string Representation { get; set; } = string.Empty;
}

/// <summary>
/// Confluence version information
/// </summary>
public class ConfluenceVersion
{
    public int Number { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime When { get; set; }
    public ConfluenceUser? By { get; set; }
}

/// <summary>
/// Confluence user model
/// </summary>
public class ConfluenceUser
{
    public string Type { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Confluence links
/// </summary>
public class ConfluenceLinks
{
    public string? Webui { get; set; }
    public string? Base { get; set; }
    public string? Context { get; set; }
    public string? Self { get; set; }
}

/// <summary>
/// Confluence search results
/// </summary>
public class ConfluenceSearchResults
{
    public List<ConfluenceSearchResult> Results { get; set; } = new();
    public int Start { get; set; }
    public int Limit { get; set; }
    public int Size { get; set; }
    public int TotalSize { get; set; }
}

/// <summary>
/// Individual search result
/// </summary>
public class ConfluenceSearchResult
{
    public ConfluencePage? Content { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Confluence space list response
/// </summary>
public class ConfluenceSpaceListResponse
{
    public List<ConfluenceSpace> Results { get; set; } = new();
    public int Start { get; set; }
    public int Limit { get; set; }
    public int Size { get; set; }
}
