namespace aspnet_api_scheduling.Models;

public sealed class Barbearia
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Slug { get; set; }
    public bool Ativa { get; set; } = true;
    public ICollection<Cliente> Clientes { get; set; } = [];
    public ICollection<Barbeiro> Barbeiros { get; set; } = [];
    public ICollection<Servico> Servicos { get; set; } = [];
    public ICollection<Produto> Produtos { get; set; } = [];
    public ICollection<Agendamento> Agendamentos { get; set; } = [];
    public ICollection<UsuarioBarbearia> Usuarios { get; set; } = [];
}
