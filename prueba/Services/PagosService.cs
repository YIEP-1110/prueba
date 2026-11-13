using Microsoft.Azure.Cosmos;
using prueba.Data;


namespace prueba.Services;

public interface IPagosService
{
    Task<Pago[]> GetPagosAsync();
    Task<Pago> RegistrarPagoAsync(Pago pago);
}

public class PagosService : IPagosService
{
    private readonly Container _container;

    public PagosService(CosmosClient client, IConfiguration config)
    {
        var database = config["CosmosDb:Database"]
            ?? throw new InvalidOperationException("Falta la configuración CosmosDb:Database.");
        var container = config["CosmosDb:ContainerPagos"]
            ?? throw new InvalidOperationException("Falta la configuración CosmosDb:ContainerPagos.");

        _container = client.GetContainer(database, container);
    }

    public async Task<Pago[]> GetPagosAsync()
    {
        var pagos = new List<Pago>();
        using var iterator = _container.GetItemQueryIterator<Pago>(
            new QueryDefinition("SELECT * FROM c"));

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            pagos.AddRange(response);
        }

        return pagos.ToArray();
    }

    public async Task<Pago> RegistrarPagoAsync(Pago pago)
    {
        pago.Id = Guid.NewGuid().ToString();

        var response = await _container.CreateItemAsync(
            pago,
            new PartitionKey(pago.Id));

        return response.Resource;
    }
}
