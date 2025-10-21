import React, { useState } from "react";
import {
  Box,
  Typography,
  Card,
  CardContent,
  TextField,
  Button,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Avatar,
  Chip,
  InputAdornment,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Tabs,
  Tab,
  Divider,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Alert,
} from "@mui/material";
import {
  Search as SearchIcon,
  Person as PersonIcon,
  Email as EmailIcon,
  Business as BusinessIcon,
  Assignment as AssignmentIcon,
  Visibility as ViewIcon,
  Refresh as RefreshIcon,
} from "@mui/icons-material";

// Mock data
const mockUsers = [
  {
    accountId: "1",
    displayName: "John Doe",
    emailAddress: "john.doe@company.com",
    active: true,
    accountType: "atlassian",
    avatarUrls: { "48x48": "" },
    locale: "en_US",
    timeZone: "America/New_York",
    groups: ["developers", "administrators"],
    assignedIssues: 15,
    reportedIssues: 8,
  },
  {
    accountId: "2",
    displayName: "Jane Smith",
    emailAddress: "jane.smith@company.com",
    active: true,
    accountType: "atlassian",
    avatarUrls: { "48x48": "" },
    locale: "en_US",
    timeZone: "America/Los_Angeles",
    groups: ["developers", "testers"],
    assignedIssues: 23,
    reportedIssues: 12,
  },
  {
    accountId: "3",
    displayName: "Bob Wilson",
    emailAddress: "bob.wilson@company.com",
    active: true,
    accountType: "atlassian",
    avatarUrls: { "48x48": "" },
    locale: "en_GB",
    timeZone: "Europe/London",
    groups: ["managers", "developers"],
    assignedIssues: 7,
    reportedIssues: 18,
  },
  {
    accountId: "4",
    displayName: "Alice Johnson",
    emailAddress: "alice.johnson@company.com",
    active: false,
    accountType: "atlassian",
    avatarUrls: { "48x48": "" },
    locale: "en_US",
    timeZone: "America/Chicago",
    groups: ["developers"],
    assignedIssues: 0,
    reportedIssues: 3,
  },
];

