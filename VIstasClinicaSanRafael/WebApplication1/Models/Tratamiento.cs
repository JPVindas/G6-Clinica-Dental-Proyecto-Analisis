namespace VistasClinicaSanRafael.Models
{
    public class Tratamiento
    {
        public int IdTratamiento { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Costo { get; set; }
        public string DuracionEstimada { get; set; }
        public int IdServicio { get; set; }

        // Relaciones
        public Servicio Servicio { get; set; }
        public ICollection<Cita> Citas { get; set; }
        public ICollection<Financiamiento> Financiamientos { get; set; }
    }
}
