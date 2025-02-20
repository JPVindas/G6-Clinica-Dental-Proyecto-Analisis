using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.DATA;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class MiPerfilController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public MiPerfilController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        // ✅ Vista del perfil del paciente
        public async Task<IActionResult> MiPerfil()
        {
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.IdUsuario == userId);

            if (paciente == null)
            {
                TempData["ErrorMessage"] = "No se encontró tu perfil.";
                return RedirectToAction("Index", "Home");
            }

            return View(paciente);
        }

        // ✅ Formulario para editar el perfil del paciente
        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.IdUsuario == userId);

            if (paciente == null)
            {
                TempData["ErrorMessage"] = "No se encontró tu perfil.";
                return RedirectToAction(nameof(MiPerfil)); // 🔹 Corrección aquí
            }

            return View(paciente);
        }

        // ✅ Guardar cambios en el perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(PacientesModel model)
        {
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.IdUsuario == userId);

            if (paciente == null)
            {
                TempData["ErrorMessage"] = "No se encontró tu perfil.";
                return RedirectToAction(nameof(MiPerfil)); // 🔹 Corrección aquí
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // ✅ Actualizar datos del paciente
                paciente.Nombre = model.Nombre;
                paciente.Apellidos = model.Apellidos;
                paciente.Telefono = model.Telefono;
                paciente.Direccion = model.Direccion;
                paciente.Correo = model.Correo;

                _context.Entry(paciente).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tu perfil ha sido actualizado correctamente.";
                return RedirectToAction(nameof(MiPerfil)); // 🔹 Corrección aquí
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al actualizar tu perfil.";
                Console.WriteLine($"❌ Error: {ex.Message}");
                return View(model);
            }
        }
    }
}
