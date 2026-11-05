using aspnet_api_scheduling.DTOs;
using FluentValidation;

namespace aspnet_api_scheduling.Validators;

public sealed class CreateAgendamentoRequestValidator : AbstractValidator<CreateAgendamentoRequest>
{
    public CreateAgendamentoRequestValidator()
    {
        RuleFor(request => request.ClienteId).GreaterThan(0);
        RuleFor(request => request.BarbeiroId)
            .GreaterThan(0)
            .When(request => request.BarbeiroId.HasValue);
        RuleFor(request => request.ServicoId).GreaterThan(0);
        RuleFor(request => request.DataHoraInicio)
            .Must(dataHora => dataHora != default)
            .WithMessage("A data e hora de início são obrigatórias.");
        RuleFor(request => request.Observacao).MaximumLength(500);
    }
}
