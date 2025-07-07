using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JiraIntegration.Models;
using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Interfaces;
using System.Text;

namespace JiraIntegration.Services.Implementations;

/// <summary>
/// Implementation of Jira reporting service
/// </summary>
public class JiraReportingService : BaseJiraHttpService, IJiraReportingService
{
    public JiraReportingService(
        HttpClient httpClient,
        IOptions<JiraSettings> settings,
        ILogger<JiraReportingService> logger
    )
        : base(httpClient, settings, logger) { }

    /// <summary>
    /// Generates sprint report data
    /// </summary>
    public async Task<SprintReport> GenerateSprintReportAsync(string sprintId)
    {
        try
        {
            _logger.LogDebug("Generating sprint report for sprint: {SprintId}", sprintId);

            // Get sprint issues using JQL
            var jql = $"sprint = {sprintId}";
            var response = await GetAsync<JiraSearchResponse>(
                $"/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=1000"
            );

            if (response?.Issues == null)
            {
                return new SprintReport();
            }

            var completedIssues = response.Issues
                .Where(
                    i =>
                        i.Fields.Status.Name.Equals("Done", StringComparison.OrdinalIgnoreCase)
                        || i.Fields.Status.Name.Equals(
                            "Resolved",
                            StringComparison.OrdinalIgnoreCase
                        )
                )
                .ToList();

            var incompleteIssues = response.Issues.Except(completedIssues).ToList();

            var report = new SprintReport
            {
                SprintName = $"Sprint {sprintId}",
                StartDate = DateTime.UtcNow.AddDays(-14), // Assume 2-week sprint
                EndDate = DateTime.UtcNow,
                PlannedPoints = response.Issues.Count * 3, // Assume 3 points average
                CompletedPoints = completedIssues.Count * 3,
                RemainingPoints = incompleteIssues.Count * 3,
                CompletedIssues = completedIssues,
                IncompleteIssues = incompleteIssues
            };

            _logger.LogInformation(
                "Sprint report generated for {SprintId}. Completed: {Completed}, Incomplete: {Incomplete}",
                sprintId,
                completedIssues.Count,
                incompleteIssues.Count
            );

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sprint report for sprint: {SprintId}", sprintId);
            throw;
        }
    }

