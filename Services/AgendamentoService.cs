using aspnet_api_scheduling.Data;
using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Models;
using Microsoft.EntityFrameworkCore;

namespace aspnet_api_scheduling.Services;

public sealed class AgendamentoService(AppDbContext dbContext, TimeZoneInfo businessTimeZone)
    : IAgendamentoService
{
    private static readonly TimeOnly OpeningTime = new(8, 0);
    private static readonly TimeOnly LastGridBoundary = new(19, 40);
    private static readonly TimeOnly LunchStart = new(12, 0);
    private static readonly TimeOnly LunchEnd = new(13, 0);
    private static readonly TimeSpan SlotStep = TimeSpan.FromMinutes(40);

    public async Task<HorariosDisponiveisResponse> GetHorariosDisponiveisAsync(
        DateOnly data,
        int servicoId,
        int? barbeiroId,
        CancellationToken cancellationToken)
    {
        var servico = await GetServicoAtivoAsync(servicoId, cancellationToken);
        var barbeiros = await GetBarbeirosAtivosAsync(barbeiroId, cancellationToken);
        var agendamentos = await GetAgendamentosDoDiaAsync(data, cancellationToken);
        var manha = new List<DateTimeOffset>();
        var tarde = new List<DateTimeOffset>();
        var noite = new List<DateTimeOffset>();

        for (var horario = OpeningTime; horario <= LastGridBoundary; horario = horario.Add(SlotStep))
        {
            var inicio = CreateBusinessDateTime(data, horario);
            var fim = inicio.AddMinutes(servico.DuracaoMinutos);

            if (CollidesWithLunch(inicio, fim))
            {
                continue;
            }

            var isAvailable = barbeiros.Any(barbeiro =>
                IsBarbeiroLivre(barbeiro.Id, inicio, fim, agendamentos));
            if (!isAvailable)
            {
                continue;
            }

            var localStart = TimeZoneInfo.ConvertTime(inicio, businessTimeZone).TimeOfDay;
            if (localStart < TimeSpan.FromHours(12))
            {
                manha.Add(inicio);
            }
            else if (localStart < TimeSpan.FromHours(19))
            {
                tarde.Add(inicio);
            }
            else
            {
                noite.Add(inicio);
            }
        }

        return new HorariosDisponiveisResponse(manha, tarde, noite);
    }

    public async Task<AgendamentoResponse> CreateAsync(
        CreateAgendamentoRequest request,
        CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.ClienteId, cancellationToken)
            ?? throw new ResourceNotFoundException("Cliente não encontrado.");
        var servico = await GetServicoAtivoAsync(request.ServicoId, cancellationToken);
        var inicio = TimeZoneInfo.ConvertTime(request.DataHoraInicio, businessTimeZone);
        var horarioLocal = TimeOnly.FromDateTime(inicio.DateTime);

        if (!IsGridTime(horarioLocal))
        {
            throw new BusinessRuleException("O início deve corresponder a um horário disponível da grade.");
        }

        var fim = inicio.AddMinutes(servico.DuracaoMinutos);
        if (CollidesWithLunch(inicio, fim))
        {
            throw new BusinessRuleException("O serviço não pode coincidir com o intervalo de almoço.");
        }

        var barbeiros = await GetBarbeirosAtivosAsync(request.BarbeiroId, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var agendamentos = await GetAgendamentosDoDiaAsync(DateOnly.FromDateTime(inicio.DateTime), cancellationToken);
        var barbeiroSelecionado = barbeiros.FirstOrDefault(barbeiro =>
            IsBarbeiroLivre(barbeiro.Id, inicio, fim, agendamentos));

        if (barbeiroSelecionado is null)
        {
            throw new BusinessConflictException("Não há barbeiro disponível para esse horário.");
        }

        var agendamento = new Agendamento
        {
            ClienteId = cliente.Id,
            BarbeiroId = barbeiroSelecionado.Id,
            ServicoId = servico.Id,
            DataHoraInicio = inicio,
            DataHoraFim = fim,
            NaoQueroConversar = request.NaoQueroConversar,
            Observacao = request.Observacao?.Trim(),
            Status = AgendamentoStatus.Pendente
        };

        dbContext.Agendamentos.Add(agendamento);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        agendamento.Cliente = cliente;
        agendamento.Barbeiro = barbeiroSelecionado;
        agendamento.Servico = servico;
        return ToResponse(agendamento);
    }

    public async Task<AgendamentoResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var agendamento = await QueryAgendamentos()
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return agendamento is null ? null : ToResponse(agendamento);
    }

    public async Task<IReadOnlyList<AgendamentoResponse>> GetAgendaBarbeiroAsync(
        int barbeiroId,
        DateOnly data,
        CancellationToken cancellationToken)
    {
        var barbeiroExiste = await dbContext.Barbeiros
            .AnyAsync(item => item.Id == barbeiroId, cancellationToken);
        if (!barbeiroExiste)
        {
            throw new ResourceNotFoundException("Barbeiro não encontrado.");
        }

        var (inicioDia, fimDia) = GetBusinessDayBounds(data);
        var agendamentos = await QueryAgendamentos()
            .AsNoTracking()
            .Where(item =>
                item.BarbeiroId == barbeiroId &&
                item.Status != AgendamentoStatus.Cancelado &&
                item.DataHoraInicio < fimDia &&
                item.DataHoraFim > inicioDia)
            .OrderBy(item => item.DataHoraInicio)
            .ToListAsync(cancellationToken);

        return agendamentos.Select(ToResponse).ToList();
    }

    public async Task<AgendamentoResponse> CancelAsync(int id, CancellationToken cancellationToken)
    {
        var agendamento = await QueryAgendamentos()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException("Agendamento não encontrado.");

        if (agendamento.Status == AgendamentoStatus.Concluido)
        {
            throw new BusinessConflictException("Agendamentos concluídos não podem ser cancelados.");
        }

        agendamento.Status = AgendamentoStatus.Cancelado;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(agendamento);
    }

    private async Task<Servico> GetServicoAtivoAsync(int servicoId, CancellationToken cancellationToken) =>
        await dbContext.Servicos
            .SingleOrDefaultAsync(item => item.Id == servicoId && item.Ativo, cancellationToken)
            ?? throw new ResourceNotFoundException("Serviço ativo não encontrado.");

    private async Task<List<Barbeiro>> GetBarbeirosAtivosAsync(
        int? barbeiroId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Barbeiros.Where(item => item.Ativo);
        if (barbeiroId.HasValue)
        {
            query = query.Where(item => item.Id == barbeiroId.Value);
        }

        var barbeiros = await query.OrderBy(item => item.Id).ToListAsync(cancellationToken);
        if (barbeiroId.HasValue && barbeiros.Count == 0)
        {
            throw new ResourceNotFoundException("Barbeiro ativo não encontrado.");
        }

        if (barbeiros.Count == 0)
        {
            throw new BusinessConflictException("Não há barbeiros ativos cadastrados.");
        }

        return barbeiros;
    }

    private async Task<List<Agendamento>> GetAgendamentosDoDiaAsync(
        DateOnly data,
        CancellationToken cancellationToken)
    {
        var (inicioDia, fimDia) = GetBusinessDayBounds(data);
        return await dbContext.Agendamentos
            .AsNoTracking()
            .Where(item =>
                item.Status != AgendamentoStatus.Cancelado &&
                item.DataHoraInicio < fimDia &&
                item.DataHoraFim > inicioDia)
            .ToListAsync(cancellationToken);
    }

    private static bool IsBarbeiroLivre(
        int barbeiroId,
        DateTimeOffset inicio,
        DateTimeOffset fim,
        IReadOnlyCollection<Agendamento> agendamentos) =>
        !agendamentos.Any(item =>
            item.BarbeiroId == barbeiroId &&
            inicio < item.DataHoraFim &&
            fim > item.DataHoraInicio);

    private static bool IsGridTime(TimeOnly horario) =>
        horario >= OpeningTime &&
        horario <= LastGridBoundary &&
        (horario.ToTimeSpan() - OpeningTime.ToTimeSpan()).Ticks % SlotStep.Ticks == 0;

    private bool CollidesWithLunch(DateTimeOffset inicio, DateTimeOffset fim)
    {
        var localInicio = TimeZoneInfo.ConvertTime(inicio, businessTimeZone);
        var inicioAlmoco = CreateBusinessDateTime(DateOnly.FromDateTime(localInicio.DateTime), LunchStart);
        var fimAlmoco = CreateBusinessDateTime(DateOnly.FromDateTime(localInicio.DateTime), LunchEnd);
        return inicio < fimAlmoco && fim > inicioAlmoco;
    }

    private DateTimeOffset CreateBusinessDateTime(DateOnly data, TimeOnly horario)
    {
        var localDateTime = data.ToDateTime(horario, DateTimeKind.Unspecified);
        if (businessTimeZone.IsInvalidTime(localDateTime))
        {
            throw new BusinessRuleException("O horário informado não existe no fuso horário configurado.");
        }

        return new DateTimeOffset(localDateTime, businessTimeZone.GetUtcOffset(localDateTime));
    }

    private (DateTimeOffset Start, DateTimeOffset End) GetBusinessDayBounds(DateOnly data) =>
        (CreateBusinessDateTime(data, TimeOnly.MinValue),
            CreateBusinessDateTime(data.AddDays(1), TimeOnly.MinValue));

    private IQueryable<Agendamento> QueryAgendamentos() =>
        dbContext.Agendamentos
            .Include(item => item.Cliente)
            .Include(item => item.Barbeiro)
            .Include(item => item.Servico);

    private AgendamentoResponse ToResponse(Agendamento item) =>
        new(
            item.Id,
            item.ClienteId,
            item.Cliente.Nome,
            item.BarbeiroId,
            item.Barbeiro.Nome,
            item.ServicoId,
            item.Servico.Nome,
            item.Servico.Preco,
            TimeZoneInfo.ConvertTime(item.DataHoraInicio, businessTimeZone),
            TimeZoneInfo.ConvertTime(item.DataHoraFim, businessTimeZone),
            item.NaoQueroConversar,
            item.Observacao,
            item.Status);
}
