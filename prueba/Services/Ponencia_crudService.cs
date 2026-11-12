using Microsoft.Azure.Cosmos;
using prueba.Data;

namespace prueba.Services;

public interface IPonenciasCrudService
{
    Task<List<Ponencia>> GetAllAsync();
    Task<List<Ponencia>> GetByPonenteAsync(string idPonente);
    Task<Ponencia?> GetAsync(string id, string idPonente);

    Task RegistrarAsync(Ponencia p);          
    Task ActualizarDatosAsync(Ponencia p);    

    Task CambiarEstadoAsync(string id, string idPonente, string nuevoEstado, string? observaciones);

    Task DeleteAsync(string id, string idPonente);
}

public class PonenciasCrudService : IPonenciasCrudService
{
    private readonly Container _container;

    public PonenciasCrudService(CosmosClient client, IConfiguration config)
    {
        var db = config["ConnectionStrings2:CosmosDb_Database"];
        var cont = config["ConnectionStrings2:CosmosDb_Container_Ponencias"];
        _container = client.GetContainer(db, cont);
    }

    private async Task<List<Ponencia>> RunAsync(QueryDefinition q)
    {
        var it = _container.GetItemQueryIterator<Ponencia>(q);
        var lista = new List<Ponencia>();
        while (it.HasMoreResults) lista.AddRange(await it.ReadNextAsync());
        return lista;
    }

    public Task<List<Ponencia>> GetAllAsync() =>
        RunAsync(new QueryDefinition("SELECT * FROM c ORDER BY c.fecha_registro DESC"));

    public Task<List<Ponencia>> GetByPonenteAsync(string idPonente) =>
        RunAsync(new QueryDefinition("SELECT * FROM c WHERE c.id_ponente = @p")
            .WithParameter("@p", idPonente));

    public async Task<Ponencia?> GetAsync(string id, string idPonente)
    {
        try
        {
            return (await _container.ReadItemAsync<Ponencia>(id, new PartitionKey(idPonente))).Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task RegistrarAsync(Ponencia p)
    {
        if (string.IsNullOrWhiteSpace(p.IdPonente))
            throw new InvalidOperationException("La ponencia debe pertenecer a un ponente.");

        p.Estado = EstadoPonencia.Recibida;
        p.Observaciones = "";
        p.FechaRevision = null;
        p.FechaRegistro = DateTime.UtcNow;
        await _container.CreateItemAsync(p, new PartitionKey(p.IdPonente));
    }

    public async Task ActualizarDatosAsync(Ponencia p)
    {
        var actual = await GetAsync(p.Id, p.IdPonente)
            ?? throw new InvalidOperationException("La ponencia no existe.");

        if (actual.Estado != EstadoPonencia.Recibida)
            throw new InvalidOperationException(
                $"La ponencia ya fue {actual.Estado}; solo se puede editar mientras esté 'recibida'.");

        // Solo campos editables por el ponente; el estado se conserva.
        actual.Titulo = p.Titulo;
        actual.Resumen = p.Resumen;
        actual.Categoria = p.Categoria;
        actual.ArchivoPdfUrl = p.ArchivoPdfUrl;

        await _container.ReplaceItemAsync(actual, actual.Id, new PartitionKey(actual.IdPonente));
    }

    public async Task CambiarEstadoAsync(string id, string idPonente, string nuevoEstado, string? observaciones)
    {
        if (!EstadoPonencia.Todos.Contains(nuevoEstado))
            throw new InvalidOperationException("Estado no válido.");

        if (nuevoEstado == EstadoPonencia.Rechazada && string.IsNullOrWhiteSpace(observaciones))
            throw new InvalidOperationException("Indica el motivo del rechazo.");

        var actual = await GetAsync(id, idPonente)
            ?? throw new InvalidOperationException("La ponencia no existe.");

        actual.Estado = nuevoEstado;
        actual.Observaciones = observaciones?.Trim() ?? "";
        actual.FechaRevision = nuevoEstado == EstadoPonencia.Recibida ? null : DateTime.UtcNow;

        await _container.ReplaceItemAsync(actual, actual.Id, new PartitionKey(actual.IdPonente));
    }

    public async Task DeleteAsync(string id, string idPonente)
    {
        var actual = await GetAsync(id, idPonente);
        if (actual is null) return;

        if (actual.Estado == EstadoPonencia.Aceptada)
            throw new InvalidOperationException("No se puede eliminar una ponencia aceptada.");

        await _container.DeleteItemAsync<Ponencia>(id, new PartitionKey(idPonente));
    }
}