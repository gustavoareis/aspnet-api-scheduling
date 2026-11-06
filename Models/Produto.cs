namespace aspnet_api_scheduling.Models;

public sealed class Produto
{
    public int Id { get; set; }
    public int BarbeariaId { get; set; }
    public required string Nome { get; set; }
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public bool Ativo { get; set; }
    public Barbearia Barbearia { get; set; } = null!;
}
