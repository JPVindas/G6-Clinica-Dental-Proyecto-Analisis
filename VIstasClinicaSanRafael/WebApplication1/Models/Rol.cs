using WebApplication1.Models;

namespace VistasClinicaSanRafael.Models
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; }

        // Relaciones
        public ICollection<Usuario> Usuarios { get; set; }
    }
}
