namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados para alterar nome, preço e duração de um serviço.</summary>
public sealed record UpdateServicoRequest
{
    public required string Nome { get; init; }
    public required decimal Preco { get; init; }
    public required int DuracaoMinutos { get; init; }
}
