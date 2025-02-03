using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VistasClinicaSanRafael.Models
{
    public class Cita
    {
        [Key] // Define que IdCita es la clave primaria
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Autoincremental
        public int IdCita { get; set; }

        [Required]
        public int IdPaciente { get; set; }

        [Required]
        public int IdEmpleado { get; set; }

        [Required]
        public int IdTratamiento { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public TimeSpan Hora { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente";

        // Relaciones con otras entidades
        [ForeignKey("IdPaciente")]
        public Paciente Paciente { get; set; }

        [ForeignKey("IdEmpleado")]
        public Empleado Empleado { get; set; }

        [ForeignKey("IdTratamiento")]
        public Tratamiento Tratamiento { get; set; }
    }
}
