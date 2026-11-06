namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados públicos de um produto ativo da barbearia.</summary>
public sealed record ProdutoResponse(int Id, string Nome, string? Descricao, decimal Preco);
