using Microsoft.Azure.Cosmos;
using prueba.Data;

namespace prueba.Services;

public interface IParticipantesCrudService
{
    Task<List<Participante>> GetAllAsync();
    Task<Participante?> GetAsync(string id, string idUsuarios);
    Task CreateAsync(Participante p);
    Task UpdateAsync(Participante p);
    Task DeleteAsync(string id, string idUsuarios);
}

public class ParticipantesCrudService : IParticipantesCrudService
{
    private readonly Container _container;

    public ParticipantesCrudService(CosmosClient client, IConfiguration config)
    {
        var db = config["ConnectionStrings2:CosmosDb_Database"];
        var cont = config["ConnectionStrings2:CosmosDb_Container"];
        _container = client.GetContainer(db, cont);
    }

    public async Task<List<Participante>> GetAllAsync()
    {
        var iterator = _container.GetItemQueryIterator<Participante>(
            new QueryDefinition("SELECT * FROM c"));
        var lista = new List<Participante>();
        while (iterator.HasMoreResults)
        {
            var resp = await iterator.ReadNextAsync();
            lista.AddRange(resp);
        }
        return lista;
    }

    public async Task<Participante?> GetAsync(string id, string idUsuarios)
    {
        try
        {
            var resp = await _container.ReadItemAsync<Participante>(id, new PartitionKey(idUsuarios));
            return resp.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateAsync(Participante p) =>
        await _container.CreateItemAsync(p, new PartitionKey(p.IdUsuarios));

    public async Task UpdateAsync(Participante p) =>
        await _container.ReplaceItemAsync(p, p.Id, new PartitionKey(p.IdUsuarios));

    public async Task DeleteAsync(string id, string idUsuarios) =>
        await _container.DeleteItemAsync<Participante>(id, new PartitionKey(idUsuarios));
}