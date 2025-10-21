import React, { useState } from "react";
import {
  Box,
  Typography,
  Card,
  CardContent,
  TextField,
  Button,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  Alert,
  Paper,
  Divider,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  IconButton,
  Tooltip,
  Collapse,
} from "@mui/material";
import {
  Save as SaveIcon,
  Clear as ClearIcon,
  Code as CodeIcon,
  ExpandMore as ExpandMoreIcon,
  ContentCopy as CopyIcon,
  Lightbulb as LightbulbIcon,
} from "@mui/icons-material";

// Mock data - will be replaced with API calls later
const mockProjects = [
  { key: "PROJ", name: "Sample Project", id: "1" },
  { key: "DEV", name: "Development Project", id: "2" },
  { key: "TEST", name: "Testing Project", id: "3" },
];

const issueTypes = [
  { id: "1", name: "Bug", iconUrl: "" },
  { id: "2", name: "Story", iconUrl: "" },
  { id: "3", name: "Task", iconUrl: "" },
  { id: "4", name: "Epic", iconUrl: "" },
];

const priorities = [
  { id: "1", name: "Highest", color: "error" },
  { id: "2", name: "High", color: "error" },
  { id: "3", name: "Medium", color: "warning" },
  { id: "4", name: "Low", color: "success" },
  { id: "5", name: "Lowest", color: "success" },
];

const mockUsers = [
  { accountId: "1", displayName: "John Doe", emailAddress: "john@example.com" },
  {
    accountId: "2",
    displayName: "Jane Smith",
    emailAddress: "jane@example.com",
  },
  {
    accountId: "3",
    displayName: "Bob Wilson",
    emailAddress: "bob@example.com",
  },
];

interface CreateTicketForm {
  project: string;
  issueType: string;
  summary: string;
  description: string;
  priority: string;
  assignee: string;
  labels: string[];
  components: string[];
}

