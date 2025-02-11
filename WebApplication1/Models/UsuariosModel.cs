﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class UsuariosModel
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string CorreoElectronico { get; set; }

        [Required]
        public string Contrasena { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; }

        [Required]
        [StringLength(20)]
        public string Cedula { get; set; }

        [Required]
        [StringLength(20)]
        public string Telefono { get; set; }

        [Required]
        [Column("RolId")]
        public int RolId { get; set; }

        [ForeignKey("RolId")]
        public virtual RolesModel? Rol { get; set; }

        //  Relación 1 a 1 con Paciente
        public virtual PacientesModel? Paciente { get; set; }
    }
}
