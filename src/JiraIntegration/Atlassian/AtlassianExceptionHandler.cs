using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JiraIntegration.Atlassian;

/// <summary>
/// Turns a failed call to Atlassian into problem details for the browser. Atlassian's
/// own 400 and 404 pass through; any other error from Atlassian is a 502, since the
/// site failed, not the caller.
/// </summary>
public sealed class AtlassianExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<AtlassianExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var problem = ToProblem(exception, context.RequestAborted);
        if (problem is null)
        {
            return false;
        }

        logger.LogWarning("{Method} {Path} failed: {Title}: {Detail}",
            context.Request.Method, context.Request.Path, problem.Title, problem.Detail);

        context.Response.StatusCode = problem.Status!.Value;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static ProblemDetails? ToProblem(Exception exception, CancellationToken requestAborted) => exception switch
    {
        AtlassianNotConfiguredException missing => new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = "The Atlassian site is not configured",
            Detail = missing.Message,
        },
        AtlassianApiException { StatusCode: HttpStatusCode.NotFound } api => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Atlassian could not find it",
            Detail = api.Message,
        },
        AtlassianApiException { StatusCode: HttpStatusCode.BadRequest } api => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Atlassian refused the request",
            Detail = api.Message,
        },
        AtlassianApiException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden } api => new ProblemDetails
        {
            Status = StatusCodes.Status502BadGateway,
            Title = "Atlassian refused the credentials or the permission",
            Detail = api.Message,
        },
        AtlassianApiException api => new ProblemDetails
        {
            Status = StatusCodes.Status502BadGateway,
            Title = $"Atlassian answered {(int)api.StatusCode}",
            Detail = api.Message,
        },
        HttpRequestException => new ProblemDetails
        {
            Status = StatusCodes.Status504GatewayTimeout,
            Title = "The Atlassian site could not be reached",
            Detail = exception.Message,
        },
        TaskCanceledException when !requestAborted.IsCancellationRequested => new ProblemDetails
        {
            Status = StatusCodes.Status504GatewayTimeout,
            Title = "The Atlassian site did not answer in time",
            Detail = exception.Message,
        },
        _ => null,
    };
}
