using JiraIntegration.Models.Dto;
using JiraIntegration.Models.Requests;

namespace JiraIntegration.Services.Interfaces;

/// <summary>
/// Service for generating reports and analytics
/// </summary>
public interface IJiraReportingService
{
    /// <summary>
    /// Generates sprint report data
    /// </summary>
    Task<SprintReport> GenerateSprintReportAsync(string sprintId);

    /// <summary>
    /// Generates team dashboard data
    /// </summary>
    Task<TeamDashboard> GenerateTeamDashboardAsync(string projectKey);

    /// <summary>
    /// Generates executive summary
    /// </summary>
    Task<ExecutiveSummary> GenerateExecutiveSummaryAsync(string projectKey);

    /// <summary>
    /// Exports report to PDF
    /// </summary>
    Task<byte[]> ExportReportToPdfAsync(object reportData, string reportType);

    /// <summary>
    /// Exports report to Excel
    /// </summary>
    Task<byte[]> ExportReportToExcelAsync(object reportData, string reportType);

    /// <summary>
    /// Gets available report types
    /// </summary>
    List<ReportType> GetAvailableReportTypes();
}
