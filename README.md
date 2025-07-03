# JIRA Integration Console Application

A comprehensive .NET console application for interacting with JIRA Cloud via REST API. This application provides 5 core functionalities for managing JIRA tickets and workflows.

## 🚀 Features

### ✅ 1. Test Connection

- Validates JIRA credentials and permissions
- Displays authenticated user information
- Confirms API connectivity and authentication status

### ✅ 2. Create Ticket

- Creates new JIRA tickets with proper validation
- Supports multiple issue types (Task, Story, Bug, Epic)
- Allows setting priority levels (High, Medium, Low)
- Automatic project verification

### ✅ 3. Get Ticket Details

- Retrieves comprehensive ticket information by key
- Displays formatted ticket details including:
  - Summary, Description, Status, Priority
  - Assignee, Reporter, Created/Updated dates
  - Direct links to JIRA web interface

### ✅ 4. Search Tickets (JQL)

- Powerful JQL (JIRA Query Language) search functionality
- Built-in example queries for common scenarios
- Configurable result limits
- Formatted search results display

### ✅ 5. Transition Ticket

- Move tickets through workflow stages
- Lists available transitions for each ticket
- Supports optional comments during transitions
- Real-time status updates

## 🛠️ Technical Architecture

### **High-Quality Code Standards**

- **Dependency Injection**: Full DI container setup with Microsoft.Extensions
- **Async/Await**: All operations are fully asynchronous
- **Error Handling**: Comprehensive exception handling and logging
- **Configuration Management**: Environment-based configuration with validation
- **HTTP Client Management**: Proper HttpClient lifecycle management
- **Logging**: Structured logging with different levels
- **Type Safety**: Strong typing with nullable reference types enabled
- **ADF Support**: Handles both legacy string and modern Atlassian Document Format descriptions

### **Project Structure**

```
JiraIntegration/
├── Application/
│   └── ConsoleApplication.cs          # Main UI and menu system
├── Configuration/
│   └── EnvironmentLoader.cs           # Environment variable management
├── Models/
│   ├── JiraSettings.cs                # Configuration model
│   ├── Dto/
│   │   └── JiraModels.cs              # JIRA API response models
│   └── Requests/
│       └── JiraRequests.cs            # JIRA API request models
├── Services/
│   ├── Base/
│   │   └── BaseJiraHttpService.cs     # Base HTTP service with auth
│   ├── Interfaces/
│   │   ├── IJiraAuthService.cs
│   │   ├── IJiraTicketService.cs
│   │   ├── IJiraSearchService.cs
│   │   └── IJiraProjectService.cs
│   └── Implementations/
│       ├── JiraAuthService.cs
│       ├── JiraTicketService.cs
│       ├── JiraSearchService.cs
│       └── JiraProjectService.cs
├── Program.cs                         # Entry point and DI setup
├── appsettings.json                   # Application configuration
└── .env                              # Environment variables (credentials)
```

## 📋 Prerequisites

- **.NET 9.0** or later
- **JIRA Cloud** instance with API access
- **JIRA API Token** (generated from Atlassian Account Settings)

## ⚙️ Setup Instructions

### 1. Clone and Build

```bash
git clone <repository-url>
cd JiraIntegration
dotnet restore
dotnet build
```

### 2. Configure Environment Variables

Create a `.env` file in the project root:

```env
# JIRA Configuration
JIRA_BASE_URL=https://your-domain.atlassian.net
JIRA_USER_EMAIL=your-email@domain.com
JIRA_API_TOKEN=your-api-token-here
JIRA_PROJECT_KEY=YOUR_PROJECT_KEY
```

### 3. Generate JIRA API Token

