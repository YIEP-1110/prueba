using Newtonsoft.Json;

namespace prueba.Data;

public class Pago
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("id_pago")]
    public int IdPago { get; set; }

    [JsonProperty("id_participante")]
    public int IdParticipante { get; set; }

    [JsonProperty("monto")]
    public decimal Monto { get; set; }

    [JsonProperty("metodo_pago")]
    public string MetodoPago { get; set; } = "";

    [JsonProperty("fecha_pago")]
    public DateOnly FechaPago { get; set; }

    [JsonProperty("estado")]
    public bool Estado { get; set; }

    [JsonProperty("comprobante")]
    public string Comprobante { get; set; } = "";
}
    