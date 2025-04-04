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
        public async Task<IActionResult> Financiamiento(int? page, string search, string estado)
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

            // Inicializamos la consulta
            var financiamientosQuery = _context.Financiamientos
                .Include(f => f.Paciente)
                .ThenInclude(p => p.Usuario)
                .AsQueryable();

            if (rolID == 3) 
            {
                var pacienteId = await _context.Pacientes
                    .Where(p => p.IdUsuario == userId)
                    .Select(p => p.IdPaciente)
                    .FirstOrDefaultAsync();

                financiamientosQuery = financiamientosQuery.Where(f => f.IdPaciente == pacienteId);
            }

            if (!string.IsNullOrEmpty(estado) && estado != "Todos")
            {
                financiamientosQuery = financiamientosQuery.Where(f => f.Estado == estado);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var searchTerms = search.ToLower().Split(' ');

                var financiamientosList = await financiamientosQuery.ToListAsync();

                financiamientosList = financiamientosList
                    .Where(f => f.Paciente != null &&
                                searchTerms.All(term =>
                                    f.Paciente.Nombre.ToLower().Contains(term) ||
                                    f.Paciente.Apellidos.ToLower().Contains(term)))
                    .ToList();

                var financiamientos = financiamientosList.ToPagedList(pageNumber, pageSize);

                ViewBag.UserRole = rolID;
                ViewBag.UserId = userId;
                ViewBag.Search = search;
                ViewBag.EstadoSeleccionado = estado;

                return View("Financiamiento", financiamientos);
            }
            else
            {
                var financiamientosList = await financiamientosQuery.OrderBy(f => f.FechaInicio).ToListAsync();
                var financiamientos = financiamientosList.ToPagedList(pageNumber, pageSize);

               
                ViewBag.UserRole = rolID;
                ViewBag.UserId = userId;
                ViewBag.EstadoSeleccionado = estado;

                return View("Financiamiento", financiamientos);
            }
        }




        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var financiamiento = await _context.Financiamientos
                .Include(f => f.Paciente) 
                .FirstOrDefaultAsync(f => f.IdFinanciamiento == id);

            if (financiamiento == null)
            {
                return NotFound();
            }

            ViewBag.NombrePaciente = financiamiento.Paciente != null
                ? $"{financiamiento.Paciente.Nombre} {financiamiento.Paciente.Apellidos}"
                : "Paciente no encontrado";

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

            financiamientoExistente.MontoPagado = financiamiento.MontoPagado;
            financiamientoExistente.TasaInteres = financiamiento.TasaInteres;
            financiamientoExistente.Estado = financiamiento.Estado;

            financiamientoExistente.SaldoPendiente = financiamientoExistente.MontoTotal - financiamientoExistente.MontoPagado;

            _context.Entry(financiamientoExistente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "El financiamiento ha sido actualizado correctamente.";
            return RedirectToAction("Financiamiento");
        }

    }
}

