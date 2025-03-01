using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.DATA;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers
{
    public class InventarioController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        // Constructor con inyección de dependencias
        public InventarioController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        // GET: Inventario
        public ActionResult Inventario()
        {
            var productos = _context.Productos.ToList();
            return View("Inventario", productos); // Aquí estamos asegurándonos de que se llama a la vista 'Inventario'
        }

        // GET: Inventario/Details/{id}
        public ActionResult Detalles(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null)
            {
                return NotFound();
            }
            return View("Detalles", producto); // Vista de detalles
        }

        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Stock")] ProductosModel producto)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productoExistente = await _context.Productos.FindAsync(id);
                    if (productoExistente == null)
                    {
                        return NotFound();
                    }

                    productoExistente.Stock = producto.Stock;

                    _context.Update(productoExistente);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Stock actualizado correctamente."; // Mensaje de éxito

                    return RedirectToAction(nameof(Editar), new { id = producto.Id }); // Redirige a la misma vista "Editar"
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(producto);
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }


        // GET: Inventario/Eliminar/5
        public ActionResult Eliminar(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null)
            {
                return NotFound(); // Si el producto no existe, retornar 404
            }

            return View(producto); // Regresar la vista con el producto a eliminar
        }

        // POST: Inventario/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, IFormCollection collection)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null)
            {
                return NotFound(); // Si el producto no existe, retornar 404
            }

            // Eliminar el producto de la base de datos
            _context.Productos.Remove(producto);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "El producto se eliminó correctamente.";
            return RedirectToAction("Inventario"); // Redirigir al inventario después de eliminar
        }

    }
}
