using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace prueba.Data;

public class Ponencia
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("id_ponente")]
    [Required(ErrorMessage = "Selecciona un ponente")]
    public string IdPonente { get; set; } = "";

    [JsonProperty("titulo")]
    [Required, StringLength(200)]
    public string Titulo { get; set; } = "";

    [JsonProperty("resumen")]
    [StringLength(2000)]
    public string Resumen { get; set; } = "";

    [JsonProperty("categoria")]
    public string Categoria { get; set; } = "";

    [JsonProperty("archivo_pdf_url")]
    public string ArchivoPdfUrl { get; set; } = "";

    [JsonProperty("estado")]
    public string Estado { get; set; } = "pendiente";
}