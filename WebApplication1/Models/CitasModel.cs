
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class CitasModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdPaciente { get; set; }

        [Required]
        public int IdEmpleado { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        [StringLength(255)]
        public string? Motivo { get; set; }

        [StringLength(255)]
        public string? Observaciones { get; set; }


        [StringLength(50)]
        [Required]
        public string Estado { get; set; } = "Pendiente";


        [ForeignKey("IdPaciente")]
        public PacientesModel Paciente { get; set; }

        [ForeignKey("IdEmpleado")]
        public EmpleadosModel Empleado { get; set; }



    }
}