    /// <summary>
    /// Generates team dashboard data
    /// </summary>
    public async Task<TeamDashboard> GenerateTeamDashboardAsync(string projectKey)
    {
        try
        {
            _logger.LogInformation(
                "Generating team dashboard for project: {ProjectKey}",
                projectKey
            );

            var jql = $"project = \"{projectKey}\"";
            var response = await GetAsync<JiraSearchResponse>(
                $"/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=1000"
            );

            if (response?.Issues == null)
            {
                return new TeamDashboard { ProjectName = projectKey };
            }

            // Group by assignee
            var workloadsByUser = response.Issues
                .GroupBy(i => i.Fields.Assignee?.DisplayName ?? "Unassigned")
                .Select(
                    g =>
                        new TeamMemberWorkload
                        {
                            UserName = g.Key,
                            OpenTickets = g.Count(
                                i =>
                                    !i.Fields.Status.Name.Equals(
                                        "Done",
                                        StringComparison.OrdinalIgnoreCase
                                    )
                            ),
                            InProgressTickets = g.Count(
                                i =>
                                    i.Fields.Status.Name.Contains(
                                        "Progress",
                                        StringComparison.OrdinalIgnoreCase
                                    )
                            ),
                            CompletedTickets = g.Count(
                                i =>
                                    i.Fields.Status.Name.Equals(
                                        "Done",
                                        StringComparison.OrdinalIgnoreCase
                                    )
                            ),
                            TotalPoints = g.Count() * 3 // Assume 3 points average
                        }
                )
                .ToList();

            // Status distribution
            var statusDistribution = response.Issues
                .GroupBy(i => i.Fields.Status.Name)
                .ToDictionary(g => g.Key, g => g.Count());

            // Priority distribution
            var priorityDistribution = response.Issues
                .GroupBy(i => i.Fields.Priority.Name)
                .ToDictionary(g => g.Key, g => g.Count());

            var dashboard = new TeamDashboard
            {
                ProjectName = projectKey,
                TeamWorkloads = workloadsByUser,
                StatusDistribution = statusDistribution,
                PriorityDistribution = priorityDistribution
            };

            _logger.LogInformation(
                "Team dashboard generated for project: {ProjectKey}",
                projectKey
            );
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error generating team dashboard for project: {ProjectKey}",
                projectKey
            );
            throw;
        }
    }

    /// <summary>
    /// Generates executive summary
    /// </summary>
    public async Task<ExecutiveSummary> GenerateExecutiveSummaryAsync(string projectKey)
    {
        try
        {
            _logger.LogInformation(
                "Generating executive summary for project: {ProjectKey}",
                projectKey
            );

            var jql = $"project = \"{projectKey}\"";
            var response = await GetAsync<JiraSearchResponse>(
                $"/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=1000"
            );

            if (response?.Issues == null)
            {
                return new ExecutiveSummary { ProjectName = projectKey };
            }

            var totalIssues = response.Issues.Count;
            var completedIssues = response.Issues.Count(
                i =>
                    i.Fields.Status.Name.Equals("Done", StringComparison.OrdinalIgnoreCase)
                    || i.Fields.Status.Name.Equals("Resolved", StringComparison.OrdinalIgnoreCase)
            );
            var inProgressIssues = response.Issues.Count(
                i => i.Fields.Status.Name.Contains("Progress", StringComparison.OrdinalIgnoreCase)
            );
            var openIssues = totalIssues - completedIssues - inProgressIssues;

            var completionPercentage =
                totalIssues > 0 ? (double)completedIssues / totalIssues * 100 : 0;

            var summary = new ExecutiveSummary
            {
                ProjectName = projectKey,
                ReportDate = DateTime.UtcNow,
                TotalIssues = totalIssues,
                CompletedIssues = completedIssues,
                InProgressIssues = inProgressIssues,
                OpenIssues = openIssues,
                CompletionPercentage = Math.Round(completionPercentage, 2),
                KeyAchievements = new List<string>
                {
                    $"Completed {completedIssues} tickets",
                    $"Achieved {completionPercentage:F1}% completion rate",
                    "Maintained active development momentum"
                },
                Risks = new List<string>
                {
                    openIssues > inProgressIssues
                        ? "High number of open tickets"
                        : "Manageable backlog",
                    completionPercentage < 50
                        ? "Low completion rate needs attention"
                        : "Good progress rate"
                }
            };

            _logger.LogInformation(
                "Executive summary generated for project: {ProjectKey}. Completion: {Completion}%",
                projectKey,
                completionPercentage
            );

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error generating executive summary for project: {ProjectKey}",
                projectKey
            );
            throw;
        }
    }

    /// <summary>
    /// Exports report to PDF (simplified implementation)
    /// </summary>
    public async Task<byte[]> ExportReportToPdfAsync(object reportData, string reportType)
    {
        try
        {
            _logger.LogDebug("Exporting {ReportType} report to PDF", reportType);

            // Simplified implementation - would normally use a PDF library like iTextSharp
            var content = $"JIRA {reportType} Report\n";
            content += $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n\n";
            content += reportData.ToString();

            var bytes = Encoding.UTF8.GetBytes(content);

            await Task.Delay(100); // Simulate processing time
            _logger.LogInformation("PDF export completed for {ReportType} report", reportType);

            return bytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting {ReportType} report to PDF", reportType);
            throw;
        }
    }

    /// <summary>
    /// Exports report to Excel (simplified implementation)
    /// </summary>
    public async Task<byte[]> ExportReportToExcelAsync(object reportData, string reportType)
    {
        try
        {
            _logger.LogDebug("Exporting {ReportType} report to Excel", reportType);

            // Simplified implementation - would normally use EPPlus or similar
            var content = $"JIRA {reportType} Report\n";
            content += $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n\n";
            content += reportData.ToString();

            var bytes = Encoding.UTF8.GetBytes(content);

            await Task.Delay(100); // Simulate processing time
            _logger.LogInformation("Excel export completed for {ReportType} report", reportType);

            return bytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting {ReportType} report to Excel", reportType);
            throw;
        }
    }

    /// <summary>
    /// Gets available report types
    /// </summary>
    public List<ReportType> GetAvailableReportTypes()
    {
        return new List<ReportType>
        {
            new()
            {
                Id = "sprint",
                Name = "Sprint Report",
                Description = "Sprint burndown and velocity tracking",
                RequiredParameters = new List<string> { "sprintId" }
            },
            new()
            {
                Id = "team",
                Name = "Team Dashboard",
                Description = "Team workload and distribution",
                RequiredParameters = new List<string> { "projectKey" }
            },
            new()
            {
                Id = "executive",
                Name = "Executive Summary",
                Description = "High-level project status",
                RequiredParameters = new List<string> { "projectKey" }
            },
            new()
            {
                Id = "velocity",
                Name = "Velocity Chart",
                Description = "Team velocity over time",
                RequiredParameters = new List<string> { "projectKey", "sprints" }
            },
            new()
            {
                Id = "burndown",
                Name = "Burndown Chart",
                Description = "Sprint or release burndown",
                RequiredParameters = new List<string> { "sprintId" }
            }
        };
    }
}
