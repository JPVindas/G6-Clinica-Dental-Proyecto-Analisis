// ComprasModel
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.Models
{
    [Table("compra")]
    public class CompraModel
    {
        [Key]
        [Column("id_compra")]
        public int IdCompra { get; set; }

        [Required]
        [Column("id_paciente")]
        public int IdPaciente { get; set; }

        [Required]
        [Column("fecha_compra")]
        public DateTime FechaCompra { get; set; }

        [Column("id_financiamiento")]
        public int? IdFinanciamiento { get; set; }

        [Required]
        [Column("estado")]
        [StringLength(10)]
        public string Estado { get; set; }

        [Required]
        [Column("monto_total", TypeName = "decimal(10,2)")]
        public decimal MontoTotal { get; set; }

        [ForeignKey("IdPaciente")]
        public PacientesModel Paciente { get; set; }

        [ForeignKey("IdFinanciamiento")]
        public virtual FinanciamientoModel Financiamiento { get; set; }

        public virtual ICollection<DetalleCompraModel> Detalles { get; set; }


        public virtual ICollection<FacturaModel> Facturas { get; set; } = new List<FacturaModel>();

    }

}