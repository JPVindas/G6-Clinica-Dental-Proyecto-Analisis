using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class PacientesModel
    {
        [Key]
        [Column("id_paciente")]
        public int IdPaciente { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
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
        [Column("fecha_registro", TypeName = "DATE")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow.Date; // Usa UTC para evitar problemas de zona horaria

        //  Relación con UsuariosModel (Opcional, ya que no todos los pacientes tienen usuario)
        [ForeignKey("IdUsuario")]
        public int? IdUsuario { get; set; }
        public virtual UsuariosModel? Usuario { get; set; }

        // Relación con CitasModel (1 Paciente puede tener muchas citas)
        public virtual ICollection<CitasModel> Citas { get; set; } = new List<CitasModel>();
    }
}
