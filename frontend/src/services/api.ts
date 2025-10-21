import axios from "axios";
import type { AxiosResponse } from "axios";
import type {
  ApiResponse,
  JiraUser,
  JiraIssue,
  JiraSearchResult,
  JiraTransition,
  JiraComment,
  JiraProject,
  JiraIssueType,
  CreateTicketRequest,
  UpdateTicketRequest,
  TransitionTicketRequest,
  AddCommentRequest,
  SearchRequest,
  AdvancedSearchRequest,
  ConfluenceSpace,
  ConfluencePage,
  ConfluenceSearchResult,
  SprintReport,
  TeamDashboard,
  ExecutiveSummary,
  BulkUpdateRequest,
  BulkUpdateResult,
  ConnectionValidationResult,
  SmartFilter,
  CommentTemplate,
  ReportType,
  ProjectStats,
  BulkOperationPreview,
  SavedSearch,
} from "../types";

// Create axios instance with base configuration
const api = axios.create({
  baseURL: "https://localhost:7000/api", // Adjust based on your API port
  timeout: 30000,
  headers: {
    "Content-Type": "application/json",
  },
});

// Request interceptor for logging
api.interceptors.request.use(
  (config) => {
    console.log(
      `Making ${config.method?.toUpperCase()} request to: ${config.url}`
    );
    return config;
  },
  (error) => {
    console.error("Request error:", error);
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error("API Error:", error.response?.data || error.message);
    return Promise.reject(error);
  }
);

// API Service Class
class JiraApiService {
  // Authentication
  async testConnection(): Promise<ApiResponse<boolean>> {
    const response: AxiosResponse<ApiResponse<boolean>> = await api.get(
      "/auth/test-connection"
    );
    return response.data;
  }

  async getCurrentUser(): Promise<ApiResponse<JiraUser>> {
    const response: AxiosResponse<ApiResponse<JiraUser>> = await api.get(
      "/auth/current-user"
    );
    return response.data;
  }

  async validateConnectionAndUser(): Promise<ApiResponse<any>> {
    const response: AxiosResponse<ApiResponse<any>> = await api.get(
      "/auth/validate"
    );
    return response.data;
  }

  // Tickets
  async getTicket(key: string): Promise<ApiResponse<JiraIssue>> {
    const response: AxiosResponse<ApiResponse<JiraIssue>> = await api.get(
      `/tickets/${key}`
    );
    return response.data;
  }

  async createTicket(
    request: CreateTicketRequest
  ): Promise<ApiResponse<JiraIssue>> {
    const response: AxiosResponse<ApiResponse<JiraIssue>> = await api.post(
      "/tickets",
      request
    );
    return response.data;
  }

  async updateTicket(
    key: string,
    request: UpdateTicketRequest
  ): Promise<ApiResponse> {
    const response: AxiosResponse<ApiResponse> = await api.put(
      `/tickets/${key}`,
      request
    );
    return response.data;
  }

  async getAvailableTransitions(
    key: string
  ): Promise<ApiResponse<JiraTransition[]>> {
    const response: AxiosResponse<ApiResponse<JiraTransition[]>> =
      await api.get(`/tickets/${key}/transitions`);
    return response.data;
  }

  async transitionTicket(
    key: string,
    request: TransitionTicketRequest
  ): Promise<ApiResponse> {
    const response: AxiosResponse<ApiResponse> = await api.post(
      `/tickets/${key}/transition`,
      request
    );
    return response.data;
  }

  // Search
  async searchJql(
    request: SearchRequest
  ): Promise<ApiResponse<JiraSearchResult>> {
    const response: AxiosResponse<ApiResponse<JiraSearchResult>> =
      await api.post("/search/jql", request);
    return response.data;
  }

  async advancedSearch(
    request: AdvancedSearchRequest
  ): Promise<ApiResponse<any>> {
    const response: AxiosResponse<ApiResponse<any>> = await api.post(
      "/search/advanced",
      request
    );
    return response.data;
  }

