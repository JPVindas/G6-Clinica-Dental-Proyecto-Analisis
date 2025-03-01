using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    [Table("Productos")] // Mapea a la tabla Productos en MySQL
    public class ProductosModel
    {
        [Key]
        [Column("id_producto")] // Mapea a la columna real en la BD
        public int Id { get; set; } // Ahora usamos Id en el código, pero se enlaza a id_producto

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        [StringLength(100)]
        public string Marca { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public int Stock { get; set; }

        [StringLength(500)]
        [Column("url_imagen")] // Mapea a la columna en la BD
        public string UrlImagen { get; set; } // Nuevo campo para la URL de la imagen

    }
}
