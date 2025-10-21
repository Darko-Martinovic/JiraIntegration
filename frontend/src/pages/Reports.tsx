import React, { useState } from "react";
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Paper,
  Divider,
  Chip,
  List,
  ListItem,
  ListItemText,
  LinearProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from "@mui/material";
import {
  TrendingUp as TrendingUpIcon,
  BugReport as BugReportIcon,
  Assignment as AssignmentIcon,
  Person as PersonIcon,
  Schedule as ScheduleIcon,
  Download as DownloadIcon,
  Refresh as RefreshIcon,
} from "@mui/icons-material";

// Mock data for reports
const mockProjectData = {
  totalIssues: 156,
  resolvedIssues: 89,
  openIssues: 67,
  averageResolutionTime: "3.2 days",
  topAssignees: [
    { name: "John Doe", issues: 23, resolved: 18 },
    { name: "Jane Smith", issues: 19, resolved: 15 },
    { name: "Bob Wilson", issues: 16, resolved: 12 },
  ],
  issuesByType: [
    { type: "Bug", count: 45, resolved: 32 },
    { type: "Story", count: 38, resolved: 25 },
    { type: "Task", count: 42, resolved: 21 },
    { type: "Epic", count: 31, resolved: 11 },
  ],
  issuesByPriority: [
    { priority: "Highest", count: 8, color: "error" },
    { priority: "High", count: 24, color: "error" },
    { priority: "Medium", count: 78, color: "warning" },
    { priority: "Low", count: 34, color: "success" },
    { priority: "Lowest", count: 12, color: "success" },
  ],
  recentActivity: [
    { date: "2025-10-19", action: "PROJ-123 resolved by John Doe" },
    { date: "2025-10-19", action: "PROJ-124 created by Jane Smith" },
    { date: "2025-10-18", action: "PROJ-125 assigned to Bob Wilson" },
    { date: "2025-10-18", action: "PROJ-126 transitioned to In Progress" },
    { date: "2025-10-17", action: "PROJ-127 commented by Alice Johnson" },
  ],
  velocityData: [
    { sprint: "Sprint 1", planned: 20, completed: 18 },
    { sprint: "Sprint 2", planned: 22, completed: 20 },
    { sprint: "Sprint 3", planned: 25, completed: 19 },
    { sprint: "Sprint 4", planned: 23, completed: 21 },
  ],
};

const mockProjects = [
  { key: "PROJ", name: "Sample Project" },
  { key: "DEV", name: "Development Project" },
  { key: "TEST", name: "Testing Project" },
];

