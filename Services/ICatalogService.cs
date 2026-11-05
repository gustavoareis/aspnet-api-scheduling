using aspnet_api_scheduling.DTOs;

namespace aspnet_api_scheduling.Services;

public interface ICatalogService
{
    Task<IReadOnlyList<ServicoResponse>> GetServicosAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<BarbeiroResponse>> GetBarbeirosAsync(CancellationToken cancellationToken);
}
