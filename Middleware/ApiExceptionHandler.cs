using aspnet_api_scheduling.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Middleware;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ResourceNotFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                exception.Message),
            BusinessConflictException => (
                StatusCodes.Status409Conflict,
                "Conflito de dados",
                exception.Message),
            BusinessRuleException => (
                StatusCodes.Status400BadRequest,
                "Regra de negócio inválida",
                exception.Message),
            InvalidProvisioningTokenException => (
                StatusCodes.Status401Unauthorized,
                "Autenticação necessária",
                exception.Message),
            BootstrapUnavailableException => (
                StatusCodes.Status503ServiceUnavailable,
                "Provisionamento indisponível",
                exception.Message),
            IdentityOperationException => (
                StatusCodes.Status400BadRequest,
                "Dados de conta inválidos",
                exception.Message),
            _ => (0, string.Empty, string.Empty)
        };

        if (statusCode == 0)
        {
            return false;
        }

        logger.LogWarning(exception, "Requisição rejeitada: {Title}", title);
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        }, cancellationToken);
        return true;
    }
}
