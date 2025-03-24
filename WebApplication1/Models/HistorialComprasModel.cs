
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

        // Relación con Pacientes
        [ForeignKey("IdPaciente")]
        public PacientesModel Paciente { get; set; }

        // Relación con Financiamiento
        [ForeignKey("IdFinanciamiento")]
        public virtual FinanciamientoModel Financiamiento { get; set; }

        // Relación con Factura
        [ForeignKey("IdFactura")]
        public virtual FacturaModel Factura { get; set; }
    }
}