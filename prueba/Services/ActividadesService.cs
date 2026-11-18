using prueba.Data;

namespace prueba.Services;

public interface IActividadesService
{
    IReadOnlyList<Actividad> ObtenerPrograma();
    IReadOnlyList<InscripcionActividad> ObtenerInscripciones(string idUsuario);
    bool EstaInscrito(string idUsuario, string idActividad);
    bool Inscribir(string idUsuario, string idActividad);
    bool Pagar(string idUsuario, string idActividad);
}

public sealed class ActividadesService : IActividadesService
{
    private readonly List<Actividad> _programa =
    [
        new() { Id = "apertura", Nombre = "Ceremonia de apertura", Descripcion = "Inicio oficial del congreso.", Fecha = "Día 1", Horario = "09:00 - 10:00", Lugar = "Auditorio principal", Precio = 0 },
        new() { Id = "taller", Nombre = "Taller de innovación", Descripcion = "Taller práctico para participantes.", Fecha = "Día 1", Horario = "11:00 - 13:00", Lugar = "Sala 1", Precio = 150 },
        new() { Id = "conferencia", Nombre = "Conferencia magistral", Descripcion = "Conferencia de clausura del congreso.", Fecha = "Día 2", Horario = "16:00 - 18:00", Lugar = "Auditorio principal", Precio = 200 }
    ];

    private readonly List<InscripcionActividad> _inscripciones = [];
    private readonly object _sync = new();

    public IReadOnlyList<Actividad> ObtenerPrograma() => _programa;

    public IReadOnlyList<InscripcionActividad> ObtenerInscripciones(string idUsuario)
    {
        lock (_sync)
            return _inscripciones.Where(i => i.IdUsuario == idUsuario).ToList();
    }

    public bool EstaInscrito(string idUsuario, string idActividad)
    {
        lock (_sync)
            return _inscripciones.Any(i => i.IdUsuario == idUsuario && i.IdActividad == idActividad);
    }

    public bool Inscribir(string idUsuario, string idActividad)
    {
        lock (_sync)
        {
            if (!_programa.Any(a => a.Id == idActividad) || EstaInscritoSinBloqueo(idUsuario, idActividad))
                return false;

            _inscripciones.Add(new InscripcionActividad { IdUsuario = idUsuario, IdActividad = idActividad });
            return true;
        }
    }

    public bool Pagar(string idUsuario, string idActividad)
    {
        lock (_sync)
        {
            var inscripcion = _inscripciones.FirstOrDefault(i => i.IdUsuario == idUsuario && i.IdActividad == idActividad);
            if (inscripcion is null)
                return false;

            inscripcion.PagoRealizado = true;
            inscripcion.FechaPago = DateTime.UtcNow;
            return true;
        }
    }

    private bool EstaInscritoSinBloqueo(string idUsuario, string idActividad) =>
        _inscripciones.Any(i => i.IdUsuario == idUsuario && i.IdActividad == idActividad);
}
