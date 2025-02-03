using WebApplication1.Models;

namespace VistasClinicaSanRafael.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }

        // Relaciones
        public Inventario Inventario { get; set; }
        public ICollection<DetalleFactura> DetallesFactura { get; set; }
    }
}
