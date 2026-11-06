namespace aspnet_api_scheduling.Models;

public sealed class Barbeiro
{
    public int Id { get; set; }
    public int BarbeariaId { get; set; }
    public required string Nome { get; set; }
    public string? FotoUrl { get; set; }
    public bool Ativo { get; set; }
    public ICollection<Agendamento> Agendamentos { get; set; } = [];
    public Barbearia Barbearia { get; set; } = null!;
}
