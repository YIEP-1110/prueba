using Newtonsoft.Json;

namespace prueba.Data;

public class Participante
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string IdUsuarios { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Apellido_paterno { get; set; } = "";
    public string Apellido_materno { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Tipo_participacion { get; set; } = "Ponente";
    public bool Estado_registro { get; set; } = true;
}
