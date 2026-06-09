using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Tests;

internal sealed class FakeChatCompletionService : IChatCompletionService
{
    private readonly string? _responseText;
    private readonly Exception? _exception;

    private FakeChatCompletionService(string? responseText, Exception? exception)
    {
        _responseText = responseText;
        _exception = exception;
    }

    public ChatHistory? LastPrompt { get; private set; }

    public int CallCount { get; private set; }

    public IReadOnlyDictionary<string, object?> Attributes { get; } = new Dictionary<string, object?>();

    public static FakeChatCompletionService Returns(string responseText) => new(responseText, null);

    public static FakeChatCompletionService Throws(Exception exception) => new(null, exception);

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastPrompt = chatHistory;

        if (_exception is not null)
        {
            throw _exception;
        }

        IReadOnlyList<ChatMessageContent> result =
            [new ChatMessageContent(AuthorRole.Assistant, _responseText)];

        return Task.FromResult(result);
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Streaming is not used by the orchestrator.");
    }
}
