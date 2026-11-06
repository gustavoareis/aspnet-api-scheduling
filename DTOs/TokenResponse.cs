namespace aspnet_api_scheduling.DTOs;

/// <summary>Token de acesso e dados básicos da sessão.</summary>
public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt, int BarbeariaId, string Papel);
