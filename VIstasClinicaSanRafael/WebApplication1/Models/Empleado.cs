using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VistasClinicaSanRafael.Models
{
    public class Empleado
    {
        [Key] // Define la clave primaria
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Generación automática del ID
        public int IdEmpleado { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellidos { get; set; }

        [Required]
        [StringLength(150)]
        public string Correo { get; set; }

        [StringLength(15)]
        public string Telefono { get; set; }

        [StringLength(100)]
        public string Especialidad { get; set; }

        public int? Experiencia { get; set; }

        // Relación con Citas
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();

        // Relación con Facturas
        public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    }
}
