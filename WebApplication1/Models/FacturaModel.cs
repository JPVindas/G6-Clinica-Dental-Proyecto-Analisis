using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.Models
{

    [Table("factura")]
    public class FacturaModel
    {
        [Key]
        [Column("id_factura")]
        public int IdFactura { get; set; }

        [Required]
        [Column("id_compra")]
        public int IdCompra { get; set; }

        [Required]
        [Column("nombre_cliente")]
        [StringLength(100)]
        public string NombreCliente { get; set; }

        [Required]
        [Column("apellidos_cliente")]
        [StringLength(100)]
        public string ApellidosCliente { get; set; }

        [Required]
        [Column("cedula_cliente")]
        [StringLength(20)]
        public string CedulaCliente { get; set; }

        [Required]
        [Column("correo_cliente")]
        [StringLength(150)]
        public string CorreoCliente { get; set; }

        [Column("id_metodopago")]
        public int? IdMetodoPago { get; set; }

        [Column("id_descuento")]
        public int? IdDescuento { get; set; }

        [Required]
        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("id_financiamiento")]
        public int? IdFinanciamiento { get; set; }

        // ForeignKey a COMPRA
        [ForeignKey("IdCompra")]
        public virtual CompraModel Compra { get; set; }



        // ForeignKey a METODOPAGO
        [ForeignKey("IdMetodoPago")]
        public virtual MetodoPagoModel MetodoPago { get; set; }

        // ForeignKey a DESCUENTO
        [ForeignKey("IdDescuento")]
        public virtual DescuentoModel Descuento { get; set; }

        // ForeignKey a FINANCIAMIENTO (opcional)
        [ForeignKey("IdFinanciamiento")]
        public virtual FinanciamientoModel Financiamiento { get; set; }

        // Relación con DETALLE_FACTURA


        [InverseProperty("Factura")]
        public virtual ICollection<DetalleFacturaModel> DetallesFactura { get; set; }

        // Propiedad calculada del total de la factura
        [NotMapped]
        public decimal MontoTotal
        {
            get
            {
                return DetallesFactura?.Sum(d => d.Total) ?? 0;
            }
        }

        }
}
