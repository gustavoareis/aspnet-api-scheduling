using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Route("api/barbeiros")]
public sealed class BarbeirosController(
    ICatalogService catalogService,
    IValidator<CreateBarbeiroRequest> validator) : ControllerBase
{
    /// <summary>Lista os barbeiros ativos.</summary>
    [HttpGet]
    [EndpointSummary("Lista barbeiros ativos")]
    [EndpointDescription("Retorna os barbeiros disponíveis para receber agendamentos.")]
    [ProducesResponseType<IReadOnlyList<BarbeiroResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BarbeiroResponse>>> Get(CancellationToken cancellationToken) =>
        Ok(await catalogService.GetBarbeirosAsync(cancellationToken));

    /// <summary>Consulta um barbeiro pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [EndpointSummary("Consulta um barbeiro")]
    [ProducesResponseType<BarbeiroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarbeiroResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await catalogService.GetBarbeiroByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>Cadastra um barbeiro ativo.</summary>
    [HttpPost]
    [Authorize(Roles = "Dono")]
    [EndpointSummary("Cadastra um barbeiro")]
    [EndpointDescription("Cria um barbeiro ativo que poderá receber agendamentos.")]
    [ProducesResponseType<BarbeiroResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BarbeiroResponse>> Create(
        CreateBarbeiroRequest request,
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

        var response = await catalogService.CreateBarbeiroAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }
}
