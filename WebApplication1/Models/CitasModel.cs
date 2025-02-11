using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class CitasModel
    {
        [Key]
        [Column(Order = 1)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Paciente")]
        [Column(Order = 2)]
        public int IdPaciente { get; set; }

        [Required]
        [ForeignKey("Empleado")]
        [Column(Order = 3)]
        public int IdEmpleado { get; set; }

        [Required]
        [Column(Order = 4)]
        public DateTime FechaHora { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(255, ErrorMessage = "El motivo no puede superar los 255 caracteres.")]
        [Column(Order = 5)]
        public string Motivo { get; set; }

        [StringLength(255)]
        [Column(Order = 6)]
        public string? Observaciones { get; set; }

        [Required]
        [StringLength(50)]
        [Column(Order = 7)]
        public string Estado { get; set; } = "Pendiente";

        // Relación con PacientesModel (Cada cita tiene un paciente asociado)
        public virtual PacientesModel? Paciente { get; set; }

        // Relación con EmpleadosModel (Cada cita tiene un empleado asignado)
        public virtual EmpleadosModel? Empleado { get; set; }
    }
}
