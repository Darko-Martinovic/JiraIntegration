import React, { useState, useEffect, useCallback } from "react";
import Prism from "prismjs";

// Import Prism theme and C# language support
import "prismjs/themes/prism-tomorrow.css";
import "prismjs/components/prism-csharp";
import {
  Plus,
  Edit,
  ExternalLink,
  RefreshCw,
  Code,
  Trash2,
  Eye,
} from "lucide-react";

interface Ticket {
  key: string;
  summary: string;
  status: string;
  priority: string;
  issueType: string;
  assignee?: string;
  created: string;
  updated: string;
  jiraUrl: string;
}

interface CreateTicketForm {
  projectKey: string;
  issueType: string;
  summary: string;
  description: string;
  priority: string;
  assignee: string;
}

interface UpdateTicketForm {
  summary?: string;
  description?: string;
  priority?: string;
  assignee?: string;
}

const TicketManagement: React.FC = () => {
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingTicket, setEditingTicket] = useState<string | null>(null);
  const [alertMessage, setAlertMessage] = useState("");
  const [alertType, setAlertType] = useState<"success" | "error">("success");
  const [showCSharpCode, setShowCSharpCode] = useState(false);
  const [lastCreatedTicket, setLastCreatedTicket] = useState<{
    key: string;
    url: string;
  } | null>(null);

  const [createForm, setCreateForm] = useState<CreateTicketForm>({
    projectKey: "KAN",
    issueType: "Task",
    summary: "",
    description: "",
    priority: "Medium",
    assignee: "",
  });

  const [updateForm, setUpdateForm] = useState<UpdateTicketForm>({});

  // Code highlighting component with proper Prism.js C# support and copy functionality
  const CodeBlock = ({ code }: { code: string }) => {
    const [copied, setCopied] = useState(false);

    useEffect(() => {
      Prism.highlightAll();
    }, [code]);

    const copyToClipboard = async () => {
      try {
        await navigator.clipboard.writeText(code);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
      } catch (err) {
        console.error("Failed to copy code:", err);
      }
    };

    return (
      <div className="relative">
        <button
          onClick={copyToClipboard}
          className="absolute top-2 right-2 bg-gray-700 hover:bg-gray-600 text-white px-3 py-1 rounded text-xs font-medium transition-colors z-10"
          title="Copy code to clipboard"
        >
          {copied ? "Copied!" : "Copy"}
        </button>
        <pre className="rounded-lg p-4 overflow-x-auto bg-gray-900">
          <code className="language-csharp text-sm">{code}</code>
        </pre>
      </div>
    );
  };

  const fetchTickets = useCallback(async () => {
    setLoading(true);
    setError(null);
    console.log("🔄 Starting fetchTickets...");

    try {
      const url = "http://localhost:5000/api/jira/tickets?maxResults=20";
      console.log("📤 Making request to:", url);

      const response = await fetch(url);
      console.log("📥 Response status:", response.status, response.statusText);
      console.log(
        "📥 Response headers:",
        Object.fromEntries(response.headers.entries())
      );

      if (response.ok) {
        const data = await response.json();
        console.log("✅ Full API response:", JSON.stringify(data, null, 2));
        console.log("📊 Response structure:", {
          hasSuccess: "Success" in data,
          hasTickets: "Tickets" in data,
          hasCount: "Count" in data,
          ticketsLength: data.Tickets ? data.Tickets.length : "undefined",
          dataKeys: Object.keys(data),
        });

        setTickets(data.tickets || []);

        // Show info about tickets loaded
        if (data.tickets && data.tickets.length > 0) {
          console.log(
            `✅ Loaded ${data.tickets.length} tickets from project KAN`
          );
          console.log("📋 First ticket sample:", data.tickets[0]);
          showAlert(
            `✅ Loaded ${data.tickets.length} tickets from project KAN`,
            "success"
          );
        } else {
          console.log("⚠️ No tickets found in response");
          console.log("🔍 Checking data structure:", data);
          showAlert("No tickets found in the project KAN", "error");
        }
      } else {
        const errorText = await response.text();
        console.error("❌ HTTP Error:", response.status, response.statusText);
        console.error("❌ Error response body:", errorText);
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to fetch tickets";
      console.error("❌ fetchTickets error:", err);
      console.error("❌ Error details:", {
        name: err instanceof Error ? err.name : "Unknown",
        message: errorMessage,
        stack: err instanceof Error ? err.stack : "No stack trace",
      });
      setError(errorMessage);
      showAlert(`❌ Failed to load tickets: ${errorMessage}`, "error");
    } finally {
      setLoading(false);
      console.log("🔄 fetchTickets completed");
    }
  }, []);

  useEffect(() => {
    fetchTickets();
  }, [fetchTickets]);

  const showAlert = (
    message: string,
    type: "success" | "error" = "success"
  ) => {
    setAlertMessage(message);
    setAlertType(type);
    // Errors stay longer (10 seconds), success messages fade after 5 seconds
    const timeout = type === "error" ? 10000 : 5000;
    setTimeout(() => setAlertMessage(""), timeout);
  };

  const createTicket = async () => {
    console.log("🎫 Starting createTicket...");

    if (!createForm.summary.trim()) {
      console.log("❌ Validation failed: Summary is required");
      showAlert("Summary is required", "error");
      return;
    }

    try {
      const requestData = {
        projectKey: createForm.projectKey,
        summary: createForm.summary,
        description: createForm.description,
        issueType: createForm.issueType,
        // Skip priority to avoid JIRA field configuration issues
        // priority: createForm.priority,
        assignee: createForm.assignee || null,
      };

      console.log(
        "📤 Create ticket request (priority omitted):",
        JSON.stringify(requestData, null, 2)
      );

      const response = await fetch("http://localhost:5000/api/jira/tickets", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(requestData),
      });

      console.log(
        "📥 Create ticket response status:",
        response.status,
        response.statusText
      );
      console.log(
        "📥 Create ticket response headers:",
        Object.fromEntries(response.headers.entries())
      );

      if (response.ok) {
        const result = await response.json();
        console.log(
          "✅ Create ticket success response:",
          JSON.stringify(result, null, 2)
        );

        const ticketKey =
          result.Ticket?.Key ||
          result.ticket?.key ||
          result.Ticket?.key ||
          result.ticket?.Key ||
          "Unknown";
        console.log("🎫 Created ticket key:", ticketKey);

        const jiraUrl =
          result.jiraUrl ||
          result.JiraUrl ||
          result.url ||
          `https://airun-1760187566049.atlassian.net/browse/${ticketKey}`;
        console.log("🔗 Jira URL:", jiraUrl);

        // Set the created ticket info for display
        setLastCreatedTicket({ key: ticketKey, url: jiraUrl });

        showAlert(
          `✅ Ticket created successfully! Key: ${ticketKey}`,
          "success"
        );
        setCreateForm({
          projectKey: "KAN",
          issueType: "Task",
          summary: "",
          description: "",
          priority: "Medium",
          assignee: "",
        });
        setShowCreateForm(false);
        console.log("🔄 Fetching updated tickets list...");
        fetchTickets();
      } else {
        const errorData = await response.json();
        console.error(
          "❌ Create ticket HTTP error:",
          response.status,
          response.statusText
        );
        console.error(
          "❌ Create ticket error response:",
          JSON.stringify(errorData, null, 2)
        );

        let errorMessage = "Failed to create ticket";

        if (errorData.message) {
          errorMessage = errorData.message;
        } else if (errorData.errors) {
          errorMessage = Array.isArray(errorData.errors)
            ? errorData.errors.join(", ")
            : JSON.stringify(errorData.errors);
        } else if (errorData.title) {
          errorMessage = errorData.title;
        }

        showAlert(`❌ Create Ticket Error: ${errorMessage}`, "error");
      }
    } catch (error) {
      console.error("❌ Create ticket network/parse error:", error);
      console.error("❌ Error details:", {
        name: error instanceof Error ? error.name : "Unknown",
        message: error instanceof Error ? error.message : "Unknown error",
        stack: error instanceof Error ? error.stack : "No stack trace",
      });

      const errorMessage =
        error instanceof Error ? error.message : "Network error";
      showAlert(`❌ Create Ticket Error: ${errorMessage}`, "error");
    } finally {
      console.log("🎫 createTicket completed");
    }
  };

  const deleteTicket = async (ticketKey: string) => {
    if (
      !window.confirm(
        `Are you sure you want to delete ticket ${ticketKey}? This action cannot be undone.`
      )
    ) {
      return;
    }

    try {
      console.log(`🗑️ Starting deleteTicket for ${ticketKey}...`);

      const response = await fetch(
        `http://localhost:5000/api/jira/tickets/${ticketKey}`,
        {
          method: "DELETE",
        }
      );

      console.log(
        `📥 Delete ticket response status: ${response.status} ${response.statusText}`
      );

      if (response.ok) {
        console.log(`✅ Ticket ${ticketKey} deleted successfully`);
        showAlert(`✅ Ticket ${ticketKey} deleted successfully!`, "success");
        fetchTickets(); // Refresh the ticket list
      } else {
        const errorData = await response.json();
        console.error("❌ Delete ticket error:", errorData);
        const errorMessage =
          errorData.message || `Failed to delete ticket ${ticketKey}`;
        showAlert(`❌ Delete Error: ${errorMessage}`, "error");
      }
    } catch (error) {
      console.error("❌ Delete ticket network error:", error);
      const errorMessage =
        error instanceof Error ? error.message : "Network error";
      showAlert(`❌ Delete Error: ${errorMessage}`, "error");
    }
  };

  const viewTicketInJira = (jiraUrl: string) => {
    window.open(jiraUrl, "_blank", "noopener,noreferrer");
  };

  const updateTicket = async (ticketKey: string) => {
    try {
      const response = await fetch(
        `http://localhost:5000/api/jira/tickets/${ticketKey}`,
        {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(updateForm),
        }
      );

      if (response.ok) {
        showAlert(`✅ Ticket ${ticketKey} updated successfully!`);
        setEditingTicket(null);
        setUpdateForm({});
        fetchTickets();
      } else {
        const errorData = await response.json();
        showAlert(
          `Error: ${errorData.message || "Failed to update ticket"}`,
          "error"
        );
      }
    } catch (error) {
      showAlert(
        `Error: ${error instanceof Error ? error.message : "Network error"}`,
        "error"
      );
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="p-8">
      <div className="max-w-7xl mx-auto">
        {/* Page Header */}
        <div className="mb-8">
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">
                Ticket Management
              </h1>
              <p className="text-gray-600 mt-1">
                Create, update, delete, and manage JIRA tickets
              </p>
            </div>
            <div className="flex space-x-3">
              <button
                onClick={fetchTickets}
                disabled={loading}
                className="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
              >
                <RefreshCw
                  className={`w-4 h-4 mr-2 ${loading ? "animate-spin" : ""}`}
                />
                Refresh
              </button>
              <button
                onClick={() => setShowCreateForm(!showCreateForm)}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                <Plus className="w-4 h-4 mr-2" />
                Create Ticket
              </button>
              <button
                onClick={() => setShowCSharpCode(!showCSharpCode)}
                className="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                <Code className="w-4 h-4 mr-2" />
                {showCSharpCode ? "Hide" : "Show"} C# Code
              </button>
            </div>
          </div>
        </div>

        {/* Alert Messages */}
        {alertMessage && (
          <div
            className={`mb-6 p-4 rounded-lg border ${
              alertType === "success"
                ? "bg-green-50 border-green-200 text-green-800"
                : "bg-red-50 border-red-200 text-red-800"
            }`}
          >
            {alertMessage}
          </div>
        )}

        {/* Created Ticket Link */}
        {lastCreatedTicket && (
          <div className="mb-6 p-4 rounded-lg border bg-blue-50 border-blue-200">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-sm font-medium text-blue-800">
                  ✅ Ticket Created: {lastCreatedTicket.key}
                </h3>
                <p className="text-sm text-blue-600 mt-1">
                  Click to view in JIRA:
                </p>
              </div>
              <div className="flex space-x-2">
                <a
                  href={lastCreatedTicket.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center px-3 py-2 border border-transparent text-sm leading-4 font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  🔗 View Ticket
                </a>
                <button
                  onClick={() => setLastCreatedTicket(null)}
                  className="inline-flex items-center px-3 py-2 border border-gray-300 text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  ✕ Dismiss
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Create Ticket Form */}
        {showCreateForm && (
          <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6 mb-8">
            <h2 className="text-xl font-semibold text-gray-900 mb-6">
              Create New Ticket
            </h2>
            <div className="space-y-6">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Project Key *
                  </label>
                  <select
                    value={createForm.projectKey}
                    onChange={(e) =>
                      setCreateForm({
                        ...createForm,
                        projectKey: e.target.value,
                      })
                    }
                    className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    required
                  >
                    <option value="KAN">KAN Project</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Issue Type *
                  </label>
                  <select
                    value={createForm.issueType}
                    onChange={(e) =>
                      setCreateForm({
                        ...createForm,
                        issueType: e.target.value,
                      })
                    }
                    className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    required
                  >
                    <option value="Task">Task</option>
                    <option value="Bug">Bug</option>
                    <option value="Story">Story</option>
                    <option value="Epic">Epic</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Summary *
                </label>
                <input
                  type="text"
                  value={createForm.summary}
                  onChange={(e) =>
                    setCreateForm({ ...createForm, summary: e.target.value })
                  }
                  className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  placeholder="Brief description of the issue"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Description
                </label>
                <textarea
                  value={createForm.description}
                  onChange={(e) =>
                    setCreateForm({
                      ...createForm,
                      description: e.target.value,
                    })
                  }
                  rows={4}
                  className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  placeholder="Detailed description of the issue"
                />
              </div>

              <div className="grid grid-cols-1 gap-4">
                {/* Priority field hidden - causes JIRA configuration issues */}

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Assignee
                  </label>
                  <input
                    type="text"
                    value={createForm.assignee}
                    onChange={(e) =>
                      setCreateForm({ ...createForm, assignee: e.target.value })
                    }
                    className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    placeholder="Enter username or email"
                  />
                </div>
              </div>

              <div className="flex space-x-4 pt-4">
                <button
                  onClick={createTicket}
                  disabled={!createForm.summary.trim()}
                  className="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-6 rounded-lg transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Create Ticket
                </button>
                <button
                  onClick={() => setShowCreateForm(false)}
                  className="bg-gray-200 hover:bg-gray-300 text-gray-900 font-medium py-2 px-6 rounded-lg transition-colors duration-200"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Tickets List */}
        <div className="bg-white rounded-lg shadow-md border border-gray-200">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">
              Project Tickets
            </h2>
          </div>

          {loading ? (
            <div className="flex items-center justify-center py-12">
              <RefreshCw className="w-8 h-8 animate-spin text-blue-600" />
              <span className="ml-3 text-lg text-gray-600">
                Loading tickets...
              </span>
            </div>
          ) : error ? (
            <div className="p-6">
              <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <p className="text-red-800">{error}</p>
              </div>
            </div>
          ) : tickets.length === 0 ? (
            <div className="p-12 text-center">
              <p className="text-gray-500 text-lg">No tickets found</p>
              <p className="text-gray-400 text-sm mt-2">
                Create your first ticket to get started
              </p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Key
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Summary
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Priority
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Type
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Updated
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {tickets.map((ticket) => (
                    <tr key={ticket.key} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <span className="text-sm font-medium text-blue-600">
                            {ticket.key}
                          </span>
                          <a
                            href={ticket.jiraUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="ml-2 text-gray-400 hover:text-gray-600"
                          >
                            <ExternalLink className="w-4 h-4" />
                          </a>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div
                          className="text-sm text-gray-900 max-w-xs truncate"
                          title={ticket.summary}
                        >
                          {ticket.summary}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-blue-100 text-blue-800">
                          {ticket.status}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span
                          className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                            ticket.priority === "Highest" ||
                            ticket.priority === "High"
                              ? "bg-red-100 text-red-800"
                              : ticket.priority === "Medium"
                              ? "bg-yellow-100 text-yellow-800"
                              : "bg-green-100 text-green-800"
                          }`}
                        >
                          {ticket.priority}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {ticket.issueType}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {formatDate(ticket.updated)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        <div className="flex space-x-2">
                          <button
                            onClick={() => {
                              setEditingTicket(ticket.key);
                              setUpdateForm({
                                summary: ticket.summary,
                                priority: ticket.priority,
                                assignee: ticket.assignee || "",
                              });
                            }}
                            className="text-blue-600 hover:text-blue-900"
                            title="Edit ticket"
                          >
                            <Edit className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => deleteTicket(ticket.key)}
                            className="text-red-600 hover:text-red-900"
                            title="Delete ticket"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => viewTicketInJira(ticket.jiraUrl)}
                            className="text-green-600 hover:text-green-900"
                            title="View in JIRA"
                          >
                            <Eye className="w-4 h-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* Edit Ticket Modal */}
        {editingTicket && (
          <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
            <div className="relative top-20 mx-auto p-5 border w-11/12 md:w-1/2 shadow-lg rounded-md bg-white">
              <div className="mb-4">
                <h3 className="text-lg font-semibold text-gray-900">
                  Edit Ticket: {editingTicket}
                </h3>
              </div>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Summary
                  </label>
                  <input
                    type="text"
                    value={updateForm.summary || ""}
                    onChange={(e) =>
                      setUpdateForm({ ...updateForm, summary: e.target.value })
                    }
                    className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Description
                  </label>
                  <textarea
                    value={updateForm.description || ""}
                    onChange={(e) =>
                      setUpdateForm({
                        ...updateForm,
                        description: e.target.value,
                      })
                    }
                    rows={3}
                    className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Priority
                    </label>
                    <select
                      value={updateForm.priority || ""}
                      onChange={(e) =>
                        setUpdateForm({
                          ...updateForm,
                          priority: e.target.value,
                        })
                      }
                      className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    >
                      <option value="Highest">Highest</option>
                      <option value="High">High</option>
                      <option value="Medium">Medium</option>
                      <option value="Low">Low</option>
                      <option value="Lowest">Lowest</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Assignee
                    </label>
                    <input
                      type="text"
                      value={updateForm.assignee || ""}
                      onChange={(e) =>
                        setUpdateForm({
                          ...updateForm,
                          assignee: e.target.value,
                        })
                      }
                      className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    />
                  </div>
                </div>
              </div>
              <div className="flex space-x-4 pt-6">
                <button
                  onClick={() => updateTicket(editingTicket)}
                  className="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-lg"
                >
                  Update Ticket
                </button>
                <button
                  onClick={() => {
                    setEditingTicket(null);
                    setUpdateForm({});
                  }}
                  className="bg-gray-200 hover:bg-gray-300 text-gray-900 font-medium py-2 px-4 rounded-lg"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        )}

        {/* C# Code Examples Section */}
        {showCSharpCode && (
          <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6 mt-8">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              C# JIRA Integration Code Examples
            </h2>

            <div className="space-y-6">
              {/* Setup & Authentication Example */}
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-3">
                  1. Setup & Authentication
                </h3>
                <CodeBlock
                  code={`// C# - JIRA API Setup & Configuration
public class JiraSettings
{
    public string BaseUrl { get; set; } = "https://your-domain.atlassian.net";
    public string Email { get; set; } = "your-email@domain.com";
    public string ApiToken { get; set; } = "your-api-token";
    public string ProjectKey { get; set; } = "YOUR-PROJECT";
}

// Program.cs or Startup.cs configuration
public void ConfigureServices(IServiceCollection services)
{
    // Configure JIRA settings
    services.Configure<JiraSettings>(Configuration.GetSection("JiraSettings"));
    
    // Configure HTTP client
    services.AddHttpClient<JiraTicketService>(client =>
    {
        client.BaseAddress = new Uri("https://your-domain.atlassian.net");
        client.Timeout = TimeSpan.FromMinutes(2);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    });
    
    services.AddScoped<JiraTicketService>();
}

// How to get API Token:
// 1. Go to https://id.atlassian.com/manage-profile/security/api-tokens
// 2. Click "Create API token"
// 3. Give it a label and click "Create"
// 4. Copy the token and store it securely

// appsettings.json
{
  "JiraSettings": {
    "BaseUrl": "https://your-domain.atlassian.net",
    "Email": "your-email@domain.com",
    "ApiToken": "your-api-token",
    "ProjectKey": "YOUR-PROJECT"
  }
}`}
                />
              </div>

              {/* Create Ticket Example */}
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-3">
                  2. Create Ticket
                </h3>
                <CodeBlock
                  code={`// C# - Create JIRA Ticket
using System.Text;
using System.Text.Json;

public class JiraTicketService
{
    private readonly HttpClient _httpClient;
    private readonly string _jiraBaseUrl = "https://your-domain.atlassian.net";
    private readonly string _email = "your-email@domain.com";
    private readonly string _apiToken = "your-api-token";

    public async Task<string> CreateTicketAsync(CreateIssueRequest request)
    {
        // Prepare authentication header
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_email}:{_apiToken}")
        );
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        // Create the request payload
        var jiraRequest = new
        {
            fields = new
            {
                project = new { key = request.ProjectKey },
                summary = request.Summary,
                description = new
                {
                    type = "doc",
                    version = 1,
                    content = new[]
                    {
                        new
                        {
                            type = "paragraph",
                            content = new[]
                            {
                                new { type = "text", text = request.Description }
                            }
                        }
                    }
                },
                issuetype = new { id = request.IssueTypeId },
                priority = new { name = request.Priority }
            }
        };

        // Send the request
        var json = JsonSerializer.Serialize(jiraRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            $"{_jiraBaseUrl}/rest/api/3/issue", 
            content
        );

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return result.GetProperty("key").GetString();
        }
        
        throw new Exception($"Failed to create ticket: {response.StatusCode}");
    }
}`}
                />
              </div>

              {/* Get Tickets Example */}
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-3">
                  3. Get Project Tickets
                </h3>
                <CodeBlock
                  code={`// C# - Get Project Tickets
public async Task<List<JiraIssue>> GetProjectTicketsAsync(string projectKey, int maxResults = 20)
{
    var credentials = Convert.ToBase64String(
        Encoding.ASCII.GetBytes($"{_email}:{_apiToken}")
    );
    _httpClient.DefaultRequestHeaders.Authorization = 
        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

    // Use JQL (JIRA Query Language) to search for tickets
    var jql = $"project = {projectKey} ORDER BY updated DESC";
    var encodedJql = Uri.EscapeDataString(jql);
    
    var response = await _httpClient.GetAsync(
        $"{_jiraBaseUrl}/rest/api/3/search/jql?jql={encodedJql}&maxResults={maxResults}"
    );

    if (response.IsSuccessStatusCode)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<JiraSearchResponse>(responseContent);
        return searchResult.Issues;
    }
    
    throw new Exception($"Failed to get tickets: {response.StatusCode}");
}

public class JiraSearchResponse
{
    public List<JiraIssue> Issues { get; set; } = new();
    public int Total { get; set; }
    public int StartAt { get; set; }
    public int MaxResults { get; set; }
}

public class JiraIssue
{
    public string Id { get; set; }
    public string Key { get; set; }
    public string Self { get; set; }
    public JiraIssueFields Fields { get; set; }
}

public class JiraIssueFields
{
    public string Summary { get; set; }
    public JiraStatus Status { get; set; }
    public JiraPriority Priority { get; set; }
    public JiraIssueType IssueType { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}`}
                />
              </div>

              {/* Update Ticket Example */}
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-3">
                  4. Update Ticket
                </h3>
                <CodeBlock
                  code={`// C# - Update JIRA Ticket
public async Task<bool> UpdateTicketAsync(string ticketKey, UpdateIssueRequest request)
{
    var credentials = Convert.ToBase64String(
        Encoding.ASCII.GetBytes($"{_email}:{_apiToken}")
    );
    _httpClient.DefaultRequestHeaders.Authorization = 
        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

    // Build update payload
    var updateRequest = new
    {
        fields = new Dictionary<string, object>()
    };

    if (!string.IsNullOrEmpty(request.Summary))
        updateRequest.fields["summary"] = request.Summary;
    
    if (!string.IsNullOrEmpty(request.Description))
        updateRequest.fields["description"] = new
        {
            type = "doc",
            version = 1,
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    content = new[] { new { type = "text", text = request.Description } }
                }
            }
        };
    
    if (!string.IsNullOrEmpty(request.Priority))
        updateRequest.fields["priority"] = new { name = request.Priority };

    var json = JsonSerializer.Serialize(updateRequest);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await _httpClient.PutAsync(
        $"{_jiraBaseUrl}/rest/api/3/issue/{ticketKey}",
        content
    );

    return response.IsSuccessStatusCode;
}

public class UpdateIssueRequest
{
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
}`}
                />
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default TicketManagement;
