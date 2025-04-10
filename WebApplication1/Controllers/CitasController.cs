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

            Console.WriteLine($"✅ Usuario autenticado - ID: {userId}, Rol: {rolID}");

            var citasQuery = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .AsQueryable();

            if (rolID == 3) // Cliente solo ve sus propias citas
            {
                // Buscar el ID del paciente asociado a este usuario
                var pacienteId = await _context.Pacientes
                    .Where(p => p.IdUsuario == userId)
                    .Select(p => p.IdPaciente)
                    .FirstOrDefaultAsync();

                citasQuery = citasQuery.Where(c => c.IdPaciente == pacienteId);
                Console.WriteLine($"🎯 Filtrando citas del paciente ID={pacienteId}");

                // Asignar el ID del paciente al ViewBag para que el layout lo pueda usar
                ViewBag.IdPaciente = pacienteId;
            }
            else if (rolID == 4) // Doctor solo ve sus citas asignadas
            {
                var empleadoId = await _context.Empleados
                    .Where(e => e.IdUsuario == userId)
                    .Select(e => e.IdEmpleado)
                    .FirstOrDefaultAsync();

                if (empleadoId > 0)
                {
                    citasQuery = citasQuery.Where(c => c.IdEmpleado == empleadoId);
                    Console.WriteLine($"🎯 Filtrando citas asignadas al doctor con ID de empleado: {empleadoId}");
                }
                else
                {
                    Console.WriteLine($"❌ ERROR: No se encontró un ID de empleado para el usuario ID={userId}");
                    TempData["ErrorMessage"] = "No tienes un perfil de empleado registrado.";
                    return RedirectToAction("Citas");
                }

                ViewBag.UserEmpleadoId = empleadoId;
            }

            // Ordenar las citas por estado y fecha
            var citasList = await citasQuery
                .OrderBy(c => c.Estado == "Pendiente" ? 1 : c.Estado == "Confirmada" ? 2 : 3)
                .ThenBy(c => c.FechaHora)
                .ToListAsync();

            var citas = citasList.ToPagedList(pageNumber, pageSize);

            if (!citas.Any())
            {
                Console.WriteLine($"⚠ No se encontraron citas para el usuario ID={userId}");
                TempData["ErrorMessage"] = "No hay citas registradas.";
            }

            ViewBag.UserRole = rolID;
            ViewBag.UserId = userId;
            return View(citas);
        }



        [HttpGet]
        public async Task<IActionResult> VerificarDisponibilidad(int idDoctor, DateTime fechaHora)
        {
            bool disponible = !await _context.Citas
                .AnyAsync(c => c.IdEmpleado == idDoctor &&
                               c.FechaHora >= fechaHora.AddMinutes(-59) &&
                               c.FechaHora <= fechaHora.AddMinutes(59));

            if (disponible)
            {
                return Json(new { disponible = true });
            }
            else
            {
                // Calcular el siguiente horario disponible (sumando 1 hora)
                DateTime siguienteHorario = fechaHora.AddHours(1);

                return Json(new
                {
                    disponible = false,
                    mensaje = "El doctor ya tiene una cita en ese horario.",
                    siguienteHorario = siguienteHorario.ToString("yyyy-MM-dd HH:mm")
                });
            }
        }



        [HttpGet]
        public IActionResult CrearCita(int? idPaciente, int? idDoctor)
        {
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");
            int userRole = Convert.ToInt32(User.FindFirst("RolID")?.Value ?? "3");

            ViewBag.UserRole = userRole;
            ViewBag.UserId = userId;

            if (userRole == 3)
            {
                var paciente = _context.Pacientes
                    .Where(p => p.IdUsuario == userId)
                    .Select(p => new { p.IdPaciente, NombreCompleto = p.Nombre + " " + p.Apellidos })
                    .FirstOrDefault();

                ViewBag.PacienteId = paciente?.IdPaciente ?? 0;
                ViewBag.NombrePaciente = paciente?.NombreCompleto ?? "No disponible";
            }
            else
            {
                ViewBag.Pacientes = new SelectList(_context.Pacientes
                    .Select(p => new { p.IdPaciente, NombreCompleto = p.Nombre + " " + p.Apellidos })
                    .ToList(), "IdPaciente", "NombreCompleto");
            }

            if (userRole == 4)
            {
                var doctor = _context.Empleados
                    .Include(e => e.Usuario)
                 .Where(e => e.IdUsuario == userId && e.Usuario.RolId == 4 && e.Activo)

                    .Select(e => new { e.IdEmpleado, NombreCompleto = e.Nombre + " " + e.Apellidos })
                    .FirstOrDefault();

                ViewBag.DoctorId = doctor?.IdEmpleado ?? 0;
                ViewBag.NombreDoctor = doctor?.NombreCompleto ?? "No disponible";
            }
            else
            {
                if (idDoctor.HasValue)
                {
                    var doctorSeleccionado = _context.Empleados
                        .Include(e => e.Usuario)
                       .Where(e => e.IdEmpleado == idDoctor && e.Usuario.RolId == 4 && e.Activo)

                        .Select(e => new { e.IdEmpleado, NombreCompleto = e.Nombre + " " + e.Apellidos })
                        .FirstOrDefault();

                    if (doctorSeleccionado != null)
                    {
                        ViewBag.DoctorId = doctorSeleccionado.IdEmpleado;
                        ViewBag.NombreDoctor = doctorSeleccionado.NombreCompleto;
                    }
                }
                ViewBag.Empleados = new SelectList(_context.Empleados
                    .Include(e => e.Usuario)
                  .Where(e => e.Usuario.RolId == 4 && e.Activo)

                    .Select(e => new { e.IdEmpleado, NombreCompleto = e.Nombre + " " + e.Apellidos })
                    .ToList(), "IdEmpleado", "NombreCompleto");

            }

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
                    TempData["ErrorMessage"] = "No tienes un perfil de paciente registrado.";
                    CargarListasDesplegables(cita, userId, rolID);
                    return View("CrearCita", cita);
                }

                cita.IdPaciente = paciente;
            }

            // Validación básica de selección
            if (cita.IdPaciente <= 0 || cita.IdEmpleado <= 0)
            {
                TempData["ErrorMessage"] = "Debe seleccionar un paciente y un doctor válidos.";
                CargarListasDesplegables(cita, userId, rolID);
                return View("CrearCita", cita);
            }

            // Validación de horario permitido
            if (!EsHorarioPermitido(cita.FechaHora))
            {
                ViewBag.HorarioError = "La cita está fuera del horario permitido.";
                CargarListasDesplegables(cita, userId, rolID);
                return View("CrearCita", cita);
            }

            // Verificar si ya hay una cita en ese horario
            if (await DoctorTieneCitaEnHorario(cita.IdEmpleado, cita.FechaHora))
            {
                ViewBag.HorarioError = "El doctor ya tiene una cita en ese horario.";
                CargarListasDesplegables(cita, userId, rolID);
                return View("CrearCita", cita);
            }

            // Verificar existencia de paciente
            var pacienteExiste = await _context.Pacientes
                .AnyAsync(p => p.IdPaciente == cita.IdPaciente);

            if (!pacienteExiste)
            {
                TempData["ErrorMessage"] = "El paciente seleccionado no existe.";
                CargarListasDesplegables(cita, userId, rolID);
                return View("CrearCita", cita);
            }

            // Verificar existencia de doctor activo
            var empleadoExiste = await _context.Empleados
                .Include(e => e.Usuario)
               .AnyAsync(e => e.IdEmpleado == cita.IdEmpleado && e.Usuario.RolId == 4 && e.Activo);


            if (!empleadoExiste)
            {
                TempData["ErrorMessage"] = "El doctor seleccionado no está disponible o ha sido archivado.";
                CargarListasDesplegables(cita, userId, rolID);
                return View("CrearCita", cita);
            }

            // Motivo y estado por defecto
            cita.Motivo ??= "Consulta General";
            cita.Estado ??= "Pendiente";

            try
            {
                _context.Citas.Add(cita);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "La cita ha sido creada exitosamente.";
                return RedirectToAction(nameof(Citas));
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"❌ Error DB: {dbEx.InnerException?.Message ?? dbEx.Message}");
                TempData["ErrorMessage"] = $"Error en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error general: {ex.Message}");
                TempData["ErrorMessage"] = $"Error inesperado: {ex.Message}";
            }
            CargarListasDesplegables(cita, userId, rolID);
            return View("CrearCita", cita);
        }



        private async Task<bool> EsHorarioDisponible(int doctorId, DateTime fechaHora)
        {
            // Definir horarios permitidos
            var diaSemana = fechaHora.DayOfWeek;
            TimeSpan horaApertura, horaCierre;

            if (diaSemana >= DayOfWeek.Monday && diaSemana <= DayOfWeek.Friday) // Lunes - Viernes
            {
                horaApertura = new TimeSpan(8, 0, 0);  // 8:00 AM
                horaCierre = new TimeSpan(22, 0, 0);   // 10:00 PM
            }
            else // Sábados y Domingos
            {
                horaApertura = new TimeSpan(7, 0, 0);  // 7:00 AM
                horaCierre = new TimeSpan(14, 0, 0);   // 2:00 PM
            }

            // Validar si la hora de la cita está dentro del horario permitido
            if (fechaHora.TimeOfDay < horaApertura || fechaHora.TimeOfDay >= horaCierre)
            {
                return false; // Hora fuera del horario permitido
            }

            // Validar que el doctor no tenga otra cita en la misma hora o con menos de 1 hora de diferencia
            var existeCita = await _context.Citas
                .AnyAsync(c => c.IdEmpleado == doctorId &&
                               c.FechaHora >= fechaHora.AddMinutes(-59) &&
                               c.FechaHora <= fechaHora.AddMinutes(59)); // Margen de 1 hora

            return !existeCita; // Devuelve true si el horario está disponible
        }

        private bool EsHorarioPermitido(DateTime fechaHora)
        {
            var diaSemana = fechaHora.DayOfWeek;
            TimeSpan horaApertura, horaCierre;

            if (diaSemana >= DayOfWeek.Monday && diaSemana <= DayOfWeek.Friday) // Lunes - Viernes
            {
                horaApertura = new TimeSpan(8, 0, 0);  // 8:00 AM
                horaCierre = new TimeSpan(22, 0, 0);   // 10:00 PM
            }
            else // Sábados y Domingos
            {
                horaApertura = new TimeSpan(7, 0, 0);  // 7:00 AM
                horaCierre = new TimeSpan(14, 0, 0);   // 2:00 PM
            }

            // Validar si la hora de la cita está dentro del horario permitido
            return fechaHora.TimeOfDay >= horaApertura && fechaHora.TimeOfDay < horaCierre;
        }

        private async Task<bool> DoctorTieneCitaEnHorario(int doctorId, DateTime fechaHora)
        {
            return await _context.Citas
                .AnyAsync(c => c.IdEmpleado == doctorId &&
                               c.FechaHora >= fechaHora.AddMinutes(-59) &&
                               c.FechaHora <= fechaHora.AddMinutes(59)); // Margen de 1 hora
        }







      [HttpPost]
