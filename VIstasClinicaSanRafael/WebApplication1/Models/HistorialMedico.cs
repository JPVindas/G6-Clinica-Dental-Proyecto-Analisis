namespace VistasClinicaSanRafael.Models
{
    public class HistorialMedico
    {
        public int IdHistorial { get; set; }
        public int IdPaciente { get; set; }
        public string Diagnostico { get; set; }
        public string Tratamientos { get; set; }
        public DateTime FechaActualizacion { get; set; }

        // Relaciones
        public Paciente Paciente { get; set; }
    }
}
