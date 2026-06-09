using System.Diagnostics;
using EnterpriseAgentAccelerator.Api.Configuration;
using EnterpriseAgentAccelerator.Api.Models;
using EnterpriseAgentAccelerator.Api.Prompt;
using EnterpriseAgentAccelerator.Api.Session;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Orchestration;

public sealed class SemanticKernelOrchestrator : ISemanticKernelOrchestrator
{
    private readonly ISessionStore _sessionStore;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly AzureOpenAiConfig _config;
    private readonly ILogger<SemanticKernelOrchestrator> _logger;

    public SemanticKernelOrchestrator(
        ISessionStore sessionStore,
        IPromptBuilder promptBuilder,
        IChatCompletionService chatCompletionService,
        IOptions<AzureOpenAiConfig> config,
        ILogger<SemanticKernelOrchestrator> logger)
    {
        ArgumentNullException.ThrowIfNull(sessionStore);
        ArgumentNullException.ThrowIfNull(promptBuilder);
        ArgumentNullException.ThrowIfNull(chatCompletionService);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(logger);

        _sessionStore = sessionStore;
        _promptBuilder = promptBuilder;
        _chatCompletionService = chatCompletionService;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<ChatResponse> ProcessChatAsync(string sessionId, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var history = _sessionStore.GetOrCreateSession(sessionId);
        var prompt = _promptBuilder.BuildPrompt(null, history, message);

        _logger.LogInformation("Model call starting for session {SessionId}", sessionId);
        var stopwatch = Stopwatch.StartNew();

        ChatMessageContent modelResponse;
        try
        {
            modelResponse = await _chatCompletionService.GetChatMessageContentAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError("Model call failed for session {SessionId}: {ErrorMessage}", sessionId, ex.Message);
            throw;
        }

        stopwatch.Stop();
        _logger.LogInformation(
            "Model call completed for session {SessionId} in {ElapsedMilliseconds}ms",
            sessionId,
            stopwatch.ElapsedMilliseconds);

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
