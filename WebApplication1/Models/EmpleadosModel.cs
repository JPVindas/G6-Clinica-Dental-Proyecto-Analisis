using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;

namespace WebApplication1.Models
{
    public class EmpleadosModel
    {
        [Key]
        [Column("id_empleado")]
        public int IdEmpleado { get; set; }

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

        [Required]
        [StringLength(15)]
        public string Telefono { get; set; }

        [Required]
        [StringLength(100)]
        public string Especialidad { get; set; }

        [Required]
        [Range(0, 50)]
        public int Experiencia { get; set; } = 0;

        // 🔹 Nuevo campo para la imagen del odontólogo (Puede ser NULL)
        [StringLength(255)]
        public string? ImagenUrl { get; set; }

        // 🔹 Relación con Citas
        public virtual ICollection<CitasModel> Citas { get; set; } = new List<CitasModel>();

        // 🔹 Relación 1 a 1 con Usuario
        [ForeignKey("IdUsuario")]
        public int? IdUsuario { get; set; }
        public virtual UsuariosModel? Usuario { get; set; }


    }
}
