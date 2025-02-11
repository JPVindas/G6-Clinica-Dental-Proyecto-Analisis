using WebApplication1.DATA;
using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class RegistroController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public RegistroController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Registro")]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [Route("Registro/Registrar")]
        public async Task<IActionResult> Registrar(UsuariosModel model)
        {
            Console.WriteLine("🔹 Iniciando proceso de registro...");

            //  Asignar RolId antes de validar el modelo
            model.RolId = 3; // 🔹 Asegurar que el usuario sea un paciente
            Console.WriteLine($"✅ RolId asignado: {model.RolId}");

            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                Console.WriteLine("⛔ ModelState inválido. Errores:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Campo: {error.Key} - Error: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return View("Registro", model);
            }

            try
            {
                Console.WriteLine("✅ ModelState es válido. Verificando si el usuario ya existe...");

                var usuarioExistente = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.NombreUsuario == model.NombreUsuario || u.CorreoElectronico == model.CorreoElectronico);

                if (usuarioExistente != null)
                {
                    Console.WriteLine("⚠️ Usuario ya existe. No se puede registrar.");
                    ModelState.AddModelError("NombreUsuario", "El nombre de usuario o correo ya está en uso.");
                    return View("Registro", model);
                }

                Console.WriteLine("✅ Usuario no existe. Procediendo con la inserción...");

                //  Hashear la contraseña antes de almacenarla
                model.Contrasena = BCrypt.Net.BCrypt.HashPassword(model.Contrasena);

                // Iniciar la transacción
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Usuarios.Add(model);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ Usuario insertado con ID: {model.Id}");

                    //  Insertar automáticamente en la tabla `Pacientes` gracias al trigger
                    Console.WriteLine("✅ Paciente creado automáticamente por el trigger.");

                    //  Confirmar transacción
                    await transaction.CommitAsync();
                    Console.WriteLine("✅ Transacción confirmada.");

                    TempData["SuccessMessage"] = "Registro exitoso. Por favor, inicia sesión.";
                    return RedirectToAction("Login", "Login");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"❌ Error en la transacción: {ex.Message}");
                    ModelState.AddModelError("", "Error inesperado en el registro.");
                    return View("Registro", model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error general en el registro: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error inesperado.");
                return View("Registro", model);
            }
        }



    }
}
