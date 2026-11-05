namespace aspnet_api_scheduling.DTOs;

/// <summary>Horários livres agrupados pelos turnos da barbearia.</summary>
public sealed record HorariosDisponiveisResponse(
    IReadOnlyList<DateTimeOffset> Manha,
    IReadOnlyList<DateTimeOffset> Tarde,
    IReadOnlyList<DateTimeOffset> Noite);
