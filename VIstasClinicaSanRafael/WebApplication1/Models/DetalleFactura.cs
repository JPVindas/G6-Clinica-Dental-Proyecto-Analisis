namespace VistasClinicaSanRafael.Models
{
    public class DetalleFactura
    {
        public int IdDetalleFactura { get; set; } // Clave primaria
        public int IdFactura { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }

        // Relaciones
        public Factura Factura { get; set; }
        public Producto Producto { get; set; }
    }
}
