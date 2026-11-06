using System.ComponentModel.DataAnnotations;

namespace aspnet_api_scheduling.DTOs;

/// <summary>Credenciais e barbearia para autenticação.</summary>
public sealed record LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public required string Email { get; init; }

    [Required]
    public required string Senha { get; init; }

    [Range(1, int.MaxValue)]
    public required int BarbeariaId { get; init; }
}
