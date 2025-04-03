using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using WebApplication1.Models;
using WebApplication1.DATA;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class FinanciamientoController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public FinanciamientoController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Financiamiento(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 5;

            string userIdClaim = User.FindFirst("UserID")?.Value;
            string rolIdClaim = User.FindFirst("RolID")?.Value;

            int userId = 0, rolID = 3;
            int.TryParse(userIdClaim, out userId);
            int.TryParse(rolIdClaim, out rolID);

            if (userId == 0)
            {
                TempData["ErrorMessage"] = "No se pudo autenticar correctamente.";
                return RedirectToAction("Login", "Login");
            }

            var financiamientosQuery = _context.Financiamientos
                .Include(f => f.Paciente)  // Incluir la relación con Paciente
                .ThenInclude(p => p.Usuario)  // Incluir la relación con Usuario a través de Paciente
                .AsQueryable();

            if (rolID == 3) // Cliente solo ve sus propios financiamientos
            {
                var pacienteId = await _context.Pacientes
                    .Where(p => p.IdUsuario == userId)
                    .Select(p => p.IdPaciente)
                    .FirstOrDefaultAsync();

                financiamientosQuery = financiamientosQuery.Where(f => f.IdPaciente == pacienteId);
            }

            var financiamientosList = await financiamientosQuery.OrderBy(f => f.FechaInicio).ToListAsync();
            var financiamientos = financiamientosList.ToPagedList(pageNumber, pageSize);

            ViewBag.UserRole = rolID;
            ViewBag.UserId = userId;
            return View("Financiamiento", financiamientos);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var financiamiento = await _context.Financiamientos.FindAsync(id);
            if (financiamiento == null)
            {
                return NotFound();
            }
            return View(financiamiento);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarEdicion(FinanciamientoModel financiamiento)
        {
            if (financiamiento.IdFinanciamiento <= 0)
            {
                TempData["ErrorMessage"] = "El ID del financiamiento no es válido.";
                return RedirectToAction("Financiamiento");
            }

            var financiamientoExistente = await _context.Financiamientos.FindAsync(financiamiento.IdFinanciamiento);
            if (financiamientoExistente == null)
            {
                TempData["ErrorMessage"] = "No se encontró el financiamiento.";
                return RedirectToAction("Financiamiento");
            }

            financiamientoExistente.MontoTotal = financiamiento.MontoTotal;
            financiamientoExistente.MontoPagado = financiamiento.MontoPagado;
            financiamientoExistente.SaldoPendiente = financiamiento.MontoTotal - financiamiento.MontoPagado;

            _context.Entry(financiamientoExistente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "El financiamiento ha sido actualizado correctamente.";
            return RedirectToAction("Financiamiento");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var financiamiento = await _context.Financiamientos.FindAsync(id);
            if (financiamiento == null)
            {
                TempData["ErrorMessage"] = "No se encontró el financiamiento.";
                return RedirectToAction("Financiamiento");
            }

            _context.Financiamientos.Remove(financiamiento);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "El financiamiento ha sido eliminado correctamente.";
            return RedirectToAction("Financiamiento");
        }
    }
}

