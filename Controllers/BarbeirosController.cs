using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Route("api/barbeiros")]
public sealed class BarbeirosController(ICatalogService catalogService) : ControllerBase
{
    /// <summary>Lista os barbeiros ativos.</summary>
    [HttpGet]
    [EndpointSummary("Lista barbeiros ativos")]
    [EndpointDescription("Retorna os barbeiros disponíveis para receber agendamentos.")]
    [ProducesResponseType<IReadOnlyList<BarbeiroResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BarbeiroResponse>>> Get(CancellationToken cancellationToken) =>
        Ok(await catalogService.GetBarbeirosAsync(cancellationToken));
}
