using aspnet_api_scheduling.Data;
using aspnet_api_scheduling.DTOs;
using Microsoft.EntityFrameworkCore;

namespace aspnet_api_scheduling.Services;

public sealed class CatalogService(AppDbContext dbContext) : ICatalogService
{
    public async Task<IReadOnlyList<ServicoResponse>> GetServicosAsync(CancellationToken cancellationToken) =>
        await dbContext.Servicos
            .AsNoTracking()
            .Where(servico => servico.Ativo)
            .OrderBy(servico => servico.Nome)
            .Select(servico => new ServicoResponse(
                servico.Id,
                servico.Nome,
                servico.Preco,
                servico.DuracaoMinutos))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<BarbeiroResponse>> GetBarbeirosAsync(CancellationToken cancellationToken) =>
        await dbContext.Barbeiros
            .AsNoTracking()
            .Where(barbeiro => barbeiro.Ativo)
            .OrderBy(barbeiro => barbeiro.Nome)
            .Select(barbeiro => new BarbeiroResponse(
                barbeiro.Id,
                barbeiro.Nome,
                barbeiro.FotoUrl,
                barbeiro.Ativo))
            .ToListAsync(cancellationToken);
}
