using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace WebApplication1.Models
{
    [Table("descuento")]
    public class DescuentoModel
    {
        [Key]
        [Column("id_descuento")]
        public int IdDescuento { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [Column("codigo")]
        [StringLength(50)]
        public string Codigo { get; set; }

        // Campo real mapeado en BD
        [Column("porcentaje_descuento", TypeName = "decimal(5,2)")]
        public decimal PorcentajeDescuento { get; set; }

        // Campo auxiliar para binding desde la vista (permite coma)
        [NotMapped]
        [Required(ErrorMessage = "El porcentaje es obligatorio.")]
        public string PorcentajeDescuentoString { get; set; }

        [Required]
        [Column("estado")]
        public bool Estado { get; set; } = true;

        public void ParsePorcentaje()
        {
            if (decimal.TryParse(PorcentajeDescuentoString, NumberStyles.Any, new CultureInfo("es-CR"), out var result))
            {
                PorcentajeDescuento = result;
            }
            else
            {
                throw new FormatException("Formato inválido para el porcentaje.");
            }
        }
    }
}