  async getSavedSearches(): Promise<ApiResponse<any[]>> {
    const response: AxiosResponse<ApiResponse<any[]>> = await api.get(
      "/search/saved"
    );
    return response.data;
  }

  async executeSavedSearch(
    searchId: string
  ): Promise<ApiResponse<JiraSearchResult>> {
    const response: AxiosResponse<ApiResponse<JiraSearchResult>> =
      await api.post(`/search/saved/${searchId}/execute`);
    return response.data;
  }

  async getSmartFilters(): Promise<ApiResponse<any[]>> {
    const response: AxiosResponse<ApiResponse<any[]>> = await api.get(
      "/search/smart-filters"
    );
    return response.data;
  }

  async executeSmartFilter(filterId: string): Promise<ApiResponse<any>> {
    const response: AxiosResponse<ApiResponse<any>> = await api.post(
      `/search/smart-filters/${filterId}/execute`
    );
    return response.data;
  }

  // Comments
  async getComments(ticketKey: string): Promise<ApiResponse<JiraComment[]>> {
    const response: AxiosResponse<ApiResponse<JiraComment[]>> = await api.get(
      `/comments/${ticketKey}`
    );
    return response.data;
  }

  async addComment(
    ticketKey: string,
    request: AddCommentRequest
  ): Promise<ApiResponse<JiraComment>> {
    const response: AxiosResponse<ApiResponse<JiraComment>> = await api.post(
      `/comments/${ticketKey}`,
      request
    );
    return response.data;
  }

  async getCommentTemplates(): Promise<ApiResponse<any[]>> {
    const response: AxiosResponse<ApiResponse<any[]>> = await api.get(
      "/comments/templates"
    );
    return response.data;
  }

  async addCommentFromTemplate(
    ticketKey: string,
    templateId: string
  ): Promise<ApiResponse<JiraComment>> {
    const response: AxiosResponse<ApiResponse<JiraComment>> = await api.post(
      `/comments/${ticketKey}/template/${templateId}`
    );
    return response.data;
  }

  // Users
  async searchUsers(
    query: string,
    maxResults: number = 20
  ): Promise<ApiResponse<JiraUser[]>> {
    const response: AxiosResponse<ApiResponse<JiraUser[]>> = await api.get(
      `/users/search?query=${encodeURIComponent(
        query
      )}&maxResults=${maxResults}`
    );
    return response.data;
  }

  async getUser(accountId: string): Promise<ApiResponse<JiraUser>> {
    const response: AxiosResponse<ApiResponse<JiraUser>> = await api.get(
      `/users/${accountId}`
    );
    return response.data;
  }

  async getAssignableUsersForProject(
    projectKey: string,
    maxResults: number = 20
  ): Promise<ApiResponse<JiraUser[]>> {
    const response: AxiosResponse<ApiResponse<JiraUser[]>> = await api.get(
      `/users/project/${projectKey}/assignable?maxResults=${maxResults}`
    );
    return response.data;
  }

  async getAssignableUsersForIssue(
    issueKey: string,
    maxResults: number = 20
  ): Promise<ApiResponse<JiraUser[]>> {
    const response: AxiosResponse<ApiResponse<JiraUser[]>> = await api.get(
      `/users/issue/${issueKey}/assignable?maxResults=${maxResults}`
    );
    return response.data;
  }

  // Projects
  async getProject(projectKey: string): Promise<ApiResponse<JiraProject>> {
    const response: AxiosResponse<ApiResponse<JiraProject>> = await api.get(
      `/projects/${projectKey}`
    );
    return response.data;
  }

  async getIssueTypes(
    projectKey: string
  ): Promise<ApiResponse<JiraIssueType[]>> {
    const response: AxiosResponse<ApiResponse<JiraIssueType[]>> = await api.get(
      `/projects/${projectKey}/issue-types`
    );
    return response.data;
  }

  async getProjectStats(projectKey: string): Promise<ApiResponse<any>> {
    const response: AxiosResponse<ApiResponse<any>> = await api.get(
      `/projects/${projectKey}/stats`
    );
    return response.data;
  }

