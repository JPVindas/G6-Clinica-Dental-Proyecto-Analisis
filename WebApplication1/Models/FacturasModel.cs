using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Facturas")]
    public class FacturasModel
    {
        [Key]
        [Column("id_factura")]
        public int IdFactura { get; set; }

        [Required]
        [Column("id_paciente")]
        public int IdPaciente { get; set; }

        [Column("id_empleado")]
        public int? IdEmpleado { get; set; } // Puede ser NULL

        [Column("id_financiamiento")]
        public int? IdFinanciamiento { get; set; } // Puede ser NULL

        [Required]
        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("total", TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Required]
        [Column("metodo_pago")]
        [StringLength(50)]
        public string MetodoPago { get; set; }

        // 🔹 Relación con Paciente (Obligatoria)
        [ForeignKey("IdPaciente")]
        public virtual PacientesModel Paciente { get; set; }

        // 🔹 Relación con Empleado (Opcional)
        [ForeignKey("IdEmpleado")]
        public virtual EmpleadosModel? Empleado { get; set; } // Agregado para evitar error

        // 🔹 Relación con Financiamiento (Opcional)
        [ForeignKey("IdFinanciamiento")]
        public virtual FinanciamientoModel? Financiamiento { get; set; } // Agregado para evitar error

        // 🔹 Relación con Detalles de Factura (1 a muchos)
        public virtual ICollection<FacturaDetalleModel> Detalles { get; set; } = new List<FacturaDetalleModel>();
    }
}
