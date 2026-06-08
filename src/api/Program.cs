using EnterpriseAgentAccelerator.Api.Configuration;
using EnterpriseAgentAccelerator.Api.Prompt;
using EnterpriseAgentAccelerator.Api.Session;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

const string LocalDevCorsPolicy = "LocalDevCors";

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AzureOpenAi:Endpoint"] = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
    ["AzureOpenAi:Deployment"] = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"),
    ["AzureOpenAi:ApiKey"] = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"),
});

var azureOpenAiSection = builder.Configuration.GetSection("AzureOpenAi");

builder.Services
    .AddOptions<AzureOpenAiConfig>()
    .Bind(azureOpenAiSection)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<AzureOpenAiConfig>>().Value);

builder.Services.AddKernel();
builder.Services.AddSingleton<IChatCompletionService>(sp =>
{
    var config = sp.GetRequiredService<IOptions<AzureOpenAiConfig>>().Value;

    return Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            deploymentName: config.Deployment,
            endpoint: config.Endpoint,
            apiKey: config.ApiKey)
        .Build()
        .GetRequiredService<IChatCompletionService>();
});
builder.Services.AddSingleton<ISessionStore, InMemorySessionStore>();
builder.Services.AddTransient<IPromptBuilder, PromptBuilder>();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["App:CorsAllowedOrigin"] = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGIN"),
    });

    var appSection = builder.Configuration.GetSection("App");

    builder.Services
        .AddOptions<AppConfig>()
        .Bind(appSection)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppConfig>>().Value);
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(LocalDevCorsPolicy, policy =>
        {
            policy
                .WithOrigins(appSection["CorsAllowedOrigin"] ?? string.Empty)
                .WithMethods("GET", "POST")
                .WithHeaders("Content-Type");
        });
    });
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

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
