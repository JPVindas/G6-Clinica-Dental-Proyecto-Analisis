using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VistasClinicaSanRafael.Data;
using VistasClinicaSanRafael.Models;

namespace VistasClinicaSanRafael.Controllers
{
    public class CitasController : Controller
    {
        private readonly ClinicaContext _context;

        public CitasController(ClinicaContext context)
        {
            _context = context;
        }

        // GET: Citas
        public async Task<IActionResult> Index(string startDate, string endDate)
        {
            var citasQuery = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .Include(c => c.Tratamiento)
                .AsQueryable();

            // Filtrar por rango de fechas
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                if (DateTime.TryParse(startDate, out var start) && DateTime.TryParse(endDate, out var end))
                {
                    citasQuery = citasQuery.Where(c => c.Fecha >= start && c.Fecha <= end);
                }
            }

            var citas = await citasQuery.ToListAsync();
            return View(citas);
        }

        // GET: Citas/Create
        public IActionResult Create()
        {
            try
            {
                var pacientes = _context.Pacientes.ToList();
                var empleados = _context.Empleados.ToList();
                var tratamientos = _context.Tratamientos.ToList();

                if (pacientes.Any() && empleados.Any() && tratamientos.Any())
                {
                    ViewData["Pacientes"] = new SelectList(pacientes, "IdPaciente", "NombreCompleto");
                    ViewData["Empleados"] = new SelectList(empleados, "IdEmpleado", "NombreCompleto");
                    ViewData["Tratamientos"] = new SelectList(tratamientos, "IdTratamiento", "Nombre");
                }
                else
                {
                    ModelState.AddModelError("", "No se encontraron datos para llenar el formulario.");
                }

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al cargar datos: " + ex.Message);
                return View();
            }
        }

        // POST: Citas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPaciente,IdEmpleado,IdTratamiento,Fecha,Hora,Estado")] Cita cita)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(cita);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar la cita: " + ex.Message);
                }
            }

            ViewData["Pacientes"] = new SelectList(_context.Pacientes, "IdPaciente", "NombreCompleto", cita.IdPaciente);
            ViewData["Empleados"] = new SelectList(_context.Empleados, "IdEmpleado", "NombreCompleto", cita.IdEmpleado);
            ViewData["Tratamientos"] = new SelectList(_context.Tratamientos, "IdTratamiento", "Nombre", cita.IdTratamiento);

            return View(cita);
        }
    


        // GET: Citas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            LoadDropdownData();
            return View(cita);
        }

        // POST: Citas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCita,IdPaciente,IdEmpleado,IdTratamiento,Fecha,Hora,Estado")] Cita cita)
        {
            if (id != cita.IdCita) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cita);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CitaExists(cita.IdCita))
                        return NotFound();
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar la cita: " + ex.Message);
                }
            }

            LoadDropdownData();
            return View(cita);
        }

        // GET: Citas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .Include(c => c.Tratamiento)
                .FirstOrDefaultAsync(m => m.IdCita == id);

            if (cita == null) return NotFound();

            return View(cita);
        }

        // POST: Citas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita != null)
            {
                try
                {
                    _context.Citas.Remove(cita);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al eliminar la cita: " + ex.Message);
                    return View(cita);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // Método para cargar las listas desplegables
        private void LoadDropdownData()
        {
            ViewBag.Pacientes = new SelectList(_context.Pacientes, "IdPaciente", "NombreCompleto");
            ViewBag.Empleados = new SelectList(_context.Empleados, "IdEmpleado", "NombreCompleto");
            ViewBag.Tratamientos = new SelectList(_context.Tratamientos, "IdTratamiento", "Nombre");
        }

        private bool CitaExists(int id)
        {
            return _context.Citas.Any(e => e.IdCita == id);
        }
    }
}
