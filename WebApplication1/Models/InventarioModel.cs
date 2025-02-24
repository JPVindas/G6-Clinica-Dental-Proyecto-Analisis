using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Inventario")] // Mapea a la tabla Inventario en MySQL
    public class InventarioModel
    {
        [Key]
        [Column("id_inventario")]
        public int Id_Inventario { get; set; }

        [Required]
        public int Cantidad_Actual { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha_Actualizacion { get; set; } = DateTime.Now;

        [Required]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        // Relación con ProductosModel
        [ForeignKey("IdProducto")]
        public virtual ProductosModel Producto { get; set; }
    }
}