const CreateTicket: React.FC = () => {
  const [formData, setFormData] = useState<CreateTicketForm>({
    project: "",
    issueType: "",
    summary: "",
    description: "",
    priority: "3", // Default to Medium
    assignee: "",
    labels: [],
    components: [],
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showCodeExample, setShowCodeExample] = useState(false);
  const [successMessage, setSuccessMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const handleInputChange = (
    field: keyof CreateTicketForm,
    value: string | string[]
  ) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Validation
    if (!formData.project || !formData.issueType || !formData.summary) {
      setErrorMessage(
        "Please fill in all required fields (Project, Issue Type, Summary)"
      );
      return;
    }

    setIsSubmitting(true);
    setErrorMessage("");
    setSuccessMessage("");

    try {
      // Mock API call - replace with actual API integration
      await new Promise((resolve) => setTimeout(resolve, 1000));

      const ticketKey = `${formData.project}-${Math.floor(
        Math.random() * 1000
      )}`;
      setSuccessMessage(`Ticket ${ticketKey} created successfully!`);

      // Reset form after successful creation
      setFormData({
        project: "",
        issueType: "",
        summary: "",
        description: "",
        priority: "3",
        assignee: "",
        labels: [],
        components: [],
      });
    } catch {
      setErrorMessage("Failed to create ticket. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClear = () => {
    setFormData({
      project: "",
      issueType: "",
      summary: "",
      description: "",
      priority: "3",
      assignee: "",
      labels: [],
      components: [],
    });
    setErrorMessage("");
    setSuccessMessage("");
  };

  const handleCopyCode = () => {
    const codeExample = generateCodeExample();
    navigator.clipboard.writeText(codeExample);
  };

  const generateCodeExample = () => {
    const projectKey = formData.project || "PROJECT_KEY";
    const selectedIssueType = issueTypes.find(
      (t) => t.id === formData.issueType
    );
    const selectedPriority = priorities.find((p) => p.id === formData.priority);
    const selectedAssignee = mockUsers.find(
      (u) => u.accountId === formData.assignee
    );

    return `// C# Example: Create Jira Ticket
using JiraIntegration.Services;
using JiraIntegration.Models.Requests;

// Initialize the Jira service
var jiraService = new JiraTicketService();

// Create the ticket request
var createRequest = new CreateIssueRequest
{
    ProjectKey = "${projectKey}",
    IssueType = "${selectedIssueType?.name || "Bug"}",
    Summary = "${formData.summary || "Your ticket summary"}",
    Description = "${
      formData.description || "Detailed description of the issue"
    }",
    Priority = "${selectedPriority?.name || "Medium"}",
    ${
      formData.assignee
        ? `Assignee = "${
            selectedAssignee?.emailAddress || "assignee@example.com"
          }",`
        : '// Assignee = "assignee@example.com", // Optional'
    }
};

// Execute the request
try 
{
    var response = await jiraService.CreateTicketAsync(createRequest);
    
    if (response.Success)
    {
        Console.WriteLine($"Ticket created successfully: {response.Data.Key}");
        Console.WriteLine($"Ticket URL: {response.Data.Self}");
    }
    else
    {
        Console.WriteLine($"Error: {response.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}");
}`;
  };

  const getPriorityColor = (
    priorityId: string
  ): "error" | "warning" | "success" | "default" => {
    const priority = priorities.find((p) => p.id === priorityId);
    return (priority?.color as "error" | "warning" | "success") || "default";
  };

  const selectedProject = mockProjects.find((p) => p.key === formData.project);
  const selectedIssueType = issueTypes.find((t) => t.id === formData.issueType);
  const selectedPriority = priorities.find((p) => p.id === formData.priority);
  const selectedAssignee = mockUsers.find(
    (u) => u.accountId === formData.assignee
  );

  return (
    <Box>
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        mb={3}
      >
        <Typography variant="h4" component="h1">
          ➕ Create New Ticket
        </Typography>
        <Button
          variant="outlined"
          startIcon={<CodeIcon />}
          onClick={() => setShowCodeExample(!showCodeExample)}
          size="small"
        >
          {showCodeExample ? "Hide Code" : "Show C# Code"}
        </Button>
      </Box>

      <Typography
        variant="body1"
        color="text.secondary"
        gutterBottom
        sx={{ mb: 3 }}
      >
        Fill out the form below to create a new Jira ticket. Click "Show C#
        Code" to see the implementation.
      </Typography>

      {successMessage && (
        <Alert severity="success" sx={{ mb: 3 }}>
          {successMessage}
        </Alert>
      )}

      {errorMessage && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {errorMessage}
        </Alert>
      )}

      {/* Code Example Section */}
      <Collapse in={showCodeExample}>
        <Card sx={{ mb: 3, bgcolor: "#1e1e1e" }}>
          <CardContent>
            <Box
              display="flex"
              justifyContent="space-between"
              alignItems="center"
              mb={2}
            >
              <Box display="flex" alignItems="center" gap={1}>
                <LightbulbIcon sx={{ color: "#ffd700" }} />
                <Typography variant="h6" sx={{ color: "#ffffff" }}>
                  C# Implementation Guide
                </Typography>
              </Box>
              <Tooltip title="Copy to clipboard">
                <IconButton onClick={handleCopyCode} sx={{ color: "#ffffff" }}>
                  <CopyIcon />
                </IconButton>
              </Tooltip>
            </Box>
            <Typography variant="body2" sx={{ color: "#cccccc", mb: 2 }}>
              This code shows how to create a Jira ticket programmatically using
              our C# service:
            </Typography>
            <Paper
              variant="outlined"
              sx={{
                p: 2,
                bgcolor: "#2d2d2d",
                border: "1px solid #444",
                fontFamily: 'Consolas, Monaco, "Courier New", monospace',
              }}
            >
              <pre
                style={{
                  margin: 0,
                  color: "#ffffff",
                  fontSize: "13px",
                  lineHeight: "1.4",
                  overflow: "auto",
                }}
              >
                {generateCodeExample()}
              </pre>
            </Paper>
          </CardContent>
        </Card>
      </Collapse>

      {/* Main Form - Fixed Layout */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Ticket Details
          </Typography>
          <Divider sx={{ mb: 3 }} />

          <Box component="form" onSubmit={handleSubmit}>
            <Grid container spacing={3}>
              {/* Row 1: Project and Issue Type */}
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth required>
                  <InputLabel>Project</InputLabel>
                  <Select
                    value={formData.project}
                    onChange={(e) =>
                      handleInputChange("project", e.target.value)
                    }
                    label="Project"
                  >
                    {mockProjects.map((project) => (
                      <MenuItem key={project.id} value={project.key}>
                        {project.key} - {project.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={6}>
                <FormControl fullWidth required>
                  <InputLabel>Issue Type</InputLabel>
                  <Select
                    value={formData.issueType}
                    onChange={(e) =>
                      handleInputChange("issueType", e.target.value)
                    }
                    label="Issue Type"
                  >
                    {issueTypes.map((type) => (
                      <MenuItem key={type.id} value={type.id}>
                        {type.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              {/* Row 2: Summary - Full Width */}
              <Grid item xs={12}>
                <TextField
                  label="Summary"
                  required
                  fullWidth
                  value={formData.summary}
                  onChange={(e) => handleInputChange("summary", e.target.value)}
                  placeholder="Brief description of the issue"
                />
              </Grid>

              {/* Row 3: Description - Full Width, Fixed Height */}
              <Grid item xs={12}>
                <TextField
                  label="Description"
                  multiline
                  rows={6}
                  fullWidth
                  value={formData.description}
                  onChange={(e) =>
                    handleInputChange("description", e.target.value)
                  }
                  placeholder="Detailed description of the issue..."
                />
              </Grid>

              {/* Row 4: Priority and Assignee */}
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth>
                  <InputLabel>Priority</InputLabel>
                  <Select
                    value={formData.priority}
                    onChange={(e) =>
                      handleInputChange("priority", e.target.value)
                    }
                    label="Priority"
                  >
                    {priorities.map((priority) => (
                      <MenuItem key={priority.id} value={priority.id}>
                        {priority.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={6}>
                <FormControl fullWidth>
                  <InputLabel>Assignee</InputLabel>
                  <Select
                    value={formData.assignee}
                    onChange={(e) =>
                      handleInputChange("assignee", e.target.value)
                    }
                    label="Assignee"
                  >
                    <MenuItem value="">
                      <em>Unassigned</em>
                    </MenuItem>
                    {mockUsers.map((user) => (
                      <MenuItem key={user.accountId} value={user.accountId}>
                        {user.displayName} ({user.emailAddress})
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              {/* Row 5: Preview Summary (Fixed Height) */}
              <Grid item xs={12}>
                <Card
                  variant="outlined"
                  sx={{ minHeight: "120px", bgcolor: "#f8f9fa" }}
                >
                  <CardContent>
                    <Typography
                      variant="subtitle2"
                      color="text.secondary"
                      gutterBottom
                    >
                      Preview Summary
                    </Typography>
                    {formData.summary ? (
                      <Box>
                        <Typography variant="body1" gutterBottom>
                          <strong>{formData.summary}</strong>
                        </Typography>
                        <Box display="flex" gap={1} flexWrap="wrap">
                          {selectedProject && (
                            <Chip
                              label={`Project: ${selectedProject.key}`}
                              size="small"
                            />
                          )}
                          {selectedIssueType && (
                            <Chip
                              label={`Type: ${selectedIssueType.name}`}
                              size="small"
                            />
                          )}
                          {selectedPriority && (
                            <Chip
                              label={`Priority: ${selectedPriority.name}`}
                              color={getPriorityColor(formData.priority)}
                              size="small"
                            />
                          )}
                          {selectedAssignee && (
                            <Chip
                              label={`Assignee: ${selectedAssignee.displayName}`}
                              size="small"
                            />
                          )}
                        </Box>
                      </Box>
                    ) : (
                      <Typography variant="body2" color="text.secondary">
                        Fill out the form to see a preview of your ticket
                      </Typography>
                    )}
                  </CardContent>
                </Card>
              </Grid>

              {/* Row 6: Action Buttons - Fixed Position */}
              <Grid item xs={12}>
                <Box display="flex" gap={2} justifyContent="flex-end" pt={2}>
                  <Button
                    type="button"
                    variant="outlined"
                    startIcon={<ClearIcon />}
                    onClick={handleClear}
                  >
                    Clear
                  </Button>
                  <Button
                    type="submit"
                    variant="contained"
                    startIcon={<SaveIcon />}
                    disabled={isSubmitting}
                  >
                    {isSubmitting ? "Creating..." : "Create Ticket"}
                  </Button>
                </Box>
              </Grid>
            </Grid>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default CreateTicket;
