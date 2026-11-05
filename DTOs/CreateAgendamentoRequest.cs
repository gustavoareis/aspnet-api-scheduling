using System.ComponentModel.DataAnnotations;

namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados necessários para criar um agendamento.</summary>
public sealed record CreateAgendamentoRequest
{
    /// <summary>Identificador do cliente cadastrado.</summary>
    [Range(1, int.MaxValue)]
    public required int ClienteId { get; init; }

    /// <summary>Barbeiro escolhido; omitido ou nulo para escolher qualquer barbeiro livre.</summary>
    [Range(1, int.MaxValue)]
    public int? BarbeiroId { get; init; }

    /// <summary>Identificador do serviço ativo.</summary>
    [Range(1, int.MaxValue)]
    public required int ServicoId { get; init; }

    /// <summary>Início do atendimento em ISO 8601 com deslocamento de fuso.</summary>
    public required DateTimeOffset DataHoraInicio { get; init; }

    /// <summary>Indica se o cliente prefere atendimento em silêncio.</summary>
    public bool NaoQueroConversar { get; init; }

    /// <summary>Observação opcional, limitada a 500 caracteres.</summary>
    [MaxLength(500)]
    public string? Observacao { get; init; }
}
