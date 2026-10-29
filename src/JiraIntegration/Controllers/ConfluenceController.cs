using System.ComponentModel.DataAnnotations;
using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using JiraIntegration.Models.Api;
using JiraIntegration.Models.Confluence;
using JiraIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JiraIntegration.Controllers;

[ApiController]
[Route("api/confluence")]
public sealed class ConfluenceController(IConfluenceService confluence, IOptions<AtlassianSettings> options) : ControllerBase
{
    private AtlassianSettings Settings => options.Value;

    [HttpGet("spaces")]
    public async Task<IEnumerable<SpaceView>> GetSpaces([Range(1, 250)] int limit = 50, CancellationToken cancellationToken = default)
    {
        var spaces = await confluence.GetSpacesAsync(limit, cancellationToken);
        return spaces.Select(s => new SpaceView(
            s.Id, s.Key, s.Name, s.Description?.Plain?.Value, Settings.ConfluenceUrl(s.Links?.WebUi)));
    }

    /// <summary>The space's pages by title, without their bodies</summary>
    [HttpGet("spaces/{spaceKey}/pages")]
    public async Task<ActionResult<IEnumerable<PageView>>> GetPages(
        [RegularExpression(Patterns.SpaceKey)] string spaceKey,
        [Range(1, 250)] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        // v2 names spaces by id, so the key is looked up first
        var space = await confluence.FindSpaceAsync(spaceKey, cancellationToken);
        if (space is null)
        {
            return SpaceNotFound(spaceKey);
        }

        var pages = await confluence.GetPagesAsync(space.Id, limit, cancellationToken);
        return Ok(pages.Select(p => ToView(p, withBody: false)));
    }

    /// <summary>The page with its body in storage format (XHTML)</summary>
    [HttpGet("pages/{pageId}")]
    public async Task<PageView> GetPage([RegularExpression(Patterns.NumericId)] string pageId, CancellationToken cancellationToken) =>
        ToView(await confluence.GetPageAsync(pageId, cancellationToken), withBody: true);

    [HttpPost("pages")]
    public async Task<ActionResult<PageView>> CreatePage(CreatePageRequest request, CancellationToken cancellationToken)
    {
        var space = await confluence.FindSpaceAsync(request.SpaceKey, cancellationToken);
        if (space is null)
        {
            return SpaceNotFound(request.SpaceKey);
        }

        var page = await confluence.CreatePageAsync(space.Id, request, cancellationToken);
        return CreatedAtAction(nameof(GetPage), new { pageId = page.Id }, ToView(page, withBody: false));
    }

    /// <summary>Pages whose text matches, optionally in one space, best match first</summary>
    [HttpGet("search")]
    public async Task<IEnumerable<SearchHit>> Search(
        [Required, MinLength(1)] string text,
        [RegularExpression(Patterns.SpaceKey)] string? spaceKey,
        [Range(1, 100)] int limit = 25,
        CancellationToken cancellationToken = default)
    {
        var found = await confluence.SearchAsync(text, spaceKey, limit, cancellationToken);
        return found.Results.Select(r => new SearchHit(
            r.Content?.Id ?? "",
            r.Title,
            QueryText.PlainExcerpt(r.Excerpt),
            r.ResultGlobalContainer?.Title,
            Settings.ConfluenceUrl(r.Url)));
    }

    private PageView ToView(ConfluencePage page, bool withBody) => new(
        page.Id,
        page.Title,
        page.SpaceId,
        page.Version?.Number,
        Settings.ConfluenceUrl(page.Links?.WebUi),
        withBody ? page.Body?.Storage?.Value : null);

    private ObjectResult SpaceNotFound(string spaceKey) =>
        Problem(statusCode: 404, title: "Atlassian could not find it", detail: $"No Confluence space {spaceKey} the account can see");
}
