namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados públicos de um barbeiro ativo.</summary>
public sealed record BarbeiroResponse(int Id, string Nome, string? FotoUrl, bool Ativo);
