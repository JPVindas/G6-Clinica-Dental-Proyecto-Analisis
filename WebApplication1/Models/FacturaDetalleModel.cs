using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;


namespace WebApplication1.Models
{
    [Table("Detalles_Factura")]
    public class FacturaDetalleModel
    {
        [Key]
        [Column("id_detalle")]
        public int IdDetalle { get; set; }

        [Required]
        [Column("id_factura")]
        public int IdFactura { get; set; }

        [Required]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("subtotal", TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [ForeignKey("IdFactura")]
        public virtual FacturasModel Factura { get; set; }

        [ForeignKey("IdProducto")]
        public virtual ProductosModel Producto { get; set; }
    }

}
