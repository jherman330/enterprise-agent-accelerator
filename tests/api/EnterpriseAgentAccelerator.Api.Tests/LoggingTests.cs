using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnterpriseAgentAccelerator.Api.Tests;

public sealed class LoggingTests
{
    private const string AllowedOrigin = "http://localhost:5173";
    private const string EndpointVariableName = "AZURE_OPENAI_ENDPOINT";
    private const string DeploymentVariableName = "AZURE_OPENAI_DEPLOYMENT";
    private const string ApiKeyVariableName = "AZURE_OPENAI_API_KEY";
    private const string CorsAllowedOriginVariableName = "CORS_ALLOWED_ORIGIN";
    private const string SemanticKernelCategory = "Microsoft.SemanticKernel";

    [Fact]
    public async Task LogOutputDoesNotContainSensitiveValuesDuringRequestProcessing()
    {
        const string secretApiKey = "wo8-super-secret-api-key";
        const string secretDeployment = "wo8-secret-deployment";
        const string secretEndpoint = "https://wo8-secret-endpoint.openai.azure.com/";

        using var environment = SetRequiredEnvironmentVariables(
            endpoint: secretEndpoint,
            apiKey: secretApiKey,
            deployment: secretDeployment);
        var logMessages = new List<string>();

        using var factory = CreateFactory("Development", logMessages);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();

        var logger = factory.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("EnterpriseAgentAccelerator.Api");
        logger.LogInformation("wo8-request-processing-probe");

        Assert.NotEmpty(logMessages);
        Assert.Contains(
            logMessages,
            message => message.Contains("wo8-request-processing-probe", StringComparison.Ordinal));

        var allLogs = string.Join(Environment.NewLine, logMessages);
        Assert.DoesNotContain(secretApiKey, allLogs);
        Assert.DoesNotContain(secretDeployment, allLogs);
        Assert.DoesNotContain(secretEndpoint, allLogs);
    }

    [Fact]
    public void DevelopmentEnvironmentEmitsSemanticKernelInformationLogs()
    {
        const string probeMessage = "wo8-semantic-kernel-information-probe";

        using var environment = SetRequiredEnvironmentVariables();
        var logMessages = new List<string>();

        using var factory = CreateFactory("Development", logMessages);
        var logger = factory.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(SemanticKernelCategory);

        logger.LogInformation(probeMessage);

        Assert.Contains(logMessages, message => message.Contains(probeMessage, StringComparison.Ordinal));
    }

    [Fact]
    public void ProductionEnvironmentSuppressesSemanticKernelInformationLogs()
    {
        const string informationProbe = "wo8-semantic-kernel-information-probe";
        const string warningProbe = "wo8-semantic-kernel-warning-probe";

        using var environment = SetRequiredEnvironmentVariables(includeCorsOrigin: false);
        var logMessages = new List<string>();

        using var factory = CreateFactory("Production", logMessages);
        var logger = factory.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(SemanticKernelCategory);

        logger.LogInformation(informationProbe);
        logger.LogWarning(warningProbe);

        Assert.DoesNotContain(logMessages, message => message.Contains(informationProbe, StringComparison.Ordinal));
        Assert.Contains(logMessages, message => message.Contains(warningProbe, StringComparison.Ordinal));
    }

    private static WebApplicationFactory<Program> CreateFactory(
        string environmentName,
        List<string> logMessages)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(environmentName);
                builder.ConfigureLogging(logging => logging.AddProvider(new ListLoggerProvider(logMessages)));
            });
    }

    private static EnvironmentVariableScope SetRequiredEnvironmentVariables(
        string endpoint = "https://example.openai.azure.com/",
        string apiKey = "test-api-key",
        string deployment = "test-deployment",
        bool includeCorsOrigin = true)
    {
        var environment = new EnvironmentVariableScope();
        environment.Set(EndpointVariableName, endpoint);
        environment.Set(DeploymentVariableName, deployment);
        environment.Set(ApiKeyVariableName, apiKey);

        if (includeCorsOrigin)
        {
            environment.Set(CorsAllowedOriginVariableName, AllowedOrigin);
        }

        return environment;
    }

    private sealed class ListLoggerProvider(List<string> messages) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new ListLogger(categoryName, messages);

        public void Dispose()
        {
        }

        private sealed class ListLogger(string category, List<string> messages) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                messages.Add($"[{category}] {formatter(state, exception)}");
            }
        }
    }
}
