using WebApplication1.Models;

namespace VistasClinicaSanRafael.Models
{
    public class Paciente
    {
        public int IdPaciente { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Relaciones
        public ICollection<Cita> Citas { get; set; }
        public ICollection<Factura> Facturas { get; set; }
        public ICollection<Financiamiento> Financiamientos { get; set; }
        public ICollection<HistorialMedico> HistorialesMedicos { get; set; }
    }
}
