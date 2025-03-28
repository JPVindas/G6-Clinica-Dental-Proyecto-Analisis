﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;


namespace WebApplication1.Models
{
    [Table("Pacientes")]
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
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow.Date;

        // Relación con UsuariosModel (1 Paciente puede tener 1 Usuario)
        [ForeignKey("IdUsuario")]
        public int? IdUsuario { get; set; }
        public virtual UsuariosModel? Usuario { get; set; }

        // Relación con Citas (1 Paciente puede tener muchas citas)
        public virtual ICollection<CitasModel> Citas { get; set; } = new List<CitasModel>();

        public virtual ICollection<HistorialMedicoModel> HistorialesMedicos { get; set; } = new List<HistorialMedicoModel>();



        // Relación con Compras (1 Paciente puede tener múltiples compras)
        public virtual ICollection<CompraModel> Compras { get; set; } = new List<CompraModel>();

        // Relación con Financiamientos (1 Paciente puede tener múltiples)
        public virtual ICollection<FinanciamientoModel> Financiamientos { get; set; } = new List<FinanciamientoModel>();

        public virtual ICollection<HistorialComprasModel> HistorialCompras { get; set; } = new List<HistorialComprasModel>();





    }
}
