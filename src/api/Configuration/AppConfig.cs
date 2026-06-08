using System.ComponentModel.DataAnnotations;

namespace EnterpriseAgentAccelerator.Api.Configuration;

public sealed class AppConfig
{
    [Required]
    [MinLength(1)]
    public string CorsAllowedOrigin { get; init; } = string.Empty;
}
