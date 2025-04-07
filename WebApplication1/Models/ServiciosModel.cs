using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Servicios")] // Mapea a la tabla Servicios en MySQL
    public class ServiciosModel
    {
        [Key]
        [Column("id_servicio")] // Mapea a la columna real en la BD
        public int Id { get; set; } // Ahora usamos Id en el código, pero se enlaza a id_servicio

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Costo { get; set; }

        [StringLength(500)]
        [Column("url_imagen")] // Mapea a la columna en la BD
        public string UrlImagen { get; set; } // Nuevo campo para la URL de la imagen

        [Column("estado")]
        public bool estado { get; set; } = true;

        [Required]
        [Column("porcentaje_iva", TypeName = "decimal(5,2)")]
        public decimal PorcentajeIVA { get; set; } = 4.00m; // Default para servicios de salud

        [Required]
        [Column("exento")]
        public bool Exento { get; set; } = false;





        public virtual ICollection<TratamientosModel> Tratamientos { get; set; } = new List<TratamientosModel>();
    }
}
