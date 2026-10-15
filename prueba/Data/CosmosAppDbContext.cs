using Microsoft.EntityFrameworkCore;

namespace prueba.Data
{
    public class CosmosAppDbContext : AppDbContext
    {
        public CosmosAppDbContext(DbContextOptions<CosmosAppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map Actividad entity to the existing Cosmos container 'Participantes'
            modelBuilder.Entity<Actividad>().ToContainer("Participantes");

            // If your container uses a partition key, configure it here. Example for /Id_sala:
            //modelBuilder.Entity<Actividad>().HasPartitionKey(a => a.Id);


            base.OnModelCreating(modelBuilder);
        }
    }
}
