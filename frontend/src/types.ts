// The shapes the API sends, as in src/JiraIntegration/Models

export interface SiteSettings {
  baseUrl: string;
  email: string;
  projectKey: string;
  hasApiToken: boolean;
}

export interface JiraUser {
  accountId: string;
  displayName: string;
  emailAddress: string | null;
  active: boolean;
}

export interface ConnectionStatus {
  settings: SiteSettings;
  missingSettings: string[];
  jiraConnected: boolean;
  jiraError: string | null;
  user: JiraUser | null;
  confluenceConnected: boolean;
  confluenceError: string | null;
}

export interface Ticket {
  key: string;
  summary: string;
  description: string | null;
  status: string;
  priority: string | null;
  issueType: string;
  assignee: string | null;
  assigneeAccountId: string | null;
  created: string | null;
  updated: string | null;
  url: string;
}

export interface TicketPage {
  tickets: Ticket[];
  nextPageToken: string | null;
}

export interface CreatedTicket {
  key: string;
  url: string;
}

export interface NewTicket {
  summary: string;
  description?: string;
  issueType: string;
  priority?: string;
  assigneeAccountId?: string;
}

/** Only the fields given change; an empty assignee unassigns, an empty description clears it */
export interface TicketChanges {
  summary?: string;
  description?: string;
  priority?: string;
  assigneeAccountId?: string;
}

export interface Transition {
  id: string;
  name: string;
  toStatus: string;
}

export interface Comment {
  id: string;
  author: string;
  body: string;
  created: string;
}

export interface IssueType {
  id: string;
  name: string;
}

export interface Priority {
  id: string;
  name: string;
}

export interface Space {
  id: string;
  key: string;
  name: string;
  description: string | null;
  url: string;
}

export interface Page {
  id: string;
  title: string;
  spaceId: string;
  version: number | null;
  url: string;
  body: string | null;
}

export interface NewPage {
  spaceKey: string;
  title: string;
  text: string;
}

export interface SearchHit {
  id: string;
  title: string;
  excerpt: string;
  space: string | null;
  url: string;
}
