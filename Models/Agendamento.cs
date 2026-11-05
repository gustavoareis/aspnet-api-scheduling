namespace aspnet_api_scheduling.Models;

public sealed class Agendamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int BarbeiroId { get; set; }
    public int ServicoId { get; set; }
    public DateTimeOffset DataHoraInicio { get; set; }
    public DateTimeOffset DataHoraFim { get; set; }
    public bool NaoQueroConversar { get; set; }
    public string? Observacao { get; set; }
    public AgendamentoStatus Status { get; set; } = AgendamentoStatus.Pendente;

    public Cliente Cliente { get; set; } = null!;
    public Barbeiro Barbeiro { get; set; } = null!;
    public Servico Servico { get; set; } = null!;
}
