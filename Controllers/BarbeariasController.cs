using System.Security.Claims;
using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Authorize(Roles = "Dono")]
[Route("api/barbearias")]
public sealed class BarbeariasController(IAccountService accountService) : ControllerBase
{
    /// <summary>Lista as barbearias associadas ao usuário autenticado.</summary>
    [HttpGet]
    [EndpointSummary("Lista as barbearias do usuário")]
    [ProducesResponseType<IReadOnlyList<BarbeariaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BarbeariaResponse>>> Get(CancellationToken cancellationToken)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await accountService.GetBarbeariasAsync(usuarioId, cancellationToken));
    }

    /// <summary>Consulta uma barbearia associada ao usuário autenticado.</summary>
    [HttpGet("{id:int}")]
    [EndpointSummary("Consulta uma barbearia")]
    [ProducesResponseType<BarbeariaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarbeariaResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await accountService.GetBarbeariaAsync(usuarioId, id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>Cria outra barbearia associada ao mesmo usuário proprietário.</summary>
    [HttpPost]
    [EndpointSummary("Cadastra outra barbearia")]
    [ProducesResponseType<BarbeariaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BarbeariaResponse>> Create(
        CreateBarbeariaRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await accountService.CreateBarbeariaAsync(usuarioId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }
}
