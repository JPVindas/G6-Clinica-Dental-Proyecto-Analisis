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

            // Filtrar por estado si se pasa como parámetro
            if (!string.IsNullOrEmpty(estado) && estado != "Todos")
            {
                financiamientosQuery = financiamientosQuery.Where(f => f.Estado == estado);
            }

            // Filtrar por nombre y apellido si se pasa el parámetro de búsqueda
            if (!string.IsNullOrEmpty(search))
            {
                var searchTerms = search.ToLower().Split(' '); // Convertir el término de búsqueda a minúsculas y dividir por espacio

                // Recuperar los financiamientos en memoria
                var financiamientosList = await financiamientosQuery.ToListAsync();

                // Filtrar en memoria, buscando cada término en el nombre o el apellido
                financiamientosList = financiamientosList
                    .Where(f => f.Paciente != null &&
                                searchTerms.All(term =>
                                    f.Paciente.Nombre.ToLower().Contains(term) ||
                                    f.Paciente.Apellidos.ToLower().Contains(term)))
                    .ToList();

                // Paginación después del filtrado
                var financiamientos = financiamientosList.ToPagedList(pageNumber, pageSize);

                // Pasar los valores de búsqueda y estado a la vista
                ViewBag.UserRole = rolID;
                ViewBag.UserId = userId;
                ViewBag.Search = search; // Pasar el valor de búsqueda
                ViewBag.EstadoSeleccionado = estado; // Pasar el estado seleccionado

                return View("Financiamiento", financiamientos);
            }
            else
            {
                // Si no hay término de búsqueda, realizar la paginación normal
                var financiamientosList = await financiamientosQuery.OrderBy(f => f.FechaInicio).ToListAsync();
                var financiamientos = financiamientosList.ToPagedList(pageNumber, pageSize);

                // Pasar los valores de búsqueda y estado a la vista
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
                .Include(f => f.Paciente) // Incluye los datos del paciente
                .FirstOrDefaultAsync(f => f.IdFinanciamiento == id);

            if (financiamiento == null)
            {
                return NotFound();
            }

            // Enviar nombre completo del paciente a la vista
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

            // Solo actualizamos los campos permitidos
            financiamientoExistente.MontoPagado = financiamiento.MontoPagado;
            financiamientoExistente.TasaInteres = financiamiento.TasaInteres;
            financiamientoExistente.Estado = financiamiento.Estado;

            // Recalculamos el saldo pendiente
            financiamientoExistente.SaldoPendiente = financiamientoExistente.MontoTotal - financiamientoExistente.MontoPagado;

            // Guardamos los cambios
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

