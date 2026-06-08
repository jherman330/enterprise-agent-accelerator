using EnterpriseAgentAccelerator.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var azureOpenAiConfig = AzureOpenAiConfig.FromEnvironment();
var appConfig = AppConfig.FromEnvironment();

builder.Services.AddSingleton(azureOpenAiConfig);
builder.Services.AddSingleton(appConfig);
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok());

app.Run();
