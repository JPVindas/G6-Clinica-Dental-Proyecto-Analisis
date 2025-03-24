using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace WebApplication1.Models
{
    [Table("detalle_factura")]
    public class DetalleFacturaModel
    {
        [Key]
        [Column("id_detallefactura")]
        public int IdDetalleFactura { get; set; }

        [Required]
        [Column("id_factura")]
        public int IdFactura { get; set; }

        [Required]
        [Column("tipo")]
        [StringLength(8)] // Puede ser "producto" o "servicio"
        public string Tipo { get; set; }

        [Column("id_producto")]
        public int? IdProducto { get; set; }

        [Column("id_servicio")]
        public int? IdServicio { get; set; }

        [Required]
        [Column("cantidad")]
        [DefaultValue(1)]
        public int Cantidad { get; set; }

        [Required]
        [Column("subtotal", TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column("impuestos", TypeName = "decimal(10,2)")]
        public decimal Impuestos { get; set; }

        [Required]
        [Column("total", TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        // ✅ Relación con la tabla de factura
        [ForeignKey("IdFactura")]
        [InverseProperty("DetallesFactura")]
        public FacturaModel Factura { get; set; }




        // ✅ Relaciones opcionales con Producto o Servicio
        [ForeignKey("IdProducto")]
        public virtual ProductosModel Producto { get; set; }

        [ForeignKey("IdServicio")]
        public virtual ServiciosModel Servicio { get; set; }
    }
}
