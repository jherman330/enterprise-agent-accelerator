using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EnterpriseAgentAccelerator.Api.Tests;

public sealed class CorsPolicyTests
{
    private const string AllowedOrigin = "http://localhost:5173";
    private const string EndpointVariableName = "AZURE_OPENAI_ENDPOINT";
    private const string DeploymentVariableName = "AZURE_OPENAI_DEPLOYMENT";
    private const string ApiKeyVariableName = "AZURE_OPENAI_API_KEY";
    private const string CorsAllowedOriginVariableName = "CORS_ALLOWED_ORIGIN";

    [Fact]
    public async Task DevelopmentEnvironmentAllowsConfiguredOriginForGetRequests()
    {
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateFactory("Development");
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", AllowedOrigin);

        using var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        AssertHeader(response, "Access-Control-Allow-Origin", AllowedOrigin);
    }

    [Fact]
    public async Task DevelopmentEnvironmentAllowsPostWithContentTypeHeaderForConfiguredOrigin()
    {
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateFactory("Development");
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", AllowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        using var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        AssertHeader(response, "Access-Control-Allow-Origin", AllowedOrigin);
        AssertHeader(response, "Access-Control-Allow-Methods", "POST");
        AssertHeader(response, "Access-Control-Allow-Headers", "Content-Type");
    }

    [Fact]
    public async Task DevelopmentEnvironmentDoesNotAllowUnconfiguredOrigin()
    {
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateFactory("Development");
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://evil.example");

        using var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task NonDevelopmentEnvironmentDoesNotApplyLocalCorsPolicy()
    {
        using var environment = SetRequiredEnvironmentVariables();
        using var factory = CreateFactory("Production");
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", AllowedOrigin);

        using var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static WebApplicationFactory<Program> CreateFactory(string environmentName)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment(environmentName));
    }

    private static EnvironmentVariableScope SetRequiredEnvironmentVariables()
    {
        var environment = new EnvironmentVariableScope();
        environment.Set(EndpointVariableName, "https://example.openai.azure.com/");
        environment.Set(DeploymentVariableName, "test-deployment");
        environment.Set(ApiKeyVariableName, "test-api-key");
        environment.Set(CorsAllowedOriginVariableName, AllowedOrigin);

        return environment;
    }

    private static void AssertHeader(HttpResponseMessage response, string name, string expectedValue)
    {
        Assert.True(response.Headers.TryGetValues(name, out var values));
        Assert.Contains(expectedValue, string.Join(",", values));
    }
}
