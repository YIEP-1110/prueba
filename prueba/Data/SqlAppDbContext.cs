using Microsoft.EntityFrameworkCore;

namespace prueba.Data
{
    public class SqlAppDbContext : AppDbContext
    {
        public SqlAppDbContext(DbContextOptions<SqlAppDbContext> options) : base(options)
        {
        }
    }
}
