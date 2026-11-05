using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Route("api/servicos")]
public sealed class ServicosController(ICatalogService catalogService) : ControllerBase
{
    /// <summary>Lista os serviços ativos com preço e duração.</summary>
    [HttpGet]
    [EndpointSummary("Lista serviços ativos")]
    [EndpointDescription("Retorna os serviços ativos com preço e duração em minutos.")]
    [ProducesResponseType<IReadOnlyList<ServicoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServicoResponse>>> Get(CancellationToken cancellationToken) =>
        Ok(await catalogService.GetServicosAsync(cancellationToken));
}
