using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Route("api/servicos")]
public sealed class ServicosController(
    ICatalogService catalogService,
    IValidator<CreateServicoRequest> createValidator,
    IValidator<UpdateServicoRequest> updateValidator) : ControllerBase
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
    [Authorize(Roles = "Dono")]
    [EndpointSummary("Cadastra um serviço")]
    [EndpointDescription("Cria um serviço ativo com preço e duração em minutos.")]
    [ProducesResponseType<ServicoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServicoResponse>> Create(
        CreateServicoRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(request, cancellationToken);
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

    /// <summary>Altera nome, preço e duração de um serviço da barbearia.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Dono")]
    [EndpointSummary("Altera um serviço")]
    [ProducesResponseType<ServicoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServicoResponse>> Update(
        int id,
        UpdateServicoRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
            return BadRequest(new ValidationProblemDetails(errors));
        }

        var response = await catalogService.UpdateServicoAsync(id, request, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>Inativa um serviço sem excluir seu histórico de agendamentos.</summary>
    [HttpPatch("{id:int}/inativar")]
    [Authorize(Roles = "Dono")]
    [EndpointSummary("Inativa um serviço")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var deactivated = await catalogService.DeactivateServicoAsync(id, cancellationToken);
        return deactivated ? NoContent() : NotFound();
    }
}
