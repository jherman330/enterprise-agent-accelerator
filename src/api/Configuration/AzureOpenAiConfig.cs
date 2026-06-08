using System.ComponentModel.DataAnnotations;

namespace EnterpriseAgentAccelerator.Api.Configuration;

public sealed class AzureOpenAiConfig
{
    [Required]
    [MinLength(1)]
    public string Endpoint { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Deployment { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string ApiKey { get; init; } = string.Empty;

    public override string ToString()
    {
        return $"{nameof(AzureOpenAiConfig)} {{ Endpoint = {Endpoint}, Deployment = {Deployment}, ApiKey = *** }}";
    }
}
