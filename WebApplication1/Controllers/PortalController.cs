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
        public async Task<IActionResult> Expediente(int pageHistorial = 1, int pageTratamientos = 1, int pageCitas = 1, int pageSize = 5)
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

            // Historial Médico
            var historialTotal = await _context.HistorialMedico
                .Where(h => h.IdPaciente == paciente.IdPaciente)
                .CountAsync();

            var historialMedico = await _context.HistorialMedico
                .Include(h => h.Paciente)
                .Include(h => h.Tratamiento)
                .Where(h => h.IdPaciente == paciente.IdPaciente)
                .Skip((pageHistorial - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tratamientos
            var tratamientosQuery = _context.Tratamientos
                .Include(t => t.Servicio)
                .Where(t => _context.HistorialMedico
                    .Where(h => h.IdPaciente == paciente.IdPaciente)
                    .Select(h => h.IdTratamiento)
                    .Contains(t.IdTratamiento));

            var tratamientosTotal = await tratamientosQuery.CountAsync();

            var tratamientos = await tratamientosQuery
                .Skip((pageTratamientos - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Citas
            var citasTotal = await _context.Citas
                .Where(c => c.IdPaciente == paciente.IdPaciente)
                .CountAsync();

            var citas = await _context.Citas
                .Where(c => c.IdPaciente == paciente.IdPaciente)
                .Include(c => c.Empleado)
                .Skip((pageCitas - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Paginación
            ViewBag.TotalPagesHistorial = (int)Math.Ceiling((double)historialTotal / pageSize);
            ViewBag.TotalPagesTratamientos = (int)Math.Ceiling((double)tratamientosTotal / pageSize);
            ViewBag.TotalPagesCitas = (int)Math.Ceiling((double)citasTotal / pageSize);

            ViewBag.CurrentPageHistorial = pageHistorial;
            ViewBag.CurrentPageTratamientos = pageTratamientos;
            ViewBag.CurrentPageCitas = pageCitas;

            return View(new Tuple<PacientesModel, List<HistorialMedicoModel>, List<TratamientosModel>, List<CitasModel>>(
                paciente, historialMedico, tratamientos, citas
            ));
        }



    }
}
