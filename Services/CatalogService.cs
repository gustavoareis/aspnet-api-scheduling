using aspnet_api_scheduling.Data;
using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace aspnet_api_scheduling.Services;

public sealed class CatalogService(AppDbContext dbContext) : ICatalogService
{
    public async Task<IReadOnlyList<ClienteResponse>> GetClientesAsync(CancellationToken cancellationToken) =>
        await dbContext.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Nome)
            .Select(cliente => new ClienteResponse(cliente.Id, cliente.Nome, cliente.Telefone, cliente.Email))
            .ToListAsync(cancellationToken);

    public async Task<ClienteResponse?> GetClienteByIdAsync(int id, CancellationToken cancellationToken) =>
        await dbContext.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.Id == id)
            .Select(cliente => new ClienteResponse(cliente.Id, cliente.Nome, cliente.Telefone, cliente.Email))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ClienteResponse> CreateClienteAsync(
        CreateClienteRequest request,
        CancellationToken cancellationToken)
    {
        var nome = request.Nome.Trim();
        var telefone = request.Telefone.Trim();
        var email = request.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();

        var emailJaCadastrado = await dbContext.Clientes
            .AnyAsync(cliente => cliente.Email.ToLower() == normalizedEmail, cancellationToken);
        if (emailJaCadastrado)
        {
            throw new BusinessConflictException("Já existe um cliente cadastrado com esse e-mail.");
        }

        var cliente = new Cliente
        {
            Nome = nome,
            Telefone = telefone,
            Email = email
        };
        dbContext.Clientes.Add(cliente);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            throw new BusinessConflictException("Já existe um cliente cadastrado com esse e-mail.");
        }

        return ToResponse(cliente);
    }

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

    public async Task<ServicoResponse?> GetServicoByIdAsync(int id, CancellationToken cancellationToken) =>
        await dbContext.Servicos
            .AsNoTracking()
            .Where(servico => servico.Id == id)
            .Select(servico => new ServicoResponse(
                servico.Id,
                servico.Nome,
                servico.Preco,
                servico.DuracaoMinutos))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ServicoResponse> CreateServicoAsync(
        CreateServicoRequest request,
        CancellationToken cancellationToken)
    {
        var servico = new Servico
        {
            Nome = request.Nome.Trim(),
            Preco = request.Preco,
            DuracaoMinutos = request.DuracaoMinutos,
            Ativo = true
        };
        dbContext.Servicos.Add(servico);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(servico);
    }

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

    public async Task<BarbeiroResponse?> GetBarbeiroByIdAsync(int id, CancellationToken cancellationToken) =>
        await dbContext.Barbeiros
            .AsNoTracking()
            .Where(barbeiro => barbeiro.Id == id)
            .Select(barbeiro => new BarbeiroResponse(
                barbeiro.Id,
                barbeiro.Nome,
                barbeiro.FotoUrl,
                barbeiro.Ativo))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<BarbeiroResponse> CreateBarbeiroAsync(
        CreateBarbeiroRequest request,
        CancellationToken cancellationToken)
    {
        var barbeiro = new Barbeiro
        {
            Nome = request.Nome.Trim(),
            FotoUrl = request.FotoUrl?.Trim(),
            Ativo = true
        };
        dbContext.Barbeiros.Add(barbeiro);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(barbeiro);
    }

    private static ClienteResponse ToResponse(Cliente cliente) =>
        new(cliente.Id, cliente.Nome, cliente.Telefone, cliente.Email);

    private static ServicoResponse ToResponse(Servico servico) =>
        new(servico.Id, servico.Nome, servico.Preco, servico.DuracaoMinutos);

    private static BarbeiroResponse ToResponse(Barbeiro barbeiro) =>
        new(barbeiro.Id, barbeiro.Nome, barbeiro.FotoUrl, barbeiro.Ativo);
}
