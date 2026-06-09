using EnterpriseAgentAccelerator.Api.Models;
using EnterpriseAgentAccelerator.Api.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAgentAccelerator.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly ISemanticKernelOrchestrator _orchestrator;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ISemanticKernelOrchestrator orchestrator, ILogger<ChatController> logger)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        ArgumentNullException.ThrowIfNull(logger);
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
    {
        _logger.LogInformation("Chat request received for session {SessionId}", request.SessionId);

        try
        {
            var response = await _orchestrator.ProcessChatAsync(request.SessionId, request.Message);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError("Model call failed for session {SessionId}: {ErrorMessage}", request.SessionId, ex.Message);
            return Problem(
                detail: "An error occurred while processing the chat request.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
