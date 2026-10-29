import type {
  Comment,
  ConnectionStatus,
  CreatedTicket,
  IssueType,
  JiraUser,
  NewPage,
  NewTicket,
  Page,
  Priority,
  SearchHit,
  SiteSettings,
  Space,
  Ticket,
  TicketChanges,
  TicketPage,
  Transition,
} from "./types";

/** A failed call, with the title and detail of the API's problem details */
export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

interface Problem {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

async function toError(response: Response): Promise<ApiError> {
  try {
    const problem = (await response.json()) as Problem;
    const parts = [problem.title, problem.detail];
    if (problem.errors) {
      parts.push(Object.values(problem.errors).flat().join(" "));
    }
    const message = parts.filter(Boolean).join(": ");
    return new ApiError(response.status, message || `${String(response.status)} ${response.statusText}`);
  } catch {
    return new ApiError(response.status, `${String(response.status)} ${response.statusText}`);
  }
}

async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  const response = await fetch(`/api${path}`, {
    method,
    headers: body === undefined ? undefined : { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  if (!response.ok) {
    throw await toError(response);
  }
  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}

function query(values: Record<string, string | undefined>): string {
  const params = new URLSearchParams();
  for (const [name, value] of Object.entries(values)) {
    if (value) {
      params.set(name, value);
    }
  }
  const text = params.toString();
  return text ? `?${text}` : "";
}

const ticket = (key: string) => `/tickets/${encodeURIComponent(key)}`;

export const api = {
  connection: () => request<ConnectionStatus>("GET", "/connection"),
  settings: () => request<SiteSettings>("GET", "/connection/settings"),

  tickets: (jql?: string, nextPageToken?: string) =>
    request<TicketPage>("GET", `/tickets${query({ jql, nextPageToken })}`),
  ticket: (key: string) => request<Ticket>("GET", ticket(key)),
  createTicket: (newTicket: NewTicket) => request<CreatedTicket>("POST", "/tickets", newTicket),
  updateTicket: (key: string, changes: TicketChanges) => request<undefined>("PUT", ticket(key), changes),
  deleteTicket: (key: string) => request<undefined>("DELETE", ticket(key)),
  transitions: (key: string) => request<Transition[]>("GET", `${ticket(key)}/transitions`),
  transition: (key: string, transitionId: string, comment?: string) =>
    request<undefined>("POST", `${ticket(key)}/transitions`, { transitionId, comment }),
  comments: (key: string) => request<Comment[]>("GET", `${ticket(key)}/comments`),
  addComment: (key: string, body: string) => request<Comment>("POST", `${ticket(key)}/comments`, { body }),
  deleteComment: (key: string, commentId: string) =>
    request<undefined>("DELETE", `${ticket(key)}/comments/${encodeURIComponent(commentId)}`),

  issueTypes: (projectKey: string) =>
    request<IssueType[]>("GET", `/projects/${encodeURIComponent(projectKey)}/issue-types`),
  assignableUsers: (projectKey: string) =>
    request<JiraUser[]>("GET", `/projects/${encodeURIComponent(projectKey)}/assignable-users`),
  priorities: () => request<Priority[]>("GET", "/priorities"),

  spaces: () => request<Space[]>("GET", "/confluence/spaces"),
  pages: (spaceKey: string) => request<Page[]>("GET", `/confluence/spaces/${encodeURIComponent(spaceKey)}/pages`),
  page: (pageId: string) => request<Page>("GET", `/confluence/pages/${encodeURIComponent(pageId)}`),
  createPage: (newPage: NewPage) => request<Page>("POST", "/confluence/pages", newPage),
  searchConfluence: (text: string, spaceKey?: string) =>
    request<SearchHit[]>("GET", `/confluence/search${query({ text, spaceKey })}`),
};

/** The message to show for a failed call */
export function errorText(error: unknown): string {
  return error instanceof Error ? error.message : String(error);
}
