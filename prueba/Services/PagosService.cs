using Microsoft.Azure.Cosmos;
using prueba.Data;


namespace prueba.Services;

public interface  IPagosService
{
    Task<List<Pago>> GetAllAsync();
}

public class PagosService : IPagosService
{
    private readonly AppDbPagos _db;
    private readonly ILogger<PagosService> _logger;

    public PagosService(AppDbPagos db, ILogger<PagosService> logger)
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
