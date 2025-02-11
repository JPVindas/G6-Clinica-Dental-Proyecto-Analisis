using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class EmpleadosModel
    {
        [Key]
        [Column("id_empleado")] // Mapeo correcto con la base de datos
        public int IdEmpleado { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres.")]
        public string Apellidos { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres.")]
        public string Correo { get; set; }

        [Required]
        [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres.")]
        public string Telefono { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "La especialidad no puede exceder los 100 caracteres.")]
        public string Especialidad { get; set; }

        [Required]
        [Range(0, 50, ErrorMessage = "La experiencia debe ser entre 0 y 50 años.")]
        public int Experiencia { get; set; } = 0; // Valor por defecto si no se especifica

        // Relación 1:N con CitasModel
        public virtual ICollection<CitasModel> Citas { get; set; } = new List<CitasModel>();
    }
}
