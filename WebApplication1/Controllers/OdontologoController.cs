using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.DATA;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class OdontologoController : Controller
    {
        private readonly MinombredeconexionDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public OdontologoController(MinombredeconexionDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ✅ LISTAR TODOS LOS ODONTÓLOGOS (RolId = 4)
        public async Task<IActionResult> Odontologos()
        {
            var odontologos = await _context.Empleados
                .Include(e => e.Usuario)
                .Where(e => e.Usuario != null && e.Usuario.RolId == 4)
                .ToListAsync();

            return View("Odontologos", odontologos);
        }

        // ✅ FORMULARIO PARA AGENDAR CITA CON UN ODONTÓLOGO
        [HttpGet]
        public async Task<IActionResult> AgendarCita(int idOdontologo)
        {
            // Buscar al odontólogo seleccionado
            var odontologo = await _context.Empleados
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.IdEmpleado == idOdontologo);

            if (odontologo == null)
            {
                TempData["ErrorMessage"] = "El odontólogo seleccionado no existe.";
                return RedirectToAction("Odontologos");
            }

            // Obtener el usuario autenticado
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para agendar una cita.";
                return RedirectToAction("Login", "Account"); // Verifica si este controlador existe
            }

            int userId = Convert.ToInt32(userIdClaim.Value);

            // Buscar el paciente asociado al usuario autenticado
            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.IdUsuario == userId);
            if (paciente == null)
            {
                TempData["ErrorMessage"] = "Solo los pacientes pueden agendar citas.";
                return RedirectToAction("Odontologos");
            }

            // **Redirigir a la vista correcta del controlador Citas**
            return RedirectToAction("CrearCita", "Citas", new
            {
                idPaciente = paciente.IdPaciente,
                idDoctor = odontologo.IdEmpleado
            });
        }
    

    // ✅ GUARDAR CITA EN LA BASE DE DATOS
    [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCita(CitasModel cita)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Citas.Add(cita);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cita agendada correctamente.";
                    return RedirectToAction("Odontologos");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error al agendar la cita. Intenta nuevamente.";
                    Console.WriteLine($"❌ Error: {ex.Message}");
                }
            }

            return View("AgendarCita", cita);
        }

        // ✅ FORMULARIO PARA EDITAR UN ODONTÓLOGO
        [HttpGet]
        public async Task<IActionResult> Editar(int idUsuario)
        {
            if (idUsuario == 0) return NotFound("IdUsuario no válido.");

            var odontologo = await _context.Empleados.FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);
            if (odontologo == null) return NotFound("No se encontró un odontólogo con este IdUsuario.");

            if (!TienePermiso())
            {
                TempData["ErrorMessage"] = "No tienes permisos para editar un odontólogo.";
                return RedirectToAction("Odontologos");
            }

            return View("Editar", odontologo);
        }

        // ✅ GUARDAR EDICIÓN DEL ODONTÓLOGO (INCLUYENDO IMAGEN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarEdicion(int idUsuario, [Bind("Especialidad, Experiencia")] EmpleadosModel odontologo, IFormFile? Imagen)
        {
            if (idUsuario == 0) return RedirectToAction("Odontologos");

            if (!TienePermiso())
            {
                TempData["ErrorMessage"] = "No tienes permisos para modificar este odontólogo.";
                return RedirectToAction("Odontologos");
            }

            try
            {
                var odontologoExistente = await _context.Empleados.FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);
                if (odontologoExistente == null) return RedirectToAction("Odontologos");

                odontologoExistente.Especialidad = odontologo.Especialidad;
                odontologoExistente.Experiencia = odontologo.Experiencia;

                // Manejo de la imagen
                if (Imagen != null && Imagen.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/odontologos");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = $"{Guid.NewGuid()}_{Imagen.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Imagen.CopyToAsync(fileStream);
                    }

                    odontologoExistente.ImagenUrl = $"/uploads/odontologos/{uniqueFileName}";
                }

                _context.Entry(odontologoExistente).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Odontólogo actualizado correctamente.";
                return RedirectToAction(nameof(Odontologos));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error inesperado. Intenta nuevamente.";
                Console.WriteLine($"❌ Error inesperado: {ex.Message}");
                return RedirectToAction("Odontologos");
            }
        }

        // ✅ MÉTODO PARA VALIDAR PERMISOS
        private bool TienePermiso()
        {
            var userRoleClaim = User.Claims.FirstOrDefault(c =>
                c.Type == "rol_id" ||
                c.Type == ClaimTypes.Role ||
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ||
                c.Type == "RolID" ||
                c.Type == "UserRole");

            int userRole = userRoleClaim != null ? Convert.ToInt32(userRoleClaim.Value) : 0;
            return userRole == 1 || userRole == 2;
        }
    }
}
