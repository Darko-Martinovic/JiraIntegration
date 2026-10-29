# Jira Integration

A small ASP.NET Core API and a React front end that show how to work with Jira Cloud and Confluence Cloud from C#. The API signs in with an Atlassian API token, searches tickets with JQL, creates, edits, moves, comments on and deletes them, lists Confluence spaces and pages, searches pages with CQL and creates new ones. Each page of the front end can show the C# service it calls, read from the source at build time.

## What it covers

| Topic | Atlassian API | Code |
|---|---|---|
| Signing in with email and API token, one typed `HttpClient` per service | Basic authentication | `Configuration/AtlassianServices.cs` |
| JSON in and out, Atlassian's error answers turned into exceptions | | `Atlassian/AtlassianHttpService.cs`, `Atlassian/AtlassianApiException.cs` |
| Errors returned to the browser as problem details | | `Atlassian/AtlassianExceptionHandler.cs` |
| Searching tickets, page by page | `GET /rest/api/3/search/jql` with `nextPageToken` | `Services/Implementations/JiraSearchService.cs` |
| Creating, reading, updating and deleting tickets | `/rest/api/3/issue` | `Services/Implementations/JiraTicketService.cs` |
| Moving a ticket through its workflow, with a comment | `/rest/api/3/issue/{key}/transitions` | `Services/Implementations/JiraTicketService.cs` |
| Comments | `/rest/api/3/issue/{key}/comment` | `Services/Implementations/JiraCommentService.cs` |
| Issue types, priorities and assignable users | `/rest/api/3/project`, `/priority/search`, `/user/assignable/search` | `JiraProjectService.cs`, `JiraUserService.cs` |
| Descriptions and comments in Atlassian Document Format | | `Atlassian/Adf.cs` |
| Confluence spaces and pages | REST API v2, `/wiki/api/v2` | `Services/Implementations/ConfluenceService.cs` |
| Confluence search | REST API v1, `/wiki/rest/api/search` with CQL | `Services/Implementations/ConfluenceService.cs`, `Atlassian/QueryText.cs` |

All paths in the Code column are under `src/JiraIntegration/`.

## Requirements

- .NET 10 SDK
- Node.js 22 or later
- An Atlassian Cloud site with Jira. Confluence is optional; without it the Confluence page reports the error and the rest works.
- An API token for an account on that site

## Setup

1. Create an API token at <https://id.atlassian.com/manage-profile/security/api-tokens> ("Create API token", not the scoped variant: scoped tokens are called through `api.atlassian.com` with the site's cloud id, which this project doesn't do).
2. Copy `.env.example` to `.env` in the repository root and fill it in:

   | Setting | Value |
   |---|---|
   | `JIRA_BASE_URL` | The site, such as `https://your-site.atlassian.net` |
   | `JIRA_USER_EMAIL` | The email of the account the token belongs to |
   | `JIRA_API_TOKEN` | The token |
   | `JIRA_PROJECT_KEY` | The project the ticket list opens on and new tickets go into |
   | `JIRA_TIMEOUT_SECONDS` | Optional, 30 by default |

   `.env` is ignored by git. The same values can come from environment variables instead.

## Running it

Start the API (it listens on <http://localhost:5000>):

```bash
dotnet run --project src/JiraIntegration
```

Start the front end in a second terminal and open <http://localhost:5173>:

```bash
cd frontend
npm install
npm run dev
```

The Vite dev server passes `/api` on to the API, so the browser talks to one origin. Set `API_URL` before `npm run dev` if the API runs elsewhere.

When the API runs in Development (the default with `dotnet run`), its OpenAPI document is at <http://localhost:5000/openapi/v1.json> and an API reference to try the routes at <http://localhost:5000/scalar>.

## API

| Method | Route | Does |
|---|---|---|
| GET | `/api/connection` | Tries the credentials on Jira and on Confluence |
| GET | `/api/connection/settings` | The settings in use, without the token |
| GET | `/api/tickets?jql=&nextPageToken=&maxResults=` | One page of tickets; without `jql`, the configured project's, newest first |
| POST | `/api/tickets` | Creates a ticket: `summary`, `description`, `issueType` (name), `priority` (name), `assigneeAccountId`, `projectKey` |
| GET | `/api/tickets/{key}` | One ticket, description as text |
| PUT | `/api/tickets/{key}` | Changes only the fields given; an empty `assigneeAccountId` unassigns |
| DELETE | `/api/tickets/{key}` | Deletes the ticket |
| GET | `/api/tickets/{key}/transitions` | The moves the workflow allows from the current status |
| POST | `/api/tickets/{key}/transitions` | Makes a move: `transitionId`, optional `comment` |
| GET | `/api/tickets/{key}/comments` | The comments, oldest first |
| POST | `/api/tickets/{key}/comments` | Adds a comment: `body` |
| DELETE | `/api/tickets/{key}/comments/{commentId}` | Deletes a comment |
| GET | `/api/projects/{projectKey}/issue-types` | The project's issue types, without sub-tasks |
| GET | `/api/projects/{projectKey}/assignable-users` | Who a ticket in the project can be assigned to |
| GET | `/api/priorities` | The site's priorities |
| GET | `/api/users?query=` | Users whose name or email starts with the query |
| GET | `/api/confluence/spaces` | The spaces the account can see |
| GET | `/api/confluence/spaces/{spaceKey}/pages` | The space's pages, by title |
| GET | `/api/confluence/pages/{pageId}` | One page with its body in storage format |
| POST | `/api/confluence/pages` | Creates a page: `spaceKey`, `title`, `text`, optional `parentId` |
| GET | `/api/confluence/search?text=&spaceKey=` | Pages whose text matches |

Errors come back as problem details. A 400 or 404 from Atlassian is passed on with Atlassian's message; refused credentials and other Atlassian errors are a 502; a site that can't be reached is a 504; missing settings are a 503 naming them.

## Tests

```bash
dotnet test
cd frontend && npm test
```

The API tests run the whole app with `WebApplicationFactory`, with every call to Atlassian going to a fake site (`tests/JiraIntegration.Tests/FakeAtlassian.cs`) that records the requests. They check what is sent to Jira and Confluence as well as what comes back, so no Atlassian account is needed. The front end tests stub `fetch`. GitHub Actions runs both on every pull request.

## Project layout

```
src/JiraIntegration/         the API
  Atlassian/                 HTTP base class, errors, ADF and query text helpers
  Configuration/             settings from .env and the HttpClient registrations
  Controllers/               the routes above
  Models/                    Jira and Confluence answers, the API's requests and views
  Services/                  one service per area, behind an interface
tests/JiraIntegration.Tests/ xUnit tests of the API
frontend/                    React, Vite and Tailwind CSS
```

## Limits

- Jira Cloud and Confluence Cloud only. Jira Data Center has REST API v2, plain-text descriptions and other authentication.
- Descriptions and comments are handled as plain text. Rich formatting in Atlassian Document Format (tables, panels, colours) is flattened when read, and text is saved as paragraphs.
- The API has no sign-in of its own and acts as the token's account. It listens on localhost only and accepts only localhost host names; don't expose it to a network.
- Issue types and priorities are given by name, so they must exist in the project.

## History

The project started as a console application and became a web API with a React front end in October 2025. The tag `v1` holds that version, before the move to .NET 10, the current Jira search API and Confluence REST API v2.

## License

MIT, see [LICENSE](LICENSE).
