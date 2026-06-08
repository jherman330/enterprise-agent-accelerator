using EnterpriseAgentAccelerator.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;

namespace EnterpriseAgentAccelerator.Api.Tests;

public sealed class ConfigurationTests
{
    private const string EndpointVariableName = "AZURE_OPENAI_ENDPOINT";
    private const string DeploymentVariableName = "AZURE_OPENAI_DEPLOYMENT";
    private const string ApiKeyVariableName = "AZURE_OPENAI_API_KEY";
    private const string CorsAllowedOriginVariableName = "CORS_ALLOWED_ORIGIN";

    [Fact]
    public void AzureOpenAiConfigToStringRedactsApiKey()
    {
        var config = new AzureOpenAiConfig
        {
            Endpoint = "https://example.openai.azure.com/",
            Deployment = "test-deployment",
            ApiKey = "super-secret-key",
        };

        var configString = config.ToString();

        Assert.DoesNotContain("super-secret-key", configString);
        Assert.Contains("ApiKey = ***", configString);
    }

    [Theory]
    [InlineData(EndpointVariableName, "Endpoint")]
    [InlineData(DeploymentVariableName, "Deployment")]
    [InlineData(ApiKeyVariableName, "ApiKey")]
    public void HostFailsToStartWhenAzureOpenAiValueIsMissing(string missingVariableName, string expectedPropertyName)
    {
        using var environment = SetAzureOpenAiEnvironmentVariables();
        environment.Set(missingVariableName, null);

        var exception = AssertHostFailsToStart("Production");

        Assert.Contains(expectedPropertyName, exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(EndpointVariableName, "Endpoint")]
    [InlineData(DeploymentVariableName, "Deployment")]
    [InlineData(ApiKeyVariableName, "ApiKey")]
    public void HostFailsToStartWhenAzureOpenAiValueIsEmpty(string emptyVariableName, string expectedPropertyName)
    {
        using var environment = SetAzureOpenAiEnvironmentVariables();
        environment.Set(emptyVariableName, " ");

        var exception = AssertHostFailsToStart("Production");

        Assert.Contains(expectedPropertyName, exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void DevelopmentHostFailsToStartWhenCorsAllowedOriginIsMissingOrEmpty(string? value)
    {
        using var environment = SetAzureOpenAiEnvironmentVariables();
        environment.Set(CorsAllowedOriginVariableName, value);

        var exception = AssertHostFailsToStart("Development");

        Assert.Contains("CorsAllowedOrigin", exception.Message, StringComparison.Ordinal);
    }

    private static OptionsValidationException AssertHostFailsToStart(string environmentName)
    {
        return Assert.Throws<OptionsValidationException>(() =>
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => builder.UseEnvironment(environmentName));

            factory.CreateClient();
        });
    }

    private static EnvironmentVariableScope SetAzureOpenAiEnvironmentVariables()
    {
        var environment = new EnvironmentVariableScope();
        environment.Set(EndpointVariableName, "https://example.openai.azure.com/");
        environment.Set(DeploymentVariableName, "test-deployment");
        environment.Set(ApiKeyVariableName, "test-api-key");

        return environment;
    }
}
