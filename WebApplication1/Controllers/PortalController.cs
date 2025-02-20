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
    public class PortalController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public PortalController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        // ✅ Mostrar el expediente del paciente autenticado
        // ✅ Mostrar el expediente del paciente autenticado
        public async Task<IActionResult> Expediente()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserID");
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para acceder a tu expediente.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(userIdClaim.Value);

            var usuario = await _context.Usuarios
                .Include(u => u.Paciente)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null || usuario.Paciente == null)
            {
                TempData["ErrorMessage"] = "No se encontró tu perfil como paciente.";
                return RedirectToAction("Index", "Home");
            }

            var paciente = usuario.Paciente;

            var historialMedico = await _context.HistorialMedico
                .Include(h => h.Paciente)
                .Include(h => h.Tratamiento) // 🔹 Asegurar la relación con Tratamiento
                .Where(h => h.IdPaciente == paciente.IdPaciente)
                .ToListAsync();

            var tratamientos = await _context.Tratamientos
                .Include(t => t.Servicio)
                .Where(t => historialMedico.Select(h => h.IdTratamiento).Contains(t.IdTratamiento)) // 🔹 Relación con historial
                .ToListAsync();

            var citas = await _context.Citas
                .Where(c => c.IdPaciente == paciente.IdPaciente)
                .Include(c => c.Empleado)
                .ToListAsync();

            return View(new Tuple<PacientesModel, List<HistorialMedicoModel>, List<TratamientosModel>, List<CitasModel>>(
                paciente, historialMedico, tratamientos, citas
            ));
        }


    }
}