1. Go to [Atlassian Account Settings](https://id.atlassian.com/manage-profile/security/api-tokens)
2. Click "Create API token"
3. Give it a descriptive name
4. Copy the token to your `.env` file

### 4. Run the Application

```bash
dotnet run
```

## 🎯 Usage Examples

### **Test Connection**

```
🔐 Test Connection & Show User Info
✅ Connection Status: ACTIVE
👤 User: John Doe
📧 Email: john.doe@company.com
🟢 Status: Active
```

### **Create Ticket**

```
🎫 Create New Ticket
Project Key: OPS
Summary: Fix login issue
Description: Users unable to login after password reset
Issue Type: Bug
Priority: High
✅ Ticket created: OPS-123
```

### **JQL Search Examples**

```
🔍 Search Tickets (JQL)
Examples:
• project = OPS
• project = OPS AND status = "In Progress"
• assignee = currentUser()
• created >= -7d
• priority = High
```

### **Transition Ticket**

```
🔄 Transition Ticket
Ticket: OPS-123
Available Transitions:
1. Start Progress → In Progress
2. Resolve Issue → Done
Select transition: 1
✅ Ticket OPS-123 moved to "In Progress"
```

## 🔧 Configuration Options

### **JiraSettings (appsettings.json)**

```json
{
  "JiraSettings": {
    "BaseUrl": "", // Set via JIRA_BASE_URL env var
    "Email": "", // Set via JIRA_USER_EMAIL env var
    "ApiToken": "", // Set via JIRA_API_TOKEN env var
    "ProjectKey": "", // Set via JIRA_PROJECT_KEY env var
    "MaxResults": 50, // Default search result limit
    "TimeoutSeconds": 30 // HTTP request timeout
  }
}
```

### **Environment Variables**

- `JIRA_BASE_URL`: Your JIRA instance URL
- `JIRA_USER_EMAIL`: Your JIRA account email
- `JIRA_API_TOKEN`: Your JIRA API token
- `JIRA_PROJECT_KEY`: Default project key to use

## 🔒 Security Features

- **Environment-based Configuration**: Sensitive data stored in environment variables
- **API Token Authentication**: Uses secure token-based authentication
- **Input Validation**: All user inputs are validated before API calls
- **Error Handling**: Sensitive information is not exposed in error messages
- **Logging Control**: Different log levels for development and production

## 📊 API Endpoints Used

| Feature           | JIRA REST API Endpoint                | Purpose                   |
| ----------------- | ------------------------------------- | ------------------------- |
| Authentication    | `/rest/api/3/myself`                  | Validate credentials      |
| Create Ticket     | `/rest/api/3/issue`                   | Create new issues         |
| Get Ticket        | `/rest/api/3/issue/{key}`             | Retrieve ticket details   |
| Search Tickets    | `/rest/api/3/search`                  | JQL-based search          |
| Get Transitions   | `/rest/api/3/issue/{key}/transitions` | Get available transitions |
| Transition Ticket | `/rest/api/3/issue/{key}/transitions` | Execute transitions       |
| Get Project       | `/rest/api/3/project/{key}`           | Project information       |
| Get Issue Types   | `/rest/api/3/issuetype`               | Available issue types     |

## 🚨 Error Handling

The application includes comprehensive error handling for:

- **Network Issues**: Connection timeouts, DNS resolution failures
- **Authentication Errors**: Invalid credentials, expired tokens
- **API Errors**: Invalid requests, permission denied, rate limiting
- **Data Validation**: Invalid input formats, missing required fields
- **Configuration Issues**: Missing environment variables, invalid URLs

## 🎛️ Logging

Structured logging is implemented using Microsoft.Extensions.Logging:

- **Information**: Normal operation flow
- **Warning**: Recoverable issues (API warnings, validation failures)
- **Error**: Exception details for troubleshooting
- **Debug**: Detailed HTTP request/response information

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/new-feature`)
3. Commit your changes (`git commit -am 'Add new feature'`)
4. Push to the branch (`git push origin feature/new-feature`)
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Troubleshooting

### **Common Issues**

**Connection Failed**

- Verify JIRA_BASE_URL is correct (include https://)
- Check JIRA_USER_EMAIL matches your Atlassian account
- Ensure JIRA_API_TOKEN is valid and not expired

**Permission Denied**

- Verify your JIRA account has necessary permissions
- Check if your API token has the required scopes

**Invalid Issue Type**

- Issue type IDs vary by JIRA instance
- Use the application to browse available issue types first

**JQL Syntax Errors**

- Refer to [JIRA JQL documentation](https://support.atlassian.com/jira-service-management-cloud/docs/use-advanced-search-with-jira-query-language-jql/)
- Start with simple queries like `project = YOUR_KEY`

**JSON Deserialization Errors**

- The application handles both legacy string descriptions and modern ADF (Atlassian Document Format) descriptions
- If you encounter description parsing errors, this is likely due to complex ADF content
- The custom converter automatically extracts text content from ADF objects

**Ticket Creation Issues**

- Ensure you're using the correct issue type ID (numbers like 10001, 10002)
- Issue type IDs vary by JIRA instance configuration
- Use the application to browse available issue types for your project

**Ticket Transition Issues**

- If transitions appear to fail but the HTTP status is 204, this indicates success
- JIRA returns HTTP 204 (No Content) for successful transitions, which is the correct behavior
- The application correctly handles JIRA's 204 No Content responses for transitions
- If you see "Failed to transition ticket" but HTTP logs show 204 status, check the ticket status in JIRA web interface to confirm the transition actually succeeded
- Enable debug logging (see Debug Mode below) to see detailed HTTP request/response information

### **Debug Mode**

Run with detailed logging:

```bash
dotnet run --configuration Debug
```

This will show HTTP request/response details for troubleshooting API issues.
