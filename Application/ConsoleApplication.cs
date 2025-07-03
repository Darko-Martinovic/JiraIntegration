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
    private readonly IJiraFieldUpdateService _fieldUpdateService;
    private readonly IJiraCommentService _commentService;
    private readonly IJiraAdvancedSearchService _advancedSearchService;
    private readonly IJiraReportingService _reportingService;
    private readonly IJiraUserService _userService;
    private readonly IConfluenceService _confluenceService;
    private readonly ILogger<ConsoleApplication> _logger;

    public ConsoleApplication(
        IJiraAuthService authService,
        IJiraTicketService ticketService,
        IJiraSearchService searchService,
        IJiraProjectService projectService,
        IJiraFieldUpdateService fieldUpdateService,
        IJiraCommentService commentService,
        IJiraAdvancedSearchService advancedSearchService,
        IJiraReportingService reportingService,
        IJiraUserService userService,
        IConfluenceService confluenceService,
        ILogger<ConsoleApplication> logger)
    {
        _authService = authService;
        _ticketService = ticketService;
        _searchService = searchService;
        _projectService = projectService;
        _fieldUpdateService = fieldUpdateService;
        _commentService = commentService;
        _advancedSearchService = advancedSearchService;
        _reportingService = reportingService;
        _userService = userService;
        _confluenceService = confluenceService;
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
        Console.WriteLine("• Update Fields - Modify ticket fields");
        Console.WriteLine("• Add Comments - Add comments to tickets");
        Console.WriteLine("• Bulk Operations - Process multiple tickets");
        Console.WriteLine("• Advanced Search - Complex queries & filters");
        Console.WriteLine("• Reporting - Generate reports and analytics");
        Console.WriteLine("• User Management - Find account IDs for assignment");
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
                    case "6":
                        await UpdateTicketFieldsAsync();
                        break;
                    case "7":
                        await AddCommentsAsync();
                        break;
                    case "8":
                        await BulkOperationsAsync();
                        break;
                    case "9":
                        await AdvancedSearchAsync();
                        break;
                    case "10":
                        await ReportingFeaturesAsync();
                        break;
                    case "11":
                        await UserManagementAsync();
                        break;
                    case "12":
                        await ConfluenceIntegrationAsync();
                        break;
                    case "0":
                        Console.WriteLine("👋 Thank you for using Jira Integration Console!");
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option. Please select a number from 0-12.");
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
        Console.WriteLine("6. ✏️  Update Ticket Fields");
        Console.WriteLine("7. 💬 Add Comments");
        Console.WriteLine("8. 🔄 Bulk Operations");
        Console.WriteLine("9. 🔎 Advanced Search");
        Console.WriteLine("10. 📊 Reporting Features");
        Console.WriteLine("11. 👥 User Management");
        Console.WriteLine("12. 📚 Confluence Integration");
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

    // === Advanced Features Menu Methods ===

    /// <summary>
    /// Updates ticket fields
    /// </summary>
    private async Task UpdateTicketFieldsAsync()
    {
        Console.WriteLine("\n=== UPDATE TICKET FIELDS ===");

        try
        {
            Console.Write("Enter ticket key (e.g., OPS-7): ");
            var ticketKey = Console.ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(ticketKey))
            {
                Console.WriteLine("❌ Ticket key is required.");
                return;
            }

            // Get current field values
            Console.WriteLine($"🔄 Getting current field values for {ticketKey}...");
            var currentValues = await _fieldUpdateService.GetTicketFieldValuesAsync(ticketKey);

            if (!currentValues.Any())
            {
                Console.WriteLine("❌ Could not retrieve ticket information.");
                return;
            }

            Console.WriteLine("\nCurrent Values:");
            foreach (var kvp in currentValues)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
            }

            var updateRequest = new UpdateFieldsRequest();

            Console.Write("\nNew Summary (press Enter to skip): ");
            var summary = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(summary))
                updateRequest.Summary = summary;

            Console.Write("New Description (press Enter to skip): ");
            var description = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(description))
                updateRequest.Description = description;

            Console.WriteLine("\n💡 Need to assign a ticket? Use option 11 (User Management) to find account IDs!");
            Console.Write("New Assignee Account ID (press Enter to skip): ");
            var assigneeId = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(assigneeId))
            {
                // Validate account ID format (should be a valid account ID)
                if (assigneeId.Length < 10 || !assigneeId.All(c => char.IsLetterOrDigit(c) || c == '-'))
                {
                    Console.WriteLine("⚠️ Warning: This doesn't look like a valid account ID.");
                    Console.WriteLine("   Account IDs are typically long alphanumeric strings.");
                    Console.WriteLine("   Use User Management (option 11) to find the correct account ID.");
                    Console.Write("Continue anyway? (y/n): ");
                    var confirm = Console.ReadLine()?.Trim().ToLower();
                    if (confirm != "y" && confirm != "yes")
                    {
                        Console.WriteLine("❌ Assignee update cancelled.");
                        assigneeId = null;
                    }
                }

                if (!string.IsNullOrWhiteSpace(assigneeId))
                    updateRequest.AssigneeId = assigneeId;
            }

            Console.Write("New Priority ID (1=Highest, 2=High, 3=Medium, 4=Low, 5=Lowest, press Enter to skip): ");
            var priorityInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(priorityInput))
                updateRequest.PriorityId = priorityInput;

            Console.Write("New Due Date (yyyy-MM-dd, press Enter to skip): ");
            var dueDateInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(dueDateInput) && DateTime.TryParse(dueDateInput, out var dueDate))
                updateRequest.DueDate = dueDate;

            if (string.IsNullOrWhiteSpace(updateRequest.Summary) &&
                string.IsNullOrWhiteSpace(updateRequest.Description) &&
                string.IsNullOrWhiteSpace(updateRequest.AssigneeId) &&
                string.IsNullOrWhiteSpace(updateRequest.PriorityId) &&
                !updateRequest.DueDate.HasValue)
            {
                Console.WriteLine("ℹ️ No changes specified. Operation cancelled.");
                return;
            }

            Console.WriteLine($"🔄 Updating fields for {ticketKey}...");
            var success = await _fieldUpdateService.UpdateTicketFieldsAsync(ticketKey, updateRequest);

            if (success)
            {
                Console.WriteLine($"✅ Successfully updated fields for {ticketKey}!");
            }
            else
            {
                Console.WriteLine("❌ Failed to update fields. Please check permissions and try again.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating ticket fields: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds comments to tickets
    /// </summary>
    private async Task AddCommentsAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== ADD COMMENTS ===");
            Console.WriteLine("1. Add comment to specific ticket");
            Console.WriteLine("2. Use comment template");
            Console.WriteLine("3. View existing comments");
            Console.WriteLine("0. ⬅️ Back to Main Menu");
            Console.Write("\nChoose an option: ");

            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        await AddCommentToTicketAsync();
                        break;
                    case "2":
                        await UseCommentTemplateAsync();
                        break;
                    case "3":
                        await ViewCommentsAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option. Please select a number from 0-3.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in comment operations");
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

    private async Task AddCommentToTicketAsync()
    {
        Console.Write("Enter ticket key (e.g., OPS-7): ");
        var ticketKey = Console.ReadLine()?.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(ticketKey))
        {
            Console.WriteLine("❌ Ticket key is required.");
            return;
        }

        Console.Write("Enter comment text: ");
        var commentText = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(commentText))
        {
            Console.WriteLine("❌ Comment text is required.");
            return;
        }

        Console.Write("Mention users (comma-separated usernames, press Enter to skip): ");
        var mentionsInput = Console.ReadLine()?.Trim();
        var mentions = !string.IsNullOrWhiteSpace(mentionsInput)
            ? mentionsInput.Split(',').Select(m => m.Trim()).ToList()
            : new List<string>();

        var request = new AddCommentRequest
        {
            Body = commentText,
            MentionedUsers = mentions,
            NotifyUsers = true
        };

        Console.WriteLine($"💬 Adding comment to {ticketKey}...");
        var comment = await _commentService.AddCommentAsync(ticketKey, request);

        if (comment != null)
        {
            Console.WriteLine($"✅ Successfully added comment to {ticketKey}!");
            Console.WriteLine($"Comment ID: {comment.Id}");
        }
        else
        {
            Console.WriteLine("❌ Failed to add comment. Please check permissions and try again.");
        }
    }

    private async Task UseCommentTemplateAsync()
    {
        var templates = _commentService.GetCommentTemplates();

        Console.WriteLine("\nAvailable Templates:");
        for (int i = 0; i < templates.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {templates[i].Name} ({templates[i].Category})");
            Console.WriteLine($"   {templates[i].Template}");
        }

        Console.Write($"\nSelect template (1-{templates.Count}): ");
        var templateInput = Console.ReadLine()?.Trim();

        if (!int.TryParse(templateInput, out var templateIndex) ||
            templateIndex < 1 || templateIndex > templates.Count)
        {
            Console.WriteLine("❌ Invalid template selection.");
            return;
        }

        var selectedTemplate = templates[templateIndex - 1];

        Console.Write("Enter ticket key (e.g., OPS-7): ");
        var ticketKey = Console.ReadLine()?.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(ticketKey))
        {
            Console.WriteLine("❌ Ticket key is required.");
            return;
        }

        var request = new AddCommentRequest
        {
            Body = selectedTemplate.Template,
            NotifyUsers = true
        };

        Console.WriteLine($"💬 Adding template comment to {ticketKey}...");
        var comment = await _commentService.AddCommentAsync(ticketKey, request);

        if (comment != null)
        {
            Console.WriteLine($"✅ Successfully added template comment to {ticketKey}!");
        }
        else
        {
            Console.WriteLine("❌ Failed to add comment. Please check permissions and try again.");
        }
    }

    private async Task ViewCommentsAsync()
    {
        Console.Write("Enter ticket key (e.g., OPS-7): ");
        var ticketKey = Console.ReadLine()?.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(ticketKey))
        {
            Console.WriteLine("❌ Ticket key is required.");
            return;
        }

        Console.WriteLine($"📖 Getting comments for {ticketKey}...");
        var comments = await _commentService.GetCommentsAsync(ticketKey);

        if (!comments.Any())
        {
            Console.WriteLine("ℹ️ No comments found for this ticket.");
            return;
        }

        Console.WriteLine($"\nComments for {ticketKey}:");
        foreach (var comment in comments)
        {
            Console.WriteLine($"\n💬 {comment.Author.DisplayName} - {comment.Created:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"   {comment.Body}");
        }
    }

    /// <summary>
    /// Bulk operations menu
    /// </summary>
    private async Task BulkOperationsAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== BULK OPERATIONS ===");
            Console.WriteLine("1. Bulk Update Fields");
            Console.WriteLine("2. Bulk Transition Tickets");
            Console.WriteLine("3. Export/Import (CSV)");
            Console.WriteLine("0. ⬅️ Back to Main Menu");
            Console.Write("\nChoose an option: ");

            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        await BulkUpdateFieldsAsync();
                        break;
                    case "2":
                        await BulkTransitionTicketsAsync();
                        break;
                    case "3":
                        await CsvOperationsAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option. Please select a number from 0-3.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk operations");
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

    private async Task BulkUpdateFieldsAsync()
    {
        Console.Write("Enter ticket keys (comma-separated, e.g., OPS-1,OPS-2,OPS-3): ");
        var ticketKeysInput = Console.ReadLine()?.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(ticketKeysInput))
        {
            Console.WriteLine("❌ Ticket keys are required.");
            return;
        }

        var ticketKeys = ticketKeysInput.Split(',')
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToList();

        if (!ticketKeys.Any())
        {
            Console.WriteLine("❌ No valid ticket keys provided.");
            return;
        }

        var updateRequest = new UpdateFieldsRequest();

        Console.Write("New Summary (applies to all, press Enter to skip): ");
        var summary = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(summary))
            updateRequest.Summary = summary;

        Console.Write("New Assignee ID (applies to all, press Enter to skip): ");
        var assigneeId = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(assigneeId))
            updateRequest.AssigneeId = assigneeId;

        Console.Write("New Priority ID (1-5, applies to all, press Enter to skip): ");
        var priorityInput = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(priorityInput))
            updateRequest.PriorityId = priorityInput;

        if (string.IsNullOrWhiteSpace(updateRequest.Summary) &&
            string.IsNullOrWhiteSpace(updateRequest.AssigneeId) &&
            string.IsNullOrWhiteSpace(updateRequest.PriorityId))
        {
            Console.WriteLine("ℹ️ No changes specified. Operation cancelled.");
            return;
        }

        Console.WriteLine($"🔄 Bulk updating {ticketKeys.Count} tickets...");
        var result = await _fieldUpdateService.BulkUpdateFieldsAsync(ticketKeys, updateRequest);

        Console.WriteLine($"\n📊 Bulk Update Results:");
        Console.WriteLine($"✅ Successful: {result.SuccessfulUpdates}");
        Console.WriteLine($"❌ Failed: {result.FailedUpdates}");

        if (result.FailedTicketKeys.Any())
        {
            Console.WriteLine("\nFailed Tickets:");
            foreach (var failedKey in result.FailedTicketKeys)
            {
                Console.WriteLine($"  - {failedKey}");
            }
        }
    }

    private async Task BulkTransitionTicketsAsync()
    {
        Console.WriteLine("ℹ️ Bulk transition feature - Use JQL to find tickets first, then transition them one by one.");
        Console.WriteLine("For now, use the regular Transition Ticket option for individual transitions.");
        await Task.Delay(100); // Placeholder - would implement actual bulk transitions
    }

    private async Task CsvOperationsAsync()
    {
        Console.WriteLine("ℹ️ CSV Import/Export feature is planned for future release.");
        Console.WriteLine("For now, you can copy ticket information manually.");
        await Task.Delay(100); // Placeholder - would implement CSV operations
    }

    /// <summary>
    /// Advanced search menu
    /// </summary>
    private async Task AdvancedSearchAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== ADVANCED SEARCH ===");
            Console.WriteLine("1. Visual JQL Builder");
            Console.WriteLine("2. Saved Searches");
            Console.WriteLine("3. Smart Filters");
            Console.WriteLine("4. Custom JQL Search");
            Console.WriteLine("0. ⬅️ Back to Main Menu");
            Console.Write("\nChoose an option: ");

            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        await VisualJqlBuilderAsync();
                        break;
                    case "2":
                        await SavedSearchesAsync();
                        break;
                    case "3":
                        await SmartFiltersAsync();
                        break;
                    case "4":
                        await CustomJqlSearchAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option. Please select a number from 0-4.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced search");
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

    private async Task VisualJqlBuilderAsync()
    {
        Console.WriteLine("\n=== VISUAL JQL BUILDER ===");

        var request = new JqlBuilderRequest();

        Console.Write("Project Key (press Enter to skip): ");
        var projectKey = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(projectKey))
            request.ProjectKey = projectKey;

        Console.Write("Assignee (currentUser, unassigned, or username, press Enter to skip): ");
        var assignee = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(assignee))
            request.AssigneeId = assignee;

        Console.Write("Status (e.g., Open, In Progress, Done, press Enter to skip): ");
        var status = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(status))
            request.Status = status;

        Console.Write("Priority (e.g., High, Medium, Low, press Enter to skip): ");
        var priority = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(priority))
            request.Priority = priority;

        Console.Write("Created after date (yyyy-MM-dd, press Enter to skip): ");
        var createdAfterInput = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(createdAfterInput) && DateTime.TryParse(createdAfterInput, out var createdAfter))
            request.CreatedAfter = createdAfter;

        Console.Write("Text search (press Enter to skip): ");
        var textSearch = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(textSearch))
            request.TextSearch = textSearch;

        var jql = _advancedSearchService.BuildJqlQuery(request);
        Console.WriteLine($"\n🔍 Generated JQL: {jql}");

        if (string.IsNullOrWhiteSpace(jql))
        {
            Console.WriteLine("ℹ️ No search criteria specified.");
            return;
        }

        Console.Write("\nExecute this search? (y/n): ");
        var execute = Console.ReadLine()?.Trim().ToLower();

        if (execute == "y" || execute == "yes")
        {
            var searchRequest = new AdvancedSearchRequest { JqlQuery = jql };
            await ExecuteAdvancedSearchAsync(searchRequest);
        }
    }

    private async Task SavedSearchesAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== SAVED SEARCHES ===");
            Console.WriteLine("1. View Saved Searches");
            Console.WriteLine("2. Save Current Search");
            Console.WriteLine("3. Execute Saved Search");
            Console.WriteLine("0. ⬅️ Back to Advanced Search");
            Console.Write("\nChoose an option: ");

            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ViewSavedSearchesAsync();
                        break;
                    case "2":
                        await SaveCurrentSearchAsync();
                        break;
                    case "3":
                        await ExecuteSavedSearchAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option. Please select a number from 0-3.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in saved searches");
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

    private async Task ViewSavedSearchesAsync()
    {
        var savedSearches = await _advancedSearchService.GetSavedSearchesAsync();

        if (!savedSearches.Any())
        {
            Console.WriteLine("ℹ️ No saved searches found.");
            return;
        }

        Console.WriteLine("\nSaved Searches:");
        foreach (var search in savedSearches)
        {
            Console.WriteLine($"\n📝 {search.Name}");
            Console.WriteLine($"   Description: {search.Description}");
            Console.WriteLine($"   JQL: {search.JqlQuery}");
            Console.WriteLine($"   Created: {search.Created:yyyy-MM-dd HH:mm}");
        }
    }

    private async Task SaveCurrentSearchAsync()
    {
        Console.Write("Enter JQL query to save: ");
        var jql = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(jql))
        {
            Console.WriteLine("❌ JQL query is required.");
            return;
        }

        Console.Write("Enter search name: ");
        var name = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("❌ Search name is required.");
            return;
        }

        Console.Write("Enter description (optional): ");
        var description = Console.ReadLine()?.Trim() ?? string.Empty;

        var saveRequest = new SaveSearchRequest
        {
            Name = name,
            Description = description,
            JqlQuery = jql,
            IsShared = false
        };

        var savedSearch = await _advancedSearchService.SaveSearchAsync(saveRequest);
        Console.WriteLine($"✅ Successfully saved search '{savedSearch.Name}' with ID: {savedSearch.Id}");
    }

    private async Task ExecuteSavedSearchAsync()
    {
        var savedSearches = await _advancedSearchService.GetSavedSearchesAsync();

        if (!savedSearches.Any())
        {
            Console.WriteLine("ℹ️ No saved searches found. Create one first.");
            return;
        }

        Console.WriteLine("\nSaved Searches:");
        for (int i = 0; i < savedSearches.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {savedSearches[i].Name}");
        }

        Console.Write($"\nSelect search to execute (1-{savedSearches.Count}): ");
        var input = Console.ReadLine()?.Trim();

        if (!int.TryParse(input, out var index) || index < 1 || index > savedSearches.Count)
        {
            Console.WriteLine("❌ Invalid search selection.");
            return;
        }

        var selectedSearch = savedSearches[index - 1];
        Console.WriteLine($"🔍 Executing search: {selectedSearch.Name}");

        var result = await _advancedSearchService.ExecuteSavedSearchAsync(selectedSearch.Id);
        DisplaySearchResults(result);
    }

    private async Task SmartFiltersAsync()
    {
        var smartFilters = _advancedSearchService.GetSmartFilters();

        Console.WriteLine("\n🎯 Smart Filters:");
        for (int i = 0; i < smartFilters.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {smartFilters[i].Name} ({smartFilters[i].Category})");
            Console.WriteLine($"   {smartFilters[i].Description}");
        }

        Console.Write($"\nSelect filter to execute (1-{smartFilters.Count}): ");
        var input = Console.ReadLine()?.Trim();

        if (!int.TryParse(input, out var index) || index < 1 || index > smartFilters.Count)
        {
            Console.WriteLine("❌ Invalid filter selection.");
            return;
        }

        var selectedFilter = smartFilters[index - 1];
        Console.WriteLine($"🔍 Executing filter: {selectedFilter.Name}");

        var searchRequest = new AdvancedSearchRequest { JqlQuery = selectedFilter.JqlQuery };
        await ExecuteAdvancedSearchAsync(searchRequest);
    }

    private async Task CustomJqlSearchAsync()
    {
        Console.Write("Enter custom JQL query: ");
        var jql = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(jql))
        {
            Console.WriteLine("❌ JQL query is required.");
            return;
        }

        var searchRequest = new AdvancedSearchRequest { JqlQuery = jql };
        await ExecuteAdvancedSearchAsync(searchRequest);
    }

    private async Task ExecuteAdvancedSearchAsync(AdvancedSearchRequest request)
    {
        Console.WriteLine("🔍 Executing search...");
        var result = await _advancedSearchService.AdvancedSearchAsync(request);
        DisplaySearchResults(result);
    }

    private static void DisplaySearchResults(JiraSearchResult result)
    {
        Console.WriteLine($"\n📊 Search Results ({result.Issues.Count} of {result.Total}) - {result.SearchTime.TotalMilliseconds:F0}ms");

        if (!result.Issues.Any())
        {
            Console.WriteLine("ℹ️ No tickets found matching the search criteria.");
            return;
        }

        foreach (var issue in result.Issues)
        {
            Console.WriteLine($"\n🎫 {issue.Key} - {issue.Fields.Summary}");
            Console.WriteLine($"   📊 {issue.Fields.Status.Name} | ⚡ {issue.Fields.Priority.Name} | 👤 {issue.Fields.Assignee?.DisplayName ?? "Unassigned"}");
        }
    }

    /// <summary>
    /// Reporting features menu
    /// </summary>
    private async Task ReportingFeaturesAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== REPORTING FEATURES ===");

            try
            {
                var reportTypes = _reportingService.GetAvailableReportTypes();

                Console.WriteLine("Available Reports:");
                for (int i = 0; i < reportTypes.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {reportTypes[i].Name}");
                    Console.WriteLine($"   {reportTypes[i].Description}");
                }

                Console.WriteLine("0. ⬅️ Back to Main Menu");
                Console.Write($"\nSelect report type (0-{reportTypes.Count}): ");
                var input = Console.ReadLine()?.Trim();

                if (input == "0")
                {
                    return;
                }

                if (!int.TryParse(input, out var index) || index < 1 || index > reportTypes.Count)
                {
                    Console.WriteLine("❌ Invalid report selection.");
                    continue;
                }

                var selectedReport = reportTypes[index - 1];

                switch (selectedReport.Id)
                {
                    case "sprint":
                        await GenerateSprintReportAsync();
                        break;
                    case "team":
                        await GenerateTeamDashboardAsync();
                        break;
                    case "executive":
                        await GenerateExecutiveSummaryAsync();
                        break;
                    default:
                        Console.WriteLine("ℹ️ This report type is not yet implemented.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reporting features");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private async Task GenerateSprintReportAsync()
    {
        Console.Write("Enter Sprint ID: ");
        var sprintId = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(sprintId))
        {
            Console.WriteLine("❌ Sprint ID is required.");
            return;
        }

        Console.WriteLine("📊 Generating sprint report...");
        var report = await _reportingService.GenerateSprintReportAsync(sprintId);

        Console.WriteLine($"\n🏃‍♂️ SPRINT REPORT: {report.SprintName}");
        Console.WriteLine("==========================================");
        Console.WriteLine($"📅 Period: {report.StartDate:yyyy-MM-dd} to {report.EndDate:yyyy-MM-dd}");
        Console.WriteLine($"📈 Planned Points: {report.PlannedPoints}");
        Console.WriteLine($"✅ Completed Points: {report.CompletedPoints}");
        Console.WriteLine($"⏳ Remaining Points: {report.RemainingPoints}");
        Console.WriteLine($"🎯 Completion Rate: {(report.PlannedPoints > 0 ? (double)report.CompletedPoints / report.PlannedPoints * 100 : 0):F1}%");

        Console.WriteLine($"\n✅ Completed Issues ({report.CompletedIssues.Count}):");
        foreach (var issue in report.CompletedIssues.Take(5))
        {
            Console.WriteLine($"  - {issue.Key}: {issue.Fields.Summary}");
        }
        if (report.CompletedIssues.Count > 5)
        {
            Console.WriteLine($"  ... and {report.CompletedIssues.Count - 5} more");
        }

        Console.WriteLine($"\n⏳ Incomplete Issues ({report.IncompleteIssues.Count}):");
        foreach (var issue in report.IncompleteIssues.Take(5))
        {
            Console.WriteLine($"  - {issue.Key}: {issue.Fields.Summary}");
        }
        if (report.IncompleteIssues.Count > 5)
        {
            Console.WriteLine($"  ... and {report.IncompleteIssues.Count - 5} more");
        }
    }

    private async Task GenerateTeamDashboardAsync()
    {
        Console.Write("Enter Project Key: ");
        var projectKey = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(projectKey))
        {
            Console.WriteLine("❌ Project key is required.");
            return;
        }

        Console.WriteLine("📊 Generating team dashboard...");
        var dashboard = await _reportingService.GenerateTeamDashboardAsync(projectKey);

        Console.WriteLine($"\n👥 TEAM DASHBOARD: {dashboard.ProjectName}");
        Console.WriteLine("==========================================");

        Console.WriteLine("\n👤 Team Workloads:");
        foreach (var workload in dashboard.TeamWorkloads)
        {
            Console.WriteLine($"  {workload.UserName}:");
            Console.WriteLine($"    📂 Open: {workload.OpenTickets} | 🔄 In Progress: {workload.InProgressTickets} | ✅ Completed: {workload.CompletedTickets}");
            Console.WriteLine($"    📊 Total Points: {workload.TotalPoints}");
        }

        Console.WriteLine("\n📊 Status Distribution:");
        foreach (var status in dashboard.StatusDistribution)
        {
            Console.WriteLine($"  {status.Key}: {status.Value} tickets");
        }

        Console.WriteLine("\n⚡ Priority Distribution:");
        foreach (var priority in dashboard.PriorityDistribution)
        {
            Console.WriteLine($"  {priority.Key}: {priority.Value} tickets");
        }
    }

    private async Task GenerateExecutiveSummaryAsync()
    {
        Console.Write("Enter Project Key: ");
        var projectKey = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(projectKey))
        {
            Console.WriteLine("❌ Project key is required.");
            return;
        }

        Console.WriteLine("📊 Generating executive summary...");
        var summary = await _reportingService.GenerateExecutiveSummaryAsync(projectKey);

        Console.WriteLine($"\n📋 EXECUTIVE SUMMARY: {summary.ProjectName}");
        Console.WriteLine("==========================================");
        Console.WriteLine($"📅 Report Date: {summary.ReportDate:yyyy-MM-dd HH:mm}");

        Console.WriteLine($"\n📊 Project Metrics:");
        Console.WriteLine($"  📝 Total Issues: {summary.TotalIssues}");
        Console.WriteLine($"  ✅ Completed: {summary.CompletedIssues} ({summary.CompletionPercentage}%)");
        Console.WriteLine($"  🔄 In Progress: {summary.InProgressIssues}");
        Console.WriteLine($"  📂 Open: {summary.OpenIssues}");

        Console.WriteLine($"\n🎯 Key Achievements:");
        foreach (var achievement in summary.KeyAchievements)
        {
            Console.WriteLine($"  • {achievement}");
        }

        Console.WriteLine($"\n⚠️ Risk Assessment:");
        foreach (var risk in summary.Risks)
        {
            Console.WriteLine($"  • {risk}");
        }

        Console.Write("\nExport this report? (y/n): ");
        var export = Console.ReadLine()?.Trim().ToLower();

        if (export == "y" || export == "yes")
        {
            Console.WriteLine("📄 Exporting report...");
            var pdfBytes = await _reportingService.ExportReportToPdfAsync(summary, "Executive Summary");
            Console.WriteLine($"✅ Report exported ({pdfBytes.Length} bytes)");
            Console.WriteLine("ℹ️ In a real implementation, this would save to a file.");
        }
    }

    /// <summary>
    /// User Management features submenu
    /// </summary>
    private async Task UserManagementAsync()
    {
        Console.WriteLine("\n=== USER MANAGEMENT ===");
        Console.WriteLine("This feature helps you find user account IDs needed for ticket assignment.");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("1. 🔍 Search Users");
            Console.WriteLine("2. 👤 Get User by Account ID");
            Console.WriteLine("3. 📋 Get Assignable Users for Project");
            Console.WriteLine("4. 🎫 Get Assignable Users for Issue");
            Console.WriteLine("0. ⬅️ Back to Main Menu");
            Console.Write("\nChoose an option: ");

            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        await SearchUsersAsync();
                        break;
                    case "2":
                        await GetUserByAccountIdAsync();
                        break;
                    case "3":
                        await GetAssignableUsersForProjectAsync();
                        break;
                    case "4":
                        await GetAssignableUsersForIssueAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option. Please select a number from 0-4.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in user management operation");
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
    /// Search for users by name, email, or username
    /// </summary>
    private async Task SearchUsersAsync()
    {
        Console.WriteLine("\n=== SEARCH USERS ===");
        Console.Write("Enter search query (name, email, or username): ");
        var query = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            Console.WriteLine("❌ Search query cannot be empty.");
            return;
        }

        Console.Write("Maximum results (default 20): ");
        var maxResultsInput = Console.ReadLine()?.Trim();
        var maxResults = 20;
        if (!string.IsNullOrWhiteSpace(maxResultsInput) && int.TryParse(maxResultsInput, out var parsed))
        {
            maxResults = Math.Max(1, Math.Min(100, parsed)); // Limit between 1-100
        }

        Console.WriteLine($"\n🔍 Searching for users matching '{query}'...");

        var users = await _userService.SearchUsersAsync(query, maxResults);

        if (!users.Any())
        {
            Console.WriteLine("❌ No users found matching your search criteria.");
            return;
        }

        Console.WriteLine($"\n✅ Found {users.Count} user(s):");
        Console.WriteLine("==========================================");

        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];
            Console.WriteLine($"{i + 1}. {user.DisplayName}");
            Console.WriteLine($"   📧 Email: {user.EmailAddress ?? "N/A"}");
            Console.WriteLine($"   🆔 Account ID: {user.AccountId}");
            Console.WriteLine($"   🟢 Status: {(user.Active ? "Active" : "Inactive")}");

            if (i < users.Count - 1)
                Console.WriteLine();
        }

        Console.WriteLine("\n💡 Tip: Copy the Account ID to use when assigning tickets!");
    }

    /// <summary>
    /// Get user details by account ID
    /// </summary>
    private async Task GetUserByAccountIdAsync()
    {
        Console.WriteLine("\n=== GET USER BY ACCOUNT ID ===");
        Console.Write("Enter user account ID: ");
        var accountId = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(accountId))
        {
            Console.WriteLine("❌ Account ID cannot be empty.");
            return;
        }

        Console.WriteLine($"\n🔍 Looking up user with account ID: {accountId}...");

        var user = await _userService.GetUserByAccountIdAsync(accountId);

        if (user == null)
        {
            Console.WriteLine("❌ User not found with the provided account ID.");
            return;
        }

        Console.WriteLine($"\n✅ User Details:");
        Console.WriteLine("==========================================");
        Console.WriteLine($"👤 Display Name: {user.DisplayName}");
        Console.WriteLine($"📧 Email: {user.EmailAddress ?? "N/A"}");
        Console.WriteLine($"🆔 Account ID: {user.AccountId}");
        Console.WriteLine($"🟢 Status: {(user.Active ? "Active" : "Inactive")}");
    }

    /// <summary>
    /// Get assignable users for a project
    /// </summary>
    private async Task GetAssignableUsersForProjectAsync()
    {
        Console.WriteLine("\n=== GET ASSIGNABLE USERS FOR PROJECT ===");
        Console.Write("Enter project key (e.g., PROJ): ");
        var projectKey = Console.ReadLine()?.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(projectKey))
        {
            Console.WriteLine("❌ Project key cannot be empty.");
            return;
        }

        Console.Write("Maximum results (default 20): ");
        var maxResultsInput = Console.ReadLine()?.Trim();
        var maxResults = 20;
        if (!string.IsNullOrWhiteSpace(maxResultsInput) && int.TryParse(maxResultsInput, out var parsed))
        {
            maxResults = Math.Max(1, Math.Min(100, parsed)); // Limit between 1-100
        }

        Console.WriteLine($"\n🔍 Getting assignable users for project '{projectKey}'...");

        var users = await _userService.GetAssignableUsersAsync(projectKey, maxResults);

        if (!users.Any())
        {
            Console.WriteLine($"❌ No assignable users found for project '{projectKey}'.");
            return;
        }

        Console.WriteLine($"\n✅ Found {users.Count} assignable user(s) for project '{projectKey}':");
        Console.WriteLine("==========================================");

        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];
            Console.WriteLine($"{i + 1}. {user.DisplayName}");
            Console.WriteLine($"   📧 Email: {user.EmailAddress ?? "N/A"}");
            Console.WriteLine($"   🆔 Account ID: {user.AccountId}");
            Console.WriteLine($"   🟢 Status: {(user.Active ? "Active" : "Inactive")}");

            if (i < users.Count - 1)
                Console.WriteLine();
        }

        Console.WriteLine("\n💡 Tip: Copy the Account ID to use when assigning tickets!");
    }

    /// <summary>
    /// Get assignable users for a specific issue
    /// </summary>
    private async Task GetAssignableUsersForIssueAsync()
    {
        Console.WriteLine("\n=== GET ASSIGNABLE USERS FOR ISSUE ===");
        Console.Write("Enter issue key (e.g., PROJ-123): ");
        var issueKey = Console.ReadLine()?.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(issueKey))
        {
            Console.WriteLine("❌ Issue key cannot be empty.");
            return;
        }

        Console.Write("Maximum results (default 20): ");
        var maxResultsInput = Console.ReadLine()?.Trim();
        var maxResults = 20;
        if (!string.IsNullOrWhiteSpace(maxResultsInput) && int.TryParse(maxResultsInput, out var parsed))
        {
            maxResults = Math.Max(1, Math.Min(100, parsed)); // Limit between 1-100
        }

        Console.WriteLine($"\n🔍 Getting assignable users for issue '{issueKey}'...");

        var users = await _userService.GetAssignableUsersForIssueAsync(issueKey, maxResults);

        if (!users.Any())
        {
            Console.WriteLine($"❌ No assignable users found for issue '{issueKey}'.");
            return;
        }

        Console.WriteLine($"\n✅ Found {users.Count} assignable user(s) for issue '{issueKey}':");
        Console.WriteLine("==========================================");

        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];
            Console.WriteLine($"{i + 1}. {user.DisplayName}");
            Console.WriteLine($"   📧 Email: {user.EmailAddress ?? "N/A"}");
            Console.WriteLine($"   🆔 Account ID: {user.AccountId}");
            Console.WriteLine($"   🟢 Status: {(user.Active ? "Active" : "Inactive")}");

            if (i < users.Count - 1)
                Console.WriteLine();
        }

        Console.WriteLine("\n💡 Tip: Copy the Account ID to use when assigning tickets!");
    }

    /// <summary>
    /// Confluence integration menu
    /// </summary>
    private async Task ConfluenceIntegrationAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== CONFLUENCE INTEGRATION ===");
            Console.WriteLine("1. 🔐 Test Confluence Connection");
            Console.WriteLine("2. 📚 List Available Spaces");
            Console.WriteLine("3. 🔍 Search Pages");
            Console.WriteLine("4. 👀 View Page Content");
            Console.WriteLine("5. 📝 Create New Page");
            Console.WriteLine("6. 📄 List Pages in Space");
            Console.WriteLine("0. ⬅️ Back to Main Menu");
            Console.Write("\nChoose an option: ");

            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        await TestConfluenceConnectionAsync();
                        break;
                    case "2":
                        await ListConfluenceSpacesAsync();
                        break;
                    case "3":
                        await SearchConfluencePagesAsync();
                        break;
                    case "4":
                        await ViewConfluencePageAsync();
                        break;
                    case "5":
                        await CreateConfluencePageAsync();
                        break;
                    case "6":
                        await ListPagesInSpaceAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option. Please select a number from 0-6.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Confluence integration menu");
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
    /// Tests Confluence connection
    /// </summary>
    private async Task TestConfluenceConnectionAsync()
    {
        Console.WriteLine("\n=== CONFLUENCE CONNECTION TEST ===");

        var isConnected = await _confluenceService.TestConnectionAsync();

        if (isConnected)
        {
            Console.WriteLine("✅ Confluence connection successful!");
            Console.WriteLine("🔗 You can access all Confluence features.");
        }
        else
        {
            Console.WriteLine("❌ Confluence connection failed.");
            Console.WriteLine("Please check your credentials and network connection.");
            Console.WriteLine("Note: The same JIRA API token should work for Confluence.");
        }
    }

    /// <summary>
    /// Lists all available Confluence spaces
    /// </summary>
    private async Task ListConfluenceSpacesAsync()
    {
        Console.WriteLine("\n=== AVAILABLE CONFLUENCE SPACES ===");

        var spaces = await _confluenceService.GetSpacesAsync();

        if (spaces.Count == 0)
        {
            Console.WriteLine("❌ No spaces found or unable to fetch spaces.");
            return;
        }

        Console.WriteLine($"📚 Found {spaces.Count} space(s):\n");

        for (int i = 0; i < spaces.Count; i++)
        {
            var space = spaces[i];
            Console.WriteLine($"{i + 1}. {space.Name}");
            Console.WriteLine($"   🔑 Key: {space.Key}");
            Console.WriteLine($"   📝 Type: {space.Type}");
            Console.WriteLine($"   🟢 Status: {space.Status}");

            if (space.Description?.Plain?.Value != null && !string.IsNullOrEmpty(space.Description.Plain.Value))
            {
                var description = space.Description.Plain.Value.Length > 100
                    ? space.Description.Plain.Value.Substring(0, 100) + "..."
                    : space.Description.Plain.Value;
                Console.WriteLine($"   📄 Description: {description}");
            }

            if (i < spaces.Count - 1)
                Console.WriteLine();
        }
    }

    /// <summary>
    /// Searches for pages in Confluence
    /// </summary>
    private async Task SearchConfluencePagesAsync()
    {
        Console.WriteLine("\n=== SEARCH CONFLUENCE PAGES ===");

        Console.Write("Enter search query: ");
        var query = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(query))
        {
            Console.WriteLine("❌ Search query cannot be empty.");
            return;
        }

        Console.Write("Enter space key (optional, press Enter to skip): ");
        var spaceKey = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(spaceKey)) spaceKey = null;

        Console.Write("Maximum results [25]: ");
        var limitInput = Console.ReadLine()?.Trim();
        var limit = 25;
        if (!string.IsNullOrEmpty(limitInput) && int.TryParse(limitInput, out var parsedLimit))
            limit = parsedLimit;

        var searchResults = await _confluenceService.SearchPagesAsync(query, spaceKey, limit);

        if (searchResults.Results.Count == 0)
        {
            Console.WriteLine($"❌ No pages found matching '{query}'.");
            return;
        }

        Console.WriteLine($"\n🔍 Found {searchResults.Results.Count} page(s) matching '{query}':\n");

        for (int i = 0; i < searchResults.Results.Count; i++)
        {
            var result = searchResults.Results[i];
            Console.WriteLine($"{i + 1}. {result.Title}");

            if (result.Content?.Space != null)
                Console.WriteLine($"   📚 Space: {result.Content.Space.Name} ({result.Content.Space.Key})");

            if (!string.IsNullOrEmpty(result.Excerpt))
            {
                var excerpt = result.Excerpt.Length > 150
                    ? result.Excerpt.Substring(0, 150) + "..."
                    : result.Excerpt;
                Console.WriteLine($"   📄 Excerpt: {excerpt}");
            }

            if (!string.IsNullOrEmpty(result.Url))
                Console.WriteLine($"   🔗 URL: {result.Url}");

            if (i < searchResults.Results.Count - 1)
                Console.WriteLine();
        }
    }

    /// <summary>
    /// Views content of a specific Confluence page
    /// </summary>
    private async Task ViewConfluencePageAsync()
    {
        Console.WriteLine("\n=== VIEW CONFLUENCE PAGE ===");

        Console.Write("Enter page ID: ");
        var pageId = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(pageId))
        {
            Console.WriteLine("❌ Page ID cannot be empty.");
            return;
        }

        var page = await _confluenceService.GetPageAsync(pageId);

        if (page == null)
        {
            Console.WriteLine($"❌ Page with ID '{pageId}' not found.");
            return;
        }

        Console.WriteLine($"\n📄 Page Details:");
        Console.WriteLine($"Title: {page.Title}");
        Console.WriteLine($"ID: {page.Id}");
        Console.WriteLine($"Type: {page.Type}");
        Console.WriteLine($"Status: {page.Status}");

        if (page.Space != null)
            Console.WriteLine($"Space: {page.Space.Name} ({page.Space.Key})");

        if (page.Version != null)
        {
            Console.WriteLine($"Version: {page.Version.Number}");
            Console.WriteLine($"Last Modified: {page.Version.When:yyyy-MM-dd HH:mm:ss}");
            if (page.Version.By != null)
                Console.WriteLine($"Modified By: {page.Version.By.DisplayName}");
        }

        if (page.Body?.View?.Value != null)
        {
            var content = page.Body.View.Value;
            // Remove HTML tags for console display
            var plainContent = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty);

            if (plainContent.Length > 500)
                plainContent = plainContent.Substring(0, 500) + "\n... (truncated)";

            Console.WriteLine($"\nContent Preview:\n{plainContent}");
        }

        if (page.Links?.Webui != null)
        {
            var baseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL")?.TrimEnd('/');
            Console.WriteLine($"\n🔗 View in browser: {baseUrl}/wiki{page.Links.Webui}");
        }
    }

    /// <summary>
    /// Creates a new Confluence page
    /// </summary>
    private async Task CreateConfluencePageAsync()
    {
        Console.WriteLine("\n=== CREATE NEW CONFLUENCE PAGE ===");

        // First, let user select a space
        var spaces = await _confluenceService.GetSpacesAsync();
        if (spaces.Count == 0)
        {
            Console.WriteLine("❌ No spaces available. Cannot create page.");
            return;
        }

        Console.WriteLine("Available spaces:");
        for (int i = 0; i < spaces.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {spaces[i].Name} ({spaces[i].Key})");
        }

        Console.Write($"Select space (1-{spaces.Count}): ");
        var spaceChoice = Console.ReadLine()?.Trim();

        if (!int.TryParse(spaceChoice, out var spaceIndex) || spaceIndex < 1 || spaceIndex > spaces.Count)
        {
            Console.WriteLine("❌ Invalid space selection.");
            return;
        }

        var selectedSpace = spaces[spaceIndex - 1];

        Console.Write("Enter page title: ");
        var title = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("❌ Page title cannot be empty.");
            return;
        }

        Console.Write("Enter page content (HTML format): ");
        var content = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(content))
        {
            content = "<p>This page was created via the JIRA Integration Console Application.</p>";
        }

        Console.Write("Enter parent page ID (optional, press Enter to skip): ");
        var parentId = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(parentId)) parentId = null;

        var createdPage = await _confluenceService.CreatePageAsync(selectedSpace.Key, title, content, parentId);

        if (createdPage != null)
        {
            Console.WriteLine($"✅ Page created successfully!");
            Console.WriteLine($"📄 Title: {createdPage.Title}");
            Console.WriteLine($"🆔 ID: {createdPage.Id}");
            Console.WriteLine($"📚 Space: {selectedSpace.Name} ({selectedSpace.Key})");

            if (createdPage.Links?.Webui != null)
            {
                var baseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL")?.TrimEnd('/');
                Console.WriteLine($"🔗 View in browser: {baseUrl}/wiki{createdPage.Links.Webui}");
            }
        }
        else
        {
            Console.WriteLine("❌ Failed to create page. Please check your permissions and try again.");
        }
    }

    /// <summary>
    /// Lists pages in a specific space
    /// </summary>
    private async Task ListPagesInSpaceAsync()
    {
        Console.WriteLine("\n=== LIST PAGES IN SPACE ===");

        // First, let user select a space
        var spaces = await _confluenceService.GetSpacesAsync();
        if (spaces.Count == 0)
        {
            Console.WriteLine("❌ No spaces available.");
            return;
        }

        Console.WriteLine("Available spaces:");
        for (int i = 0; i < spaces.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {spaces[i].Name} ({spaces[i].Key})");
        }

        Console.Write($"Select space (1-{spaces.Count}): ");
        var spaceChoice = Console.ReadLine()?.Trim();

        if (!int.TryParse(spaceChoice, out var spaceIndex) || spaceIndex < 1 || spaceIndex > spaces.Count)
        {
            Console.WriteLine("❌ Invalid space selection.");
            return;
        }

        var selectedSpace = spaces[spaceIndex - 1];

        Console.Write("Maximum results [25]: ");
        var limitInput = Console.ReadLine()?.Trim();
        var limit = 25;
        if (!string.IsNullOrEmpty(limitInput) && int.TryParse(limitInput, out var parsedLimit))
            limit = parsedLimit;

        var pages = await _confluenceService.GetPagesInSpaceAsync(selectedSpace.Key, limit);

        if (pages.Count == 0)
        {
            Console.WriteLine($"❌ No pages found in space '{selectedSpace.Name}'.");
            return;
        }

        Console.WriteLine($"\n📚 Found {pages.Count} page(s) in '{selectedSpace.Name}':\n");

        for (int i = 0; i < pages.Count; i++)
        {
            var page = pages[i];
            Console.WriteLine($"{i + 1}. {page.Title}");
            Console.WriteLine($"   🆔 ID: {page.Id}");
            Console.WriteLine($"   📝 Type: {page.Type}");
            Console.WriteLine($"   🟢 Status: {page.Status}");

            if (page.Version != null)
            {
                Console.WriteLine($"   📅 Last Modified: {page.Version.When:yyyy-MM-dd HH:mm:ss}");
                if (page.Version.By != null)
                    Console.WriteLine($"   👤 Modified By: {page.Version.By.DisplayName}");
            }

            if (i < pages.Count - 1)
                Console.WriteLine();
        }
    }
}
