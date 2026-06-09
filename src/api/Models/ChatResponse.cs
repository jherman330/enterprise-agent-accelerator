namespace EnterpriseAgentAccelerator.Api.Models;

public record ChatResponse(
    string SessionId,
    string MessageId,
    string Response,
    string Model,
    string CreatedAt);
