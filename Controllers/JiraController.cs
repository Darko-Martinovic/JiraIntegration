using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<JiraController> _logger;

        public JiraController(
            IJiraTicketService ticketService,
            IJiraAuthService authService,
            ILogger<JiraController> logger)
        {
            _ticketService = ticketService;
            _authService = authService;
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
                    User = user
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
        /// Create a new Jira ticket
        /// </summary>
        [HttpPost("tickets")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateIssueRequest request)
        {
            try
            {
                _logger.LogInformation("Creating ticket with summary: {Summary}", request.Summary);

                var createdIssue = await _ticketService.CreateTicketAsync(request);

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
                    Message = "Failed to create ticket"
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
    }
}