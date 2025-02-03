using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class PacientesModel
    {
        [Key]
        [Column("id_paciente")] // Coincide con el nombre de la base de datos
        public int IdPaciente { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres.")]
        public string Apellidos { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Correo { get; set; }

        [StringLength(15)]
        public string? Telefono { get; set; }

        [StringLength(255)]
        public string? Direccion { get; set; }

        [Required]
        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
