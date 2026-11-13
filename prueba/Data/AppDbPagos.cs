using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace prueba.Data;

public class Pago
{
    [JsonProperty("id")]
    [Required]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("id_pago")]
    public int IdPago { get; set; }

    [JsonProperty("id_participante")]
    [Range(1, int.MaxValue)]
    public int IdParticipante { get; set; }

    [JsonProperty("monto")]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Monto { get; set; }

    [JsonProperty("metodo_pago")]
    [Required]
    public string MetodoPago { get; set; } = "";

    [JsonProperty("fecha_pago")]
    public DateOnly FechaPago { get; set; }

    [JsonProperty("estado")]
    public bool Estado { get; set; }

    [JsonProperty("comprobante")]
    [Required]
    public string Comprobante { get; set; } = "";
}
    