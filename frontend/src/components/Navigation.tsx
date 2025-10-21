import React from "react";import React from "react";import React, { useState } from "react";

import { NavLink } from "react-router-dom";

import { Home, Plus, Search, Users, BarChart3, Settings } from "lucide-react";import { useNavigate, useLocation } from "react-router-dom";import {



interface NavigationProps {  AppBar,

  children: React.ReactNode;

}interface NavigationProps {  Box,



const Navigation: React.FC<NavigationProps> = ({ children }) => {  children: React.ReactNode;  CssBaseline,

  const navigationItems = [

    { path: "/", label: "Dashboard", icon: Home },}  Drawer,

    { path: "/create-ticket", label: "Create Ticket", icon: Plus },

    { path: "/tickets", label: "Tickets", icon: Search },  IconButton,

    { path: "/users", label: "Users", icon: Users },

    { path: "/reports", label: "Reports", icon: BarChart3 },const Navigation: React.FC<NavigationProps> = ({ children }) => {  List,

    { path: "/configuration", label: "Settings", icon: Settings },

  ];  const navigate = useNavigate();  ListItem,



  return (  const location = useLocation();  ListItemButton,

    <div className="min-h-screen bg-gray-50">

      {/* Top Navigation */}  ListItemIcon,

      <nav className="bg-white shadow-sm border-b border-gray-200">

        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">  const menuItems = [  ListItemText,

          <div className="flex justify-between h-16">

            <div className="flex items-center">    { path: "/", label: "Dashboard", icon: "🏠" },  Toolbar,

              <h1 className="text-xl font-bold text-gray-900">

                JIRA Integration    { path: "/create-ticket", label: "Create Ticket", icon: "➕" },  Typography,

              </h1>

            </div>  ];  useTheme,

            <div className="flex items-center space-x-4">

              <span className="text-sm text-gray-500">  useMediaQuery,

                Connected to JIRA Cloud

              </span>  return (} from "@mui/material";

              <div className="w-2 h-2 bg-green-500 rounded-full"></div>

            </div>    <div className="min-h-screen bg-gray-100">import {

          </div>

        </div>      {/* Top Navigation */}  Menu as MenuIcon,

      </nav>

      <nav className="bg-white shadow-sm border-b border-gray-200">  Dashboard as DashboardIcon,

      <div className="flex">

        {/* Sidebar Navigation */}        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">  ConfirmationNumber as TicketIcon,

        <aside className="w-64 bg-white shadow-sm border-r border-gray-200 min-h-screen">

          <nav className="mt-8 px-4">          <div className="flex justify-between h-16">  Search as SearchIcon,

            <ul className="space-y-2">

              {navigationItems.map((item) => {            <div className="flex items-center">  Person as PersonIcon,

                const Icon = item.icon;

                return (              <h1 className="text-xl font-bold text-gray-900">JIRA Integration</h1>  Assessment as ReportsIcon,

                  <li key={item.path}>

                    <NavLink            </div>  Article as ConfluenceIcon,

                      to={item.path}

                      className={({ isActive }) =>            <div className="flex space-x-8">  GroupWork as BulkIcon,

                        `flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors duration-200 ${

                          isActive              {menuItems.map((item) => (  Add as AddIcon,

                            ? "bg-blue-50 text-blue-700 border-r-2 border-blue-600"

                            : "text-gray-700 hover:bg-gray-100"                <button  Settings as SettingsIcon,

                        }`

                      }                  key={item.path}} from "@mui/icons-material";

                    >

                      <Icon className="w-5 h-5 mr-3" />                  onClick={() => navigate(item.path)}import { useNavigate, useLocation } from "react-router-dom";

                      {item.label}

                    </NavLink>                  className={`inline-flex items-center px-1 pt-1 text-sm font-medium ${

                  </li>

                );                    location.pathname === item.pathconst drawerWidth = 240;

              })}

            </ul>                      ? "text-blue-600 border-b-2 border-blue-600"

          </nav>

        </aside>                      : "text-gray-500 hover:text-gray-700"interface NavigationProps {



        {/* Main Content */}                  }`}  children: React.ReactNode;

        <main className="flex-1 overflow-auto">

          {children}                >}

        </main>

      </div>                  <span className="mr-2">{item.icon}</span>

    </div>

  );                  {item.label}const Navigation: React.FC<NavigationProps> = ({ children }) => {

};

                </button>  const [mobileOpen, setMobileOpen] = useState(false);

export default Navigation;
              ))}  const theme = useTheme();

            </div>  const isMobile = useMediaQuery(theme.breakpoints.down("md"));

          </div>  const navigate = useNavigate();

        </div>  const location = useLocation();

      </nav>

  const handleDrawerToggle = () => {

      {/* Main Content */}    setMobileOpen(!mobileOpen);

      <main className="flex-1">  };

        {children}

      </main>  const menuItems = [

    </div>    { text: "Dashboard", icon: <DashboardIcon />, path: "/" },

  );    { text: "Configuration", icon: <SettingsIcon />, path: "/configuration" },

};    { text: "Tickets", icon: <TicketIcon />, path: "/tickets" },

    { text: "Create Ticket", icon: <AddIcon />, path: "/create-ticket" },

export default Navigation;    { text: "Search", icon: <SearchIcon />, path: "/search" },
    { text: "User Management", icon: <PersonIcon />, path: "/users" },
    { text: "Bulk Operations", icon: <BulkIcon />, path: "/bulk" },
    { text: "Reports", icon: <ReportsIcon />, path: "/reports" },
    { text: "Confluence", icon: <ConfluenceIcon />, path: "/confluence" },
  ];

  const drawer = (
    <div>
      <Toolbar>
        <Typography variant="h6" noWrap component="div">
          Jira Integration
        </Typography>
      </Toolbar>
      <List>
        {menuItems.map((item) => (
          <ListItem key={item.text} disablePadding>
            <ListItemButton
              selected={location.pathname === item.path}
              onClick={() => {
                navigate(item.path);
                if (isMobile) {
                  setMobileOpen(false);
                }
              }}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </div>
  );

  return (
    <Box sx={{ display: "flex" }}>
      <CssBaseline />
      <AppBar
        position="fixed"
        sx={{
          width: { md: `calc(100% - ${drawerWidth}px)` },
          ml: { md: `${drawerWidth}px` },
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            aria-label="open drawer"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { md: "none" } }}
          >
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" noWrap component="div">
            Jira Integration Portal
          </Typography>
        </Toolbar>
      </AppBar>
      <Box
        component="nav"
        sx={{ width: { md: drawerWidth }, flexShrink: { md: 0 } }}
      >
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{
            keepMounted: true, // Better open performance on mobile.
          }}
          sx={{
            display: { xs: "block", md: "none" },
            "& .MuiDrawer-paper": {
              boxSizing: "border-box",
              width: drawerWidth,
            },
          }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: "none", md: "block" },
            "& .MuiDrawer-paper": {
              boxSizing: "border-box",
              width: drawerWidth,
            },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: { md: `calc(100% - ${drawerWidth}px)` },
        }}
      >
        <Toolbar />
        {children}
      </Box>
    </Box>
  );
};

export default Navigation;
