using Microsoft.Extensions.Logging;
using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;
using JiraIntegration.Services.Interfaces;

namespace JiraIntegration.Application;

/// <summary>
/// Main console application that provides interactive menu for Jira operations
/// </summary>
public class ConsoleApplication
{
    private readonly IJiraAuthService _authService;
    private readonly IJiraTicketService _ticketService;
    private readonly IJiraSearchService _searchService;
    private readonly IJiraProjectService _projectService;
    private readonly ILogger<ConsoleApplication> _logger;

    public ConsoleApplication(
        IJiraAuthService authService,
        IJiraTicketService ticketService,
        IJiraSearchService searchService,
        IJiraProjectService projectService,
        ILogger<ConsoleApplication> logger)
    {
        _authService = authService;
        _ticketService = ticketService;
        _searchService = searchService;
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Main application entry point
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            Console.Clear();
            ShowWelcomeMessage();

            // Test connection first
            if (!await TestConnectionAsync())
            {
                Console.WriteLine("❌ Connection failed. Please check your configuration in the .env file.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            await ShowMainMenuAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in main application");
            Console.WriteLine($"❌ An unexpected error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows welcome message and application info
    /// </summary>
    private static void ShowWelcomeMessage()
    {
        Console.WriteLine("🎫 JIRA INTEGRATION CONSOLE APPLICATION");
        Console.WriteLine("==========================================");
        Console.WriteLine("Features:");
        Console.WriteLine("• Test Connection - Validate credentials");
        Console.WriteLine("• Create Ticket - Create new issues");
        Console.WriteLine("• Get Ticket Details - Retrieve ticket info");
        Console.WriteLine("• Search Tickets - JQL search functionality");
        Console.WriteLine("• Transition Ticket - Move tickets through workflow");
        Console.WriteLine("==========================================\n");
    }

    /// <summary>
    /// Tests connection and shows user info
    /// </summary>
    private async Task<bool> TestConnectionAsync()
    {
        try
        {
            Console.Write("🔄 Testing Jira connection... ");
            var isValid = await _authService.ValidateConnectionAsync();

            if (isValid)
            {
                Console.WriteLine("✅ SUCCESS");
                var user = await _authService.GetCurrentUserAsync();
                if (user != null)
                {
                    Console.WriteLine($"👤 Authenticated as: {user.DisplayName} ({user.EmailAddress})");
                }
                Console.WriteLine();
                return true;
            }
            else
            {
                Console.WriteLine("❌ FAILED");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Shows main menu and handles user input
    /// </summary>
    private async Task ShowMainMenuAsync()
    {
        while (true)
        {
            ShowMainMenu();
            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        await TestConnectionAndShowUserInfoAsync();
                        break;
                    case "2":
                        await CreateTicketAsync();
                        break;
                    case "3":
                        await GetTicketDetailsAsync();
                        break;
                    case "4":
                        await SearchTicketsAsync();
                        break;
                    case "5":
                        await TransitionTicketAsync();
                        break;
                    case "0":
                        Console.WriteLine("👋 Thank you for using Jira Integration Console!");
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option. Please select a number from 0-5.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing menu option {Choice}", choice);
                Console.WriteLine($"❌ Error: {ex.Message}");
            }

            if (choice != "0")
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }

    /// <summary>
    /// Displays the main menu options
    /// </summary>
    private static void ShowMainMenu()
    {
        Console.WriteLine("=== MAIN MENU ===");
        Console.WriteLine("1. 🔐 Test Connection & Show User Info");
        Console.WriteLine("2. 🎫 Create New Ticket");
        Console.WriteLine("3. 📖 Get Ticket Details");
        Console.WriteLine("4. 🔍 Search Tickets (JQL)");
        Console.WriteLine("5. 🔄 Transition Ticket");
        Console.WriteLine("0. ❌ Exit");
        Console.Write("\nChoose an option: ");
    }

    /// <summary>
    /// Tests connection and displays current user information
    /// </summary>
    private async Task TestConnectionAndShowUserInfoAsync()
    {
        Console.WriteLine("\n=== CONNECTION TEST & USER INFO ===");

        var isValid = await _authService.ValidateConnectionAsync();
        if (isValid)
        {
            Console.WriteLine("✅ Connection Status: ACTIVE");

            var user = await _authService.GetCurrentUserAsync();
            if (user != null)
            {
                Console.WriteLine($"👤 User: {user.DisplayName}");
                Console.WriteLine($"📧 Email: {user.EmailAddress}");
                Console.WriteLine($"🆔 Account ID: {user.AccountId}");
                Console.WriteLine($"🟢 Status: {(user.Active ? "Active" : "Inactive")}");
            }
        }
        else
        {
            Console.WriteLine("❌ Connection Status: FAILED");
            Console.WriteLine("Please check your credentials and network connection.");
        }
    }

    /// <summary>
    /// Creates a new ticket with user input
    /// </summary>
    private async Task CreateTicketAsync()
    {
        Console.WriteLine("\n=== CREATE NEW TICKET ===");

        try
        {
            Console.Write("Project Key (e.g., OPS): ");
            var projectKey = Console.ReadLine()?.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(projectKey))
            {
                Console.WriteLine("❌ Project key is required.");
                return;
            }

            // Verify project exists
            Console.WriteLine($"🔄 Verifying project {projectKey}...");
            var project = await _projectService.GetProjectAsync(projectKey);
            if (project == null)
            {
                Console.WriteLine($"❌ Project '{projectKey}' not found or access denied.");
                return;
            }

            Console.WriteLine($"✅ Project found: {project.Name}");

            Console.Write("Summary/Title: ");
            var summary = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(summary))
            {
                Console.WriteLine("❌ Summary is required.");
                return;
            }

            Console.Write("Description: ");
            var description = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                description = summary; // Use summary as description if empty
            }

            // Get available issue types
            Console.WriteLine("🔄 Getting available issue types...");
            var issueTypes = await GetAvailableIssueTypesAsync(projectKey);

            if (!issueTypes.Any())
            {
                Console.WriteLine("❌ No issue types found for this project.");
                return;
            }

            Console.WriteLine("\nAvailable Issue Types:");
            for (int i = 0; i < issueTypes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {issueTypes[i].Name}");
            }

            Console.Write("Select issue type (number, default: 1): ");
            var issueTypeInput = Console.ReadLine()?.Trim();
            var issueTypeIndex = 0;

            if (!string.IsNullOrWhiteSpace(issueTypeInput))
            {
                if (!int.TryParse(issueTypeInput, out issueTypeIndex) ||
                    issueTypeIndex < 1 || issueTypeIndex > issueTypes.Count)
                {
                    Console.WriteLine("❌ Invalid issue type selection. Using default (Task).");
                    issueTypeIndex = 1;
                }
                else
                {
                    issueTypeIndex--; // Convert to 0-based index
                }
            }

            var selectedIssueType = issueTypes[issueTypeIndex];

            Console.Write("Priority (High/Medium/Low, default: Medium): ");
            var priorityInput = Console.ReadLine()?.Trim();
            var priority = string.IsNullOrWhiteSpace(priorityInput) ? "Medium" : priorityInput;

            var request = new CreateIssueRequest
            {
                ProjectKey = projectKey,
                Summary = summary,
                Description = description,
                IssueTypeId = selectedIssueType.Id,
                Priority = priority
            };

            Console.WriteLine($"🔄 Creating {selectedIssueType.Name} ticket...");
            var createdIssue = await _ticketService.CreateTicketAsync(request);

            if (createdIssue != null)
            {
                Console.WriteLine("✅ Ticket created successfully!");
                Console.WriteLine($"🎫 Key: {createdIssue.Key}");
                Console.WriteLine($"📋 Summary: {createdIssue.Fields.Summary}");
                Console.WriteLine($"🔗 URL: {createdIssue.Self}");
            }
            else
            {
                Console.WriteLine("❌ Failed to create ticket. Please check your input and permissions.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creating ticket: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets available issue types for project, with fallback to common types
    /// </summary>
    private async Task<List<JiraIssueType>> GetAvailableIssueTypesAsync(string projectKey)
    {
        try
        {
            var issueTypes = await _projectService.GetIssueTypesAsync(projectKey);

            if (issueTypes.Any())
            {
                return issueTypes;
            }

            // Fallback to common issue types with typical IDs
            return new List<JiraIssueType>
            {
                new() { Id = "10001", Name = "Task" },
                new() { Id = "10002", Name = "Story" },
                new() { Id = "10003", Name = "Bug" },
                new() { Id = "10004", Name = "Epic" }
            };
        }
        catch (Exception)
        {
            // Return default issue types on error
            return new List<JiraIssueType>
            {
                new() { Id = "10001", Name = "Task" },
                new() { Id = "10002", Name = "Story" },
                new() { Id = "10003", Name = "Bug" },
                new() { Id = "10004", Name = "Epic" }
            };
        }
    }

    /// <summary>
    /// Gets and displays ticket details
    /// </summary>
    private async Task GetTicketDetailsAsync()
    {
        Console.WriteLine("\n=== GET TICKET DETAILS ===");

        try
        {
            Console.Write("Enter ticket key (e.g., OPS-7): ");
            var ticketKey = Console.ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                Console.WriteLine("❌ Ticket key is required.");
                return;
            }

            Console.WriteLine($"🔄 Fetching details for {ticketKey}...");
            var ticket = await _ticketService.GetTicketAsync(ticketKey);

            if (ticket != null)
            {
                DisplayTicketDetails(ticket);
            }
            else
            {
                Console.WriteLine($"❌ Ticket '{ticketKey}' not found or access denied.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error getting ticket details: {ex.Message}");
        }
    }

    /// <summary>
    /// Searches for tickets using JQL
    /// </summary>
    private async Task SearchTicketsAsync()
    {
        Console.WriteLine("\n=== SEARCH TICKETS ===");
        Console.WriteLine("Examples:");
        Console.WriteLine("• project = OPS");
        Console.WriteLine("• project = OPS AND status = Open");
        Console.WriteLine("• assignee = currentUser()");
        Console.WriteLine("• created >= -7d");
        Console.WriteLine();

        try
        {
            Console.Write("Enter JQL query: ");
            var jql = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(jql))
            {
                Console.WriteLine("❌ JQL query is required.");
                return;
            }

            Console.Write("Max results (default: 10): ");
            var maxResultsInput = Console.ReadLine()?.Trim();
            var maxResults = int.TryParse(maxResultsInput, out var parsed) ? parsed : 10;

            Console.WriteLine($"🔄 Searching with JQL: {jql}");
            var searchResponse = await _searchService.SearchTicketsAsync(jql, maxResults);

            if (searchResponse != null && searchResponse.Issues.Any())
            {
                Console.WriteLine($"✅ Found {searchResponse.Total} ticket(s), showing {searchResponse.Issues.Count}:");
                Console.WriteLine();

                foreach (var issue in searchResponse.Issues)
                {
                    Console.WriteLine($"🎫 {issue.Key}: {issue.Fields.Summary}");
                    Console.WriteLine($"   Status: {issue.Fields.Status.Name}");
                    Console.WriteLine($"   Assignee: {issue.Fields.Assignee?.DisplayName ?? "Unassigned"}");
                    Console.WriteLine($"   Created: {issue.Fields.Created:yyyy-MM-dd HH:mm}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("❌ No tickets found matching your search criteria.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error searching tickets: {ex.Message}");
        }
    }

    /// <summary>
    /// Transitions a ticket to a new status
    /// </summary>
    private async Task TransitionTicketAsync()
    {
        Console.WriteLine("\n=== TRANSITION TICKET ===");

        try
        {
            Console.Write("Enter ticket key (e.g., OPS-7): ");
            var ticketKey = Console.ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                Console.WriteLine("❌ Ticket key is required.");
                return;
            }

            Console.WriteLine($"🔄 Getting available transitions for {ticketKey}...");
            var transitions = await _ticketService.GetAvailableTransitionsAsync(ticketKey);

            if (!transitions.Any())
            {
                Console.WriteLine("❌ No transitions available for this ticket.");
                return;
            }

            Console.WriteLine("\nAvailable Transitions:");
            for (int i = 0; i < transitions.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {transitions[i].Name} → {transitions[i].To.Name}");
            }

            Console.Write("\nSelect transition (number): ");
            var transitionInput = Console.ReadLine()?.Trim();

            if (!int.TryParse(transitionInput, out var transitionIndex) ||
                transitionIndex < 1 || transitionIndex > transitions.Count)
            {
                Console.WriteLine("❌ Invalid transition selection.");
                return;
            }

            var selectedTransition = transitions[transitionIndex - 1];

            Console.Write("Add comment (optional): ");
            var comment = Console.ReadLine()?.Trim();

            Console.WriteLine($"🔄 Transitioning {ticketKey} to '{selectedTransition.To.Name}'...");
            var success = await _ticketService.TransitionTicketAsync(
                ticketKey, selectedTransition.Id, comment);

            if (success)
            {
                Console.WriteLine($"✅ Successfully transitioned {ticketKey} to '{selectedTransition.To.Name}'!");
            }
            else
            {
                Console.WriteLine("❌ Failed to transition ticket. Please check permissions and try again.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error transitioning ticket: {ex.Message}");
        }
    }

    /// <summary>
    /// Displays detailed ticket information in a formatted way
    /// </summary>
    private static void DisplayTicketDetails(JiraIssue ticket)
    {
        Console.WriteLine("\n📋 TICKET DETAILS");
        Console.WriteLine("==================");
        Console.WriteLine($"🎫 Key: {ticket.Key}");
        Console.WriteLine($"📝 Summary: {ticket.Fields.Summary}");
        Console.WriteLine($"📊 Status: {ticket.Fields.Status.Name}");
        Console.WriteLine($"🏷️  Type: {ticket.Fields.IssueType.Name}");
        Console.WriteLine($"⚡ Priority: {ticket.Fields.Priority.Name}");
        Console.WriteLine($"👤 Assignee: {ticket.Fields.Assignee?.DisplayName ?? "Unassigned"}");
        Console.WriteLine($"📝 Reporter: {ticket.Fields.Reporter?.DisplayName ?? "Unknown"}");
        Console.WriteLine($"📅 Created: {ticket.Fields.Created:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"🔄 Updated: {ticket.Fields.Updated:yyyy-MM-dd HH:mm}");

        if (ticket.Fields.ResolutionDate.HasValue)
        {
            Console.WriteLine($"✅ Resolved: {ticket.Fields.ResolutionDate:yyyy-MM-dd HH:mm}");
        }

        Console.WriteLine($"🔗 URL: {ticket.Self}");

        if (!string.IsNullOrWhiteSpace(ticket.Fields.Description))
        {
            Console.WriteLine($"\n📄 Description:");
            Console.WriteLine($"{ticket.Fields.Description}");
        }
    }
}
