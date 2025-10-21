import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { ThemeProvider, createTheme } from "@mui/material/styles";
import Navigation from "./components/Navigation";
import Dashboard from "./pages/Dashboard";
import TicketList from "./pages/TicketList";
import CreateTicket from "./pages/CreateTicket";
import AdvancedSearch from "./pages/AdvancedSearch";
import UserManagement from "./pages/UserManagement";
import Reports from "./pages/Reports";

// Create Material-UI theme
const theme = createTheme({
  palette: {
    primary: {
      main: "#1976d2",
    },
    secondary: {
      main: "#dc004e",
    },
  },
});

// Placeholder components for other routes
const PlaceholderPage: React.FC<{ title: string }> = ({ title }) => (
  <div style={{ padding: "20px" }}>
    <h2>{title}</h2>
    <p>This page is coming soon...</p>
  </div>
);

function App() {
  return (
    <ThemeProvider theme={theme}>
      <Router>
        <Navigation>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/tickets" element={<TicketList />} />
            <Route path="/create-ticket" element={<CreateTicket />} />
            <Route path="/search" element={<AdvancedSearch />} />
            <Route path="/users" element={<UserManagement />} />
            <Route
              path="/bulk"
              element={<PlaceholderPage title="Bulk Operations" />}
            />
            <Route path="/reports" element={<Reports />} />
            <Route
              path="/confluence"
              element={<PlaceholderPage title="Confluence Integration" />}
            />
          </Routes>
        </Navigation>
      </Router>
    </ThemeProvider>
  );
}

export default App;
