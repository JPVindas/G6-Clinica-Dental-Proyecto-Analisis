using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class TratamientosModel
    {
        [Key]
        [Column("id_tratamiento")]
        public int IdTratamiento { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "DECIMAL(10,2)")]
        public decimal Costo { get; set; }

        [Column("duracion_estimada")]
        [StringLength(50)]
        public string? DuracionEstimada { get; set; }

        [Required]
        [Column("id_servicio")]
        public int IdServicio { get; set; }

        [ForeignKey("IdServicio")]
        public virtual ServiciosModel? Servicio { get; set; }

        public virtual ICollection<HistorialMedicoModel> HistorialesMedicos { get; set; } = new List<HistorialMedicoModel>();
        public virtual ICollection<FinanciamientoModel> Financiamientos { get; set; } = new List<FinanciamientoModel>();

    }
}
