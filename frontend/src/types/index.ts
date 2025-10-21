// API Response Types
export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

// Auth Types
export interface ConnectionValidationResult {
  connectionValid: boolean;
  user?: JiraUser;
}

// Smart Filter Types
export interface SmartFilter {
  id: string;
  name: string;
  description: string;
  category: string;
  jqlQuery: string;
}

// Comment Template Types
export interface CommentTemplate {
  id: string;
  name: string;
  category: string;
  template: string;
}

// Report Types
export interface ReportType {
  id: string;
  name: string;
  description: string;
}

// Project Statistics
export interface ProjectStats {
  projectKey: string;
  totalIssues: number;
  openIssues: number;
  inProgressIssues: number;
  completedIssues: number;
  lastUpdated: Date;
}

// Bulk Operation Preview
export interface BulkOperationPreview {
  ticketKey: string;
  currentValues: Record<string, string | number | boolean | null>;
  proposedChanges: Record<string, string | number | boolean | null>;
}

// Saved Search Types
export interface SavedSearch {
  id: string;
  name: string;
  description: string;
  jqlQuery: string;
  created: Date;
  isShared: boolean;
}

// Jira User Types
export interface JiraUser {
  accountId: string;
  displayName: string;
  emailAddress: string;
  active: boolean;
}

// Jira Issue Types
export interface JiraIssue {
  id: string;
  key: string;
  self: string;
  fields: JiraIssueFields;
}

export interface JiraIssueFields {
  summary: string;
  description: string;
  status: JiraStatus;
  issueType: JiraIssueType;
  priority: JiraPriority;
  assignee?: JiraUser;
  reporter?: JiraUser;
  created: Date;
  updated: Date;
  resolutionDate?: Date;
}

export interface JiraStatus {
  id: string;
  name: string;
  statusCategory: {
    id: string;
    name: string;
    colorName: string;
  };
}

export interface JiraIssueType {
  id: string;
  name: string;
  iconUrl: string;
}

export interface JiraPriority {
  id: string;
  name: string;
  iconUrl: string;
}

// Search Types
export interface JiraSearchResult {
  issues: JiraIssue[];
  total: number;
  startAt: number;
  maxResults: number;
  searchTime: {
    totalMilliseconds: number;
  };
}

// Transition Types
export interface JiraTransition {
  id: string;
  name: string;
  to: JiraStatus;
}

// Comment Types
export interface JiraComment {
  id: string;
  body: string;
  author: JiraUser;
  created: Date;
  updated: Date;
}

// Project Types
export interface JiraProject {
  id: string;
  key: string;
  name: string;
  description: string;
  projectTypeKey: string;
  lead?: JiraUser;
}

// Request Types
export interface CreateTicketRequest {
  projectKey: string;
  summary: string;
  description: string;
  issueTypeId: string;
  priority: string;
  assigneeId?: string;
}

export interface UpdateTicketRequest {
  summary?: string;
  description?: string;
  assigneeId?: string;
  priorityId?: string;
  dueDate?: Date;
}

export interface TransitionTicketRequest {
  transitionId: string;
  comment?: string;
}

export interface AddCommentRequest {
  body: string;
  mentionedUsers: string[];
  notifyUsers: boolean;
}

export interface SearchRequest {
  jqlQuery: string;
  maxResults: number;
  startAt: number;
}

export interface AdvancedSearchRequest {
  projectKey?: string;
  assigneeId?: string;
  status?: string;
  priority?: string;
  createdAfter?: Date;
  createdBefore?: Date;
  textSearch?: string;
  maxResults: number;
}

// Confluence Types
export interface ConfluenceSpace {
  id: string;
  key: string;
  name: string;
  type: string;
  status: string;
  description?: {
    plain?: {
      value: string;
    };
  };
}

export interface ConfluencePage {
  id: string;
  title: string;
  type: string;
  status: string;
  space?: ConfluenceSpace;
  body?: {
    view?: {
      value: string;
    };
  };
  version?: {
    number: number;
    when: Date;
    by?: JiraUser;
  };
  links?: {
    webui: string;
  };
}

export interface ConfluenceSearchResult {
  results: ConfluenceSearchResultItem[];
  start: number;
  limit: number;
  size: number;
}

export interface ConfluenceSearchResultItem {
  title: string;
  excerpt: string;
  url: string;
  content?: {
    space?: ConfluenceSpace;
  };
}

// Report Types
export interface SprintReport {
  sprintId: string;
  sprintName: string;
  startDate: Date;
  endDate: Date;
  plannedPoints: number;
  completedPoints: number;
  remainingPoints: number;
  completedIssues: JiraIssue[];
  incompleteIssues: JiraIssue[];
}

export interface TeamDashboard {
  projectName: string;
  teamWorkloads: TeamWorkload[];
  statusDistribution: Record<string, number>;
  priorityDistribution: Record<string, number>;
}

export interface TeamWorkload {
  userName: string;
  openTickets: number;
  inProgressTickets: number;
  completedTickets: number;
  totalPoints: number;
}

export interface ExecutiveSummary {
  projectName: string;
  reportDate: Date;
  totalIssues: number;
  completedIssues: number;
  inProgressIssues: number;
  openIssues: number;
  completionPercentage: number;
  keyAchievements: string[];
  risks: string[];
}

// Bulk Operations
export interface BulkUpdateRequest {
  ticketKeys: string[];
  updateData: UpdateTicketRequest;
}

export interface BulkUpdateResult {
  successfulUpdates: number;
  failedUpdates: number;
  failedTicketKeys: string[];
}
