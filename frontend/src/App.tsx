import React from "react";
import {
  BrowserRouter as Router,
  Routes,
  Route,
  Link,
  useLocation,
} from "react-router-dom";

// Import page components
import Dashboard from "./pages/Dashboard";
import Configuration from "./pages/Configuration";
import TicketManagement from "./pages/TicketManagement";

function Navigation() {
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  return (
    <nav className="bg-white shadow-sm border-b border-gray-200">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          <div className="flex items-center">
            <h1 className="text-xl font-bold text-gray-900">
              JIRA Integration
            </h1>
          </div>
          <div className="flex space-x-8">
            <Link
              to="/"
              className={`inline-flex items-center px-1 pt-1 text-sm font-medium ${
                isActive("/")
                  ? "text-blue-600 border-b-2 border-blue-600"
                  : "text-gray-500 hover:text-gray-700"
              }`}
            >
              🏠 Dashboard
            </Link>
            <Link
              to="/configuration"
              className={`inline-flex items-center px-1 pt-1 text-sm font-medium ${
                isActive("/configuration")
                  ? "text-blue-600 border-b-2 border-blue-600"
                  : "text-gray-500 hover:text-gray-700"
              }`}
            >
              ⚙️ Configuration
            </Link>
            <Link
              to="/tickets"
              className={`inline-flex items-center px-1 pt-1 text-sm font-medium ${
                isActive("/tickets")
                  ? "text-blue-600 border-b-2 border-blue-600"
                  : "text-gray-500 hover:text-gray-700"
              }`}
            >
              🎫 Tickets
            </Link>
          </div>
        </div>
      </div>
    </nav>
  );
}

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <Router>
        <Navigation />
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/configuration" element={<Configuration />} />
          <Route path="/tickets" element={<TicketManagement />} />
        </Routes>
      </Router>
    </div>
  );
}

export default App;
