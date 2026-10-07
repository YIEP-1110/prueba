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
        private readonly AppDbContext _db;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(AppDbContext db, ILogger<WeatherService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Actividad[]> GetActividadesAsync()
        {
            try
            {
                return await _db.Actividades
                    .FromSqlRaw("SELECT * FROM dbo.Actividades")
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
