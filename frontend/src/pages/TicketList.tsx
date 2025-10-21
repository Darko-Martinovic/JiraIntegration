import React, { useState } from "react";
import {
  Box,
  Typography,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  IconButton,
  Button,
  TextField,
  InputAdornment,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Tabs,
  Tab,
  Divider,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from "@mui/material";
import {
  Search as SearchIcon,
  Visibility as ViewIcon,
  Edit as EditIcon,
  Add as AddIcon,
  FilterList as FilterIcon,
  Refresh as RefreshIcon,
  Code as CodeIcon,
  ContentCopy as CopyIcon,
  Lightbulb as LightbulbIcon,
  ExpandMore as ExpandMoreIcon,
} from "@mui/icons-material";

// Type definitions
interface Ticket {
  id: string;
  summary: string;
  status: string;
  priority: string;
  assignee: string;
  reporter: string;
  created: string;
  updated: string;
  type: string;
  project: string;
}

// Mock ticket data - will be replaced with API calls later
const mockTickets: Ticket[] = [
  {
    id: "PROJ-123",
    summary: "Fix login authentication issue",
    status: "In Progress",
    priority: "High",
    assignee: "John Doe",
    reporter: "Jane Smith",
    created: "2025-10-15",
    updated: "2025-10-19",
    type: "Bug",
    project: "PROJECT",
  },
  {
    id: "PROJ-124",
    summary: "Implement new dashboard feature",
    status: "To Do",
    priority: "Medium",
    assignee: "Alice Johnson",
    reporter: "Bob Wilson",
    created: "2025-10-18",
    updated: "2025-10-18",
    type: "Story",
    project: "PROJECT",
  },
  {
    id: "PROJ-125",
    summary: "Update API documentation",
    status: "Done",
    priority: "Low",
    assignee: "Charlie Brown",
    reporter: "Diana Ross",
    created: "2025-10-10",
    updated: "2025-10-17",
    type: "Task",
    project: "PROJECT",
  },
];

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const TicketList: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedTicket, setSelectedTicket] = useState<Ticket | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [tabValue, setTabValue] = useState(0);
  const [tickets] = useState(mockTickets);

  // Generate C# code example based on current search and filter operations
  const generateCodeExample = () => {
    return `// Ticket List - C# Implementation Example

// 1. Service Setup
public class JiraTicketService : BaseJiraHttpService
{
    public JiraTicketService(JiraSettings settings) : base(settings) { }

    // Get all tickets with optional search filter
    public async Task<List<JiraTicket>> GetTicketsAsync(string searchTerm = "")
    {
        var jql = string.IsNullOrEmpty(searchTerm) 
            ? "order by created DESC" 
            : $"(summary ~ \\"{searchTerm}\\" OR key ~ \\"{searchTerm}\\") order by created DESC";
            
        var request = new RestRequest($"rest/api/2/search", Method.Get);
        request.AddParameter("jql", jql);
        request.AddParameter("maxResults", 100);
        request.AddParameter("fields", "id,key,summary,status,priority,assignee,reporter,created,updated,issuetype");
        
        var response = await ExecuteAsync<JiraSearchResponse>(request);
        return response?.Issues ?? new List<JiraTicket>();
    }
    
    // Get specific ticket by key
    public async Task<JiraTicket> GetTicketByKeyAsync(string ticketKey)
    {
        var request = new RestRequest($"rest/api/2/issue/{ticketKey}", Method.Get);
        return await ExecuteAsync<JiraTicket>(request);
    }
}

// 2. Search Implementation${
      searchTerm
        ? `
// Current search: "${searchTerm}"
var searchResults = await jiraService.GetTicketsAsync("${searchTerm}");`
        : `
// No active search - getting all tickets
var allTickets = await jiraService.GetTicketsAsync();`
    }

// 3. Data Model
public class JiraTicket
{
    public string Id { get; set; }
    public string Key { get; set; }
    public string Summary { get; set; }
    public JiraStatus Status { get; set; }
    public JiraPriority Priority { get; set; }
    public JiraUser Assignee { get; set; }
    public JiraUser Reporter { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public JiraIssueType IssueType { get; set; }
}

// 4. Usage in Console Application
static async Task Main(string[] args)
{
    var settings = EnvironmentLoader.LoadJiraSettings();
    var ticketService = new JiraTicketService(settings);
    
    try 
    {
        Console.WriteLine("Fetching Jira tickets...");
        var tickets = await ticketService.GetTicketsAsync("${
          searchTerm || "your-search-term"
        }");
        
        Console.WriteLine($"Found {tickets.Count} tickets:");
        foreach (var ticket in tickets)
        {
            Console.WriteLine($"{ticket.Key}: {ticket.Summary} [{ticket.Status.Name}]");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}`;
  };

  const copyToClipboard = async () => {
    try {
      await navigator.clipboard.writeText(generateCodeExample());
      // You could add a toast notification here
    } catch (err) {
      console.error("Failed to copy code to clipboard:", err);
    }
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleViewTicket = (ticket: Ticket) => {
    setSelectedTicket(ticket);
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setSelectedTicket(null);
  };

  const getStatusColor = (status: string): "success" | "info" | "default" => {
    switch (status.toLowerCase()) {
      case "done":
        return "success";
      case "in progress":
        return "info";
      case "to do":
        return "default";
      default:
        return "default";
    }
  };

  const getPriorityColor = (
    priority: string
  ): "error" | "warning" | "success" | "default" => {
    switch (priority.toLowerCase()) {
      case "high":
        return "error";
      case "medium":
        return "warning";
      case "low":
        return "success";
      default:
        return "default";
    }
  };

  const filteredTickets = tickets.filter(
    (ticket) =>
      ticket.summary.toLowerCase().includes(searchTerm.toLowerCase()) ||
      ticket.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
      ticket.assignee.toLowerCase().includes(searchTerm.toLowerCase())
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
          🎫 Ticket Management
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} color="primary">
          Create Ticket
        </Button>
      </Box>

      {/* Search and Filter Bar */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box display="flex" gap={2} alignItems="center">
            <TextField
              placeholder="Search tickets by ID, summary, or assignee..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              variant="outlined"
              size="small"
              sx={{ flexGrow: 1 }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon />
                  </InputAdornment>
                ),
              }}
            />
            <Button variant="outlined" startIcon={<FilterIcon />} size="small">
              Filters
            </Button>
            <Button variant="outlined" startIcon={<RefreshIcon />} size="small">
              Refresh
            </Button>
          </Box>
        </CardContent>
      </Card>

      {/* C# Code Example Panel */}
      <Accordion sx={{ mb: 3, border: "1px solid #e0e0e0" }}>
        <AccordionSummary
          expandIcon={<ExpandMoreIcon />}
          sx={{
            backgroundColor: "#f5f5f5",
            "&:hover": { backgroundColor: "#eeeeee" },
          }}
        >
          <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
            <LightbulbIcon sx={{ color: "#1976d2", fontSize: 20 }} />
            <Typography variant="subtitle1" sx={{ fontWeight: "medium" }}>
              🎯 C# Implementation Guide - Ticket Search & Management
            </Typography>
          </Box>
        </AccordionSummary>
        <AccordionDetails sx={{ p: 0 }}>
          <Box sx={{ position: "relative" }}>
            <IconButton
              onClick={copyToClipboard}
              sx={{
                position: "absolute",
                top: 8,
                right: 8,
                zIndex: 1,
                backgroundColor: "rgba(255, 255, 255, 0.8)",
                "&:hover": { backgroundColor: "rgba(255, 255, 255, 0.95)" },
              }}
              size="small"
              title="Copy to clipboard"
            >
              <CopyIcon fontSize="small" />
            </IconButton>
            <Box
              component="pre"
              sx={{
                backgroundColor: "#1e1e1e",
                color: "#d4d4d4",
                p: 2,
                m: 0,
                fontSize: "13px",
                fontFamily: '"Consolas", "Monaco", "Courier New", monospace',
                overflow: "auto",
                maxHeight: "500px",
                whiteSpace: "pre-wrap",
                borderRadius: 0,
              }}
            >
              {generateCodeExample()}
            </Box>
          </Box>
        </AccordionDetails>
      </Accordion>

      {/* Tickets Table */}
      <Card>
        <CardContent>
          <TableContainer component={Paper} elevation={0}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>
                    <strong>Key</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Summary</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Status</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Priority</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Assignee</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Type</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Updated</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Actions</strong>
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredTickets.map((ticket) => (
                  <TableRow key={ticket.id} hover>
                    <TableCell>
                      <Typography
                        variant="body2"
                        color="primary"
                        sx={{ fontWeight: "bold" }}
                      >
                        {ticket.id}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{ticket.summary}</Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={ticket.status}
                        color={getStatusColor(ticket.status)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={ticket.priority}
                        color={getPriorityColor(ticket.priority)}
                        size="small"
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{ticket.assignee}</Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{ticket.type}</Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{ticket.updated}</Typography>
                    </TableCell>
                    <TableCell>
                      <IconButton
                        size="small"
                        onClick={() => handleViewTicket(ticket)}
                        title="View Details"
                      >
                        <ViewIcon />
                      </IconButton>
                      <IconButton size="small" title="Edit Ticket">
                        <EditIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          {filteredTickets.length === 0 && (
            <Box textAlign="center" py={4}>
              <Typography variant="body1" color="text.secondary">
                No tickets found matching your search criteria.
              </Typography>
            </Box>
          )}
        </CardContent>
      </Card>

      {/* Ticket Details Dialog */}
      <Dialog
        open={dialogOpen}
        onClose={handleCloseDialog}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Box
            display="flex"
            justifyContent="space-between"
            alignItems="center"
          >
            <Typography variant="h6">
              {selectedTicket?.id} - Ticket Details
            </Typography>
          </Box>
        </DialogTitle>
        <DialogContent>
          {selectedTicket && (
            <Box>
              <Tabs value={tabValue} onChange={handleTabChange}>
                <Tab label="Details" />
                <Tab label="Comments" />
                <Tab label="History" />
              </Tabs>

              <TabPanel value={tabValue} index={0}>
                <Box>
                  <Typography variant="h6" gutterBottom>
                    {selectedTicket.summary}
                  </Typography>
                  <Divider sx={{ mb: 3 }} />

                  <Box
                    sx={{ display: "flex", flexDirection: "column", gap: 2 }}
                  >
                    <Box sx={{ display: "flex", gap: 4 }}>
                      <Box>
                        <Typography variant="subtitle2" color="text.secondary">
                          Status
                        </Typography>
                        <Chip
                          label={selectedTicket.status}
                          color={getStatusColor(selectedTicket.status)}
                          size="small"
                        />
                      </Box>
                      <Box>
                        <Typography variant="subtitle2" color="text.secondary">
                          Priority
                        </Typography>
                        <Chip
                          label={selectedTicket.priority}
                          color={getPriorityColor(selectedTicket.priority)}
                          size="small"
                          variant="outlined"
                        />
                      </Box>
                    </Box>

                    <Box sx={{ display: "flex", gap: 4 }}>
                      <Box>
                        <Typography variant="subtitle2" color="text.secondary">
                          Assignee
                        </Typography>
                        <Typography variant="body1">
                          {selectedTicket.assignee}
                        </Typography>
                      </Box>
                      <Box>
                        <Typography variant="subtitle2" color="text.secondary">
                          Reporter
                        </Typography>
                        <Typography variant="body1">
                          {selectedTicket.reporter}
                        </Typography>
                      </Box>
                    </Box>

                    <Box sx={{ display: "flex", gap: 4 }}>
                      <Box>
                        <Typography variant="subtitle2" color="text.secondary">
                          Type
                        </Typography>
                        <Typography variant="body1">
                          {selectedTicket.type}
                        </Typography>
                      </Box>
                      <Box>
                        <Typography variant="subtitle2" color="text.secondary">
                          Project
                        </Typography>
                        <Typography variant="body1">
                          {selectedTicket.project}
                        </Typography>
                      </Box>
                    </Box>

                    <Box sx={{ display: "flex", gap: 4 }}>
                      <Box>
                        <Typography variant="subtitle2" color="text.secondary">
                          Created
                        </Typography>
                        <Typography variant="body1">
                          {selectedTicket.created}
                        </Typography>
                      </Box>
                      <Box>
                        <Typography variant="subtitle2" color="text.secondary">
                          Updated
                        </Typography>
                        <Typography variant="body1">
                          {selectedTicket.updated}
                        </Typography>
                      </Box>
                    </Box>
                  </Box>
                </Box>
              </TabPanel>

              <TabPanel value={tabValue} index={1}>
                <Typography variant="body1" color="text.secondary">
                  Comments feature will be implemented here...
                </Typography>
              </TabPanel>

              <TabPanel value={tabValue} index={2}>
                <Typography variant="body1" color="text.secondary">
                  History feature will be implemented here...
                </Typography>
              </TabPanel>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Close</Button>
          <Button variant="contained" color="primary">
            Edit Ticket
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default TicketList;
