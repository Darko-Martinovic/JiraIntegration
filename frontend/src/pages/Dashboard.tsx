import React from "react";
import { Typography, Box, Card, CardContent, Chip } from "@mui/material";
import { CheckCircle as CheckCircleIcon } from "@mui/icons-material";

const Dashboard: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        🎫 Jira Integration Dashboard
      </Typography>

      <Typography variant="body1" color="text.secondary" gutterBottom>
        Welcome to the Jira Integration Portal. Manage your Jira tickets,
        search, and more.
      </Typography>

      {/* Connection Status */}
      <Card sx={{ mb: 3, mt: 3 }}>
        <CardContent>
          <Box display="flex" alignItems="center" mb={2}>
            <CheckCircleIcon color="success" sx={{ mr: 1 }} />
            <Typography variant="h6">
              Connection Status:{" "}
              <Chip label="Ready" color="success" size="small" />
            </Typography>
          </Box>
          <Typography variant="body2" color="text.secondary">
            Backend API and Frontend are running successfully.
          </Typography>
        </CardContent>
      </Card>

      {/* Features List */}
      <Typography variant="h5" component="h2" gutterBottom sx={{ mt: 4 }}>
        Available Features
      </Typography>

      <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
        {[
          "Ticket Management - Create, view, update, and transition Jira tickets",
          "Advanced Search - Search tickets using JQL and smart filters",
          "User Management - Find users and manage assignments",
          "Bulk Operations - Perform bulk updates on multiple tickets",
          "Reports & Analytics - Generate reports and view project analytics",
          "Confluence Integration - Manage Confluence pages and spaces",
        ].map((feature, index) => (
          <Card key={index}>
            <CardContent>
              <Typography variant="body1">✅ {feature}</Typography>
            </CardContent>
          </Card>
        ))}
      </Box>

      {/* Quick Stats */}
      <Typography variant="h5" component="h2" gutterBottom sx={{ mt: 4 }}>
        Technology Stack
      </Typography>

      <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}>
        <Card sx={{ minWidth: 150 }}>
          <CardContent>
            <Typography variant="h6" color="primary">
              .NET API
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Backend Service
            </Typography>
            <Chip label="Running" color="success" size="small" sx={{ mt: 1 }} />
          </CardContent>
        </Card>

        <Card sx={{ minWidth: 150 }}>
          <CardContent>
            <Typography variant="h6" color="primary">
              React + TypeScript
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Frontend Framework
            </Typography>
            <Chip label="Ready" color="success" size="small" sx={{ mt: 1 }} />
          </CardContent>
        </Card>

        <Card sx={{ minWidth: 150 }}>
          <CardContent>
            <Typography variant="h6" color="primary">
              Vite
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Build Tool
            </Typography>
            <Chip label="Fast" color="info" size="small" sx={{ mt: 1 }} />
          </CardContent>
        </Card>

        <Card sx={{ minWidth: 150 }}>
          <CardContent>
            <Typography variant="h6" color="primary">
              Material-UI
            </Typography>
            <Typography variant="body2" color="text.secondary">
              UI Framework
            </Typography>
            <Chip label="Modern" color="info" size="small" sx={{ mt: 1 }} />
          </CardContent>
        </Card>
      </Box>
    </Box>
  );
};

export default Dashboard;
