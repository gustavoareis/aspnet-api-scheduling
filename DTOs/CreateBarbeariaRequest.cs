using System.ComponentModel.DataAnnotations;

namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados para cadastrar outra barbearia na conta autenticada.</summary>
public sealed record CreateBarbeariaRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public required string Nome { get; init; }

    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$"), StringLength(100)]
    public required string Slug { get; init; }
}
