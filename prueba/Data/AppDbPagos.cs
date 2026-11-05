using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace prueba.Data
{
    public class AppDbPagos : DbPagos
    {
        public AppDbPagos(DbPagosOptions<AppDbPagos> options) : base(options)
        {
        }
        public DbSet<Pago> Pagos { get; set; } = null!;

    }
    [Table("Pagos")]
    public class Pago
    {
        [Key]
        [Column("Id_pago")]
        public int IdPago { get; set; }


        [Column("Id_participante")]
        public int IdParticipante { get; set; }

        [Column("monto")]
        public decimal Monto { get; set; }

        [Column("metodo_pago")]
        public string MetodoPago { get; set; } = null!;

        [Column("fecha_pago")]
        public DateOnly FechaPago { get; set; }

        [Column("estado")]
        public bool Estado { get; set; }

        [Column("comprobante")]
        public string Comprobante { get; set; } = null!;


    }
}
