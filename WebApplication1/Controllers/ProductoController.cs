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
        // CREAR PRODUCTO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarProducto([Bind("Nombre,Descripcion,Marca,Precio,Stock,UrlImagen,PorcentajeIVA,Exento")] ProductosModel producto)
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
        [HttpGet]
        public async Task<IActionResult> Productos(int page = 1)
        {
            int pageSize = 6; // Número de productos por página

            // 🔹 Obtener el ID del usuario autenticado
            string userIdClaim = User.FindFirst("UserID")?.Value;
            int userId = int.TryParse(userIdClaim, out var id) ? id : 0;

            // 🔹 Buscar si el usuario tiene un paciente asociado
            var pacienteId = await _context.Pacientes
                .Where(p => p.IdUsuario == userId)
                .Select(p => p.IdPaciente)
                .FirstOrDefaultAsync();

            ViewBag.IdPaciente = pacienteId > 0 ? (int?)pacienteId : null;

            // 🔹 Obtener los productos activos con paginación
            var productosActivos = _context.Productos
                .Where(p => p.estado)
                .OrderBy(p => p.Nombre);

            var totalProductos = await productosActivos.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProductos / (double)pageSize);

            var productosPaginados = await productosActivos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("Productos", productosPaginados);
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
        public async Task<IActionResult> GuardarProductoEdicion(int id, [Bind("Id,Nombre,Descripcion,Marca,Precio,Stock,UrlImagen,PorcentajeIVA,Exento")] ProductosModel productoEditado)
        {
            if (id != productoEditado.Id) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    producto.Nombre = productoEditado.Nombre;
                    producto.Descripcion = productoEditado.Descripcion;
                    producto.Marca = productoEditado.Marca;
                    producto.Precio = productoEditado.Precio;
                    producto.Stock = productoEditado.Stock;
                    producto.UrlImagen = productoEditado.UrlImagen;
                    producto.PorcentajeIVA = productoEditado.PorcentajeIVA;
                    producto.Exento = productoEditado.Exento;

                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExiste(productoEditado.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Productos));
            }
            return View("EditarProducto", productoEditado);
        }

        private bool ProductoExiste(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }

        // CONFIRMACIÓN PARA ELIMINAR
        public async Task<IActionResult> EliminarProducto(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null) return NotFound();

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
                producto.estado = false; // ⚠️ Cambiar estado en lugar de eliminar
                _context.Update(producto);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Productos));
        }

    }
}
