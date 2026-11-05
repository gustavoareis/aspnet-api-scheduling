namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados para cadastrar um cliente.</summary>
public sealed record CreateClienteRequest
{
    /// <summary>Nome completo do cliente.</summary>
    public required string Nome { get; init; }

    /// <summary>Telefone para contato.</summary>
    public required string Telefone { get; init; }

    /// <summary>E-mail único do cliente.</summary>
    public required string Email { get; init; }
}
