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
    private readonly IParticipantesCrudService _participantes;

    public PonenteActualService(AuthenticationStateProvider auth, IParticipantesCrudService participantes)
    {
        _auth = auth;
        _participantes = participantes;
    }

    public async Task<Participante?> GetAsync()
    {
        var user = (await _auth.GetAuthenticationStateAsync()).User;
        if (user.Identity?.IsAuthenticated != true) return null;

        var idUsuario = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idUsuario)) return null;

        var participante = await _participantes.GetByUsuarioAsync(idUsuario);
        if (participante is null)
            return null;

        var rol = user.FindFirst(ClaimTypes.Role)?.Value;
        if (participante.Ponente is null && (rol == "ponente" || rol == "ambos"))
        {
            participante.Ponente = new PonenteInfo();
            await _participantes.UpdateAsync(participante);
        }

        return participante;
    }
}