using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Route("api/produtos")]
public sealed class ProdutosController(
    ICatalogService catalogService,
    IValidator<CreateProdutoRequest> createValidator,
    IValidator<UpdateProdutoRequest> updateValidator) : ControllerBase
{
    /// <summary>Lista produtos ativos da barbearia selecionada.</summary>
    [HttpGet]
    [EndpointSummary("Lista produtos ativos")]
    [ProducesResponseType<IReadOnlyList<ProdutoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProdutoResponse>>> Get(CancellationToken cancellationToken) =>
        Ok(await catalogService.GetProdutosAsync(cancellationToken));

    /// <summary>Consulta um produto ativo pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [EndpointSummary("Consulta um produto")]
    [ProducesResponseType<ProdutoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await catalogService.GetProdutoByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>Cadastra um produto ativo.</summary>
    [HttpPost]
    [Authorize(Roles = "Dono")]
    [EndpointSummary("Cadastra um produto")]
    [ProducesResponseType<ProdutoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProdutoResponse>> Create(
        CreateProdutoRequest request,
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

        var response = await catalogService.CreateProdutoAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Altera nome, descrição e preço de um produto.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Dono")]
    [EndpointSummary("Altera um produto")]
    [ProducesResponseType<ProdutoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponse>> Update(
        int id,
        UpdateProdutoRequest request,
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

        var response = await catalogService.UpdateProdutoAsync(id, request, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>Inativa um produto sem removê-lo do cadastro.</summary>
    [HttpPatch("{id:int}/inativar")]
    [Authorize(Roles = "Dono")]
    [EndpointSummary("Inativa um produto")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var deactivated = await catalogService.DeactivateProdutoAsync(id, cancellationToken);
        return deactivated ? NoContent() : NotFound();
    }
}
