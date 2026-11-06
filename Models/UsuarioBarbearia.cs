namespace aspnet_api_scheduling.Models;

public sealed class UsuarioBarbearia
{
    public required string UsuarioId { get; set; }
    public int BarbeariaId { get; set; }
    public required string Papel { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Barbearia Barbearia { get; set; } = null!;
}
