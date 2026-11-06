using aspnet_api_scheduling.DTOs;

namespace aspnet_api_scheduling.Services;

public interface ICatalogService
{
    Task<IReadOnlyList<ClienteResponse>> GetClientesAsync(CancellationToken cancellationToken);
    Task<ClienteResponse?> GetClienteByIdAsync(int id, CancellationToken cancellationToken);
    Task<ClienteResponse> CreateClienteAsync(CreateClienteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ServicoResponse>> GetServicosAsync(CancellationToken cancellationToken);
    Task<ServicoResponse?> GetServicoByIdAsync(int id, CancellationToken cancellationToken);
    Task<ServicoResponse> CreateServicoAsync(CreateServicoRequest request, CancellationToken cancellationToken);
    Task<ServicoResponse?> UpdateServicoAsync(
        int id,
        UpdateServicoRequest request,
        CancellationToken cancellationToken);
    Task<bool> DeactivateServicoAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProdutoResponse>> GetProdutosAsync(CancellationToken cancellationToken);
    Task<ProdutoResponse?> GetProdutoByIdAsync(int id, CancellationToken cancellationToken);
    Task<ProdutoResponse> CreateProdutoAsync(CreateProdutoRequest request, CancellationToken cancellationToken);
    Task<ProdutoResponse?> UpdateProdutoAsync(
        int id,
        UpdateProdutoRequest request,
        CancellationToken cancellationToken);
    Task<bool> DeactivateProdutoAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<BarbeiroResponse>> GetBarbeirosAsync(CancellationToken cancellationToken);
    Task<BarbeiroResponse?> GetBarbeiroByIdAsync(int id, CancellationToken cancellationToken);
    Task<BarbeiroResponse> CreateBarbeiroAsync(CreateBarbeiroRequest request, CancellationToken cancellationToken);
}
