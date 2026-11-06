using System.ComponentModel.DataAnnotations;

namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados para cadastrar um produto da barbearia.</summary>
public sealed record CreateProdutoRequest
{
    [Required, StringLength(150, MinimumLength = 1)]
    public required string Nome { get; init; }

    [StringLength(500)]
    public string? Descricao { get; init; }

    public required decimal Preco { get; init; }
}
