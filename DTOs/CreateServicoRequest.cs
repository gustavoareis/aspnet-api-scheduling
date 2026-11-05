namespace aspnet_api_scheduling.DTOs;

/// <summary>Dados para cadastrar um serviço ativo.</summary>
public sealed record CreateServicoRequest
{
    /// <summary>Nome do serviço.</summary>
    public required string Nome { get; init; }

    /// <summary>Preço do serviço em reais.</summary>
    public required decimal Preco { get; init; }

    /// <summary>Duração do serviço em minutos.</summary>
    public required int DuracaoMinutos { get; init; }
}
