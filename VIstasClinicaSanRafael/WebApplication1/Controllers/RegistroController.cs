using WebApplication1.DATA;
using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Linq;

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
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(UsuariosModel model)
        {
            // Mostrar los errores si el modelo no es válido
            if (!ModelState.IsValid)
            {
                return View("Registro" ,model); // Retorna la vista con los errores de validación
            }

            // Verificar si el nombre de usuario ya está en uso
            var usuarioExistente = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == model.NombreUsuario);
            if (usuarioExistente != null)
            {
                ModelState.AddModelError("NombreUsuario", "El nombre de usuario ya está en uso.");
                return View(model);
            }

            // Encriptar la contraseña
            model.Contrasena = BCrypt.Net.BCrypt.HashPassword(model.Contrasena);

            // Asignar el rol de Paciente por defecto
            var rolPaciente = _context.Roles.FirstOrDefault(r => r.Nombre == "Paciente");
            if (rolPaciente == null)
            {
                ModelState.AddModelError("", "No se pudo encontrar el rol de Paciente.");
                return View(model);
            }

            model.RolId = rolPaciente.Id;

            // Guardar el usuario en la base de datos
            _context.Usuarios.Add(model);
            _context.SaveChanges();

            // Redirigir a la página de inicio de sesión
            return RedirectToAction("Login", "Login");
        }
    }
}