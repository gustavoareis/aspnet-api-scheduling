using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Route("api/provisionamento")]
public sealed class ProvisionamentoController(IAccountService accountService) : ControllerBase
{
    /// <summary>Cria a primeira conta proprietária; requer uma chave de provisionamento do operador.</summary>
    [HttpPost("dono")]
    [EndpointSummary("Provisiona a primeira conta proprietária")]
    [ProducesResponseType<BarbeariaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<BarbeariaResponse>> BootstrapOwner(
        BootstrapOwnerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await accountService.BootstrapOwnerAsync(
            request,
            Request.Headers["X-Provisioning-Key"].FirstOrDefault(),
            cancellationToken);
        return Created($"/api/barbearias/{result.Id}", result);
    }
}
