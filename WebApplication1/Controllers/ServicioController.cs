using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DATA;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ServicioController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public ServicioController(MinombredeconexionDbContext context)
        {
            _context = context;
        }
        // ✅ FORMULARIO PARA AGREGAR SERVICIO (GET)
        public IActionResult Agregar()
        {
            return View();
        }

        // ✅ PROCESAR CREACIÓN DE SERVICIO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar([Bind("Nombre,Descripcion,Costo,UrlImagen")] ServiciosModel servicio)
        {
            if (ModelState.IsValid)
            {
                _context.Servicios.Add(servicio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Servicios));
            }
            return View(servicio);
        }

        // ✅ LISTAR TODOS LOS SERVICIOS
        public async Task<IActionResult> Servicios()
        {
            var servicios = await _context.Servicios.ToListAsync();
            return View("Servicios", servicios);
        }

        // ✅ FORMULARIO PARA EDITAR SERVICIO (GET)
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null)
            {
                return NotFound();
            }

            return View("Editar", servicio); // 🔹 Llamar explícitamente la vista "Editar"
        }

        // ✅ PROCESAR EDICIÓN DEL SERVICIO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarEdicion(int id, [Bind("Id,Nombre,Descripcion,Costo,UrlImagen")] ServiciosModel servicio)
        {
            if (id != servicio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(servicio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicioExiste(servicio.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Servicios));
            }
            return View("Editar", servicio);
        }

        private bool ServicioExiste(int id)
        {
            return _context.Servicios.Any(e => e.Id == id);
        }

        // ✅ CONFIRMACIÓN PARA ELIMINAR SERVICIO
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.Servicios.FirstOrDefaultAsync(m => m.Id == id);
            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }

        // ✅ PROCESAR ELIMINACIÓN
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio != null)
            {
                _context.Servicios.Remove(servicio);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Servicios));
        }
    }

}
