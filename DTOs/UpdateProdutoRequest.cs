namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados para alterar as informações de um produto.</summary>
public sealed record UpdateProdutoRequest
{
    public required string Nome { get; init; }
    public string? Descricao { get; init; }
    public required decimal Preco { get; init; }
}
