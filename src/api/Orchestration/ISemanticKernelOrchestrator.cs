using EnterpriseAgentAccelerator.Api.Models;

namespace EnterpriseAgentAccelerator.Api.Orchestration;

public interface ISemanticKernelOrchestrator
{
    Task<ChatResponse> ProcessChatAsync(string sessionId, string message);
}
