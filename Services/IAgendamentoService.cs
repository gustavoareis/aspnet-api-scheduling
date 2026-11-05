using aspnet_api_scheduling.DTOs;

namespace aspnet_api_scheduling.Services;

public interface IAgendamentoService
{
    Task<HorariosDisponiveisResponse> GetHorariosDisponiveisAsync(
        DateOnly data,
        int servicoId,
        int? barbeiroId,
        CancellationToken cancellationToken);

    Task<AgendamentoResponse> CreateAsync(
        CreateAgendamentoRequest request,
        CancellationToken cancellationToken);

    Task<AgendamentoResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<AgendamentoResponse>> GetAgendaBarbeiroAsync(
        int barbeiroId,
        DateOnly data,
        CancellationToken cancellationToken);

    Task<AgendamentoResponse> CancelAsync(int id, CancellationToken cancellationToken);
}
