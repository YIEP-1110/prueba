using Newtonsoft.Json;

namespace prueba.Data;

public sealed class Usuario
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonProperty("correo")]
    public string Correo { get; set; } = "";

    [JsonProperty("idUsuario")]
    public string IdUsuario { get; set; } = "u" + Guid.NewGuid().ToString("N")[..8];

    [JsonProperty("password_hash")]
    public string PasswordHash { get; set; } = "";

    [JsonProperty("rol")]
    public RolUsuario Rol { get; set; } = new();
}

public sealed class RolUsuario
{
    [JsonProperty("id_roles")]
    public int IdRoles { get; set; }

    [JsonProperty("tipo")]
    public string Tipo { get; set; } = "participante";
}

public sealed record ResultadoAutenticacion(bool Correcta, Usuario? Usuario, bool ContraseñaHeredada = false);
