using System.Net;
using System.Net.Http.Json;
using EnterpriseAgentAccelerator.Api.Models;
using EnterpriseAgentAccelerator.Api.Orchestration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnterpriseAgentAccelerator.Api.Tests;

public sealed class ChatControllerTests
{
    private const string EndpointVariableName = "AZURE_OPENAI_ENDPOINT";
    private const string DeploymentVariableName = "AZURE_OPENAI_DEPLOYMENT";
    private const string ApiKeyVariableName = "AZURE_OPENAI_API_KEY";

    [Fact]
    public async Task ValidRequestReturnsStructuredResponseAndInvokesPipeline()
    {
        var expected = new ChatResponse("session-1", Guid.NewGuid().ToString(), "model reply", "test-deployment", DateTime.UtcNow.ToString("o"));
        var orchestrator = FakeSemanticKernelOrchestrator.Returns(expected);
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateFactory(orchestrator);
        using var client = factory.CreateClient();

        var httpResponse = await client.PostAsJsonAsync("/api/chat", new { sessionId = "session-1", message = "hello" });

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        var body = await httpResponse.Content.ReadFromJsonAsync<ChatResponse>();
        Assert.NotNull(body);
        Assert.Equal(expected.SessionId, body!.SessionId);
        Assert.Equal(expected.MessageId, body.MessageId);
        Assert.Equal(expected.Response, body.Response);
        Assert.Equal(expected.Model, body.Model);
        Assert.Equal(1, orchestrator.CallCount);
    }

    [Fact]
    public async Task AbsentSessionIdReturnsValidationErrorWithoutInvokingPipeline()
    {
        var orchestrator = FakeSemanticKernelOrchestrator.Returns(SampleResponse());
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateFactory(orchestrator);
        using var client = factory.CreateClient();

        var httpResponse = await client.PostAsJsonAsync("/api/chat", new { message = "hello" });

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.Equal(0, orchestrator.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyOrWhitespaceSessionIdReturnsValidationErrorWithoutInvokingPipeline(string sessionId)
    {
        var orchestrator = FakeSemanticKernelOrchestrator.Returns(SampleResponse());
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateFactory(orchestrator);
        using var client = factory.CreateClient();

        var httpResponse = await client.PostAsJsonAsync("/api/chat", new { sessionId, message = "hello" });

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.Equal(0, orchestrator.CallCount);
    }

    [Fact]
    public async Task EmptyMessageReturnsValidationErrorWithoutInvokingPipeline()
    {
        var orchestrator = FakeSemanticKernelOrchestrator.Returns(SampleResponse());
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateFactory(orchestrator);
        using var client = factory.CreateClient();

        var httpResponse = await client.PostAsJsonAsync("/api/chat", new { sessionId = "session-1", message = "" });

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.Equal(0, orchestrator.CallCount);
    }

    [Fact]
    public async Task WhitespaceMessageReturnsValidationErrorWithoutInvokingPipeline()
    {
        var orchestrator = FakeSemanticKernelOrchestrator.Returns(SampleResponse());
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateFactory(orchestrator);
        using var client = factory.CreateClient();

        var httpResponse = await client.PostAsJsonAsync("/api/chat", new { sessionId = "session-1", message = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.Equal(0, orchestrator.CallCount);
    }

    [Fact]
    public async Task ModelFailureReturnsStructuredErrorWithoutLeakingSensitiveInformation()
    {
        const string sensitive = "SECRET-API-KEY-1234567890";
        var orchestrator = FakeSemanticKernelOrchestrator.Throws(
            new InvalidOperationException($"Azure call failed using {sensitive}"));
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateFactory(orchestrator);
        using var client = factory.CreateClient();

        var httpResponse = await client.PostAsJsonAsync("/api/chat", new { sessionId = "session-1", message = "hello" });

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponse.StatusCode);
        var rawBody = await httpResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain(sensitive, rawBody);
        Assert.DoesNotContain("InvalidOperationException", rawBody);
        Assert.DoesNotContain(ApiKeyValue, rawBody);
    }

    private const string ApiKeyValue = "test-api-key";

    private static ChatResponse SampleResponse() =>
        new("session-1", Guid.NewGuid().ToString(), "reply", "test-deployment", DateTime.UtcNow.ToString("o"));

    private static WebApplicationFactory<Program> CreateFactory(ISemanticKernelOrchestrator orchestrator)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
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
        environment.Set(EndpointVariableName, "https://example.openai.azure.com/");
        environment.Set(DeploymentVariableName, "test-deployment");
        environment.Set(ApiKeyVariableName, ApiKeyValue);
        return environment;
    }
}
