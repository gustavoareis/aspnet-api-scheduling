using System.Security.Cryptography;
using System.Text;
using aspnet_api_scheduling.Data;
using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace aspnet_api_scheduling.Services;

public sealed class AccountService(
    AppDbContext dbContext,
    UserManager<Usuario> userManager,
    SignInManager<Usuario> signInManager,
    JwtTokenService tokenService,
    IConfiguration configuration) : IAccountService
{
    public async Task<BarbeariaResponse> BootstrapOwnerAsync(
        BootstrapOwnerRequest request,
        string? provisioningToken,
        CancellationToken cancellationToken)
    {
        var configuredToken = configuration["Bootstrap:Token"];
        if (string.IsNullOrWhiteSpace(configuredToken) || configuredToken.Length < 32)
        {
            throw new BootstrapUnavailableException();
        }

        if (!TokensMatch(configuredToken, provisioningToken))
        {
            throw new InvalidProvisioningTokenException();
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (await userManager.Users.AnyAsync(cancellationToken))
        {
            throw new BusinessConflictException("O provisionamento inicial já foi concluído.");
        }

        var barbearia = await dbContext.Barbearias.IgnoreQueryFilters()
            .OrderBy(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await dbContext.Barbearias.IgnoreQueryFilters()
                .AnyAsync(item => item.Slug == slug && (barbearia == null || item.Id != barbearia.Id), cancellationToken))
        {
            throw new BusinessConflictException("Já existe uma barbearia com esse identificador.");
        }

        if (barbearia is null)
        {
            barbearia = new Barbearia
            {
                Nome = request.NomeBarbearia.Trim(),
                Slug = slug,
                Ativa = true
            };
            dbContext.Barbearias.Add(barbearia);
        }
        else
        {
            barbearia.Nome = request.NomeBarbearia.Trim();
            barbearia.Slug = slug;
            barbearia.Ativa = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var usuario = new Usuario
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            EmailConfirmed = true,
            LockoutEnabled = true
        };
        var createResult = await userManager.CreateAsync(usuario, request.Senha);
        if (!createResult.Succeeded)
        {
            throw new IdentityOperationException(createResult.Errors.Select(error => error.Description));
        }

        dbContext.UsuariosBarbearias.Add(new UsuarioBarbearia
        {
            UsuarioId = usuario.Id,
            BarbeariaId = barbearia.Id,
            Papel = "Dono"
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new BarbeariaResponse(barbearia.Id, barbearia.Nome, barbearia.Slug, "Dono");
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var usuario = await userManager.FindByEmailAsync(request.Email.Trim());
        if (usuario is null ||
            !(await signInManager.CheckPasswordSignInAsync(usuario, request.Senha, lockoutOnFailure: true)).Succeeded)
        {
            return null;
        }

        var membership = await dbContext.UsuariosBarbearias
            .IgnoreQueryFilters()
            .Include(item => item.Barbearia)
            .SingleOrDefaultAsync(
                item => item.UsuarioId == usuario.Id &&
                        item.BarbeariaId == request.BarbeariaId &&
                        item.Barbearia.Ativa,
                cancellationToken);
        if (membership is null)
        {
            return null;
        }

        var (token, expiresAt) = tokenService.Create(usuario, membership.BarbeariaId, membership.Papel);
        return new TokenResponse(token, expiresAt, membership.BarbeariaId, membership.Papel);
    }

    public async Task<IReadOnlyList<BarbeariaResponse>> GetBarbeariasAsync(
        string usuarioId,
        CancellationToken cancellationToken) =>
        await dbContext.UsuariosBarbearias
            .AsNoTracking()
            .Where(item => item.UsuarioId == usuarioId && item.Barbearia.Ativa)
            .OrderBy(item => item.Barbearia.Nome)
            .Select(item => new BarbeariaResponse(
                item.BarbeariaId,
                item.Barbearia.Nome,
                item.Barbearia.Slug,
                item.Papel))
            .ToListAsync(cancellationToken);

    public async Task<BarbeariaResponse?> GetBarbeariaAsync(
        string usuarioId,
        int barbeariaId,
        CancellationToken cancellationToken) =>
        await dbContext.UsuariosBarbearias
            .AsNoTracking()
            .Where(item =>
                item.UsuarioId == usuarioId &&
                item.BarbeariaId == barbeariaId &&
                item.Barbearia.Ativa)
            .Select(item => new BarbeariaResponse(
                item.BarbeariaId,
                item.Barbearia.Nome,
                item.Barbearia.Slug,
                item.Papel))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<BarbeariaResponse> CreateBarbeariaAsync(
        string usuarioId,
        CreateBarbeariaRequest request,
        CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await dbContext.Barbearias.IgnoreQueryFilters().AnyAsync(item => item.Slug == slug, cancellationToken))
        {
            throw new BusinessConflictException("Já existe uma barbearia com esse identificador.");
        }

        var barbearia = new Barbearia
        {
            Nome = request.Nome.Trim(),
            Slug = slug,
            Ativa = true
        };
        dbContext.Barbearias.Add(barbearia);
        dbContext.UsuariosBarbearias.Add(new UsuarioBarbearia
        {
            UsuarioId = usuarioId,
            Barbearia = barbearia,
            Papel = "Dono"
        });
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            throw new BusinessConflictException("Já existe uma barbearia com esse identificador.");
        }

        return new BarbeariaResponse(barbearia.Id, barbearia.Nome, barbearia.Slug, "Dono");
    }

    private static bool TokensMatch(string expected, string? provided)
    {
        if (string.IsNullOrEmpty(provided))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)),
            SHA256.HashData(Encoding.UTF8.GetBytes(provided)));
    }
}
