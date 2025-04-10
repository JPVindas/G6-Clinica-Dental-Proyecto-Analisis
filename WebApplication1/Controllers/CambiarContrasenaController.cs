using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using System;
using System.Threading.Tasks;
using BCrypt.Net;

namespace WebApplication1.Controllers
{
    [Authorize] // Aplica a cualquier usuario autenticado
    public class CambiarContrasenaController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public CambiarContrasenaController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string actual, string nueva, string confirmar)
        {
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");
            if (userId == 0)
            {
                TempData["ErrorMessage"] = "No se pudo identificar al usuario.";
                return RedirectToAction("Login", "Login");
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado.";
                return RedirectToAction("Login", "Login");
            }

            if (!BCrypt.Net.BCrypt.Verify(actual, usuario.Contrasena))
            {
                TempData["ErrorMessage"] = "La contraseña actual no es válida.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(nueva) || nueva.Length < 6)
            {
                TempData["ErrorMessage"] = "La nueva contraseña debe tener al menos 6 caracteres.";
                return RedirectToAction("Index");
            }

            if (nueva != confirmar)
            {
                TempData["ErrorMessage"] = "La nueva contraseña y la confirmación no coinciden.";
                return RedirectToAction("Index");
            }

            try
            {
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(nueva);
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al cambiar contraseña: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al actualizar la contraseña.";
                return RedirectToAction("Index");
            }
        }
    }
}
