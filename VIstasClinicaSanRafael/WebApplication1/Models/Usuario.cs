namespace VistasClinicaSanRafael.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int IdRol { get; set; }
        public int? IdEmpleado { get; set; }

        // Relaciones
        public Rol Rol { get; set; }
        public Empleado Empleado { get; set; }
    }
}
