using aspnet_api_scheduling.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace aspnet_api_scheduling.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Barbeiro> Barbeiros => Set<Barbeiro>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, long>(
            value => value.UtcTicks,
            ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(cliente => cliente.Nome).HasMaxLength(150).IsRequired();
            entity.Property(cliente => cliente.Telefone).HasMaxLength(30).IsRequired();
            entity.Property(cliente => cliente.Email).HasMaxLength(254).IsRequired();
            entity.HasIndex(cliente => cliente.Email).IsUnique();
        });

        modelBuilder.Entity<Barbeiro>(entity =>
        {
            entity.Property(barbeiro => barbeiro.Nome).HasMaxLength(150).IsRequired();
            entity.Property(barbeiro => barbeiro.FotoUrl).HasMaxLength(2048);
            entity.HasData(
                new Barbeiro { Id = 1, Nome = "Breno Sousa", Ativo = true },
                new Barbeiro { Id = 2, Nome = "Bruno Sousa", Ativo = true });
        });

        modelBuilder.Entity<Servico>(entity =>
        {
            entity.Property(servico => servico.Nome).HasMaxLength(150).IsRequired();
            entity.Property(servico => servico.Preco).HasPrecision(10, 2);
            entity.HasData(
                new Servico { Id = 1, Nome = "Corte Fade", Preco = 50m, DuracaoMinutos = 40, Ativo = true },
                new Servico { Id = 2, Nome = "Luzes", Preco = 80m, DuracaoMinutos = 120, Ativo = true },
                new Servico { Id = 3, Nome = "Combo Corte + Barba", Preco = 75m, DuracaoMinutos = 60, Ativo = true });
        });

        modelBuilder.Entity<Cliente>().HasData(
            new Cliente
            {
                Id = 1,
                Nome = "Cliente de demonstração",
                Telefone = "(11) 99999-0000",
                Email = "cliente.demo@example.com"
            });

        modelBuilder.Entity<Agendamento>(entity =>
        {
            entity.Property(agendamento => agendamento.DataHoraInicio).HasConversion(dateTimeOffsetConverter);
            entity.Property(agendamento => agendamento.DataHoraFim).HasConversion(dateTimeOffsetConverter);
            entity.Property(agendamento => agendamento.Observacao).HasMaxLength(500);
            entity.Property(agendamento => agendamento.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(agendamento => new { agendamento.BarbeiroId, agendamento.DataHoraInicio });
            entity.HasOne(agendamento => agendamento.Cliente)
                .WithMany(cliente => cliente.Agendamentos)
                .HasForeignKey(agendamento => agendamento.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(agendamento => agendamento.Barbeiro)
                .WithMany(barbeiro => barbeiro.Agendamentos)
                .HasForeignKey(agendamento => agendamento.BarbeiroId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(agendamento => agendamento.Servico)
                .WithMany(servico => servico.Agendamentos)
                .HasForeignKey(agendamento => agendamento.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
