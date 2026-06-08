namespace EnterpriseAgentAccelerator.Api.Configuration;

public sealed record AzureOpenAiConfig(
    string Endpoint,
    string Deployment,
    string ApiKey)
{
    private const string EndpointVariableName = "AZURE_OPENAI_ENDPOINT";
    private const string DeploymentVariableName = "AZURE_OPENAI_DEPLOYMENT";
    private const string ApiKeyVariableName = "AZURE_OPENAI_API_KEY";

    public static AzureOpenAiConfig FromEnvironment()
    {
        return new AzureOpenAiConfig(
            GetRequiredEnvironmentVariable(EndpointVariableName),
            GetRequiredEnvironmentVariable(DeploymentVariableName),
            GetRequiredEnvironmentVariable(ApiKeyVariableName));
    }

    public override string ToString()
    {
        return $"{nameof(AzureOpenAiConfig)} {{ Endpoint = {Endpoint}, Deployment = {Deployment}, ApiKey = *** }}";
    }

    private static string GetRequiredEnvironmentVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required environment variable '{variableName}' is missing or empty.");
        }

        return value;
    }
}
