using EnterpriseAgentAccelerator.Api.Configuration;
using EnterpriseAgentAccelerator.Api.Orchestration;
using EnterpriseAgentAccelerator.Api.Prompt;
using EnterpriseAgentAccelerator.Api.Session;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Tests;

public sealed class SemanticKernelOrchestratorTests
{
    private const string Deployment = "test-deployment";

    private static SemanticKernelOrchestrator CreateOrchestrator(
        IChatCompletionService chatCompletionService,
        ISessionStore? sessionStore = null)
    {
        var config = Options.Create(new AzureOpenAiConfig
        {
            Endpoint = "https://example.openai.azure.com/",
            Deployment = Deployment,
            ApiKey = "test-api-key",
        });

        return new SemanticKernelOrchestrator(
            sessionStore ?? new InMemorySessionStore(),
            new PromptBuilder(),
            chatCompletionService,
            config);
    }

    [Fact]
    public async Task ProcessChatAsyncRoutesModelCallThroughChatCompletionService()
    {
        var chat = FakeChatCompletionService.Returns("model reply");
        var orchestrator = CreateOrchestrator(chat);

        await orchestrator.ProcessChatAsync("session-1", "hello");

        Assert.Equal(1, chat.CallCount);
        Assert.NotNull(chat.LastPrompt);
    }

    [Fact]
    public async Task ProcessChatAsyncReturnsFullyStructuredResponse()
    {
        var chat = FakeChatCompletionService.Returns("model reply");
        var orchestrator = CreateOrchestrator(chat);

        var response = await orchestrator.ProcessChatAsync("session-1", "hello");

        Assert.Equal("session-1", response.SessionId);
        Assert.Equal("model reply", response.Response);
        Assert.Equal(Deployment, response.Model);
        Assert.True(Guid.TryParse(response.MessageId, out _));
        Assert.True(DateTimeOffset.TryParse(response.CreatedAt, out _));
    }

    [Fact]
    public async Task ProcessChatAsyncAppendsUserMessageAndModelResponseToSession()
    {
        var chat = FakeChatCompletionService.Returns("model reply");
        var store = new InMemorySessionStore();
        var orchestrator = CreateOrchestrator(chat, store);

        await orchestrator.ProcessChatAsync("session-1", "hello");

        var history = store.GetOrCreateSession("session-1");
        Assert.Equal(2, history.Count);
        Assert.Equal(AuthorRole.User, history[0].Role);
        Assert.Equal("hello", history[0].Content);
        Assert.Equal(AuthorRole.Assistant, history[1].Role);
        Assert.Equal("model reply", history[1].Content);
    }

    [Fact]
    public async Task ProcessChatAsyncPassesPriorSessionHistoryToPromptOnSubsequentTurns()
    {
        var chat = FakeChatCompletionService.Returns("reply");
        var store = new InMemorySessionStore();
        var orchestrator = CreateOrchestrator(chat, store);

        await orchestrator.ProcessChatAsync("session-1", "first question");
        await orchestrator.ProcessChatAsync("session-1", "second question");

        var prompt = chat.LastPrompt!;
        Assert.Contains(prompt, message => message.Content == "first question");
        Assert.Contains(prompt, message => message.Content == "second question");
    }

    [Fact]
    public async Task ProcessChatAsyncPropagatesModelCallFailureWithoutUpdatingSession()
    {
        var chat = FakeChatCompletionService.Throws(new InvalidOperationException("model down"));
        var store = new InMemorySessionStore();
        var orchestrator = CreateOrchestrator(chat, store);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => orchestrator.ProcessChatAsync("session-1", "hello"));

        var history = store.GetOrCreateSession("session-1");
        Assert.Empty(history);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ProcessChatAsyncThrowsWhenMessageIsNullEmptyOrWhitespace(string? message)
    {
        var chat = FakeChatCompletionService.Returns("reply");
        var orchestrator = CreateOrchestrator(chat);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => orchestrator.ProcessChatAsync("session-1", message!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ProcessChatAsyncThrowsWhenSessionIdIsNullEmptyOrWhitespace(string? sessionId)
    {
        var chat = FakeChatCompletionService.Returns("reply");
        var orchestrator = CreateOrchestrator(chat);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => orchestrator.ProcessChatAsync(sessionId!, "hello"));
    }
}
