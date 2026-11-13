using System.Security.Cryptography;
using Microsoft.Azure.Cosmos;
using prueba.Data;

namespace prueba.Services;

public interface IUsuariosService
{
    Task<Usuario?> BuscarPorCorreoAsync(string correo);
    Task<ResultadoAutenticacion> AutenticarAsync(string correo, string contraseña);
    Task<(bool Creado, string? Error)> RegistrarParticipanteAsync(string correo, string contraseña);
}

public sealed class UsuariosService : IUsuariosService
{
    private const int Iteraciones = 120_000;
    private const int TamañoSal = 16;
    private const int TamañoHash = 32;
    private readonly Container _container;

    public UsuariosService(CosmosClient client, IConfiguration config)
    {
        var database = config["ConnectionStrings2:CosmosDb_Database"];
        var container = config["ConnectionStrings2:CosmosDb_Container_Usuarios"];

        if (string.IsNullOrWhiteSpace(database) || string.IsNullOrWhiteSpace(container))
            throw new InvalidOperationException(
                "Faltan ConnectionStrings2:CosmosDb_Database o ConnectionStrings2:CosmosDb_Container_Usuarios.");

        _container = client.GetContainer(database, container);
    }

    public async Task<Usuario?> BuscarPorCorreoAsync(string correo)
    {
        correo = NormalizarCorreo(correo);
        var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE LOWER(c.correo) = @correo")
            .WithParameter("@correo", correo);
        var iterator = _container.GetItemQueryIterator<Usuario>(query, requestOptions: new QueryRequestOptions
        {
            MaxItemCount = 1,
            MaxConcurrency = -1
        });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var usuario = response.Resource.FirstOrDefault();
            if (usuario is not null)
                return usuario;
        }

        return null;
    }

    public async Task<ResultadoAutenticacion> AutenticarAsync(string correo, string contraseña)
    {
        var usuario = await BuscarPorCorreoAsync(correo);
        if (usuario is null || !VerificarContraseña(contraseña, usuario.PasswordHash, out var heredada))
            return new(false, null);

        if (heredada)
        {
            usuario.PasswordHash = CrearHash(contraseña);
            await _container.ReplaceItemAsync(usuario, usuario.Id, new PartitionKey(usuario.Correo));
        }

        return new(true, usuario, heredada);
    }

    public async Task<(bool Creado, string? Error)> RegistrarParticipanteAsync(string correo, string contraseña)
    {
        correo = NormalizarCorreo(correo);
        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@'))
            return (false, "Introduce un correo electrónico válido.");
        if (contraseña.Length < 8)
            return (false, "La contraseña debe tener al menos 8 caracteres.");
        if (await BuscarPorCorreoAsync(correo) is not null)
            return (false, "Ya existe un usuario con ese correo.");

        var usuario = new Usuario
        {
            Correo = correo,
            PasswordHash = CrearHash(contraseña),
            Rol = new RolUsuario { IdRoles = 2, Tipo = "participante" }
        };

        await _container.CreateItemAsync(usuario, new PartitionKey(usuario.Correo));
        return (true, null);
    }

    private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();

    private static string CrearHash(string contraseña)
    {
        var sal = RandomNumberGenerator.GetBytes(TamañoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, Iteraciones, HashAlgorithmName.SHA256, TamañoHash);
        return $"v1${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    private static bool VerificarContraseña(string contraseña, string almacenada, out bool heredada)
    {
        heredada = false;
        var partes = almacenada.Split('$');
        if (partes.Length == 4 && partes[0] == "v1" && int.TryParse(partes[1], out var iteraciones))
        {
            try
            {
                var sal = Convert.FromBase64String(partes[2]);
                var esperado = Convert.FromBase64String(partes[3]);
                var calculado = Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
                return CryptographicOperations.FixedTimeEquals(calculado, esperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        heredada = almacenada.Length > 0;
        return heredada && CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(almacenada),
            System.Text.Encoding.UTF8.GetBytes(contraseña));
    }
}
