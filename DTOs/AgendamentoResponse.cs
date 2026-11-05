using aspnet_api_scheduling.Models;

namespace aspnet_api_scheduling.DTOs;

/// <summary>Representa um agendamento e os dados resumidos de seus participantes.</summary>
public sealed record AgendamentoResponse(
    int Id,
    int ClienteId,
    string ClienteNome,
    int BarbeiroId,
    string BarbeiroNome,
    int ServicoId,
    string ServicoNome,
    decimal Preco,
    DateTimeOffset DataHoraInicio,
    DateTimeOffset DataHoraFim,
    bool NaoQueroConversar,
    string? Observacao,
    AgendamentoStatus Status);
