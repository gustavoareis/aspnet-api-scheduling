namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados públicos de um serviço oferecido pela barbearia.</summary>
public sealed record ServicoResponse(int Id, string Nome, decimal Preco, int DuracaoMinutos);
