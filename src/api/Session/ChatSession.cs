using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Session;

public sealed class ChatSession
{
    public ChatSession(string sessionId, ChatHistory history)
    {
        SessionId = sessionId;
        History = history;
    }

    public string SessionId { get; }

    public ChatHistory History { get; }
}
