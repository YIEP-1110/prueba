using prueba.Data;

namespace prueba.Services;


public interface IPonentesCrudService
{
    Task<List<Participante>> GetAllAsync();                 
    Task<List<Participante>> GetCandidatosAsync();          
    Task AsignarAsync(Participante p, PonenteInfo info);    
    Task ActualizarAsync(Participante p, PonenteInfo info); 
    Task QuitarAsync(Participante p);                       
}

public class PonentesCrudService : IPonentesCrudService
{
    private readonly IParticipantesCrudService _participantes;
    private readonly IPonenciasCrudService _ponencias;

    public PonentesCrudService(IParticipantesCrudService participantes, IPonenciasCrudService ponencias)
    {
        _participantes = participantes;
        _ponencias = ponencias;
    }

    public async Task<List<Participante>> GetAllAsync() =>
        (await _participantes.GetAllAsync()).Where(p => p.Ponente != null).ToList();

    public async Task<List<Participante>> GetCandidatosAsync() =>
        (await _participantes.GetAllAsync()).Where(p => p.Ponente == null).ToList();

    public async Task AsignarAsync(Participante p, PonenteInfo info)
    {
        p.Ponente = info;
        p.Tipo_participacion = "ponente";
        await _participantes.UpdateAsync(p);
    }

    public async Task ActualizarAsync(Participante p, PonenteInfo info)
    {
        p.Ponente = info;
        await _participantes.UpdateAsync(p);
    }

    public async Task QuitarAsync(Participante p)
    {
        if (p.Ponente is null) return;

        var tiene = await _ponencias.GetByPonenteAsync(p.Ponente.IdPonente);
        if (tiene.Count > 0)
            throw new InvalidOperationException(
                $"No se puede quitar: tiene {tiene.Count} ponencia(s). Elimínalas primero.");

        p.Ponente = null;
        p.Tipo_participacion = "asistente";
        await _participantes.UpdateAsync(p);
    }
}