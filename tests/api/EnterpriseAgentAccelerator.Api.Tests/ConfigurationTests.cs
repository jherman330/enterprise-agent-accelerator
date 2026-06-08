using EnterpriseAgentAccelerator.Api.Configuration;

namespace EnterpriseAgentAccelerator.Api.Tests;

public sealed class ConfigurationTests
{
    private const string EndpointVariableName = "AZURE_OPENAI_ENDPOINT";
    private const string DeploymentVariableName = "AZURE_OPENAI_DEPLOYMENT";
    private const string ApiKeyVariableName = "AZURE_OPENAI_API_KEY";
    private const string CorsAllowedOriginVariableName = "CORS_ALLOWED_ORIGIN";

    [Fact]
    public void AzureOpenAiConfigFromEnvironmentReadsRequiredValues()
    {
        using var environment = new EnvironmentVariableScope();
        environment.Set(EndpointVariableName, "https://example.openai.azure.com/");
        environment.Set(DeploymentVariableName, "test-deployment");
        environment.Set(ApiKeyVariableName, "test-api-key");

        var config = AzureOpenAiConfig.FromEnvironment();

        Assert.Equal("https://example.openai.azure.com/", config.Endpoint);
        Assert.Equal("test-deployment", config.Deployment);
        Assert.Equal("test-api-key", config.ApiKey);
    }

    [Theory]
    [InlineData(EndpointVariableName)]
    [InlineData(DeploymentVariableName)]
    [InlineData(ApiKeyVariableName)]
    public void AzureOpenAiConfigFromEnvironmentThrowsWhenRequiredValueIsMissing(string missingVariableName)
    {
        using var environment = new EnvironmentVariableScope();
        environment.Set(EndpointVariableName, "https://example.openai.azure.com/");
        environment.Set(DeploymentVariableName, "test-deployment");
        environment.Set(ApiKeyVariableName, "test-api-key");
        environment.Set(missingVariableName, null);

        var exception = Assert.Throws<InvalidOperationException>(AzureOpenAiConfig.FromEnvironment);

        Assert.Contains(missingVariableName, exception.Message);
    }

    [Theory]
    [InlineData(EndpointVariableName)]
    [InlineData(DeploymentVariableName)]
    [InlineData(ApiKeyVariableName)]
    public void AzureOpenAiConfigFromEnvironmentThrowsWhenRequiredValueIsEmpty(string emptyVariableName)
    {
        using var environment = new EnvironmentVariableScope();
        environment.Set(EndpointVariableName, "https://example.openai.azure.com/");
        environment.Set(DeploymentVariableName, "test-deployment");
        environment.Set(ApiKeyVariableName, "test-api-key");
        environment.Set(emptyVariableName, " ");

        var exception = Assert.Throws<InvalidOperationException>(AzureOpenAiConfig.FromEnvironment);

        Assert.Contains(emptyVariableName, exception.Message);
    }

    [Fact]
    public void AzureOpenAiConfigToStringRedactsApiKey()
    {
        var config = new AzureOpenAiConfig(
            "https://example.openai.azure.com/",
            "test-deployment",
            "super-secret-key");

        var configString = config.ToString();

        Assert.DoesNotContain("super-secret-key", configString);
        Assert.Contains("ApiKey = ***", configString);
    }

    [Fact]
    public void AppConfigFromEnvironmentReadsRequiredValues()
    {
        using var environment = new EnvironmentVariableScope();
        environment.Set(CorsAllowedOriginVariableName, "http://localhost:5173");

        var config = AppConfig.FromEnvironment();

        Assert.Equal("http://localhost:5173", config.CorsAllowedOrigin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AppConfigFromEnvironmentThrowsWhenCorsAllowedOriginIsMissingOrEmpty(string? value)
    {
        using var environment = new EnvironmentVariableScope();
        environment.Set(CorsAllowedOriginVariableName, value);

        var exception = Assert.Throws<InvalidOperationException>(AppConfig.FromEnvironment);

        Assert.Contains(CorsAllowedOriginVariableName, exception.Message);
    }
}
