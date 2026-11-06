using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController(
    ICatalogService catalogService,
    IValidator<CreateClienteRequest> validator) : ControllerBase
{
    /// <summary>Lista os clientes cadastrados.</summary>
    [HttpGet]
    [Authorize(Roles = "Dono")]
    [EndpointSummary("Lista clientes")]
    [EndpointDescription("Retorna os clientes cadastrados para uso em agendamentos.")]
    [ProducesResponseType<IReadOnlyList<ClienteResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClienteResponse>>> Get(CancellationToken cancellationToken) =>
        Ok(await catalogService.GetClientesAsync(cancellationToken));

    /// <summary>Consulta um cliente pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Dono")]
    [EndpointSummary("Consulta um cliente")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await catalogService.GetClienteByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>Cadastra um cliente que poderá ser associado a agendamentos.</summary>
    [HttpPost]
    [EndpointSummary("Cadastra um cliente")]
    [EndpointDescription("Cria um cliente e retorna a URL para consultar o cadastro.")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteResponse>> Create(
        CreateClienteRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
            return BadRequest(new ValidationProblemDetails(errors));
        }

        var response = await catalogService.CreateClienteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }
}
