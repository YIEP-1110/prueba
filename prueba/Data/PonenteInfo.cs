using Newtonsoft.Json;

namespace prueba.Data;

public class PonenteInfo
{
    [JsonProperty("id_ponente")]
    public string IdPonente { get; set; } = "pon" + Guid.NewGuid().ToString("N")[..8];

    [JsonProperty("especialidad")]
    public string Especialidad { get; set; } = "";

    [JsonProperty("biografia")]
    public string Biografia { get; set; } = "";

    [JsonProperty("foto_url")]
    public string FotoUrl { get; set; } = "";
}