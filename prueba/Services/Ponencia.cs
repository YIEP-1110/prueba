using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace prueba.Data;

public static class EstadoPonencia
{
    public const string Recibida = "recibida";
    public const string Aceptada = "aceptada";
    public const string Rechazada = "rechazada";

    public static readonly string[] Todos = { Recibida, Aceptada, Rechazada };

    public static string Etiqueta(string? e) => e switch
    {
        Recibida => "Recibida",
        Aceptada => "Aceptada",
        Rechazada => "Rechazada",
        _ => string.IsNullOrEmpty(e) ? "-" : e
    };

    public static string Badge(string? e) => e switch
    {
        Aceptada => "bg-success",
        Rechazada => "bg-danger",
        _ => "bg-warning text-dark"
    };
}

public class Ponencia
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // Partition key del contenedor "ponencias" => /id_ponente
    [JsonProperty("id_ponente")]
    public string IdPonente { get; set; } = "";

    [JsonProperty("titulo")]
    [Required(ErrorMessage = "El título es obligatorio"), StringLength(200)]
    public string Titulo { get; set; } = "";

    [JsonProperty("resumen")]
    [StringLength(2000)]
    public string Resumen { get; set; } = "";

    [JsonProperty("categoria")]
    public string Categoria { get; set; } = "";

    [JsonProperty("archivo_pdf_url")]
    public string ArchivoPdfUrl { get; set; } = "";

    [JsonProperty("estado")]
    public string Estado { get; set; } = EstadoPonencia.Recibida;

    [JsonProperty("observaciones")]
    public string Observaciones { get; set; } = "";

    [JsonProperty("fecha_registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    [JsonProperty("fecha_revision")]
    public DateTime? FechaRevision { get; set; }
}