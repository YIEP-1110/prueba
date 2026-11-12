using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using prueba.Data;

namespace prueba.Services;

public interface IPonenteActualService
{
    Task<Participante?> GetAsync();
}

public class PonenteActualService : IPonenteActualService
{
    private readonly AuthenticationStateProvider _auth;
    private readonly IPonentesCrudService _ponentes;

    public PonenteActualService(AuthenticationStateProvider auth, IPonentesCrudService ponentes)
    {
        _auth = auth;
        _ponentes = ponentes;
    }

    public async Task<Participante?> GetAsync()
    {
        var user = (await _auth.GetAuthenticationStateAsync()).User;
        if (user.Identity?.IsAuthenticated != true) return null;

        var idUsuario = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idUsuario)) return null;

        return (await _ponentes.GetAllAsync()).FirstOrDefault(p => p.IdUsuarios == idUsuario);
    }
}