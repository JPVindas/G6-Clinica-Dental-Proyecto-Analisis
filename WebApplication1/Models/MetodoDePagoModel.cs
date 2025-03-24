using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("metodopago")]
    public class MetodoPagoModel
    {
        [Key]
        [Column("id_metodopago")]
        public int IdMetodoPago { get; set; }

        [Required]
        [Column("nombre")]
        [StringLength(100)]
        public string Nombre { get; set; }

        // Relación inversa opcional a Facturas si deseas:
        public virtual ICollection<FacturaModel> Facturas { get; set; }


    }
}