namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados básicos de uma barbearia associada à conta.</summary>
public sealed record BarbeariaResponse(int Id, string Nome, string Slug, string Papel);
