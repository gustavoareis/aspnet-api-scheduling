using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Route("api/servicos")]
public sealed class ServicosController(
    ICatalogService catalogService,
    IValidator<CreateServicoRequest> validator) : ControllerBase
{
    /// <summary>Lista os serviços ativos com preço e duração.</summary>
    [HttpGet]
    [EndpointSummary("Lista serviços ativos")]
    [EndpointDescription("Retorna os serviços ativos com preço e duração em minutos.")]
    [ProducesResponseType<IReadOnlyList<ServicoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServicoResponse>>> Get(CancellationToken cancellationToken) =>
        Ok(await catalogService.GetServicosAsync(cancellationToken));

    /// <summary>Consulta um serviço pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [EndpointSummary("Consulta um serviço")]
    [ProducesResponseType<ServicoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServicoResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await catalogService.GetServicoByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>Cadastra um serviço ativo que poderá ser agendado.</summary>
    [HttpPost]
    [EndpointSummary("Cadastra um serviço")]
    [EndpointDescription("Cria um serviço ativo com preço e duração em minutos.")]
    [ProducesResponseType<ServicoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServicoResponse>> Create(
        CreateServicoRequest request,
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

        var response = await catalogService.CreateServicoAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }
}
