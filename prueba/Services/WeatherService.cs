using Microsoft.EntityFrameworkCore;
using prueba.Data;
using Microsoft.Extensions.Logging;

namespace prueba.Services
{
    public interface IWeatherService
    {
        Task<Actividad[]> GetActividadesAsync();
    }

    public class WeatherService : IWeatherService
    {
        private readonly SqlAppDbContext _sqlDb;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(SqlAppDbContext sqlDb, ILogger<WeatherService> logger)
        {
            _sqlDb = sqlDb;
            _logger = logger;
        }

        public async Task<Actividad[]> GetActividadesAsync()
        {
            try
            {
                // Always query the relational database for activities
                return await Microsoft.EntityFrameworkCore.RelationalQueryableExtensions
                    .FromSqlRaw(_sqlDb.Actividades, "SELECT * FROM dbo.Actividades")
                    .ToArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SELECT * FROM dbo.Actividades");
                throw;
            }
        }
    }
}
