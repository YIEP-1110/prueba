using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prueba.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Actividad> Actividades { get; set; } = null!;
    }

    // Entity mapping for dbo.Actividades. Properties mapped to actual column names.

    [Table("Actividades")]
    public class Actividad
    {
        [Key]
        [Column("Id_actividad")]
        public int IdActividad { get; set; }

        [Column("Id_sala")]
        public int? IdSala { get; set; }

        [Column("Id_ponencia")]
        public int? IdPonencia { get; set; }

        [Column("fecha")]
        public DateOnly Fecha { get; set; }

        [Column("hora_inicio")]
        public TimeOnly HoraInicio { get; set; }

        [Column("hora_fin")]
        public TimeOnly HoraFin { get; set; }

        [Column("tipo_actividad")]
        public string TipoActividad { get; set; } = null!;
    }
}
