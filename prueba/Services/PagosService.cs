using Microsoft.EntityFrameworkCore;
using prueba.Data;
using System.Security.Cryptography.X509Certificates;

namespace prueba.Services
{
    public interface  IPagosService
    {
        Task<Pago[]> GetPagosAsync();
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
                return await _db.Pagos
                    .FromSqlRaw("SELECT * FROM dbo.Pagos")
                    .ToArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SELECT * FROM dbo.Pagos");
                throw;
            }
        }
    }
}
