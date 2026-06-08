namespace EnterpriseAgentAccelerator.Api.Configuration;

public sealed record AppConfig(string CorsAllowedOrigin)
{
    private const string CorsAllowedOriginVariableName = "CORS_ALLOWED_ORIGIN";

    public static AppConfig FromEnvironment()
    {
        return new AppConfig(GetRequiredEnvironmentVariable(CorsAllowedOriginVariableName));
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
