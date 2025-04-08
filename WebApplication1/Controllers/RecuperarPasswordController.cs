using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using WebApplication1.DATA;
using WebApplication1.Models;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    public class RecuperarPasswordController : Controller
    {
        private readonly MinombredeconexionDbContext _context;
        private readonly EmailSettings _emailSettings;

        // Inyección de EmailSettings a través de IOptions<EmailSettings>
        public RecuperarPasswordController(MinombredeconexionDbContext context, IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _emailSettings = emailSettings.Value;
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
            // Buscar usuario por correo
            var usuario = _context.Usuarios.FirstOrDefault(u => u.CorreoElectronico == email);
            if (usuario == null)
            {
                ModelState.AddModelError("", "El correo ingresado no está registrado.");
                return View();
            }

            // Generar una nueva contraseña aleatoria y actualizar el usuario
            string nuevaContrasena = GenerarContraseñaAleatoria();
            string contrasenaEncriptada = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);

            usuario.Contrasena = contrasenaEncriptada;
            _context.SaveChanges();

            // Enviar correo con la nueva contraseña
            EnviarCorreoContrasena(email, nuevaContrasena);

            ViewBag.Mensaje = "Tu contraseña ha sido restablecida y enviada a tu correo.";
            return View();
        }

        // Método para generar una contraseña aleatoria
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
                using (var mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(_emailSettings.From, "Clínica Dental San Rafael");
                    mensaje.To.Add(email);
                    mensaje.Subject = "Recuperación de Contraseña";
                    mensaje.Body = $"Tu nueva contraseña es: {nuevaContrasena}";
                    mensaje.IsBodyHtml = false; // O true si deseas enviar HTML

                    using (var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
                    {
                        Port = int.Parse(_emailSettings.Port),
                        Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                        EnableSsl = true
                    })
                    {
                        smtpClient.Send(mensaje);
                    }
                }
            }
            catch (Exception ex)
            {
                // Guarda el error en TempData para pruebas o registra en tu log
                TempData["ErrorEmail"] = $"Error al enviar correo de recuperación: {ex.Message}";
                Console.WriteLine($"Error al enviar correo de recuperación: {ex.Message}");
            }
        }
    }
}
