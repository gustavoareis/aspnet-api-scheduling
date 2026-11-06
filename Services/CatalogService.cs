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
            .Where(servico => servico.Id == id && servico.Ativo)
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

    public async Task<ServicoResponse?> UpdateServicoAsync(
        int id,
        UpdateServicoRequest request,
        CancellationToken cancellationToken)
    {
        var servico = await dbContext.Servicos
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (servico is null)
        {
            return null;
        }

        servico.Nome = request.Nome.Trim();
        servico.Preco = request.Preco;
        servico.DuracaoMinutos = request.DuracaoMinutos;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(servico);
    }

    public async Task<bool> DeactivateServicoAsync(int id, CancellationToken cancellationToken)
    {
        var servico = await dbContext.Servicos
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (servico is null)
        {
            return false;
        }

        servico.Ativo = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ProdutoResponse>> GetProdutosAsync(CancellationToken cancellationToken) =>
        await dbContext.Produtos
            .AsNoTracking()
            .Where(item => item.Ativo)
            .OrderBy(item => item.Nome)
            .Select(item => new ProdutoResponse(item.Id, item.Nome, item.Descricao, item.Preco))
            .ToListAsync(cancellationToken);

    public async Task<ProdutoResponse?> GetProdutoByIdAsync(int id, CancellationToken cancellationToken) =>
        await dbContext.Produtos
            .AsNoTracking()
            .Where(item => item.Id == id && item.Ativo)
            .Select(item => new ProdutoResponse(item.Id, item.Nome, item.Descricao, item.Preco))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ProdutoResponse> CreateProdutoAsync(
        CreateProdutoRequest request,
        CancellationToken cancellationToken)
    {
        var produto = new Produto
        {
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao?.Trim(),
            Preco = request.Preco,
            Ativo = true
        };
        dbContext.Produtos.Add(produto);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(produto);
    }

    public async Task<ProdutoResponse?> UpdateProdutoAsync(
        int id,
        UpdateProdutoRequest request,
        CancellationToken cancellationToken)
    {
        var produto = await dbContext.Produtos
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (produto is null)
        {
            return null;
        }

        produto.Nome = request.Nome.Trim();
        produto.Descricao = request.Descricao?.Trim();
        produto.Preco = request.Preco;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(produto);
    }

    public async Task<bool> DeactivateProdutoAsync(int id, CancellationToken cancellationToken)
    {
        var produto = await dbContext.Produtos
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (produto is null)
        {
            return false;
        }

        produto.Ativo = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
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

    private static ProdutoResponse ToResponse(Produto produto) =>
        new(produto.Id, produto.Nome, produto.Descricao, produto.Preco);

    private static BarbeiroResponse ToResponse(Barbeiro barbeiro) =>
        new(barbeiro.Id, barbeiro.Nome, barbeiro.FotoUrl, barbeiro.Ativo);
}
