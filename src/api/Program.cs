using EnterpriseAgentAccelerator.Api.Configuration;
using EnterpriseAgentAccelerator.Api.Prompt;
using EnterpriseAgentAccelerator.Api.Session;

const string LocalDevCorsPolicy = "LocalDevCors";

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

var azureOpenAiConfig = AzureOpenAiConfig.FromEnvironment();

builder.Services.AddSingleton(azureOpenAiConfig);
builder.Services.AddSingleton<ISessionStore, InMemorySessionStore>();
builder.Services.AddTransient<IPromptBuilder, PromptBuilder>();

if (builder.Environment.IsDevelopment())
{
    var appConfig = AppConfig.FromEnvironment();
    builder.Services.AddSingleton(appConfig);
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(LocalDevCorsPolicy, policy =>
        {
            policy
                .WithOrigins(appConfig.CorsAllowedOrigin)
                .WithMethods("GET", "POST")
                .WithHeaders("Content-Type");
        });
    });
}
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(LocalDevCorsPolicy);
}

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok());

app.Run();

public partial class Program;
