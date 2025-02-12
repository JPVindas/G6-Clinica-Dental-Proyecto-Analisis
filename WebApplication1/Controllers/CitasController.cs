using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using WebApplication1.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using X.PagedList.Extensions;

namespace WebApplication1.Controllers
{
    public class CitasController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public CitasController(MinombredeconexionDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Citas(int? page)
        {
            int pageNumber = page ?? 1; // Página actual, por defecto 1
            int pageSize = 8;  // Tamaño de la página

            // Obtener el UserID y RolID del usuario autenticado desde los Claims
            string userIdClaim = User.FindFirst("UserID")?.Value;
            string rolIdClaim = User.FindFirst("RolID")?.Value;

            Console.WriteLine($"🔍 Claims -> UserID: {userIdClaim}, RolID: {rolIdClaim}");

            // Convertir a enteros
            int userId = 0, rolID = 3; // Cliente por defecto
            int.TryParse(userIdClaim, out userId);
            int.TryParse(rolIdClaim, out rolID);

            if (userId == 0)
            {
                Console.WriteLine("❌ ERROR: No se pudo obtener el UserID del usuario autenticado.");
                TempData["ErrorMessage"] = "No se pudo autenticar correctamente. Intente iniciar sesión nuevamente.";
                return RedirectToAction("Login", "Login");
            }

            Console.WriteLine($"✅ Usuario autenticado - ID: {userId}, Rol: {rolID}");

            // Obtener el IdPaciente del usuario autenticado si es un cliente
            var paciente = await _context.Pacientes
                .Where(p => p.IdUsuario == userId)
                .Select(p => p.IdPaciente)
                .FirstOrDefaultAsync();

            Console.WriteLine($"📌 IdPaciente asignado: {paciente}");

            if (rolID == 3 && paciente == 0)
            {
                TempData["ErrorMessage"] = "No tienes un perfil de paciente registrado. Contacta con el administrador.";
                return RedirectToAction("Index", "Home");
            }

            // Construcción de la consulta con inclusión de relaciones
            var citasQuery = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .OrderBy(c => c.FechaHora)
                .AsQueryable();

            // Si el usuario es Cliente (rolID = 3), solo verá sus propias citas
            if (rolID == 3)
            {
                Console.WriteLine($"🎯 Filtrando citas del paciente ID={paciente}");
                citasQuery = citasQuery.Where(c => c.IdPaciente == paciente);
            }

            // Convertir la consulta en una lista antes de aplicar la paginación
            var citasList = await citasQuery.ToListAsync();

            // Aplicar paginación usando ToPagedList()
            var citas = citasList.ToPagedList(pageNumber, pageSize);

            if (!citas.Any())
            {
                Console.WriteLine($"⚠ No se encontraron citas para el paciente con ID {paciente}");
                TempData["ErrorMessage"] = "No hay citas registradas.";
            }

            Console.WriteLine($"📋 Total de citas encontradas: {citas.Count}");

            // Pasar datos a la vista
            ViewBag.UserRole = rolID;
            ViewBag.UserId = userId;

            return View(citas);
        }


        public IActionResult CrearCita()
        {
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");
            int userRole = Convert.ToInt32(User.FindFirst("RolID")?.Value ?? "3");

            var paciente = _context.Pacientes
                .Where(p => p.IdUsuario == userId)
                .Select(p => new { p.IdPaciente, NombreCompleto = p.Nombre + " " + p.Apellidos })
                .FirstOrDefault();

            ViewBag.UserRole = userRole;
            ViewBag.UserId = userId;
            ViewBag.PacienteId = paciente != null ? paciente.IdPaciente : 0;
            ViewBag.NombrePaciente = paciente != null ? paciente.NombreCompleto : "No disponible";

            Console.WriteLine($"📌 ID de paciente encontrado en CrearCita: {ViewBag.PacienteId}");

            ViewBag.Pacientes = new SelectList(_context.Pacientes
                .Select(p => new { p.IdPaciente, NombreCompleto = p.Nombre + " " + p.Apellidos })
                .ToList(), "IdPaciente", "NombreCompleto");

            ViewBag.Empleados = new SelectList(_context.Empleados
                .Select(e => new { e.IdEmpleado, NombreCompleto = e.Nombre + " " + e.Apellidos })
                .ToList(), "IdEmpleado", "NombreCompleto");

            return View();
        }



        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CrearCita(CitasModel cita)
        {
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");
            int rolID = Convert.ToInt32(User.FindFirst("RolID")?.Value ?? "3");

            Console.WriteLine($"📌 Datos recibidos - Paciente={cita.IdPaciente}, Doctor={cita.IdEmpleado}, Fecha={cita.FechaHora}, Usuario={userId}, Rol={rolID}");

            // Si el usuario es Cliente (rol 3), obtener su IdPaciente desde la base de datos
            if (rolID == 3)
            {
                var paciente = await _context.Pacientes
                    .Where(p => p.IdUsuario == userId)
                    .Select(p => p.IdPaciente)
                    .FirstOrDefaultAsync();

                if (paciente == 0)
                {
                    Console.WriteLine($"❌ ERROR: No se encontró un paciente vinculado al usuario autenticado con ID {userId}.");
                    TempData["ErrorMessage"] = "No tienes un perfil de paciente registrado. Contacta con el administrador.";
                    return RedirectToAction("CrearCita");
                }

                cita.IdPaciente = paciente; // 🔹 Asignar correctamente el ID
                Console.WriteLine($"✅ Cliente asignado a su propio paciente ID: {cita.IdPaciente}");
            }

            // Validaciones
            if (cita.IdPaciente <= 0 || cita.IdEmpleado <= 0)
            {
                Console.WriteLine("❌ ERROR: El paciente o el doctor no son válidos.");
                TempData["ErrorMessage"] = "Debe seleccionar un paciente y un doctor válidos.";
                return RedirectToAction("CrearCita");
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
                    Console.WriteLine("❌ ERROR: La fecha no es válida.");
                    TempData["ErrorMessage"] = "Debe seleccionar una fecha y hora válida.";
                    return RedirectToAction("CrearCita");
                }

                if (string.IsNullOrEmpty(cita.Motivo))
                {
                    cita.Motivo = "Consulta General"; // Se asigna un motivo predeterminado
                }

                if (string.IsNullOrEmpty(cita.Estado))
                {
                    cita.Estado = "Pendiente";
                }

                // Confirmar que el paciente y doctor existen en la base de datos
                var pacienteExiste = await _context.Pacientes
                    .AnyAsync(p => p.IdPaciente == cita.IdPaciente);

                var empleadoExiste = await _context.Empleados
                    .AnyAsync(e => e.IdEmpleado == cita.IdEmpleado);

                if (!pacienteExiste)
                {
                    Console.WriteLine($"❌ ERROR: Paciente con ID {cita.IdPaciente} no existe en la base de datos.");
                    TempData["ErrorMessage"] = "El paciente seleccionado no existe.";
                    return RedirectToAction("CrearCita");
                }

                if (!empleadoExiste)
                {
                    Console.WriteLine($"❌ ERROR: Doctor con ID {cita.IdEmpleado} no existe en la base de datos.");
                    TempData["ErrorMessage"] = "El doctor seleccionado no existe.";
                    return RedirectToAction("CrearCita");
                }

                // Guardar la cita en la base de datos
                _context.Citas.Add(cita);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Cita guardada correctamente en la base de datos.");

                TempData["SuccessMessage"] = "La cita ha sido creada exitosamente.";
                return RedirectToAction(nameof(Citas));
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"❌ Error de base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
                TempData["ErrorMessage"] = $"Error en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inesperado: {ex.Message}");
                TempData["ErrorMessage"] = $"Error inesperado: {ex.Message}";
            }

