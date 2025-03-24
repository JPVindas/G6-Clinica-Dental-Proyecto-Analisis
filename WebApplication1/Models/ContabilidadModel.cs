
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{

        [Table("contabilidad")]
        public class ContabilidadModel
        {
            [Key]
            [Column("id_contabilidad")]
            public int IdContabilidad { get; set; }

            [Required]
            [Column("id_factura")]
            public int IdFactura { get; set; }

            [Required]
            [Column("ingreso", TypeName = "decimal(10,2)")]
            public decimal Ingreso { get; set; }

            [Required]
            [Column("gasto", TypeName = "decimal(10,2)")]
            public decimal Gasto { get; set; }

            [Required]
            [Column("saldo", TypeName = "decimal(10,2)")]
            public decimal Saldo { get; set; }

            [Required]
            [Column("fecha_reporte")]
            public DateTime FechaReporte { get; set; }

            // Relación con Factura
            [ForeignKey("IdFactura")]
            public virtual FacturaModel Factura { get; set; }
        }
    }