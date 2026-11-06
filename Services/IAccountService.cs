using aspnet_api_scheduling.DTOs;

namespace aspnet_api_scheduling.Services;

public interface IAccountService
{
    Task<BarbeariaResponse> BootstrapOwnerAsync(
        BootstrapOwnerRequest request,
        string? provisioningToken,
        CancellationToken cancellationToken);

    Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<BarbeariaResponse>> GetBarbeariasAsync(
        string usuarioId,
        CancellationToken cancellationToken);

    Task<BarbeariaResponse?> GetBarbeariaAsync(
        string usuarioId,
        int barbeariaId,
        CancellationToken cancellationToken);

    Task<BarbeariaResponse> CreateBarbeariaAsync(
        string usuarioId,
        CreateBarbeariaRequest request,
        CancellationToken cancellationToken);
}
