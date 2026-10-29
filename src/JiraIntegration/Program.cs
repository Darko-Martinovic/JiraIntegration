using DotNetEnv;
using DotNetEnv.Configuration;
using JiraIntegration.Atlassian;
using JiraIntegration.Configuration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// The credentials live in .env (see .env.example), which is looked for in the
// working directory and its parents. Tests set their own values instead.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());
}

builder.Services.AddAtlassianSettings(builder.Configuration);
builder.Services.AddAtlassianClients();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AtlassianExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.Run();

public partial class Program;
