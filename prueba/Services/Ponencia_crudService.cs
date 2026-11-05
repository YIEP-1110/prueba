using Microsoft.Azure.Cosmos;
using prueba.Data;

namespace prueba.Services;

public interface IPonenciasCrudService
{
    Task<List<Ponencia>> GetAllAsync();
    Task<List<Ponencia>> GetByPonenteAsync(string idPonente);
    Task<Ponencia?> GetAsync(string id, string idPonente);
    Task CreateAsync(Ponencia p);
    Task UpdateAsync(Ponencia p);
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
        RunAsync(new QueryDefinition("SELECT * FROM c"));

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

    public async Task CreateAsync(Ponencia p) =>
        await _container.CreateItemAsync(p, new PartitionKey(p.IdPonente));

    public async Task UpdateAsync(Ponencia p) =>
        await _container.ReplaceItemAsync(p, p.Id, new PartitionKey(p.IdPonente));

    public async Task DeleteAsync(string id, string idPonente) =>
        await _container.DeleteItemAsync<Ponencia>(id, new PartitionKey(idPonente));
}