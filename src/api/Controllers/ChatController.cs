using EnterpriseAgentAccelerator.Api.Models;
using EnterpriseAgentAccelerator.Api.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAgentAccelerator.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly ISemanticKernelOrchestrator _orchestrator;

    public ChatController(ISemanticKernelOrchestrator orchestrator)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        _orchestrator = orchestrator;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
    {
        try
        {
            var response = await _orchestrator.ProcessChatAsync(request.SessionId, request.Message);
            return Ok(response);
        }
        catch (Exception)
        {
            return Problem(
                detail: "An error occurred while processing the chat request.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
