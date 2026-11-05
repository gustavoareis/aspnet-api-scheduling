using aspnet_api_scheduling.DTOs;
using FluentValidation;

namespace aspnet_api_scheduling.Validators;

public sealed class CreateClienteRequestValidator : AbstractValidator<CreateClienteRequest>
{
    public CreateClienteRequestValidator()
    {
        RuleFor(request => request.Nome)
            .Must(nome => !string.IsNullOrWhiteSpace(nome))
            .WithMessage("O nome é obrigatório.")
            .MaximumLength(150);
        RuleFor(request => request.Telefone)
            .Must(telefone => !string.IsNullOrWhiteSpace(telefone))
            .WithMessage("O telefone é obrigatório.")
            .MaximumLength(30);
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);
    }
}
