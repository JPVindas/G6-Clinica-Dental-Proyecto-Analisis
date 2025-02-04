using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;
using BCrypt.Net;
using WebApplication1.DATA;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public UsuariosController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return View(usuarios);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/AgregarUsuario
        public IActionResult AgregarUsuario()
        {
            // Pasar los roles a la vista para que se puedan seleccionar
            ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Nombre");
            return View();
        }

        // POST: Usuarios/AgregarUsuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarUsuario([Bind("NombreUsuario,CorreoElectronico,Contrasena,Nombre,Apellido,Cedula,Telefono,RolId")] UsuariosModel usuario)
        {
            if (ModelState.IsValid)
            {
                // Hashear la contraseña antes de guardarla
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                // Redirige a la acción de Usuarios
                return RedirectToAction("Usuarios"); 
            }
            // Si la validación falla, vuelve a pasar los roles
            ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Nombre", usuario.RolId);
            return View(usuario);
        }

        // GET: Usuarios/ModificarUsuario/5
        public async Task<IActionResult> ModificarUsuario(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Pasar los roles a la vista para que se pueda seleccionar uno
            ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Nombre", usuario.RolId);
            return View(usuario);
        }

        // POST: Usuarios/ModificarUsuario/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarUsuario(int id, [Bind("Id,NombreUsuario,CorreoElectronico,Contrasena,Nombre,Apellido,Telefono,RolId")] UsuariosModel usuario)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            // Verificar si el modelo es válido
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, recarga los roles (si los tienes)
                ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Nombre", usuario.RolId);
                return View(usuario);  // Regresa al formulario con los errores
            }

            try
            {
                // Si la contraseña ha sido modificada, se hashea
                var existingUsuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                if (existingUsuario != null && usuario.Contrasena != existingUsuario.Contrasena && !string.IsNullOrEmpty(usuario.Contrasena))
                {
                    usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);  // Hasheamos la nueva contraseña
                }

                // Actualizamos el usuario
                _context.Update(usuario);
                await _context.SaveChangesAsync();

                // Redirigimos a la lista de usuarios
                return RedirectToAction("Usuarios");
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifica si el usuario aún existe
                if (!UsuarioExists(usuario.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Usuarios/EliminarUsuario/5
        public async Task<IActionResult> EliminarUsuario(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/EliminarUsuario/5
        [HttpPost, ActionName("EliminarUsuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Usuarios)); 
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}