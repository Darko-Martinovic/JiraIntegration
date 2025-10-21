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
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Alert,
  Tabs,
  Tab,
  Divider,
} from "@mui/material";
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  Save as SaveIcon,
  Visibility as ViewIcon,
  Code as CodeIcon,
  FilterList as FilterIcon,
} from "@mui/icons-material";

// Mock data
const mockProjects = [
  { key: "PROJ", name: "Sample Project" },
  { key: "DEV", name: "Development Project" },
  { key: "TEST", name: "Testing Project" },
];

const issueTypes = ["Bug", "Story", "Task", "Epic"];
const statuses = ["To Do", "In Progress", "Done", "Closed"];
const priorities = ["Highest", "High", "Medium", "Low", "Lowest"];
const assignees = ["John Doe", "Jane Smith", "Bob Wilson", "Alice Johnson"];

interface SearchResult {
  id: string;
  summary: string;
  status: string;
  priority: string;
  assignee: string;
  type: string;
  updated: string;
}

interface SearchFilter {
  project: string[];
  issueType: string[];
  status: string[];
  priority: string[];
  assignee: string[];
  reporter: string[];
  createdDate: { from: string; to: string };
  updatedDate: { from: string; to: string };
  labels: string[];
  components: string[];
}

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
      id={`search-tabpanel-${index}`}
      aria-labelledby={`search-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 0 }}>{children}</Box>}
    </div>
  );
}

const AdvancedSearch: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);
  const [jqlQuery, setJqlQuery] = useState("");
  const [isSearching, setIsSearching] = useState(false);
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [savedSearches] = useState([
    {
      id: 1,
      name: "My Open Issues",
      jql: "assignee = currentUser() AND status != Done",
    },
    {
      id: 2,
      name: "High Priority Bugs",
      jql: "type = Bug AND priority = High",
    },
    { id: 3, name: "Recent Updates", jql: "updated >= -7d" },
  ]);

  const [filters, setFilters] = useState<SearchFilter>({
    project: [],
    issueType: [],
    status: [],
    priority: [],
    assignee: [],
    reporter: [],
    createdDate: { from: "", to: "" },
    updatedDate: { from: "", to: "" },
    labels: [],
    components: [],
  });

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleFilterChange = (
    field: keyof SearchFilter,
    value: string[] | { from: string; to: string }
  ) => {
    setFilters((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  // Remove unused function

  const generateJQLFromFilters = () => {
    const jqlParts: string[] = [];

    if (filters.project.length > 0) {
      jqlParts.push(
        `project in (${filters.project.map((p) => `"${p}"`).join(", ")})`
      );
    }
    if (filters.issueType.length > 0) {
      jqlParts.push(
        `type in (${filters.issueType.map((t) => `"${t}"`).join(", ")})`
      );
    }
    if (filters.status.length > 0) {
      jqlParts.push(
        `status in (${filters.status.map((s) => `"${s}"`).join(", ")})`
      );
    }
    if (filters.priority.length > 0) {
      jqlParts.push(
        `priority in (${filters.priority.map((p) => `"${p}"`).join(", ")})`
      );
    }
    if (filters.assignee.length > 0) {
      jqlParts.push(
        `assignee in (${filters.assignee.map((a) => `"${a}"`).join(", ")})`
      );
    }
    if (filters.createdDate.from) {
      jqlParts.push(`created >= "${filters.createdDate.from}"`);
    }
    if (filters.createdDate.to) {
      jqlParts.push(`created <= "${filters.createdDate.to}"`);
    }

    return jqlParts.join(" AND ");
  };

  const handleSearch = async () => {
    setIsSearching(true);

    try {
      // Mock search results
      await new Promise((resolve) => setTimeout(resolve, 1000));

      const mockResults = [
        {
          id: "PROJ-123",
          summary: "Fix authentication bug",
          status: "In Progress",
          priority: "High",
          assignee: "John Doe",
          type: "Bug",
          updated: "2025-10-19",
        },
        {
          id: "PROJ-124",
          summary: "Implement dashboard feature",
          status: "To Do",
          priority: "Medium",
          assignee: "Jane Smith",
          type: "Story",
          updated: "2025-10-18",
        },
      ];

      setSearchResults(mockResults);
    } catch (error) {
      console.error("Search failed:", error);
    } finally {
      setIsSearching(false);
    }
  };

  const handleClearFilters = () => {
    setFilters({
      project: [],
      issueType: [],
      status: [],
      priority: [],
      assignee: [],
      reporter: [],
      createdDate: { from: "", to: "" },
      updatedDate: { from: "", to: "" },
      labels: [],
      components: [],
    });
    setJqlQuery("");
    setSearchResults([]);
  };

  const getStatusColor = (status: string): "success" | "info" | "default" => {
    switch (status.toLowerCase()) {
      case "done":
      case "closed":
        return "success";
      case "in progress":
        return "info";
      default:
        return "default";
    }
  };

  const getPriorityColor = (
    priority: string
  ): "error" | "warning" | "success" | "default" => {
    switch (priority.toLowerCase()) {
      case "highest":
      case "high":
        return "error";
      case "medium":
        return "warning";
      case "low":
      case "lowest":
        return "success";
      default:
        return "default";
    }
  };

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        🔍 Advanced Search
      </Typography>

      <Typography
        variant="body1"
        color="text.secondary"
        gutterBottom
        sx={{ mb: 3 }}
      >
        Search for Jira tickets using advanced filters or JQL queries
      </Typography>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Tabs value={tabValue} onChange={handleTabChange} sx={{ mb: 3 }}>
            <Tab icon={<FilterIcon />} label="Smart Filters" />
            <Tab icon={<CodeIcon />} label="JQL Query" />
            <Tab icon={<SaveIcon />} label="Saved Searches" />
          </Tabs>

          <TabPanel value={tabValue} index={0}>
            {/* Smart Filters */}
            <Grid container spacing={3}>
              <Grid item xs={12} sm={6} md={4}>
                <FormControl fullWidth>
                  <InputLabel>Project</InputLabel>
                  <Select
                    multiple
                    value={filters.project}
                    onChange={(e) =>
                      handleFilterChange("project", e.target.value)
                    }
                    label="Project"
                    renderValue={(selected) => (
                      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                        {(selected as string[]).map((value) => (
                          <Chip key={value} label={value} size="small" />
                        ))}
                      </Box>
                    )}
                  >
                    {mockProjects.map((project) => (
                      <MenuItem key={project.key} value={project.key}>
                        {project.key} - {project.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={6} md={4}>
                <FormControl fullWidth>
                  <InputLabel>Issue Type</InputLabel>
                  <Select
                    multiple
                    value={filters.issueType}
                    onChange={(e) =>
                      handleFilterChange("issueType", e.target.value)
                    }
                    label="Issue Type"
                    renderValue={(selected) => (
                      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                        {(selected as string[]).map((value) => (
                          <Chip key={value} label={value} size="small" />
                        ))}
                      </Box>
                    )}
                  >
                    {issueTypes.map((type) => (
                      <MenuItem key={type} value={type}>
                        {type}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={6} md={4}>
                <FormControl fullWidth>
                  <InputLabel>Status</InputLabel>
                  <Select
                    multiple
                    value={filters.status}
                    onChange={(e) =>
                      handleFilterChange("status", e.target.value)
                    }
                    label="Status"
                    renderValue={(selected) => (
                      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                        {(selected as string[]).map((value) => (
                          <Chip key={value} label={value} size="small" />
                        ))}
                      </Box>
                    )}
                  >
                    {statuses.map((status) => (
                      <MenuItem key={status} value={status}>
                        {status}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={6} md={4}>
                <FormControl fullWidth>
                  <InputLabel>Priority</InputLabel>
                  <Select
                    multiple
                    value={filters.priority}
                    onChange={(e) =>
                      handleFilterChange("priority", e.target.value)
                    }
                    label="Priority"
                    renderValue={(selected) => (
                      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                        {(selected as string[]).map((value) => (
                          <Chip key={value} label={value} size="small" />
                        ))}
                      </Box>
                    )}
                  >
                    {priorities.map((priority) => (
                      <MenuItem key={priority} value={priority}>
                        {priority}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={6} md={4}>
                <FormControl fullWidth>
                  <InputLabel>Assignee</InputLabel>
                  <Select
                    multiple
                    value={filters.assignee}
                    onChange={(e) =>
                      handleFilterChange("assignee", e.target.value)
                    }
                    label="Assignee"
                    renderValue={(selected) => (
                      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                        {(selected as string[]).map((value) => (
                          <Chip key={value} label={value} size="small" />
                        ))}
                      </Box>
                    )}
                  >
                    {assignees.map((assignee) => (
                      <MenuItem key={assignee} value={assignee}>
                        {assignee}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={6} md={4}>
                <TextField
                  label="Created From"
                  type="date"
                  fullWidth
                  InputLabelProps={{ shrink: true }}
                  value={filters.createdDate.from}
                  onChange={(e) =>
                    handleFilterChange("createdDate", {
                      ...filters.createdDate,
                      from: e.target.value,
                    })
                  }
                />
              </Grid>

              <Grid item xs={12} sm={6} md={4}>
                <TextField
                  label="Created To"
                  type="date"
                  fullWidth
                  InputLabelProps={{ shrink: true }}
                  value={filters.createdDate.to}
                  onChange={(e) =>
                    handleFilterChange("createdDate", {
                      ...filters.createdDate,
                      to: e.target.value,
                    })
                  }
                />
              </Grid>
            </Grid>

            {/* Generated JQL Preview */}
            <Box sx={{ mt: 3 }}>
              <Typography variant="subtitle2" gutterBottom>
                Generated JQL Query:
              </Typography>
              <Paper variant="outlined" sx={{ p: 2, bgcolor: "grey.50" }}>
                <Typography variant="body2" sx={{ fontFamily: "monospace" }}>
                  {generateJQLFromFilters() || "No filters applied"}
                </Typography>
              </Paper>
            </Box>
          </TabPanel>

          <TabPanel value={tabValue} index={1}>
            {/* JQL Query */}
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextField
                  label="JQL Query"
                  multiline
                  rows={4}
                  fullWidth
                  value={jqlQuery}
                  onChange={(e) => setJqlQuery(e.target.value)}
                  placeholder="Enter your JQL query here... e.g., project = PROJ AND assignee = currentUser()"
                  sx={{ fontFamily: "monospace" }}
                />
              </Grid>
              <Grid item xs={12}>
                <Alert severity="info">
                  <Typography variant="body2">
                    <strong>JQL Examples:</strong>
                    <br />•{" "}
                    <code>project = PROJ AND status = "In Progress"</code>
                    <br />•{" "}
                    <code>assignee = currentUser() AND priority = High</code>
                    <br />• <code>created &gt;= -7d AND type = Bug</code>
                    <br />• <code>updated &gt;= startOfWeek()</code>
                  </Typography>
                </Alert>
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={tabValue} index={2}>
            {/* Saved Searches */}
            <Grid container spacing={2}>
              {savedSearches.map((search) => (
                <Grid item xs={12} key={search.id}>
                  <Card variant="outlined">
                    <CardContent>
                      <Box
                        display="flex"
                        justifyContent="space-between"
                        alignItems="center"
                      >
                        <Box>
                          <Typography variant="h6">{search.name}</Typography>
                          <Typography
                            variant="body2"
                            color="text.secondary"
                            sx={{ fontFamily: "monospace" }}
                          >
                            {search.jql}
                          </Typography>
                        </Box>
                        <Button
                          variant="outlined"
                          size="small"
                          onClick={() => setJqlQuery(search.jql)}
                        >
                          Use Query
                        </Button>
                      </Box>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>
          </TabPanel>

          {/* Action Buttons */}
          <Box display="flex" gap={2} justifyContent="flex-end" sx={{ mt: 3 }}>
            <Button
              variant="outlined"
              startIcon={<ClearIcon />}
              onClick={handleClearFilters}
            >
              Clear All
            </Button>
            <Button
              variant="contained"
              startIcon={<SearchIcon />}
              onClick={handleSearch}
              disabled={isSearching}
            >
              {isSearching ? "Searching..." : "Search"}
            </Button>
          </Box>
        </CardContent>
      </Card>

      {/* Search Results */}
      {searchResults.length > 0 && (
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Search Results ({searchResults.length} issues found)
            </Typography>
            <Divider sx={{ mb: 2 }} />

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
                      <strong>Type</strong>
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
                      <strong>Updated</strong>
                    </TableCell>
                    <TableCell>
                      <strong>Actions</strong>
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {searchResults.map((ticket) => (
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
                        <Typography variant="body2">
                          {ticket.summary}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">{ticket.type}</Typography>
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
                        <Typography variant="body2">
                          {ticket.assignee}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {ticket.updated}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <IconButton size="small" title="View Details">
                          <ViewIcon />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      )}
    </Box>
  );
};

export default AdvancedSearch;
