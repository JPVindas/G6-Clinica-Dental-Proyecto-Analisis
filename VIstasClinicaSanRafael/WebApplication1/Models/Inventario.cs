namespace VistasClinicaSanRafael.Models
{
    public class Inventario
    {
        public int IdInventario { get; set; }
        public int IdProducto { get; set; }
        public int CantidadActual { get; set; }
        public DateTime FechaActualizacion { get; set; }

        // Relaciones
        public Producto Producto { get; set; }
    }
}
