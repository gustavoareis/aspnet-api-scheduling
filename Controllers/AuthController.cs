using aspnet_api_scheduling.DTOs;
using aspnet_api_scheduling.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_api_scheduling.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAccountService accountService) : ControllerBase
{
    /// <summary>Autentica uma conta na barbearia selecionada.</summary>
    [HttpPost("login")]
    [EndpointSummary("Autentica um usuário")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await accountService.LoginAsync(request, cancellationToken);
        return response is null
            ? Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Não foi possível autenticar",
                Detail = "E-mail, senha ou acesso à barbearia inválido."
            })
            : Ok(response);
    }
}
