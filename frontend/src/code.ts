// The C# each page calls, read from the API's source at build time, so what the
// pages show is the code that runs

import atlassianHttpService from "../../src/JiraIntegration/Atlassian/AtlassianHttpService.cs?raw";
import adf from "../../src/JiraIntegration/Atlassian/Adf.cs?raw";
import queryText from "../../src/JiraIntegration/Atlassian/QueryText.cs?raw";
import atlassianServices from "../../src/JiraIntegration/Configuration/AtlassianServices.cs?raw";
import confluenceService from "../../src/JiraIntegration/Services/Implementations/ConfluenceService.cs?raw";
import jiraAuthService from "../../src/JiraIntegration/Services/Implementations/JiraAuthService.cs?raw";
import jiraCommentService from "../../src/JiraIntegration/Services/Implementations/JiraCommentService.cs?raw";
import jiraSearchService from "../../src/JiraIntegration/Services/Implementations/JiraSearchService.cs?raw";
import jiraTicketService from "../../src/JiraIntegration/Services/Implementations/JiraTicketService.cs?raw";

export interface SourceFile {
  name: string;
  code: string;
}

export const connectionCode: SourceFile[] = [
  { name: "AtlassianServices.cs", code: atlassianServices },
  { name: "AtlassianHttpService.cs", code: atlassianHttpService },
  { name: "JiraAuthService.cs", code: jiraAuthService },
];

export const ticketCode: SourceFile[] = [
  { name: "JiraSearchService.cs", code: jiraSearchService },
  { name: "JiraTicketService.cs", code: jiraTicketService },
  { name: "JiraCommentService.cs", code: jiraCommentService },
  { name: "Adf.cs", code: adf },
];

export const confluenceCode: SourceFile[] = [
  { name: "ConfluenceService.cs", code: confluenceService },
  { name: "QueryText.cs", code: queryText },
];
