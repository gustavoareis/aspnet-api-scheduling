using Microsoft.AspNetCore.Identity;

namespace aspnet_api_scheduling.Models;

public sealed class Usuario : IdentityUser
{
    public ICollection<UsuarioBarbearia> Barbearias { get; set; } = [];
}
