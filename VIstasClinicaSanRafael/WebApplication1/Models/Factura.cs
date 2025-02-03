using WebApplication1.Models;

namespace VistasClinicaSanRafael.Models
{
    public class Factura
    {
        public int IdFactura { get; set; }
        public int IdPaciente { get; set; }
        public int IdEmpleado { get; set; }
        public int? IdFinanciamiento { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; }

        // Relaciones
        public Paciente Paciente { get; set; }
        public Empleado Empleado { get; set; }
        public Financiamiento Financiamiento { get; set; }
        public ICollection<DetalleFactura> DetallesFactura { get; set; }
    }
}
