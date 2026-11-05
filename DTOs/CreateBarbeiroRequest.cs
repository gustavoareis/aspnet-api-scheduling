namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados para cadastrar um barbeiro ativo.</summary>
public sealed record CreateBarbeiroRequest
{
    /// <summary>Nome do barbeiro.</summary>
    public required string Nome { get; init; }

    /// <summary>URL opcional da foto do barbeiro.</summary>
    public string? FotoUrl { get; init; }
}