interface User {
  accountId: string;
  displayName: string;
  emailAddress: string;
  active: boolean;
  accountType: string;
  avatarUrls: { "48x48": string };
  locale: string;
  timeZone: string;
  groups: string[];
  assignedIssues: number;
  reportedIssues: number;
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
      id={`user-tabpanel-${index}`}
      aria-labelledby={`user-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const UserManagement: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [users, setUsers] = useState<User[]>(mockUsers);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [tabValue, setTabValue] = useState(0);
  const [isSearching, setIsSearching] = useState(false);
  const [showActiveOnly, setShowActiveOnly] = useState(true);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleSearch = async () => {
    if (!searchTerm.trim()) return;

    setIsSearching(true);
    try {
      // Mock API call
      await new Promise((resolve) => setTimeout(resolve, 1000));

      // Filter users based on search term
      const filteredUsers = mockUsers.filter(
        (user) =>
          user.displayName.toLowerCase().includes(searchTerm.toLowerCase()) ||
          user.emailAddress.toLowerCase().includes(searchTerm.toLowerCase())
      );

      setUsers(filteredUsers);
    } catch (error) {
      console.error("Search failed:", error);
    } finally {
      setIsSearching(false);
    }
  };

  const handleViewUser = (user: User) => {
    setSelectedUser(user);
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setSelectedUser(null);
  };

  const handleRefresh = async () => {
    setIsSearching(true);
    try {
      await new Promise((resolve) => setTimeout(resolve, 1000));
      setUsers(mockUsers);
      setSearchTerm("");
    } catch (error) {
      console.error("Refresh failed:", error);
    } finally {
      setIsSearching(false);
    }
  };

  const filteredUsers = users.filter((user) => !showActiveOnly || user.active);

  const getInitials = (name: string) => {
    return name
      .split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase();
  };

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        👥 User Management
      </Typography>

      <Typography
        variant="body1"
        color="text.secondary"
        gutterBottom
        sx={{ mb: 3 }}
      >
        Search and manage Jira users, view their assignments and details
      </Typography>

      {/* Search Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={8}>
              <TextField
                fullWidth
                placeholder="Search users by name or email..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onKeyPress={(e) => e.key === "Enter" && handleSearch()}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <PersonIcon />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <Box display="flex" gap={1}>
                <Button
                  variant="contained"
                  startIcon={<SearchIcon />}
                  onClick={handleSearch}
                  disabled={isSearching}
                  fullWidth
                >
                  {isSearching ? "Searching..." : "Search"}
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<RefreshIcon />}
                  onClick={handleRefresh}
                  disabled={isSearching}
                >
                  Refresh
                </Button>
              </Box>
            </Grid>
          </Grid>

          <Box sx={{ mt: 2, display: "flex", gap: 1 }}>
            <Button
              variant={showActiveOnly ? "contained" : "outlined"}
              size="small"
              onClick={() => setShowActiveOnly(true)}
            >
              Active Users ({mockUsers.filter((u) => u.active).length})
            </Button>
            <Button
              variant={!showActiveOnly ? "contained" : "outlined"}
              size="small"
              onClick={() => setShowActiveOnly(false)}
            >
              All Users ({mockUsers.length})
            </Button>
          </Box>
        </CardContent>
      </Card>

      {/* Users Table */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Users ({filteredUsers.length} found)
          </Typography>
          <Divider sx={{ mb: 2 }} />

          <TableContainer component={Paper} elevation={0}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>
                    <strong>User</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Email</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Status</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Groups</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Assigned Issues</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Reported Issues</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Actions</strong>
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredUsers.map((user) => (
                  <TableRow key={user.accountId} hover>
                    <TableCell>
                      <Box display="flex" alignItems="center" gap={2}>
                        <Avatar sx={{ width: 40, height: 40 }}>
                          {getInitials(user.displayName)}
                        </Avatar>
                        <Box>
                          <Typography variant="body2" fontWeight="bold">
                            {user.displayName}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            ID: {user.accountId}
                          </Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {user.emailAddress}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={user.active ? "Active" : "Inactive"}
                        color={user.active ? "success" : "default"}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Box display="flex" gap={0.5} flexWrap="wrap">
                        {user.groups.slice(0, 2).map((group) => (
                          <Chip
                            key={group}
                            label={group}
                            size="small"
                            variant="outlined"
                          />
                        ))}
                        {user.groups.length > 2 && (
                          <Chip
                            label={`+${user.groups.length - 2}`}
                            size="small"
                            variant="outlined"
                          />
                        )}
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {user.assignedIssues}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {user.reportedIssues}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <IconButton
                        size="small"
                        onClick={() => handleViewUser(user)}
                        title="View Details"
                      >
                        <ViewIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          {filteredUsers.length === 0 && (
            <Box textAlign="center" py={4}>
              <Typography variant="body1" color="text.secondary">
                No users found matching your criteria.
              </Typography>
            </Box>
          )}
        </CardContent>
      </Card>

      {/* User Details Dialog */}
      <Dialog
        open={dialogOpen}
        onClose={handleCloseDialog}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Box display="flex" alignItems="center" gap={2}>
            <Avatar sx={{ width: 48, height: 48 }}>
              {selectedUser && getInitials(selectedUser.displayName)}
            </Avatar>
            <Box>
              <Typography variant="h6">{selectedUser?.displayName}</Typography>
              <Typography variant="body2" color="text.secondary">
                User Details
              </Typography>
            </Box>
          </Box>
        </DialogTitle>
        <DialogContent>
          {selectedUser && (
            <Box>
              <Tabs value={tabValue} onChange={handleTabChange}>
                <Tab label="Profile" />
                <Tab label="Activity" />
                <Tab label="Groups" />
              </Tabs>

              <TabPanel value={tabValue} index={0}>
                <Grid container spacing={3}>
                  <Grid item xs={12} sm={6}>
                    <Box display="flex" alignItems="center" gap={1} mb={2}>
                      <EmailIcon color="primary" />
                      <Box>
                        <Typography variant="subtitle2" color="text.secondary">
                          Email Address
                        </Typography>
                        <Typography variant="body1">
                          {selectedUser.emailAddress}
                        </Typography>
                      </Box>
                    </Box>
                  </Grid>

                  <Grid item xs={12} sm={6}>
                    <Box display="flex" alignItems="center" gap={1} mb={2}>
                      <BusinessIcon color="primary" />
                      <Box>
                        <Typography variant="subtitle2" color="text.secondary">
                          Account Type
                        </Typography>
                        <Typography variant="body1">
                          {selectedUser.accountType}
                        </Typography>
                      </Box>
                    </Box>
                  </Grid>

                  <Grid item xs={12} sm={6}>
                    <Box mb={2}>
                      <Typography variant="subtitle2" color="text.secondary">
                        Status
                      </Typography>
                      <Chip
                        label={selectedUser.active ? "Active" : "Inactive"}
                        color={selectedUser.active ? "success" : "default"}
                        size="small"
                      />
                    </Box>
                  </Grid>

                  <Grid item xs={12} sm={6}>
                    <Box mb={2}>
                      <Typography variant="subtitle2" color="text.secondary">
                        Locale
                      </Typography>
                      <Typography variant="body1">
                        {selectedUser.locale}
                      </Typography>
                    </Box>
                  </Grid>

                  <Grid item xs={12}>
                    <Box mb={2}>
                      <Typography variant="subtitle2" color="text.secondary">
                        Time Zone
                      </Typography>
                      <Typography variant="body1">
                        {selectedUser.timeZone}
                      </Typography>
                    </Box>
                  </Grid>
                </Grid>
              </TabPanel>

              <TabPanel value={tabValue} index={1}>
                <Grid container spacing={3}>
                  <Grid item xs={6}>
                    <Card variant="outlined">
                      <CardContent>
                        <Box display="flex" alignItems="center" gap={1} mb={1}>
                          <AssignmentIcon color="primary" />
                          <Typography variant="h6">
                            {selectedUser.assignedIssues}
                          </Typography>
                        </Box>
                        <Typography variant="body2" color="text.secondary">
                          Assigned Issues
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={6}>
                    <Card variant="outlined">
                      <CardContent>
                        <Box display="flex" alignItems="center" gap={1} mb={1}>
                          <PersonIcon color="primary" />
                          <Typography variant="h6">
                            {selectedUser.reportedIssues}
                          </Typography>
                        </Box>
                        <Typography variant="body2" color="text.secondary">
                          Reported Issues
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                </Grid>
              </TabPanel>

              <TabPanel value={tabValue} index={2}>
                <List>
                  {selectedUser.groups.map((group, index) => (
                    <ListItem key={index}>
                      <ListItemAvatar>
                        <Avatar>
                          <BusinessIcon />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText primary={group} secondary="User Group" />
                    </ListItem>
                  ))}
                </List>
                {selectedUser.groups.length === 0 && (
                  <Alert severity="info">
                    This user is not a member of any groups.
                  </Alert>
                )}
              </TabPanel>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default UserManagement;
