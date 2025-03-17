using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Carrito")]
    public class CarritoModel
    {
        [Key]
        [Column("id_carrito")]
        public int IdCarrito { get; set; }

        [Required]
        [Column("tipo")]
        public  string Tipo { get; set; }  // ✅ `required` obliga a asignar un valor


        [Column("id_producto")]
        public int? IdProducto { get; set; }

        [Column("id_servicio")]
        public int? IdServicio { get; set; }

        [Column("imagen")]
        public string? Imagen { get; set; }

        [Column("nombre")]
        public required string Nombre { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal? PrecioUnitario { get; set; }

        [Required]
        [Column("impuestos", TypeName = "decimal(10,2)")]
        public decimal? Impuestos { get; set; }

        [NotMapped] // Esto evita que Entity Framework intente guardar 'Total'
        public decimal Total => (Cantidad * (PrecioUnitario ?? 0m)) + (Impuestos ?? 0m);

        // 🔹 Relación con Productos
        [ForeignKey("IdProducto")]
        public virtual ProductosModel? Producto { get; set; }

        // 🔹 Relación con Servicios
        [ForeignKey("IdServicio")]
        public virtual ServiciosModel? Servicio { get; set; }
 

    }
}
