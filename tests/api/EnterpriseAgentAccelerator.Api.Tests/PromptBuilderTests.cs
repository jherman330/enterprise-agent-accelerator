using EnterpriseAgentAccelerator.Api.Prompt;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAgentAccelerator.Api.Tests;

public sealed class PromptBuilderTests
{
    [Fact]
    public void BuildPromptForNewSessionIncludesDefaultSystemInstructionAndUserMessage()
    {
        var builder = new PromptBuilder();
        var history = new ChatHistory();

        var prompt = builder.BuildPrompt(string.Empty, history, "current question");

        Assert.Equal(2, prompt.Count);
        Assert.Equal(AuthorRole.System, prompt[0].Role);
        Assert.Equal(PromptBuilder.DefaultSystemInstruction, prompt[0].Content);
        Assert.Equal(AuthorRole.User, prompt[1].Role);
        Assert.Equal("current question", prompt[1].Content);
    }

    [Fact]
    public void BuildPromptIncludesExistingHistoryInChronologicalOrderBeforeCurrentUserMessage()
    {
        var builder = new PromptBuilder();
        var history = new ChatHistory();
        history.AddUserMessage("first");
        history.AddAssistantMessage("second");

        var prompt = builder.BuildPrompt("custom instruction", history, "third");

        Assert.Equal(4, prompt.Count);
        Assert.Equal(AuthorRole.System, prompt[0].Role);
        Assert.Equal("custom instruction", prompt[0].Content);
        Assert.Equal("first", prompt[1].Content);
        Assert.Equal("second", prompt[2].Content);
        Assert.Equal("third", prompt[3].Content);
    }

    [Fact]
    public void BuildPromptDoesNotDuplicateSystemInstructionWhenHistoryAlreadyContainsOne()
    {
        var builder = new PromptBuilder();
        var history = new ChatHistory();
        history.AddSystemMessage("existing system");
        history.AddUserMessage("prior");

        var prompt = builder.BuildPrompt("ignored custom", history, "current");

        Assert.Equal(3, prompt.Count);
        Assert.Equal(AuthorRole.System, prompt[0].Role);
        Assert.Equal("existing system", prompt[0].Content);
        Assert.Equal("prior", prompt[1].Content);
        Assert.Equal("current", prompt[2].Content);
    }

    [Fact]
    public void BuildPromptDoesNotMutateInputHistory()
    {
        var builder = new PromptBuilder();
        var history = new ChatHistory();
        history.AddUserMessage("prior user");
        history.AddAssistantMessage("prior assistant");

        _ = builder.BuildPrompt(null, history, "current question");

        Assert.Equal(2, history.Count);
        Assert.Equal("prior user", history[0].Content);
        Assert.Equal("prior assistant", history[1].Content);
    }

    [Fact]
    public void BuildPromptThrowsWhenHistoryIsNull()
    {
        var builder = new PromptBuilder();

        Assert.Throws<ArgumentNullException>(
            () => builder.BuildPrompt(null, null!, "current question"));
    }

    [Fact]
    public void BuildPromptThrowsWhenUserMessageIsNull()
    {
        var builder = new PromptBuilder();
        var history = new ChatHistory();

        Assert.Throws<ArgumentNullException>(
            () => builder.BuildPrompt(null, history, null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void BuildPromptThrowsWhenUserMessageIsEmptyOrWhitespace(string userMessage)
    {
        var builder = new PromptBuilder();
        var history = new ChatHistory();

        Assert.Throws<ArgumentException>(
            () => builder.BuildPrompt(null, history, userMessage));
    }
}
