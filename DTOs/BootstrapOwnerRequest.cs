using System.ComponentModel.DataAnnotations;

namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados para criar a primeira conta proprietária e sua barbearia.</summary>
public sealed record BootstrapOwnerRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public required string NomeBarbearia { get; init; }

    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$"), StringLength(100)]
    public required string Slug { get; init; }

    [Required, StringLength(150, MinimumLength = 2)]
    public required string NomeDono { get; init; }

    [Required, EmailAddress, StringLength(254)]
    public required string Email { get; init; }

    [Required, MinLength(12), StringLength(128)]
    public required string Senha { get; init; }
}
