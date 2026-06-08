using EnterpriseAgentAccelerator.Api.Session;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Tests;

public sealed class InMemorySessionStoreTests
{
    [Fact]
    public void GetOrCreateSessionReturnsEmptyHistoryForUnknownSessionId()
    {
        var store = new InMemorySessionStore();

        var history = store.GetOrCreateSession("session-new");

        Assert.Empty(history);
    }

    [Fact]
    public void GetOrCreateSessionReturnsExistingHistoryForKnownSessionId()
    {
        var store = new InMemorySessionStore();
        var history = store.GetOrCreateSession("session-existing");
        history.AddUserMessage("hello");
        store.UpdateSession("session-existing", history);

        var retrieved = store.GetOrCreateSession("session-existing");

        Assert.Single(retrieved);
        Assert.Equal("hello", retrieved[0].Content);
    }

    [Theory]
    [InlineData(18, 20, 0)]
    [InlineData(20, 20, 2)]
    public void UpdateSessionEnforcesDefaultHistoryLimitWhenExchangeAddsTwoMessages(
        int currentCount,
        int expectedCount,
        int expectedEvictedCount)
    {
        var store = new InMemorySessionStore();
        var history = CreateHistory(currentCount);
        history.AddUserMessage("new-user-message");
        history.AddAssistantMessage("new-assistant-message");

        store.UpdateSession("session-limit", history);

        var stored = store.GetOrCreateSession("session-limit");
        Assert.Equal(expectedCount, stored.Count);
        Assert.Equal($"message-{expectedEvictedCount}", stored[0].Content);
        Assert.Equal("new-assistant-message", stored[^1].Content);
    }

    [Fact]
    public void GetOrCreateSessionReturnsDetachedHistoryThatDoesNotMutateStoredSessionUntilUpdate()
    {
        var store = new InMemorySessionStore();
        var history = store.GetOrCreateSession("session-detached");
        history.AddUserMessage("unsaved-message");

        var retrievedBeforeUpdate = store.GetOrCreateSession("session-detached");

        Assert.Empty(retrievedBeforeUpdate);

        store.UpdateSession("session-detached", history);

        var retrievedAfterUpdate = store.GetOrCreateSession("session-detached");
        Assert.Single(retrievedAfterUpdate);
        Assert.Equal("unsaved-message", retrievedAfterUpdate[0].Content);
    }

    [Fact]
    public void ConcurrentUpdatesForSameSessionRemainWithinHistoryLimit()
    {
        var store = new InMemorySessionStore();
        const string sessionId = "session-concurrent";

        Parallel.For(0, 10, index =>
        {
            var history = store.GetOrCreateSession(sessionId);
            history.AddUserMessage($"user-{index}");
            history.AddAssistantMessage($"assistant-{index}");
            store.UpdateSession(sessionId, history);
        });

        var stored = store.GetOrCreateSession(sessionId);
        Assert.InRange(stored.Count, 1, InMemorySessionStore.DefaultHistoryLimit);
    }

    [Fact]
    public void DefaultHistoryLimitIsTwentyMessages()
    {
        Assert.Equal(20, InMemorySessionStore.DefaultHistoryLimit);

        var store = new InMemorySessionStore();
        var history = CreateHistory(21);

        store.UpdateSession("session-default-limit", history);

        var stored = store.GetOrCreateSession("session-default-limit");
        Assert.Equal(20, stored.Count);
        Assert.Equal("message-1", stored[0].Content);
        Assert.Equal("message-20", stored[^1].Content);
    }

    private static ChatHistory CreateHistory(int messageCount)
    {
        var history = new ChatHistory();

        for (var i = 0; i < messageCount; i++)
        {
            if (i % 2 == 0)
            {
                history.AddUserMessage($"message-{i}");
            }
            else
            {
                history.AddAssistantMessage($"message-{i}");
            }
        }

        return history;
    }
}
