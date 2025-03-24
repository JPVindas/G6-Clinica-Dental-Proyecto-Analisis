using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("carrito")]
    public class CarritoModel
    {
        [Key]
        [Column("id_carrito")]
        public int IdCarrito { get; set; }

        [Required]
        [Column("id_paciente")]
        public int IdPaciente { get; set; }

        [Required]
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Required]
        [Column("estado")]
        [StringLength(8)] // "abierto" o "cerrado"
        public string Estado { get; set; }

        // Relación con la tabla de pacientes
        [ForeignKey("IdPaciente")]
        public virtual PacientesModel Paciente { get; set; }

        // Relación con CarritoItem (1 a muchos)
        public virtual ICollection<CarritoItemModel> Items { get; set; }

    }
}