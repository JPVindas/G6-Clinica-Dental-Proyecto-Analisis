using WebApplication1.Models;

namespace VistasClinicaSanRafael.Models
{
    public class Servicio
    {
        public int IdServicio { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Costo { get; set; }

        // Relaciones
        public ICollection<Tratamiento> Tratamientos { get; set; }
    }
}