            return RedirectToAction("CrearCita");
        }




        [HttpGet]
        [Authorize]
        public async Task<IActionResult> FiltrarCitas(DateTime? startDate = null, DateTime? endDate = null)
        {
            int rolID = Convert.ToInt32(User.FindFirst("RolID")?.Value ?? "3"); // Cliente por defecto
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");

            var citasQuery = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .AsQueryable();

            if (rolID == 3) // Si el usuario es Cliente, solo ve sus citas
            {
                citasQuery = citasQuery.Where(c => c.IdPaciente == userId);
            }

            // Aplicar filtros de fecha solo si startDate y endDate tienen valores
            if (startDate.HasValue)
            {
                citasQuery = citasQuery.Where(c => c.FechaHora.Date >= startDate.Value.Date);
                Console.WriteLine($"📆 Filtrando desde {startDate.Value.Date}");
            }

            if (endDate.HasValue)
            {
                citasQuery = citasQuery.Where(c => c.FechaHora.Date <= endDate.Value.Date);
                Console.WriteLine($"📆 Filtrando hasta {endDate.Value.Date}");
            }

            var citas = await citasQuery.OrderBy(c => c.FechaHora).ToListAsync();

            Console.WriteLine($"📋 Total citas encontradas después del filtro: {citas.Count}");

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.UserRole = rolID;
            ViewBag.UserId = userId;

            return View("Citas", citas); // Devuelve la misma vista que el listado general
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
