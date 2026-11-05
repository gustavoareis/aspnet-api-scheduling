using aspnet_api_scheduling.DTOs;
using FluentValidation;

namespace aspnet_api_scheduling.Validators;

public sealed class CreateBarbeiroRequestValidator : AbstractValidator<CreateBarbeiroRequest>
{
    public CreateBarbeiroRequestValidator()
    {
        RuleFor(request => request.Nome)
            .Must(nome => !string.IsNullOrWhiteSpace(nome))
            .WithMessage("O nome é obrigatório.")
            .MaximumLength(150);
        RuleFor(request => request.FotoUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) ||
                Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("A foto deve usar uma URL HTTP ou HTTPS válida.");
        RuleFor(request => request.FotoUrl).MaximumLength(2048);
    }
}
