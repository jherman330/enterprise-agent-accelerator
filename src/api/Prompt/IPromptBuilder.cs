using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Prompt;

public interface IPromptBuilder
{
    ChatHistory BuildPrompt(string? systemInstruction, ChatHistory history, string userMessage);
}
