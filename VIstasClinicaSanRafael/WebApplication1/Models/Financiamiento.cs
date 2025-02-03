namespace VistasClinicaSanRafael.Models
{
    public class Financiamiento
    {
        public int IdFinanciamiento { get; set; }
        public int IdPaciente { get; set; }
        public int IdTratamiento { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public decimal TasaInteres { get; set; }
        public int Cuotas { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFinal { get; set; }
        public string Estado { get; set; }

        // Relaciones
        public Paciente Paciente { get; set; }
        public Tratamiento Tratamiento { get; set; }
    }
}
