using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Session;

public interface ISessionStore
{
    ChatHistory GetOrCreateSession(string sessionId);

    void UpdateSession(string sessionId, ChatHistory history);
}
