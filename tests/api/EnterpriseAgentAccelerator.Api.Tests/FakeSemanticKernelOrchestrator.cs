using EnterpriseAgentAccelerator.Api.Models;
using EnterpriseAgentAccelerator.Api.Orchestration;

namespace EnterpriseAgentAccelerator.Api.Tests;

internal sealed class FakeSemanticKernelOrchestrator : ISemanticKernelOrchestrator
{
    private readonly ChatResponse? _response;
    private readonly Exception? _exception;

    private FakeSemanticKernelOrchestrator(ChatResponse? response, Exception? exception)
    {
        _response = response;
        _exception = exception;
    }

    public int CallCount { get; private set; }

    public static FakeSemanticKernelOrchestrator Returns(ChatResponse response) => new(response, null);

    public static FakeSemanticKernelOrchestrator Throws(Exception exception) => new(null, exception);

    public Task<ChatResponse> ProcessChatAsync(string sessionId, string message)
    {
        CallCount++;

        if (_exception is not null)
        {
            throw _exception;
        }

        return Task.FromResult(_response!);
    }
}
