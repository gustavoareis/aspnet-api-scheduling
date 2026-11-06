using aspnet_api_scheduling.Data;
using aspnet_api_scheduling.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aspnet_api_scheduling.Middleware;

public sealed class BarbeariaContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        IBarbeariaContext barbeariaContext,
        AppDbContext dbContext)
    {
        var path = httpContext.Request.Path;
        if (!path.StartsWithSegments("/api") ||
            path.StartsWithSegments("/api/auth/login") ||
            path.StartsWithSegments("/api/provisionamento/dono"))
        {
            await next(httpContext);
            return;
        }

        var claimValue = httpContext.User.FindFirst("barbearia_id")?.Value;
        var headerValue = httpContext.Request.Headers["X-Barbearia-Id"].FirstOrDefault();
        int? tenantId = null;
        if (claimValue is not null)
        {
            if (!int.TryParse(claimValue, out var claimTenantId))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            tenantId = claimTenantId;
            if (headerValue is not null &&
                (!int.TryParse(headerValue, out var requestedId) || requestedId != claimTenantId))
            {
                await WriteProblemAsync(httpContext, StatusCodes.Status403Forbidden,
                    "A barbearia solicitada não corresponde à sessão autenticada.");
                return;
            }
        }
        else if (int.TryParse(headerValue, out var publicTenantId))
        {
            tenantId = publicTenantId;
        }

        if (tenantId is null || tenantId <= 0)
        {
            await WriteProblemAsync(httpContext, StatusCodes.Status400BadRequest,
                "Informe X-Barbearia-Id para selecionar a barbearia.");
            return;
        }

        var exists = await dbContext.Barbearias
            .AsNoTracking()
            .AnyAsync(item => item.Id == tenantId && item.Ativa, httpContext.RequestAborted);
        if (!exists)
        {
            await WriteProblemAsync(httpContext, StatusCodes.Status404NotFound, "Barbearia não encontrada.");
            return;
        }

        barbeariaContext.BarbeariaId = tenantId;
        await next(httpContext);
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == StatusCodes.Status403Forbidden ? "Acesso negado" : "Requisição inválida",
            Detail = detail,
            Instance = context.Request.Path
        });
    }
}
