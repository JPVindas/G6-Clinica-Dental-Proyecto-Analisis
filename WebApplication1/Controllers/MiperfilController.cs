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

            ViewBag.IdPaciente = paciente.IdPaciente;

            return View(paciente);
        }

        // ✅ Formulario GET
        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.IdUsuario == userId);

            if (paciente == null)
            {
                TempData["ErrorMessage"] = "No se encontró tu perfil.";
                return RedirectToAction(nameof(MiPerfil));
            }

            return View(paciente);
        }

        // ✅ Guardar cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(PacientesModel model)
        {
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Hay errores en el formulario.";
                return View(model);
            }

            try
            {
                var pacienteExistente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.IdUsuario == userId);

                if (pacienteExistente == null)
                {
                    TempData["ErrorMessage"] = "No se encontró tu perfil.";
                    return RedirectToAction(nameof(MiPerfil));
                }

                // ✅ Actualizar campos permitidos
                pacienteExistente.Nombre = model.Nombre;
                pacienteExistente.Apellidos = model.Apellidos;
                pacienteExistente.Correo = model.Correo;
                pacienteExistente.Telefono = model.Telefono;
                pacienteExistente.Direccion = model.Direccion;

                _context.Update(pacienteExistente);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tu perfil ha sido actualizado correctamente.";
                return RedirectToAction(nameof(MiPerfil));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar perfil: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al actualizar tu perfil.";
                return View(model);
            }
        }
    }
}
