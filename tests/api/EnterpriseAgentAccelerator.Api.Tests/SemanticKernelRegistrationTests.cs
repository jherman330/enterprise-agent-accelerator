using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Tests;

public sealed class SemanticKernelRegistrationTests
{
    private const string AllowedOrigin = "http://localhost:5173";
    private const string EndpointVariableName = "AZURE_OPENAI_ENDPOINT";
    private const string DeploymentVariableName = "AZURE_OPENAI_DEPLOYMENT";
    private const string ApiKeyVariableName = "AZURE_OPENAI_API_KEY";
    private const string CorsAllowedOriginVariableName = "CORS_ALLOWED_ORIGIN";

    [Fact]
    public void DevelopmentHostRegistersIChatCompletionService()
    {
        using var environment = SetRequiredEnvironmentVariables(includeCorsOrigin: true);
        using var factory = CreateFactory("Development");

        var chatCompletionService = factory.Services.GetRequiredService<IChatCompletionService>();

        Assert.IsAssignableFrom<IChatCompletionService>(chatCompletionService);
    }

    [Fact]
    public void ProductionHostRegistersIChatCompletionService()
    {
        using var environment = SetRequiredEnvironmentVariables(includeCorsOrigin: false);
        using var factory = CreateFactory("Production");

        var chatCompletionService = factory.Services.GetRequiredService<IChatCompletionService>();

        Assert.IsAssignableFrom<IChatCompletionService>(chatCompletionService);
    }

    private static WebApplicationFactory<Program> CreateFactory(string environmentName)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment(environmentName));
    }

    private static EnvironmentVariableScope SetRequiredEnvironmentVariables(bool includeCorsOrigin)
    {
        var environment = new EnvironmentVariableScope();
        environment.Set(EndpointVariableName, "https://example.openai.azure.com/");
        environment.Set(DeploymentVariableName, "test-deployment");
        environment.Set(ApiKeyVariableName, "test-api-key");

        if (includeCorsOrigin)
        {
            environment.Set(CorsAllowedOriginVariableName, AllowedOrigin);
        }

        return environment;
    }
}
