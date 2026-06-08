using System.Collections.Concurrent;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Session;

public sealed class InMemorySessionStore : ISessionStore
{
    public const int DefaultHistoryLimit = 20;

    private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new();

    public ChatHistory GetOrCreateSession(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        var entry = _sessions.GetOrAdd(sessionId, id => new SessionEntry(id));
        lock (entry.Sync)
        {
            return CloneHistory(entry.ChatSession.History);
        }
    }

    public void UpdateSession(string sessionId, ChatHistory history)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentNullException.ThrowIfNull(history);

        var entry = _sessions.GetOrAdd(sessionId, id => new SessionEntry(id));
        var storedHistory = CloneHistory(history);
        EnforceHistoryLimit(storedHistory);

        lock (entry.Sync)
        {
            entry.ChatSession = new ChatSession(sessionId, storedHistory);
        }
    }

    private static ChatHistory CloneHistory(ChatHistory source)
    {
        var clone = new ChatHistory();

        foreach (var message in source)
        {
            clone.Add(message);
        }

        return clone;
    }

    private static void EnforceHistoryLimit(ChatHistory history)
    {
        while (history.Count > DefaultHistoryLimit)
        {
            history.RemoveAt(0);
        }
    }

    private sealed class SessionEntry
    {
        public SessionEntry(string sessionId)
        {
            ChatSession = new ChatSession(sessionId, new ChatHistory());
        }

        public ChatSession ChatSession { get; set; }

        public object Sync { get; } = new();
    }
}
