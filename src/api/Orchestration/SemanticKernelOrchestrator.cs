using EnterpriseAgentAccelerator.Api.Configuration;
using EnterpriseAgentAccelerator.Api.Models;
using EnterpriseAgentAccelerator.Api.Prompt;
using EnterpriseAgentAccelerator.Api.Session;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Orchestration;

public sealed class SemanticKernelOrchestrator : ISemanticKernelOrchestrator
{
    private readonly ISessionStore _sessionStore;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly AzureOpenAiConfig _config;

    public SemanticKernelOrchestrator(
        ISessionStore sessionStore,
        IPromptBuilder promptBuilder,
        IChatCompletionService chatCompletionService,
        IOptions<AzureOpenAiConfig> config)
    {
        ArgumentNullException.ThrowIfNull(sessionStore);
        ArgumentNullException.ThrowIfNull(promptBuilder);
        ArgumentNullException.ThrowIfNull(chatCompletionService);
        ArgumentNullException.ThrowIfNull(config);

        _sessionStore = sessionStore;
        _promptBuilder = promptBuilder;
        _chatCompletionService = chatCompletionService;
        _config = config.Value;
    }

    public async Task<ChatResponse> ProcessChatAsync(string sessionId, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var history = _sessionStore.GetOrCreateSession(sessionId);
        var prompt = _promptBuilder.BuildPrompt(null, history, message);

        var modelResponse = await _chatCompletionService.GetChatMessageContentAsync(prompt);
        var responseText = modelResponse.Content ?? string.Empty;

        history.AddUserMessage(message);
        history.AddAssistantMessage(responseText);
        _sessionStore.UpdateSession(sessionId, history);

        return new ChatResponse(
            SessionId: sessionId,
            MessageId: Guid.NewGuid().ToString(),
            Response: responseText,
            Model: _config.Deployment,
            CreatedAt: DateTime.UtcNow.ToString("o"));
    }
}
