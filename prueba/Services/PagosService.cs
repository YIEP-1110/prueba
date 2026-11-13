using Microsoft.EntityFrameworkCore;
using prueba.Data;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace prueba.Services;

public interface IPagosService
{
    Task<Pago[]> GetPagosAsync();
}

public class PagosService : IPagosService
{
    private readonly Container _container;

    //public PagosService(CosmosClient client, IConfiguration config)
    //{
    //    var db = config["ConnectionStrings2:CosmosDb_Database"];
    //    var cont = config["ConnectionStrings2:CosmosDb_Container_Pagos"];
    //    _container = client.GetContainer(db, cont);
    //}

    private readonly Pago _db;
    private readonly ILogger<PagosService> _logger;

    public PagosService(Pago db, ILogger<PagosService> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task<Pago[]> GetPagosAsync()
    {
        try
        {
            return await Microsoft.EntityFrameworkCore.RelationalQueryableExtensions
                .FromSqlRaw(_db.Pagos, "SELECT * FROM dbo.Pagos")
                .ToArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SELECT * FROM dbo.Pagos");
            throw;
        }
    }
}
