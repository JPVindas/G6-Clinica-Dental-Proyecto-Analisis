using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;
using BCrypt.Net;
using WebApplication1.DATA;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;
using X.PagedList.Mvc.Core;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authorization;




namespace WebApplication1.Controllers
{
    [Authorize(Policy = "Roles1y2")]
    public class UsuariosController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public UsuariosController(MinombredeconexionDbContext context)
        {
            _context = context;
        }




        public async Task<IActionResult> Usuarios(int? page)
        {
            int pageNumber = page ?? 1;  // Página actual, por defecto 1
            int pageSize = 8;  // Tamaño de la página

            // ✅ Solución: Usa `ToListAsync()` antes de `ToPagedList()`
            var usuarios = await _context.Usuarios
                .OrderBy(u => u.Id) // Ordenar por ID
                .ToListAsync(); // Convierte a lista primero

            var pagedUsuarios = usuarios.ToPagedList(pageNumber, pageSize); // ✅ Usa paginación correcta

            return View(pagedUsuarios);
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


        // ✅ GET: Usuarios/ModificarUsuario/5
        public async Task<IActionResult> ModificarUsuario(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var roles = await _context.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Nombre", usuario.RolId);

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarUsuario(
            int id,
            [Bind("Id,NombreUsuario,CorreoElectronico,Nombre,Apellido,Cedula,Telefono,RolId")] UsuariosModel usuario,
            string? Contrasena,
            bool cambiarContrasena,
            string ContrasenaActual)
        {
            Console.WriteLine("📌 POST ModificarUsuario ejecutado");

            if (id != usuario.Id)
            {
                Console.WriteLine("❌ IDs no coinciden.");
                return NotFound();
            }

            var usuarioExistente = await _context.Usuarios.FindAsync(id);
            if (usuarioExistente == null)
            {
                Console.WriteLine("❌ No se encontró el usuario en la base de datos.");
                return NotFound();
            }

            //  Eliminar validación de Contrasena si no se está cambiando
            if (!cambiarContrasena)
            {
                ModelState.Remove("Contrasena");
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState inválido");
                var roles = await _context.Roles.ToListAsync();
                ViewBag.Roles = new SelectList(roles, "Id", "Nombre", usuario.RolId);
                return View(usuario);
            }

            try
            {
                Console.WriteLine($"🔍 Actualizando usuario con ID: {id}");

                //  Asignar los nuevos valores
                usuarioExistente.NombreUsuario = usuario.NombreUsuario;
                usuarioExistente.CorreoElectronico = usuario.CorreoElectronico;
                usuarioExistente.Nombre = usuario.Nombre;
                usuarioExistente.Apellido = usuario.Apellido;
                usuarioExistente.Cedula = usuario.Cedula;
                usuarioExistente.Telefono = usuario.Telefono;
                usuarioExistente.RolId = usuario.RolId;

                //  Solo actualizar la contraseña si el usuario la quiere cambiar
                if (cambiarContrasena && !string.IsNullOrWhiteSpace(Contrasena))
                {
                    usuarioExistente.Contrasena = BCrypt.Net.BCrypt.HashPassword(Contrasena);
                    Console.WriteLine("🔑 Contraseña actualizada.");
                }
                else
                {
                    usuarioExistente.Contrasena = ContrasenaActual; // Mantener la contraseña actual
                    Console.WriteLine("🔒 Manteniendo contraseña existente.");
                }

                // Marcar entidad como modificada antes de guardar
                _context.Entry(usuarioExistente).State = EntityState.Modified;
                Console.WriteLine($"🔄 Estado de la entidad: {_context.Entry(usuarioExistente).State}");

                int cambios = await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Cambios guardados: {cambios}");

                if (cambios > 0)
                {
                    TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
                    return RedirectToAction("Usuarios");
                }
                else
                {
                    TempData["ErrorMessage"] = "No se realizaron cambios en la base de datos.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar usuario: {ex.Message}");
                TempData["ErrorMessage"] = "Error al actualizar usuario.";
            }

            return View(usuario);
        }






        /* METODO PARA ELIMINAR USUARIOS
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
        }*/
        // GET: Usuarios/ArchivarUsuario/5
        // GET: Usuarios/ArchivarUsuario/5
        public async Task<IActionResult> ArchivarUsuario(int? id)
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

        // POST: Usuarios/ArchivarUsuario/5
        [HttpPost, ActionName("ArchivarUsuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchivarConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Se cambia el estado a Archivado (true)
            usuario.Archivado = true;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Usuarios));
        }

        // POST: Usuarios/ReactivarUsuario/5
        [HttpPost, ActionName("ReactivarUsuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivarConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Se cambia el estado a NO archivado (false)
            usuario.Archivado = false;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Usuarios));
        }

        // Método para verificar si un usuario existe
        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }




    }

}