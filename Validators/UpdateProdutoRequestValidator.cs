using aspnet_api_scheduling.DTOs;
using FluentValidation;

namespace aspnet_api_scheduling.Validators;

public sealed class UpdateProdutoRequestValidator : AbstractValidator<UpdateProdutoRequest>
{
    public UpdateProdutoRequestValidator()
    {
        RuleFor(request => request.Nome)
            .Must(nome => !string.IsNullOrWhiteSpace(nome))
            .WithMessage("O nome é obrigatório.")
            .MaximumLength(150);
        RuleFor(request => request.Descricao).MaximumLength(500);
        RuleFor(request => request.Preco).InclusiveBetween(0.01m, 999999.99m);
    }
}