public async Task<IActionResult> SaveCita(CitasModel cita, int userId, int rolID)
{
    try
    {
        if (cita.FechaHora == DateTime.MinValue)
        {
            ViewBag.HorarioError = "Debe seleccionar una fecha y hora válida.";
            CargarListasDesplegables(cita, userId, rolID);
            return View("CrearCita", cita);
        }

        if (!EsHorarioPermitido(cita.FechaHora))
        {
            ViewBag.HorarioError = "La cita está fuera del horario permitido.";
            CargarListasDesplegables(cita, userId, rolID);
            return View("CrearCita", cita);
        }

        if (await DoctorTieneCitaEnHorario(cita.IdEmpleado, cita.FechaHora))
        {
            ViewBag.HorarioError = "El doctor ya tiene una cita programada en ese horario.";
            CargarListasDesplegables(cita, userId, rolID);
            return View("CrearCita", cita);
        }

        if (string.IsNullOrEmpty(cita.Motivo))
            cita.Motivo = "Consulta General";

        if (string.IsNullOrEmpty(cita.Estado))
            cita.Estado = "Pendiente";

        var pacienteExiste = await _context.Pacientes.AnyAsync(p => p.IdPaciente == cita.IdPaciente);
        var empleadoExiste = await _context.Empleados
            .Include(e => e.Usuario)
            .AnyAsync(e => e.IdEmpleado == cita.IdEmpleado && !e.Usuario.Archivado);

        if (!pacienteExiste)
        {
            ViewBag.HorarioError = "El paciente seleccionado no existe.";
            CargarListasDesplegables(cita, userId, rolID);
            return View("CrearCita", cita);
        }

        if (!empleadoExiste)
        {
            ViewBag.HorarioError = "El doctor seleccionado no está disponible o ha sido archivado.";
            CargarListasDesplegables(cita, userId, rolID);
            return View("CrearCita", cita);
        }

        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "La cita ha sido creada exitosamente.";
        return RedirectToAction(nameof(Citas));
    }
    catch (Exception ex)
    {
        ViewBag.HorarioError = $"Ocurrió un error: {ex.Message}";
        CargarListasDesplegables(cita, userId, rolID);
        return View("CrearCita", cita);
    }
}







        [HttpGet]
        [Authorize]
        public async Task<IActionResult> FiltrarCitas(DateTime? startDate = null, DateTime? endDate = null, int? page = 1)
        {
            int pageNumber = page ?? 1;
            int pageSize = 8;

            int rolID = Convert.ToInt32(User.FindFirst("RolID")?.Value ?? "3"); // Cliente por defecto
            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");

            var citasQuery = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Empleado)
                .OrderBy(c => c.FechaHora)
                .AsQueryable();

            if (rolID == 3) // Si el usuario es Cliente, solo ve sus citas
            {
                var pacienteId = await _context.Pacientes
                    .Where(p => p.IdUsuario == userId)
                    .Select(p => p.IdPaciente)
                    .FirstOrDefaultAsync();

                citasQuery = citasQuery.Where(c => c.IdPaciente == pacienteId);
            }
            else if (rolID == 4) // Si es doctor, solo ve sus citas asignadas
            {
                var empleadoId = await _context.Empleados
                    .Where(e => e.IdUsuario == userId)
                    .Select(e => e.IdEmpleado)
                    .FirstOrDefaultAsync();

                if (empleadoId > 0)
                {
                    citasQuery = citasQuery.Where(c => c.IdEmpleado == empleadoId);
                }
                else
                {
                    TempData["ErrorMessage"] = "No tienes un perfil de empleado registrado.";
                    return RedirectToAction("Citas");
                }
            }

            // Aplicar filtros de fecha
            if (startDate.HasValue)
            {
                citasQuery = citasQuery.Where(c => c.FechaHora.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                citasQuery = citasQuery.Where(c => c.FechaHora.Date <= endDate.Value.Date);
            }

            // Convertir la lista de citas a IPagedList para evitar el error
            var citasPagedList = citasQuery.ToPagedList(pageNumber, pageSize);


            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.UserRole = rolID;
            ViewBag.UserId = userId;

            return View("Citas", citasPagedList);
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
     .Include(e => e.Usuario) // 🔹 Asegurar que se incluya la relación
     .Where(e => e.Usuario.RolId == 4) // 🔹 Filtrar solo odontólogos
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

            int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");
            int rolID = Convert.ToInt32(User.FindFirst("RolID")?.Value ?? "3");

            CargarListasDesplegables(cita, userId, rolID); // ✅ Correcto

            return View("EditarCita", cita);
        }






        private void CargarListasDesplegables(CitasModel cita, int userId, int rolId)
        {
            if (rolId == 3) // Cliente
            {
                var paciente = _context.Pacientes
                    .Where(p => p.IdUsuario == userId)
                    .Select(p => new { p.IdPaciente, NombreCompleto = p.Nombre + " " + p.Apellidos })
                    .FirstOrDefault();

                ViewBag.PacienteId = paciente?.IdPaciente ?? 0;
                ViewBag.NombrePaciente = paciente?.NombreCompleto ?? "No disponible";
            }
            else
            {
                ViewBag.Pacientes = new SelectList(_context.Pacientes
                    .Select(p => new { p.IdPaciente, NombreCompleto = p.Nombre + " " + p.Apellidos })
                    .ToList(), "IdPaciente", "NombreCompleto", cita.IdPaciente);
            }

            // 🔐 Mostrar solo empleados activos
            var empleadosActivos = _context.Empleados
                .Include(e => e.Usuario)
                .Where(e => e.Usuario.RolId == 4 && e.Activo)
                .Select(e => new
                {
                    e.IdEmpleado,
                    NombreCompleto = e.Nombre + " " + e.Apellidos
                })
                .ToList();

            ViewBag.Empleados = new SelectList(empleadosActivos, "IdEmpleado", "NombreCompleto", cita.IdEmpleado);

            ViewBag.UserRole = rolId;
            ViewBag.UserId = userId;
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
                    TempData["ErrorMessage"] = "No se encontró la cita.";
                    return RedirectToAction("Citas");
                }

                int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");
                int rolID = Convert.ToInt32(User.FindFirst("RolID")?.Value ?? "3");

                var empleadoId = await _context.Empleados
                    .Where(e => e.IdUsuario == userId)
                    .Select(e => e.IdEmpleado)
                    .FirstOrDefaultAsync();

                if (rolID != 1 && rolID != 2 && (rolID != 4 || cita.IdEmpleado != empleadoId))
                {
                    TempData["ErrorMessage"] = "No tienes permiso para confirmar esta cita.";
                    return RedirectToAction("Citas");
                }

                cita.Estado = "Confirmada";
                _context.Entry(cita).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "La cita ha sido confirmada correctamente.";
                return RedirectToAction("Citas");
            }
            catch (Exception ex)
            {
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

                int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");
                int rolID = Convert.ToInt32(User.FindFirst("RolID")?.Value ?? "3");

                // Permitir que el doctor asignado pueda cancelar la cita
                if (rolID != 1 && rolID != 2 && (rolID != 4 || citaExistente.IdEmpleado != userId))
                {
                    TempData["ErrorMessage"] = "No tienes permiso para cancelar esta cita.";
                    return RedirectToAction("Citas");
                }

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
