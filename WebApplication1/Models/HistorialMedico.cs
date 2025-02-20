using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class HistorialMedicoModel
    {
        [Key]
        [Column("id_historial")]
        public int IdHistorial { get; set; }

        [Required]
        [Column("id_paciente")]
        public int IdPaciente { get; set; }

        [ForeignKey("IdPaciente")]
        public virtual PacientesModel? Paciente { get; set; }

        [Required]
        [Column("id_empleado")]
        public int IdEmpleado { get; set; }

        [ForeignKey("IdEmpleado")]
        public virtual EmpleadosModel? Empleado { get; set; }

        [Required]
        [StringLength(255)]
        public string Diagnostico { get; set; }

        [Required]
        [Column("id_tratamiento")]
        public int IdTratamiento { get; set; }  // 🔹 Clave foránea de tratamiento

        [ForeignKey("IdTratamiento")]
        public virtual TratamientosModel? Tratamiento { get; set; } // 🔹 Relación con TratamientosModel

        [Column("fecha_actualizacion")]
        public DateTime FechaActualizacion { get; set; }
    }
}
