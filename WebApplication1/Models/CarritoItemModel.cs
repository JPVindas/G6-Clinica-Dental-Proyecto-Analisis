using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("carrito_item")]
    public class CarritoItemModel
    {
        [Key]
        [Column("id_carrito_item")]
        public int IdCarritoItem { get; set; }

        [Required]
        [Column("id_carrito")]
        public int IdCarrito { get; set; }

        [Required]
        [Column("tipo")]
        [StringLength(8)] // "producto" o "servicio"
        public string Tipo { get; set; }

        [Column("id_producto")]
        public int? IdProducto { get; set; }

        [Column("id_servicio")]
        public int? IdServicio { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column("impuestos", TypeName = "decimal(10,2)")]
        public decimal Impuestos { get; set; }

        // La columna 'total' en la BD está generada. Se suele omitir o mapear como NotMapped
        [NotMapped]
        public decimal Total => (Cantidad * PrecioUnitario) + Impuestos;

        // Relación con Carrito
        [ForeignKey("IdCarrito")]
        public virtual CarritoModel Carrito { get; set; }

        // Opcionales: relación con Producto / Servicio
         [ForeignKey("IdProducto")]
         public virtual ProductosModel Producto { get; set; }
        [ForeignKey("IdServicio")]
         public virtual ServiciosModel Servicio { get; set; }
    }
}