  // Bulk Operations
  async bulkUpdateTickets(
    request: BulkUpdateRequest
  ): Promise<ApiResponse<BulkUpdateResult>> {
    const response: AxiosResponse<ApiResponse<BulkUpdateResult>> =
      await api.post("/bulk/update", request);
    return response.data;
  }

  async previewBulkUpdate(
    request: BulkUpdateRequest
  ): Promise<ApiResponse<any[]>> {
    const response: AxiosResponse<ApiResponse<any[]>> = await api.post(
      "/bulk/preview",
      request
    );
    return response.data;
  }

  // Reports
  async getReportTypes(): Promise<ApiResponse<any[]>> {
    const response: AxiosResponse<ApiResponse<any[]>> = await api.get(
      "/reports/types"
    );
    return response.data;
  }

  async getSprintReport(sprintId: string): Promise<ApiResponse<SprintReport>> {
    const response: AxiosResponse<ApiResponse<SprintReport>> = await api.get(
      `/reports/sprint/${sprintId}`
    );
    return response.data;
  }

  async getTeamDashboard(
    projectKey: string
  ): Promise<ApiResponse<TeamDashboard>> {
    const response: AxiosResponse<ApiResponse<TeamDashboard>> = await api.get(
      `/reports/team/${projectKey}`
    );
    return response.data;
  }

  async getExecutiveSummary(
    projectKey: string
  ): Promise<ApiResponse<ExecutiveSummary>> {
    const response: AxiosResponse<ApiResponse<ExecutiveSummary>> =
      await api.get(`/reports/executive/${projectKey}`);
    return response.data;
  }

  async exportReportToPdf(
    reportData: any,
    reportType: string = "General Report"
  ): Promise<Blob> {
    const response = await api.post("/reports/export/pdf", reportData, {
      params: { reportType },
      responseType: "blob",
    });
    return response.data;
  }

  // Confluence
  async testConfluenceConnection(): Promise<ApiResponse<boolean>> {
    const response: AxiosResponse<ApiResponse<boolean>> = await api.get(
      "/confluence/test-connection"
    );
    return response.data;
  }

  async getConfluenceSpaces(): Promise<ApiResponse<ConfluenceSpace[]>> {
    const response: AxiosResponse<ApiResponse<ConfluenceSpace[]>> =
      await api.get("/confluence/spaces");
    return response.data;
  }

  async searchConfluencePages(
    query: string,
    spaceKey?: string,
    limit: number = 25
  ): Promise<ApiResponse<ConfluenceSearchResult>> {
    const params = new URLSearchParams({ query, limit: limit.toString() });
    if (spaceKey) params.append("spaceKey", spaceKey);

    const response: AxiosResponse<ApiResponse<ConfluenceSearchResult>> =
      await api.get(`/confluence/search?${params}`);
    return response.data;
  }

  async getConfluencePage(
    pageId: string
  ): Promise<ApiResponse<ConfluencePage>> {
    const response: AxiosResponse<ApiResponse<ConfluencePage>> = await api.get(
      `/confluence/pages/${pageId}`
    );
    return response.data;
  }

  async getConfluencePagesInSpace(
    spaceKey: string,
    limit: number = 25
  ): Promise<ApiResponse<ConfluencePage[]>> {
    const response: AxiosResponse<ApiResponse<ConfluencePage[]>> =
      await api.get(`/confluence/spaces/${spaceKey}/pages?limit=${limit}`);
    return response.data;
  }

  async createConfluencePage(
    spaceKey: string,
    title: string,
    content: string,
    parentId?: string
  ): Promise<ApiResponse<ConfluencePage>> {
    const request = { spaceKey, title, content, parentId };
    const response: AxiosResponse<ApiResponse<ConfluencePage>> = await api.post(
      "/confluence/pages",
      request
    );
    return response.data;
  }
}

// Export singleton instance
export const jiraApi = new JiraApiService();
export default jiraApi;
