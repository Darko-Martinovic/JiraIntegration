import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";

// Simple placeholder components
const Dashboard: React.FC = () => (
  <div className="p-8">
    <h1 className="text-3xl font-bold text-gray-900 mb-4">Dashboard</h1>
    <div className="bg-white rounded-lg shadow p-6">
      <h2 className="text-xl font-semibold mb-4">JIRA Integration Dashboard</h2>
      <p className="text-gray-600">
        Welcome to the JIRA Integration platform built with Tailwind CSS!
      </p>
    </div>
  </div>
);

const CreateTicket: React.FC = () => (
  <div className="p-8">
    <h1 className="text-3xl font-bold text-gray-900 mb-4">Create Ticket</h1>
    <div className="bg-white rounded-lg shadow p-6">
      <p className="text-gray-600">Create ticket functionality coming soon!</p>
    </div>
  </div>
);

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      {/* Simple Navigation */}
      <nav className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-bold text-gray-900">
                JIRA Integration
              </h1>
            </div>
            <div className="flex space-x-8">
              <a
                href="/"
                className="inline-flex items-center px-1 pt-1 text-sm font-medium text-blue-600 border-b-2 border-blue-600"
              >
                🏠 Dashboard
              </a>
              <a
                href="/create"
                className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-500 hover:text-gray-700"
              >
                ➕ Create Ticket
              </a>
            </div>
          </div>
        </div>
      </nav>

      <Router>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/create" element={<CreateTicket />} />
        </Routes>
      </Router>
    </div>
  );
}

export default App;
