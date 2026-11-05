using System.ComponentModel.DataAnnotations;
using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Route("api/agendamentos")]
public sealed class AgendamentosController(
    IAgendamentoService agendamentoService,
    IValidator<CreateAgendamentoRequest> validator) : ControllerBase
{
    /// <summary>Lista os horários disponíveis separados por turno.</summary>
    [HttpGet("horarios-disponiveis")]
    [EndpointSummary("Consulta horários disponíveis")]
    [EndpointDescription("Verifica os intervalos completos do serviço e agrupa os horários livres por turno.")]
    [ProducesResponseType<HorariosDisponiveisResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HorariosDisponiveisResponse>> GetHorariosDisponiveis(
        [FromQuery, BindRequired] DateOnly data,
        [FromQuery, BindRequired, Range(1, int.MaxValue)] int servicoId,
        [FromQuery, Range(1, int.MaxValue)] int? barbeiroId,
        CancellationToken cancellationToken)
    {
        var response = await agendamentoService.GetHorariosDisponiveisAsync(
            data,
            servicoId,
            barbeiroId,
            cancellationToken);
        return Ok(response);
    }

    /// <summary>Cria um agendamento e calcula o horário de término pela duração do serviço.</summary>
    [HttpPost]
    [EndpointSummary("Cria um agendamento")]
    [EndpointDescription("Seleciona um barbeiro disponível e calcula o término com base na duração do serviço.")]
    [ProducesResponseType<AgendamentoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AgendamentoResponse>> Create(
        [FromBody] CreateAgendamentoRequest request,
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

        var response = await agendamentoService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Consulta um agendamento por identificador.</summary>
    [HttpGet("{id:int}")]
    [EndpointSummary("Consulta um agendamento")]
    [EndpointDescription("Retorna os dados do agendamento identificado pelo ID.")]
    [ProducesResponseType<AgendamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgendamentoResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await agendamentoService.GetByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>Lista os agendamentos não cancelados de um barbeiro em uma data.</summary>
    [HttpGet("barbeiro/{barbeiroId:int}")]
    [EndpointSummary("Consulta a agenda do barbeiro")]
    [EndpointDescription("Retorna os agendamentos não cancelados do barbeiro na data informada.")]
    [ProducesResponseType<IReadOnlyList<AgendamentoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AgendamentoResponse>>> GetAgendaBarbeiro(
        int barbeiroId,
        [FromQuery, BindRequired] DateOnly data,
        CancellationToken cancellationToken)
    {
        var response = await agendamentoService.GetAgendaBarbeiroAsync(barbeiroId, data, cancellationToken);
        return Ok(response);
    }

    /// <summary>Cancela um agendamento; a operação é idempotente.</summary>
    [HttpPatch("{id:int}/cancelar")]
    [EndpointSummary("Cancela um agendamento")]
    [EndpointDescription("Marca o agendamento como cancelado e libera o intervalo reservado.")]
    [ProducesResponseType<AgendamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AgendamentoResponse>> Cancel(int id, CancellationToken cancellationToken) =>
        Ok(await agendamentoService.CancelAsync(id, cancellationToken));
}
