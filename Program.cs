using JiraIntegration.Configuration;
using JiraIntegration.Models;
using JiraIntegration.Services.Base;
using JiraIntegration.Services.Implementations;
using JiraIntegration.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:4173", "http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Load environment variables from .env file
DotNetEnv.Env.Load(".env");

// Configure Jira settings
builder.Services.Configure<JiraSettings>(options =>
{
    options.BaseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL") ?? "";
    options.Email = Environment.GetEnvironmentVariable("JIRA_USER_EMAIL") ?? "";
    options.ApiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN") ?? "";
    options.ProjectKey = Environment.GetEnvironmentVariable("JIRA_PROJECT_KEY") ?? "";
});

// Configure HTTP client
builder.Services.AddHttpClient<BaseJiraHttpService>(client =>
{
    var baseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL");
    if (!string.IsNullOrEmpty(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Register Jira services
builder.Services.AddScoped<IJiraAuthService, JiraAuthService>();
builder.Services.AddScoped<IJiraTicketService, JiraTicketService>();
builder.Services.AddScoped<IJiraSearchService, JiraSearchService>();
builder.Services.AddScoped<IJiraProjectService, JiraProjectService>();
builder.Services.AddScoped<IJiraFieldUpdateService, JiraFieldUpdateService>();
builder.Services.AddScoped<IJiraCommentService, JiraCommentService>();
builder.Services.AddScoped<IJiraAdvancedSearchService, JiraAdvancedSearchService>();
builder.Services.AddScoped<IJiraReportingService, JiraReportingService>();
builder.Services.AddScoped<IJiraUserService, JiraUserService>();
builder.Services.AddScoped<IConfluenceService, ConfluenceService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRouting();
app.MapControllers();

app.Run();
