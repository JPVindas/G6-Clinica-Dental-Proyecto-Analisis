using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using WebApplication1.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace WebApplication1.Controllers
{
    public class CitasController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public CitasController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Citas()
        {
            var citas = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .ToListAsync();
            return View(citas);
        }

        public IActionResult CrearCita()
        {
            ViewBag.Pacientes = new SelectList(_context.Pacientes.Select(p => new { p.IdPaciente, NombreCompleto = p.Nombre + " " + p.Apellidos }), "IdPaciente", "NombreCompleto");
            ViewBag.Empleados = new SelectList(_context.Empleados.Select(e => new { e.IdEmpleado, NombreCompleto = e.Nombre + " " + e.Apellidos }), "IdEmpleado", "NombreCompleto");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearCita(CitasModel cita)
        {
            Console.WriteLine($"Intentando guardar cita: IdPaciente={cita.IdPaciente}, IdEmpleado={cita.IdEmpleado}, FechaHora={cita.FechaHora}, Estado={cita.Estado}");

            if (cita.IdPaciente <= 0 || cita.IdEmpleado <= 0)
            {
                TempData["ErrorMessage"] = "Debe seleccionar un paciente y un doctor válidos.";
                return RedirectToAction("CrearCita");
            }

            var pacienteExiste = await _context.Pacientes.FindAsync(cita.IdPaciente);
            var empleadoExiste = await _context.Empleados.FindAsync(cita.IdEmpleado);

            if (pacienteExiste == null)
            {
                ModelState.AddModelError("IdPaciente", "El paciente seleccionado no existe.");
            }
            if (empleadoExiste == null)
            {
                ModelState.AddModelError("IdEmpleado", "El doctor seleccionado no existe.");
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Error en ModelState: Paciente o Doctor no existen.");
                ViewBag.Pacientes = new SelectList(_context.Pacientes.Select(p => new { p.IdPaciente, NombreCompleto = p.Nombre + " " + p.Apellidos }), "IdPaciente", "NombreCompleto", cita.IdPaciente);
                ViewBag.Empleados = new SelectList(_context.Empleados.Select(e => new { e.IdEmpleado, NombreCompleto = e.Nombre + " " + e.Apellidos }), "IdEmpleado", "NombreCompleto", cita.IdEmpleado);
                return View(cita);
            }

            return await SaveCita(cita);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCita(CitasModel cita)
        {
            try
            {
                if (cita.FechaHora == DateTime.MinValue)
                {
                    TempData["ErrorMessage"] = "Debe seleccionar una fecha y hora válida.";
                    return RedirectToAction("CrearCita");
                }

                if (string.IsNullOrEmpty(cita.Estado))
                {
                    cita.Estado = "Pendiente";
                }

                _context.Citas.Add(cita);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Cita guardada en la base de datos.");

                TempData["SuccessMessage"] = "La cita ha sido creada exitosamente.";
                return RedirectToAction(nameof(Citas));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar la cita: {ex.Message}");
                TempData["ErrorMessage"] = "Hubo un error al guardar la cita.";
                ModelState.AddModelError("", "Error al guardar en la base de datos: " + ex.Message);
            }

            return RedirectToAction("CrearCita");
        }

        [HttpGet] // 👈 Especificamos que este método responde a solicitudes GET
        public async Task<IActionResult> Citas(DateTime? startDate, DateTime? endDate)
        {
            var citasQuery = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .AsQueryable();

            // Aplicar filtro por fecha si se ingresan valores
            if (startDate.HasValue)
            {
                citasQuery = citasQuery.Where(c => c.FechaHora.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                citasQuery = citasQuery.Where(c => c.FechaHora.Date <= endDate.Value.Date);
            }

            var citas = await citasQuery.OrderBy(c => c.FechaHora).ToListAsync();

            // Pasar valores a la vista para mantener las fechas seleccionadas
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(citas);
        }


        [HttpGet]
        public async Task<IActionResult> EditarCita(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

            // Cargar listas desplegables
            ViewBag.Pacientes = new SelectList(await _context.Pacientes
                .Select(p => new { p.IdPaciente, NombreCompleto = p.Nombre + " " + p.Apellidos })
                .ToListAsync(), "IdPaciente", "NombreCompleto", cita.IdPaciente);

            ViewBag.Empleados = new SelectList(await _context.Empleados
                .Select(e => new { e.IdEmpleado, NombreCompleto = e.Nombre + " " + e.Apellidos })
                .ToListAsync(), "IdEmpleado", "NombreCompleto", cita.IdEmpleado);

            return View(cita);
        }
        [HttpPost]
        public async Task<IActionResult> GuardarEdicion(CitasModel cita)
        {
            Console.WriteLine("==== 📌 DATOS RECIBIDOS DESDE LA VISTA ====");
            Console.WriteLine($"Id={cita.Id}, IdPaciente={cita.IdPaciente}, IdEmpleado={cita.IdEmpleado}, FechaHora={cita.FechaHora}, Motivo={cita.Motivo}, Observaciones={cita.Observaciones}, Estado={cita.Estado}");

            if (cita.Id <= 0)
            {
                Console.WriteLine("❌ ERROR: El Id de la cita es inválido.");
                TempData["ErrorMessage"] = "Hubo un error al procesar la cita. El ID no es válido.";
                return RedirectToAction("Citas");
            }

            var citaExistente = await _context.Citas.FindAsync(cita.Id);
            if (citaExistente == null)
            {
                Console.WriteLine($"❌ ERROR: La cita con Id={cita.Id} no fue encontrada.");
                TempData["ErrorMessage"] = "No se encontró la cita a editar.";
                return RedirectToAction("Citas");
            }

            // Actualizar datos
            citaExistente.IdPaciente = cita.IdPaciente;
            citaExistente.IdEmpleado = cita.IdEmpleado;
            citaExistente.FechaHora = cita.FechaHora;
            citaExistente.Motivo = cita.Motivo;
            citaExistente.Observaciones = cita.Observaciones;
            citaExistente.Estado = cita.Estado;

            // Forzar la actualización
            _context.Entry(citaExistente).State = EntityState.Modified;
            int cambios = await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Cambios guardados en la base de datos: {cambios} registros afectados.");

            TempData["SuccessMessage"] = "La cita ha sido actualizada correctamente.";
            return RedirectToAction("Citas");
        }



        private async Task<IActionResult> ProcesarEdicion(CitasModel cita)
        {
            try
            {
                var citaExistente = await _context.Citas.FindAsync(cita.Id);
                if (citaExistente == null)
                {
                    Console.WriteLine($"❌ La cita con Id={cita.Id} no fue encontrada.");
                    return NotFound();
                }

                Console.WriteLine("🔄 **ANTES DE ACTUALIZAR**");
                Console.WriteLine($"Paciente={citaExistente.IdPaciente}, Doctor={citaExistente.IdEmpleado}, Fecha={citaExistente.FechaHora}, Estado={citaExistente.Estado}");

                // Asignar valores actualizados
                citaExistente.IdPaciente = cita.IdPaciente;
                citaExistente.IdEmpleado = cita.IdEmpleado;
                citaExistente.FechaHora = cita.FechaHora;
                citaExistente.Motivo = cita.Motivo;
                citaExistente.Observaciones = cita.Observaciones;
                citaExistente.Estado = cita.Estado;

                _context.Entry(citaExistente).State = EntityState.Modified;
                int cambios = await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Cambios guardados en la base de datos: {cambios} registros afectados.");

                if (cambios > 0)
                {
                    TempData["SuccessMessage"] = "La cita ha sido actualizada correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se realizaron cambios en la base de datos.";
                }

                return RedirectToAction("Citas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar la cita: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al actualizar la cita.";
            }

            CargarListasDesplegables(cita);
            return View("EditarCita", cita);
        }







        private void CargarListasDesplegables(CitasModel cita)
        {
            ViewBag.Pacientes = new SelectList(_context.Pacientes.Select(p => new
            {
                p.IdPaciente,
                NombreCompleto = p.Nombre + " " + p.Apellidos
            }).ToList(), "IdPaciente", "NombreCompleto", cita.IdPaciente);

            ViewBag.Empleados = new SelectList(_context.Empleados.Select(e => new
            {
                e.IdEmpleado,
                NombreCompleto = e.Nombre + " " + e.Apellidos
            }).ToList(), "IdEmpleado", "NombreCompleto", cita.IdEmpleado);
        }




        [HttpGet]
        public async Task<IActionResult> ConfirmarCita(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarCitaPost(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita == null)
                {
                    Console.WriteLine($"❌ ERROR: La cita con Id={id} no fue encontrada.");
                    TempData["ErrorMessage"] = "No se encontró la cita.";
                    return RedirectToAction("Citas");
                }

                // Actualizar estado
                cita.Estado = "Confirmada";

                _context.Entry(cita).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Cita con Id={id} confirmada correctamente.");
                TempData["SuccessMessage"] = "La cita ha sido confirmada correctamente.";

                return RedirectToAction("Citas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al confirmar la cita: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al confirmar la cita.";
                return RedirectToAction("Citas");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CancelarCita(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        [HttpPost]
        public async Task<IActionResult> CancelarCitaPost(CitasModel cita)
        {
            try
            {
                var citaExistente = await _context.Citas.FindAsync(cita.Id);
                if (citaExistente == null)
                {
                    Console.WriteLine($"❌ ERROR: La cita con Id={cita.Id} no fue encontrada.");
                    TempData["ErrorMessage"] = "No se encontró la cita.";
                    return RedirectToAction("Citas");
                }

                // Guardar motivo de cancelación y cambiar estado
                citaExistente.Estado = "Cancelada";
                citaExistente.Observaciones = cita.Observaciones;
                citaExistente.Motivo = "Cancelado por: " + cita.Observaciones; // Motivo incluye observaciones

                _context.Entry(citaExistente).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Cita con Id={cita.Id} cancelada correctamente con motivo: {cita.Observaciones}");
                TempData["SuccessMessage"] = "La cita ha sido cancelada correctamente.";

                return RedirectToAction("Citas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al cancelar la cita: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al cancelar la cita.";
                return RedirectToAction("Citas");
            }
        }




        [HttpPost]
        public async Task<IActionResult> EliminarCita(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita == null)
                {
                    Console.WriteLine($"❌ ERROR: La cita con Id={id} no fue encontrada.");
                    TempData["ErrorMessage"] = "No se encontró la cita.";
                    return RedirectToAction("Citas");
                }

                _context.Citas.Remove(cita);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Cita con Id={id} eliminada correctamente.");
                TempData["SuccessMessage"] = "La cita ha sido eliminada correctamente.";

                return RedirectToAction("Citas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al eliminar la cita: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar la cita.";
                return RedirectToAction("Citas");
            }
        }


        [HttpPost, ActionName("EliminarCitaConfirmado")]
        public async Task<IActionResult> EliminarCitaConfirmado(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita != null)
            {
                _context.Citas.Remove(cita);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Citas));
        }

    }


}
