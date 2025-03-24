
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
        [Table("detalle_compra")]
        public class DetalleCompraModel
        {
            [Key]
            [Column("id_detalle_compra")]
            public int IdDetalleCompra { get; set; }

            [Required]
            [Column("id_compra")]
            public int IdCompra { get; set; }

            [Required]
            [Column("tipo")]
            [StringLength(8)] // "producto" o "servicio"
            public string Tipo { get; set; }

            [Column("id_producto")]
            public int? IdProducto { get; set; }

            [Column("id_servicio")]
            public int? IdServicio { get; set; }

            [Required]
            [Column("cantidad")]
            public int Cantidad { get; set; }

            [Required]
            [Column("precio_unitario", TypeName = "decimal(10,2)")]
            public decimal PrecioUnitario { get; set; }

            [Required]
            [Column("impuestos", TypeName = "decimal(10,2)")]
            public decimal Impuestos { get; set; }

            // La columna 'total' en la BD es generada (STORED).
            // En EF, normalmente podemos omitirlo o mapearlo como [NotMapped].
            [NotMapped]
            public decimal Total => (Cantidad * PrecioUnitario) + Impuestos;

            // Relación con Compra
            [ForeignKey("IdCompra")]
            public virtual CompraModel Compra { get; set; }

            // Opcionales: relación con Producto/Servicio
           [ForeignKey("IdProducto")]
            public virtual ProductosModel Producto { get; set; }
            [ForeignKey("IdServicio")]
             public virtual ServiciosModel Servicio { get; set; }
        }
    }
