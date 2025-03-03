using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DATA;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ProductoController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public ProductoController(MinombredeconexionDbContext context)
        {
            _context = context;
        }
        // ✅ FORMULARIO PARA AGREGAR PRODUCTO (GET)
        public IActionResult AgregarProducto()
        {
            return View(new ProductosModel()); // Evita que Model sea null
        }

        // ✅ PROCESAR CREACIÓN DE PRODUCTO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarProducto([Bind("Nombre,Descripcion,Marca,Costo,Stock,UrlImagen")] ProductosModel producto)
        {
            if (ModelState.IsValid)
            {
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Productos));
            }
            return View(producto);
        }

        // ✅ LISTAR TODOS LOS PRODUCTOS
        public async Task<IActionResult> Productos()
        {
            var productos = await _context.Productos.ToListAsync();
            return View("Productos", productos);
        }

        // ✅ FORMULARIO PARA EDITAR PRODUCTO (GET)
        public async Task<IActionResult> EditarProducto(int? id)
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

            return View("EditarProducto", producto); // 🔹 Llamar explícitamente la vista "Editar"
        }

        // ✅ PROCESAR EDICIÓN DEL PRODUCTO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarProductoEdicion(int id, ProductosModel productoEditado)
        {
            if (id != productoEditado.Id)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mostrar cambios en consola para debug
                    Console.WriteLine($"Precio antes: {producto.Precio}, Precio nuevo: {productoEditado.Precio}");

                    producto.Nombre = productoEditado.Nombre;
                    producto.Descripcion = productoEditado.Descripcion;
                    producto.Marca = productoEditado.Marca;
                    producto.Precio = productoEditado.Precio; // Asegurar que realmente se actualiza
                    producto.Stock = productoEditado.Stock;
                    producto.UrlImagen = productoEditado.UrlImagen;

                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExiste(productoEditado.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Productos));
            }
            return View("EditarProducto", productoEditado);
        }


        private bool ProductoExiste(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }

        // ✅ CONFIRMACIÓN PARA ELIMINAR PRODUCTO
        public async Task<IActionResult> EliminarProducto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // ✅ PROCESAR ELIMINACIÓN
        [HttpPost, ActionName("EliminarProducto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductoConfirmado(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Productos));
        }
    }

}
