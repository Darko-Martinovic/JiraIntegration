using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;
using JiraIntegration.Services.Interfaces;

namespace JiraIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JiraController : ControllerBase
    {
        private readonly IJiraTicketService _ticketService;
        private readonly IJiraAuthService _authService;
        private readonly IJiraSearchService _searchService;
        private readonly ILogger<JiraController> _logger;
        private readonly JiraSettings _jiraSettings;

        public JiraController(
            IJiraTicketService ticketService,
            IJiraAuthService authService,
            IJiraSearchService searchService,
            IOptions<JiraSettings> jiraSettings,
            ILogger<JiraController> logger)
        {
            _ticketService = ticketService;
            _authService = authService;
            _searchService = searchService;
            _jiraSettings = jiraSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Test Jira connection
        /// </summary>
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var isValid = await _authService.ValidateConnectionAsync();
                var user = isValid ? await _authService.GetCurrentUserAsync() : null;

                return Ok(new
                {
                    IsConnected = isValid,
                    Message = isValid ? "Successfully connected to Jira" : "Failed to connect to Jira",
                    User = user,
                    Settings = new
                    {
                        BaseUrl = _jiraSettings.BaseUrl,
                        ProjectKey = _jiraSettings.ProjectKey,
                        Email = _jiraSettings.Email,
                        MaxResults = _jiraSettings.MaxResults,
                        TimeoutSeconds = _jiraSettings.TimeoutSeconds
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Jira connection");
                return Ok(new
                {
                    IsConnected = false,
                    Message = $"Connection error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get configuration settings (without sensitive data)
        /// </summary>
        [HttpGet("configuration")]
        public IActionResult GetConfiguration()
        {
            try
            {
                return Ok(new
                {
                    BaseUrl = _jiraSettings.BaseUrl,
                    ProjectKey = _jiraSettings.ProjectKey,
                    Email = _jiraSettings.Email,
                    MaxResults = _jiraSettings.MaxResults,
                    TimeoutSeconds = _jiraSettings.TimeoutSeconds,
                    HasApiToken = !string.IsNullOrEmpty(_jiraSettings.ApiToken),
                    IsValid = _jiraSettings.IsValid()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration");
                return StatusCode(500, $"Error retrieving configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new Jira ticket
        /// </summary>
        [HttpPost("tickets")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequestDto request)
        {
            try
            {
                // Log the incoming request for debugging
                _logger.LogInformation("Creating ticket with request: {@Request}", request);

                // Check model state for validation errors
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>())
                        .ToArray();
                    _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                // Validate required fields manually
                if (string.IsNullOrWhiteSpace(request.Summary))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Summary is required"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.ProjectKey))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Project key is required"
                    });
                }

                _logger.LogInformation("Creating ticket with summary: {Summary}", request.Summary);

                // Map issue type name to ID (you may need to adjust these IDs based on your JIRA setup)
                var issueTypeId = request.IssueType switch
                {
                    "Task" => "10001",
                    "Bug" => "10004",
                    "Story" => "10002",
                    "Epic" => "10000",
                    _ => "10001" // Default to Task
                };

                // For now, we'll skip assignee if it's an email - you'd need to look up account ID
                var createRequest = new CreateIssueRequest
                {
                    ProjectKey = request.ProjectKey,
                    Summary = request.Summary,
                    Description = request.Description,
                    IssueTypeId = issueTypeId,
                    Priority = request.Priority,
                    AssigneeId = string.IsNullOrEmpty(request.Assignee) || request.Assignee.Contains("@") ? null : request.Assignee
                };

                var createdIssue = await _ticketService.CreateTicketAsync(createRequest);

                if (createdIssue != null)
                {
                    var baseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL")?.TrimEnd('/');
                    var jiraUrl = $"{baseUrl}/browse/{createdIssue.Key}";

                    return Ok(new
                    {
                        Success = true,
                        Message = "Ticket created successfully",
                        Ticket = createdIssue,
                        JiraUrl = jiraUrl
                    });
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to create ticket - JIRA did not return a created issue"
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("POST request failed"))
            {
                _logger.LogError(ex, "JIRA API error creating ticket");

                // Extract more specific error information
                var errorMessage = "Failed to create ticket";
                if (ex.Message.Contains("priority"))
                {
                    errorMessage = "Failed to create ticket: Priority field is not available for this project";
                }
                else if (ex.Message.Contains("assignee"))
                {
                    errorMessage = "Failed to create ticket: Invalid assignee specified";
                }
                else if (ex.Message.Contains("screen"))
                {
                    errorMessage = "Failed to create ticket: Some fields are not available on the creation screen";
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = errorMessage,
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Error creating ticket: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get tickets for the configured project
        /// </summary>
        [HttpGet("tickets")]
        public async Task<IActionResult> GetProjectTickets([FromQuery] int maxResults = 20)
        {
            try
            {
                var tickets = await _searchService.GetProjectTicketsAsync(_jiraSettings.ProjectKey, maxResults);
                var baseUrl = _jiraSettings.BaseUrl?.TrimEnd('/');

                var ticketsWithUrls = tickets.Select(ticket => new
                {
                    key = ticket.Key,
                    summary = ticket.Fields?.Summary ?? "No Summary",
                    status = ticket.Fields?.Status?.Name ?? "Unknown",
                    priority = ticket.Fields?.Priority?.Name ?? "No Priority",
                    issueType = ticket.Fields?.IssueType?.Name ?? "Unknown",
                    assignee = ticket.Fields?.Assignee?.DisplayName ?? "Unassigned",
                    created = ticket.Fields?.Created ?? DateTime.MinValue,
                    updated = ticket.Fields?.Updated ?? DateTime.MinValue,
                    jiraUrl = $"{baseUrl}/browse/{ticket.Key}"
                }).ToList();

                return Ok(new
                {
                    success = true,
                    tickets = ticketsWithUrls,
                    count = ticketsWithUrls.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project tickets");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Error retrieving tickets: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get ticket by key
        /// </summary>
        [HttpGet("tickets/{ticketKey}")]
        public async Task<IActionResult> GetTicket(string ticketKey)
        {
            try
            {
                var ticket = await _ticketService.GetTicketAsync(ticketKey);
                if (ticket == null)
                {
                    return NotFound($"Ticket '{ticketKey}' not found");
                }

                var baseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL")?.TrimEnd('/');
                var jiraUrl = $"{baseUrl}/browse/{ticket.Key}";

                return Ok(new
                {
                    Ticket = ticket,
                    JiraUrl = jiraUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket {TicketKey}", ticketKey);
                return StatusCode(500, $"Error retrieving ticket: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing ticket
        /// </summary>
        [HttpPut("tickets/{ticketKey}")]
        public async Task<IActionResult> UpdateTicket(string ticketKey, [FromBody] UpdateIssueRequest request)
        {
            try
            {
                _logger.LogInformation("Updating ticket {TicketKey}", ticketKey);

                var success = await _ticketService.UpdateTicketAsync(ticketKey, request);

                if (success)
                {
                    var baseUrl = _jiraSettings.BaseUrl?.TrimEnd('/');
                    var jiraUrl = $"{baseUrl}/browse/{ticketKey}";

                    return Ok(new
                    {
                        Success = true,
                        Message = "Ticket updated successfully",
                        TicketKey = ticketKey,
                        JiraUrl = jiraUrl
                    });
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to update ticket"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketKey}", ticketKey);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Error updating ticket: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Delete a ticket by key
        /// </summary>
        [HttpDelete("tickets/{ticketKey}")]
        public async Task<IActionResult> DeleteTicket(string ticketKey)
        {
            try
            {
                _logger.LogInformation("Deleting ticket {TicketKey}", ticketKey);

                var success = await _ticketService.DeleteTicketAsync(ticketKey);

                if (success)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = $"Ticket {ticketKey} deleted successfully"
                    });
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to delete ticket"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket {TicketKey}", ticketKey);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Error deleting ticket: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get available transitions for a ticket
        /// </summary>
        [HttpGet("tickets/{ticketKey}/transitions")]
        public async Task<IActionResult> GetTicketTransitions(string ticketKey)
        {
            try
            {
                var transitions = await _ticketService.GetAvailableTransitionsAsync(ticketKey);
                return Ok(new
                {
                    Success = true,
                    TicketKey = ticketKey,
                    Transitions = transitions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transitions for ticket {TicketKey}", ticketKey);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Error retrieving transitions: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Transition a ticket to a new status
        /// </summary>
        [HttpPost("tickets/{ticketKey}/transitions")]
        public async Task<IActionResult> TransitionTicket(string ticketKey, [FromBody] TransitionIssueRequest request)
        {
            try
            {
                _logger.LogInformation("Transitioning ticket {TicketKey} with transition {TransitionId}", ticketKey, request.TransitionId);

                var success = await _ticketService.TransitionTicketAsync(ticketKey, request.TransitionId, request.Comment);

                if (success)
                {
                    var baseUrl = _jiraSettings.BaseUrl?.TrimEnd('/');
                    var jiraUrl = $"{baseUrl}/browse/{ticketKey}";

                    return Ok(new
                    {
                        Success = true,
                        Message = "Ticket transitioned successfully",
                        TicketKey = ticketKey,
                        JiraUrl = jiraUrl
                    });
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to transition ticket"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transitioning ticket {TicketKey}", ticketKey);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Error transitioning ticket: {ex.Message}"
                });
            }
        }
    }
}