using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Prompt;

public sealed class PromptBuilder : IPromptBuilder
{
    public const string DefaultSystemInstruction =
        "You are a developer-focused AI workbench. Respond clearly and concisely. Do not fabricate information you are not confident about. Indicate when you lack sufficient context to answer.";

    public ChatHistory BuildPrompt(string? systemInstruction, ChatHistory history, string userMessage)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        var prompt = new ChatHistory();
        var hasSystemMessage = history.Any(message => message.Role == AuthorRole.System);

        if (!hasSystemMessage)
        {
            var instruction = string.IsNullOrWhiteSpace(systemInstruction)
                ? DefaultSystemInstruction
                : systemInstruction;

            prompt.AddSystemMessage(instruction);
        }

        foreach (var message in history)
        {
            prompt.Add(message);
        }

        prompt.AddUserMessage(userMessage);

        return prompt;
    }
}
