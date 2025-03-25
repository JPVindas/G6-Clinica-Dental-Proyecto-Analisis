using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Financiamiento")]
    public class FinanciamientoModel
    {
        [Key]
        [Column("id_financiamiento")]
        public int IdFinanciamiento { get; set; }

        [Required]
        [Column("id_paciente")]
        public int IdPaciente { get; set; }


        [Column("id_tratamiento")]
        public int? IdTratamiento { get; set; } // 🔹 Ahora permite NULL

        [Required]
        [Column("monto_total", TypeName = "decimal(10,2)")]
        public decimal MontoTotal { get; set; }

        [Column("monto_pagado", TypeName = "decimal(10,2)")]
        public decimal MontoPagado { get; set; } = 0m;

        [Column("saldo_pendiente", TypeName = "decimal(10,2)")]
        public decimal SaldoPendiente { get; private set; } // Se calcula en la BD.

        [Required]
        [Column("tasa_interes", TypeName = "decimal(5,2)")]
        public decimal TasaInteres { get; set; }

        [Required]
        [Column("cuotas")]
        public int Cuotas { get; set; }

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;

        [Column("fecha_final")]
        public DateTime? FechaFinal { get; set; }

        [Column("estado")]
        public string Estado { get; set; } // solo si existe la columna


        // Relaciones
        [ForeignKey("IdPaciente")]
        public virtual PacientesModel Paciente { get; set; }





        [ForeignKey("IdTratamiento")]
        public virtual TratamientosModel? Tratamiento { get; set; } // 🔹 Ahora permite NULL
    }
}
