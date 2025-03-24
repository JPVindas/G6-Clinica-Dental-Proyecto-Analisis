
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{

    [Table("historial_compras")]
    public class HistorialComprasModel
    {
        [Key]
        [Column("id_historialcompra")]
        public int IdHistorialCompra { get; set; }

        [Required]
        [Column("id_paciente")]
        public int IdPaciente { get; set; }

        [Required]
        [Column("fecha_compra")]
        public DateTime FechaCompra { get; set; }

        [Column("id_financiamiento")]
        public int? IdFinanciamiento { get; set; }

        [Column("id_factura")]
        public int? IdFactura { get; set; }

        [Column("tipo")]
        [StringLength(50)]
        public string Tipo { get; set; }

        [Column("monto_total", TypeName = "decimal(10,2)")]
        public decimal? MontoTotal { get; set; }

        // Relación correcta con Paciente
        [ForeignKey(nameof(IdPaciente))]
        public virtual PacientesModel Paciente { get; set; }

        [ForeignKey(nameof(IdFinanciamiento))]
        public virtual FinanciamientoModel Financiamiento { get; set; }

        [ForeignKey(nameof(IdFactura))]
        public virtual FacturaModel Factura { get; set; }
    }


}