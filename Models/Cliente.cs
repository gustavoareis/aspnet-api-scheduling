namespace aspnet_api_scheduling.Models;

public sealed class Cliente
{
    public int Id { get; set; }
    public int BarbeariaId { get; set; }
    public required string Nome { get; set; }
    public required string Telefone { get; set; }
    public required string Email { get; set; }
    public ICollection<Agendamento> Agendamentos { get; set; } = [];
    public Barbearia Barbearia { get; set; } = null!;
}
