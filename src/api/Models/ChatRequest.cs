using System.ComponentModel.DataAnnotations;
using EnterpriseAgentAccelerator.Api.Validation;

namespace EnterpriseAgentAccelerator.Api.Models;

public record ChatRequest(
    [Required, MinLength(1), NotWhitespace] string SessionId,
    [Required, MinLength(1), NotWhitespace] string Message);
