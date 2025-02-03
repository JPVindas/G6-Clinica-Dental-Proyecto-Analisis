using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DATA;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RecuperarPasswordController : Controller
    {
        private readonly MinombredeconexionDbContext _context;
        private readonly EmailSettings _emailSettings;

        // Inyectamos el servicio IOptions<EmailSettings> para acceder a la configuración
        public RecuperarPasswordController(MinombredeconexionDbContext context, IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _emailSettings = emailSettings.Value; // Accedemos a la configuración de correo
        }

        // Acción GET para mostrar el formulario de recuperación de contraseña
        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        // Acción POST para procesar la recuperación de contraseña
        [HttpPost]
        public IActionResult RecuperarPassword(string email)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.CorreoElectronico == email);

            if (usuario == null)
            {
                ModelState.AddModelError("", "El correo ingresado no está registrado.");
                return View();
            }

            // Lógica para generar y cambiar la contraseña
            string nuevaContrasena = GenerarContraseñaAleatoria();
            string contrasenaEncriptada = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);

            usuario.Contrasena = contrasenaEncriptada;
            _context.SaveChanges();

            // Enviar correo con la nueva contraseña
            EnviarCorreoContrasena(email, nuevaContrasena);

            ViewBag.Mensaje = "Tu contraseña ha sido restablecida y enviada a tu correo.";
            return View();
        }

        // Método para generar una nueva contraseña aleatoria
        private string GenerarContraseñaAleatoria()
        {
            var random = new Random();
            const string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var longitud = 8;
            var nuevaContrasena = new string(Enumerable.Repeat(caracteres, longitud)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return nuevaContrasena;
        }

        // Método para enviar el correo con la nueva contraseña
        private void EnviarCorreoContrasena(string email, string nuevaContrasena)
        {
            try
            {
                var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
                {
                    Port = int.Parse(_emailSettings.Port),
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = true,
                };

                smtpClient.Send(_emailSettings.From, email, "Recuperación de Contraseña", $"Tu nueva contraseña es: {nuevaContrasena}");
            }
            catch (Exception ex)
            {
                // Manejar el error de envío de correo
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
            }
        }
    }
}