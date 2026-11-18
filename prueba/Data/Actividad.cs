using System.Collections.Concurrent;

namespace prueba.Data;

public sealed class Actividad
{
    public string Id { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Fecha { get; set; } = "";
    public string Horario { get; set; } = "";
    public string Lugar { get; set; } = "";
    public decimal Precio { get; set; }
}

public sealed class InscripcionActividad
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string IdUsuario { get; set; } = "";
    public string IdActividad { get; set; } = "";
    public bool PagoRealizado { get; set; }
    public DateTime? FechaPago { get; set; }
}