const Reports: React.FC = () => {
  const [selectedProject, setSelectedProject] = useState("PROJ");
  const [selectedTimeRange, setSelectedTimeRange] = useState("30");
  const [isLoading, setIsLoading] = useState(false);

  const handleRefresh = async () => {
    setIsLoading(true);
    try {
      // Mock API call
      await new Promise((resolve) => setTimeout(resolve, 1000));
    } catch (error) {
      console.error("Refresh failed:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleExport = () => {
    // Mock export functionality
    console.log("Exporting report...");
  };

  const getProgressPercentage = (resolved: number, total: number) => {
    return Math.round((resolved / total) * 100);
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
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        mb={3}
      >
        <Typography variant="h4" component="h1">
          📊 Reports & Analytics
        </Typography>
        <Box display="flex" gap={1}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
            disabled={isLoading}
          >
            Refresh
          </Button>
          <Button
            variant="outlined"
            startIcon={<DownloadIcon />}
            onClick={handleExport}
          >
            Export
          </Button>
        </Box>
      </Box>

      <Typography
        variant="body1"
        color="text.secondary"
        gutterBottom
        sx={{ mb: 3 }}
      >
        View project analytics, team performance, and issue trends
      </Typography>

      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={3} alignItems="center">
            <Grid item xs={12} sm={6} md={4}>
              <FormControl fullWidth>
                <InputLabel>Project</InputLabel>
                <Select
                  value={selectedProject}
                  onChange={(e) => setSelectedProject(e.target.value)}
                  label="Project"
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
                <InputLabel>Time Range</InputLabel>
                <Select
                  value={selectedTimeRange}
                  onChange={(e) => setSelectedTimeRange(e.target.value)}
                  label="Time Range"
                >
                  <MenuItem value="7">Last 7 days</MenuItem>
                  <MenuItem value="30">Last 30 days</MenuItem>
                  <MenuItem value="90">Last 3 months</MenuItem>
                  <MenuItem value="365">Last year</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {isLoading && <LinearProgress sx={{ mb: 2 }} />}

      {/* Key Metrics */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" gap={1} mb={1}>
                <AssignmentIcon color="primary" />
                <Typography variant="h4">
                  {mockProjectData.totalIssues}
                </Typography>
              </Box>
              <Typography variant="body2" color="text.secondary">
                Total Issues
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" gap={1} mb={1}>
                <TrendingUpIcon color="success" />
                <Typography variant="h4">
                  {mockProjectData.resolvedIssues}
                </Typography>
              </Box>
              <Typography variant="body2" color="text.secondary">
                Resolved Issues
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" gap={1} mb={1}>
                <BugReportIcon color="warning" />
                <Typography variant="h4">
                  {mockProjectData.openIssues}
                </Typography>
              </Box>
              <Typography variant="body2" color="text.secondary">
                Open Issues
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" gap={1} mb={1}>
                <ScheduleIcon color="info" />
                <Typography variant="h4">
                  {mockProjectData.averageResolutionTime}
                </Typography>
              </Box>
              <Typography variant="body2" color="text.secondary">
                Avg Resolution Time
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        {/* Issues by Type */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Issues by Type
              </Typography>
              <Divider sx={{ mb: 2 }} />

              {mockProjectData.issuesByType.map((item, index) => (
                <Box key={index} sx={{ mb: 2 }}>
                  <Box
                    display="flex"
                    justifyContent="space-between"
                    alignItems="center"
                    mb={1}
                  >
                    <Typography variant="body2">{item.type}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {item.resolved}/{item.count} (
                      {getProgressPercentage(item.resolved, item.count)}%)
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={getProgressPercentage(item.resolved, item.count)}
                    sx={{ height: 8, borderRadius: 4 }}
                  />
                </Box>
              ))}
            </CardContent>
          </Card>
        </Grid>

        {/* Issues by Priority */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Issues by Priority
              </Typography>
              <Divider sx={{ mb: 2 }} />

              <Box display="flex" flexDirection="column" gap={1}>
                {mockProjectData.issuesByPriority.map((item, index) => (
                  <Box
                    key={index}
                    display="flex"
                    justifyContent="space-between"
                    alignItems="center"
                  >
                    <Typography variant="body2">{item.priority}</Typography>
                    <Chip
                      label={item.count}
                      color={getPriorityColor(item.priority)}
                      size="small"
                    />
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Top Assignees */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Top Assignees
              </Typography>
              <Divider sx={{ mb: 2 }} />

              <List dense>
                {mockProjectData.topAssignees.map((assignee, index) => (
                  <ListItem key={index} divider>
                    <PersonIcon color="primary" sx={{ mr: 2 }} />
                    <ListItemText
                      primary={assignee.name}
                      secondary={`${assignee.resolved}/${assignee.issues} issues resolved`}
                    />
                    <Chip
                      label={`${getProgressPercentage(
                        assignee.resolved,
                        assignee.issues
                      )}%`}
                      color="primary"
                      size="small"
                    />
                  </ListItem>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Activity */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Recent Activity
              </Typography>
              <Divider sx={{ mb: 2 }} />

              <List dense>
                {mockProjectData.recentActivity.map((activity, index) => (
                  <ListItem key={index} divider>
                    <ListItemText
                      primary={activity.action}
                      secondary={activity.date}
                    />
                  </ListItem>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid>

        {/* Sprint Velocity */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Sprint Velocity
              </Typography>
              <Divider sx={{ mb: 2 }} />

              <TableContainer component={Paper} elevation={0}>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>
                        <strong>Sprint</strong>
                      </TableCell>
                      <TableCell>
                        <strong>Planned</strong>
                      </TableCell>
                      <TableCell>
                        <strong>Completed</strong>
                      </TableCell>
                      <TableCell>
                        <strong>Completion Rate</strong>
                      </TableCell>
                      <TableCell>
                        <strong>Progress</strong>
                      </TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {mockProjectData.velocityData.map((sprint, index) => (
                      <TableRow key={index} hover>
                        <TableCell>{sprint.sprint}</TableCell>
                        <TableCell>{sprint.planned}</TableCell>
                        <TableCell>{sprint.completed}</TableCell>
                        <TableCell>
                          <Chip
                            label={`${getProgressPercentage(
                              sprint.completed,
                              sprint.planned
                            )}%`}
                            color={
                              sprint.completed >= sprint.planned
                                ? "success"
                                : "warning"
                            }
                            size="small"
                          />
                        </TableCell>
                        <TableCell width="200px">
                          <LinearProgress
                            variant="determinate"
                            value={Math.min(
                              getProgressPercentage(
                                sprint.completed,
                                sprint.planned
                              ),
                              100
                            )}
                            sx={{ height: 8, borderRadius: 4 }}
                            color={
                              sprint.completed >= sprint.planned
                                ? "success"
                                : "warning"
                            }
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Reports;
