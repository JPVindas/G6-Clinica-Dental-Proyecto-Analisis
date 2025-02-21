using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using WebApplication1.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using X.PagedList;
using X.PagedList.Extensions;

namespace WebApplication1.Controllers
{
    public class HistorialMedicoController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public HistorialMedicoController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        // 🔹 Mostrar lista de historiales médicos con paginación y filtro por paciente
        [HttpGet]
        [Authorize]
        public IActionResult Index(int? page, int? idPaciente)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var historiales = _context.HistorialMedico
                .Include(h => h.Paciente)
                .Include(h => h.Empleado)
                .Include(h => h.Tratamiento)
                .OrderByDescending(h => h.FechaActualizacion)
                .AsQueryable();

            if (idPaciente.HasValue)
            {
                historiales = historiales.Where(h => h.IdPaciente == idPaciente);
            }

            ViewBag.Pacientes = new SelectList(_context.Pacientes
                .Select(p => new { p.IdPaciente, NombreCompleto = p.Nombre + " " + p.Apellidos }),
                "IdPaciente", "NombreCompleto");

            return View(historiales.ToPagedList(pageNumber, pageSize));
        }

        // 🔹 Mostrar detalles de un historial médico
        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
            var historial = await _context.HistorialMedico
                .Include(h => h.Paciente)
                .Include(h => h.Empleado)
                .Include(h => h.Tratamiento)
                .FirstOrDefaultAsync(h => h.IdHistorial == id);

            if (historial == null)
            {
                return NotFound();
            }

            return View(historial);
        }

        // 🔹 Vista para crear un historial médico (Doctor autenticado)
        [HttpGet]
        [Authorize]
        public IActionResult Crear()
        {
            // Obtener usuario autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserID");
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para registrar un historial.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(userIdClaim.Value);

            // Buscar el doctor autenticado
            var empleado = _context.Empleados.FirstOrDefault(e => e.IdUsuario == userId);
            if (empleado == null)
            {
                TempData["ErrorMessage"] = "No se encontró tu perfil como doctor.";
                return RedirectToAction("Index");
            }

            // Cargar listas de pacientes y tratamientos
            ViewBag.Pacientes = new SelectList(_context.Pacientes, "IdPaciente", "Nombre");
            ViewBag.Tratamientos = new SelectList(_context.Tratamientos, "IdTratamiento", "Nombre");

            // Modelo prellenado con el doctor autenticado
            var nuevoHistorial = new HistorialMedicoModel
            {
                IdEmpleado = empleado.IdEmpleado, // Asigna automáticamente el doctor autenticado
                FechaActualizacion = DateTime.UtcNow
            };

            return View(nuevoHistorial);
        }

        // 🔹 Guardar nuevo historial médico
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Crear(HistorialMedicoModel historial)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Hay errores en el formulario.";
                ViewBag.Pacientes = new SelectList(_context.Pacientes, "IdPaciente", "Nombre", historial.IdPaciente);
                ViewBag.Tratamientos = new SelectList(_context.Tratamientos, "IdTratamiento", "Nombre", historial.IdTratamiento);
                return View(historial);
            }

            if (historial.IdTratamiento == 0)
            {
                TempData["ErrorMessage"] = "Debe seleccionar un tratamiento válido.";
                ViewBag.Pacientes = new SelectList(_context.Pacientes, "IdPaciente", "Nombre", historial.IdPaciente);
                ViewBag.Tratamientos = new SelectList(_context.Tratamientos, "IdTratamiento", "Nombre", historial.IdTratamiento);
                return View(historial);
            }

            historial.FechaActualizacion = DateTime.Now.Date;

            _context.HistorialMedico.Add(historial);

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Historial médico creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al guardar en la base de datos: " + ex.Message;
                return View(historial);
            }
        }


        // 🔹 Guardar cambios en historial médico
        [HttpPost]
        public async Task<IActionResult> Editar(HistorialMedicoModel historial)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(historial).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CargarViewBags(historial);
            return View(historial);
        }

        // 🔹 Confirmar eliminación de historial médico
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var historial = await _context.HistorialMedico
                .Include(h => h.Paciente)
                .Include(h => h.Empleado)
                .Include(h => h.Tratamiento)
                .FirstOrDefaultAsync(h => h.IdHistorial == id);

            if (historial == null)
            {
                return NotFound();
            }

            return View(historial);
        }

        // 🔹 Eliminar historial médico
        [HttpPost, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var historial = await _context.HistorialMedico.FindAsync(id);
            if (historial != null)
            {
                _context.HistorialMedico.Remove(historial);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 🔹 Método auxiliar para cargar listas en ViewBag
        private void CargarViewBags(HistorialMedicoModel historial = null)
        {
            ViewBag.Pacientes = new SelectList(_context.Pacientes, "IdPaciente", "Nombre", historial?.IdPaciente);
            ViewBag.Tratamientos = new SelectList(_context.Tratamientos, "IdTratamiento", "Nombre", historial?.IdTratamiento);
        }
    } 
}
