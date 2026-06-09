using System.Net.Http.Json;
using EnterpriseAgentAccelerator.Api.Configuration;
using EnterpriseAgentAccelerator.Api.Models;
using EnterpriseAgentAccelerator.Api.Orchestration;
using EnterpriseAgentAccelerator.Api.Prompt;
using EnterpriseAgentAccelerator.Api.Session;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Tests;

public sealed class ChatPipelineLoggingTests
{
    private const string AllowedOrigin = "http://localhost:5173";

    [Fact]
    public async Task OrchestratorLogsModelCallStartAndCompletionWithDuration()
    {
        var messages = new List<string>();
        var orchestrator = CreateOrchestrator(FakeChatCompletionService.Returns("reply"), messages);

        await orchestrator.ProcessChatAsync("session-1", "hello");

        Assert.Contains(messages, m =>
            m.Contains("Model call starting", StringComparison.Ordinal) &&
            m.Contains("session-1", StringComparison.Ordinal));
        Assert.Contains(messages, m =>
            m.Contains("Model call completed", StringComparison.Ordinal) &&
            m.Contains("ms", StringComparison.Ordinal));
    }

    [Fact]
    public async Task OrchestratorLogsErrorAndRethrowsOnModelFailure()
    {
        var messages = new List<string>();
        var orchestrator = CreateOrchestrator(
            FakeChatCompletionService.Throws(new InvalidOperationException("model down")), messages);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => orchestrator.ProcessChatAsync("session-1", "hello"));

        Assert.Contains(messages, m =>
            m.Contains("Model call failed", StringComparison.Ordinal) &&
            m.Contains("session-1", StringComparison.Ordinal) &&
            m.Contains("model down", StringComparison.Ordinal));
    }

    [Fact]
    public async Task OrchestratorDoesNotLogConfiguredSecrets()
    {
        const string secretDeployment = "wo15-secret-deployment";
        const string secretApiKey = "wo15-secret-api-key";
        const string secretEndpoint = "https://wo15-secret-endpoint.openai.azure.com/";
        var messages = new List<string>();
        var orchestrator = CreateOrchestrator(
            FakeChatCompletionService.Returns("reply"),
            messages,
            endpoint: secretEndpoint,
            deployment: secretDeployment,
            apiKey: secretApiKey);

        await orchestrator.ProcessChatAsync("session-1", "hello");

        var allLogs = string.Join(Environment.NewLine, messages);
        Assert.NotEmpty(messages);
        Assert.DoesNotContain(secretDeployment, allLogs);
        Assert.DoesNotContain(secretApiKey, allLogs);
        Assert.DoesNotContain(secretEndpoint, allLogs);
    }

    [Fact]
    public async Task ControllerLogsRequestReceivedForValidRequest()
    {
        var messages = new List<string>();
        var orchestrator = FakeSemanticKernelOrchestrator.Returns(SampleResponse());
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateWebFactory(orchestrator, messages);
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/chat", new { sessionId = "session-1", message = "hello" });

        Assert.Contains(messages, m =>
            m.Contains("Chat request received", StringComparison.Ordinal) &&
            m.Contains("session-1", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ControllerLogsErrorOnOrchestratorFailure()
    {
        var messages = new List<string>();
        var orchestrator = FakeSemanticKernelOrchestrator.Throws(new InvalidOperationException("model down"));
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateWebFactory(orchestrator, messages);
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/chat", new { sessionId = "session-1", message = "hello" });

        Assert.Contains(messages, m =>
            m.Contains("Model call failed", StringComparison.Ordinal) &&
            m.Contains("session-1", StringComparison.Ordinal));
    }

    private static ChatResponse SampleResponse() =>
        new("session-1", Guid.NewGuid().ToString(), "reply", "test-deployment", DateTime.UtcNow.ToString("o"));

    private static SemanticKernelOrchestrator CreateOrchestrator(
        IChatCompletionService chatCompletionService,
        List<string> messages,
        string endpoint = "https://example.openai.azure.com/",
        string deployment = "test-deployment",
        string apiKey = "test-api-key")
    {
        var config = Options.Create(new AzureOpenAiConfig
        {
            Endpoint = endpoint,
            Deployment = deployment,
            ApiKey = apiKey,
        });

        return new SemanticKernelOrchestrator(
            new InMemorySessionStore(),
            new PromptBuilder(),
            chatCompletionService,
            config,
            new CapturingLogger<SemanticKernelOrchestrator>(messages));
    }

    private static WebApplicationFactory<Program> CreateWebFactory(
        ISemanticKernelOrchestrator orchestrator,
        List<string> messages)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureLogging(logging => logging.AddProvider(new CapturingLoggerProvider(messages)));
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<ISemanticKernelOrchestrator>();
                    services.AddScoped<ISemanticKernelOrchestrator>(_ => orchestrator);
                });
            });
    }

    private static EnvironmentVariableScope SetRequiredEnvironmentVariables()
    {
        var environment = new EnvironmentVariableScope();
        environment.Set("AZURE_OPENAI_ENDPOINT", "https://example.openai.azure.com/");
        environment.Set("AZURE_OPENAI_DEPLOYMENT", "test-deployment");
        environment.Set("AZURE_OPENAI_API_KEY", "test-api-key");
        environment.Set("CORS_ALLOWED_ORIGIN", AllowedOrigin);
        return environment;
    }
}
