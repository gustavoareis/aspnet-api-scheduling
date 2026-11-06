using aspnet_api_scheduling.Models;
using aspnet_api_scheduling.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace aspnet_api_scheduling.Data;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IBarbeariaContext barbeariaContext) : IdentityDbContext<Usuario>(options)
{
    public DbSet<Barbearia> Barbearias => Set<Barbearia>();
    public DbSet<UsuarioBarbearia> UsuariosBarbearias => Set<UsuarioBarbearia>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Barbeiro> Barbeiros => Set<Barbeiro>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, long>(
            value => value.UtcTicks,
            ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

        modelBuilder.Entity<Barbearia>(entity =>
        {
            entity.Property(item => item.Nome).HasMaxLength(150).IsRequired();
            entity.Property(item => item.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(item => item.Slug).IsUnique();
        });

        modelBuilder.Entity<UsuarioBarbearia>(entity =>
        {
            entity.HasKey(item => new { item.UsuarioId, item.BarbeariaId });
            entity.Property(item => item.Papel).HasMaxLength(30).IsRequired();
            entity.HasOne(item => item.Usuario)
                .WithMany(usuario => usuario.Barbearias)
                .HasForeignKey(item => item.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Barbearia)
                .WithMany(barbearia => barbearia.Usuarios)
                .HasForeignKey(item => item.BarbeariaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(item => item.Nome).HasMaxLength(150).IsRequired();
            entity.Property(item => item.Telefone).HasMaxLength(30).IsRequired();
            entity.Property(item => item.Email).HasMaxLength(254).UseCollation("NOCASE").IsRequired();
            entity.HasAlternateKey(item => new { item.Id, item.BarbeariaId });
            entity.HasIndex(item => new { item.BarbeariaId, item.Email }).IsUnique();
            entity.HasOne(item => item.Barbearia).WithMany(item => item.Clientes)
                .HasForeignKey(item => item.BarbeariaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(item => item.BarbeariaId == barbeariaContext.BarbeariaId);
        });

        modelBuilder.Entity<Barbeiro>(entity =>
        {
            entity.Property(item => item.Nome).HasMaxLength(150).IsRequired();
            entity.Property(item => item.FotoUrl).HasMaxLength(2048);
            entity.HasAlternateKey(item => new { item.Id, item.BarbeariaId });
            entity.HasIndex(item => new { item.BarbeariaId, item.Ativo, item.Nome });
            entity.HasOne(item => item.Barbearia).WithMany(item => item.Barbeiros)
                .HasForeignKey(item => item.BarbeariaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(item => item.BarbeariaId == barbeariaContext.BarbeariaId);
        });

        modelBuilder.Entity<Servico>(entity =>
        {
            entity.Property(item => item.Nome).HasMaxLength(150).IsRequired();
            entity.Property(item => item.Preco).HasPrecision(10, 2);
            entity.HasAlternateKey(item => new { item.Id, item.BarbeariaId });
            entity.HasIndex(item => new { item.BarbeariaId, item.Ativo, item.Nome });
            entity.HasOne(item => item.Barbearia).WithMany(item => item.Servicos)
                .HasForeignKey(item => item.BarbeariaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(item => item.BarbeariaId == barbeariaContext.BarbeariaId);
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.Property(item => item.Nome).HasMaxLength(150).IsRequired();
            entity.Property(item => item.Descricao).HasMaxLength(500);
            entity.Property(item => item.Preco).HasPrecision(10, 2);
            entity.HasAlternateKey(item => new { item.Id, item.BarbeariaId });
            entity.HasIndex(item => new { item.BarbeariaId, item.Ativo, item.Nome });
            entity.HasOne(item => item.Barbearia).WithMany(item => item.Produtos)
                .HasForeignKey(item => item.BarbeariaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(item => item.BarbeariaId == barbeariaContext.BarbeariaId);
        });

        modelBuilder.Entity<Agendamento>(entity =>
        {
            entity.Property(item => item.DataHoraInicio).HasConversion(dateTimeOffsetConverter);
            entity.Property(item => item.DataHoraFim).HasConversion(dateTimeOffsetConverter);
            entity.Property(item => item.Observacao).HasMaxLength(500);
            entity.Property(item => item.ServicoNome).HasMaxLength(150).IsRequired();
            entity.Property(item => item.Preco).HasPrecision(10, 2);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(item => new { item.BarbeariaId, item.BarbeiroId, item.DataHoraInicio });
            entity.HasOne(item => item.Barbearia).WithMany(item => item.Agendamentos)
                .HasForeignKey(item => item.BarbeariaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Cliente).WithMany(item => item.Agendamentos)
                .HasForeignKey(item => new { item.ClienteId, item.BarbeariaId })
                .HasPrincipalKey(item => new { item.Id, item.BarbeariaId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Barbeiro).WithMany(item => item.Agendamentos)
                .HasForeignKey(item => new { item.BarbeiroId, item.BarbeariaId })
                .HasPrincipalKey(item => new { item.Id, item.BarbeariaId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Servico).WithMany(item => item.Agendamentos)
                .HasForeignKey(item => new { item.ServicoId, item.BarbeariaId })
                .HasPrincipalKey(item => new { item.Id, item.BarbeariaId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(item => item.BarbeariaId == barbeariaContext.BarbeariaId);
        });

    }

    public override int SaveChanges()
    {
        ApplyTenantId();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantId();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantId()
    {
        foreach (var entry in ChangeTracker.Entries()
                     .Where(entry => entry.State == EntityState.Added))
        {
            switch (entry.Entity)
            {
                case Cliente item:
                    item.BarbeariaId = RequireTenantId();
                    break;
                case Barbeiro item:
                    item.BarbeariaId = RequireTenantId();
                    break;
                case Servico item:
                    item.BarbeariaId = RequireTenantId();
                    break;
                case Produto item:
                    item.BarbeariaId = RequireTenantId();
                    break;
                case Agendamento item:
                    item.BarbeariaId = RequireTenantId();
                    break;
            }
        }
    }

    private int RequireTenantId() =>
        barbeariaContext.BarbeariaId
        ?? throw new InvalidOperationException("Não foi possível determinar a barbearia da operação.");
}
