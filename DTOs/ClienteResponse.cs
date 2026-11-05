namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados de um cliente cadastrado.</summary>
public sealed record ClienteResponse(int Id, string Nome, string Telefone, string Email);
