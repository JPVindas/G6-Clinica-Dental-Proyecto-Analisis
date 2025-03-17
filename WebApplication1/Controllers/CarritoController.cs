using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using WebApplication1.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace WebApplication1.Controllers
{
    public class CarritoController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public CarritoController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        // ✅ Mostrar el carrito con productos y servicios correctamente cargados
        [HttpGet]
        public async Task<IActionResult> Carrito()
        {
            var carrito = await _context.Carrito
                .Include(c => c.Producto)
                .Include(c => c.Servicio)
                .ToListAsync();

            ViewBag.CarritoItems = carrito.Sum(c => c.Cantidad);
            return View(carrito);
        }
        [HttpPost]
        public async Task<IActionResult> AgregarProdu([FromBody] CarritoModel model)
        {
            if (model == null || model.Cantidad <= 0 || model.Tipo != "producto" || !model.IdProducto.HasValue)
            {
                return BadRequest(new { mensaje = "Datos inválidos para agregar un producto." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal precioUnitario = 0m;
                decimal impuestos = 0m;
                string nombreProducto = "Producto sin nombre";

                // 🔹 Buscar el producto en la base de datos
                var producto = await _context.Productos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == model.IdProducto);

                if (producto == null) return NotFound(new { mensaje = "Producto no encontrado." });

                nombreProducto = producto.Nombre ?? nombreProducto;
                precioUnitario = producto.Precio;
                impuestos = precioUnitario * 0.10m;

                // 🔹 Buscar si el producto ya está en el carrito
                var itemExistente = await _context.Carrito
                    .FirstOrDefaultAsync(c => c.IdProducto == model.IdProducto && c.Tipo == "producto");

                if (itemExistente != null)
                {
                    // 🔹 Si ya existe, aumentar la cantidad
                    itemExistente.Cantidad += model.Cantidad;
                    _context.Carrito.Update(itemExistente);
                }
                else
                {
                    // 🔹 Si no existe, agregarlo correctamente
                    var nuevoItem = new CarritoModel
                    {
                        Tipo = "producto",
                        IdProducto = model.IdProducto,
                        Nombre = nombreProducto,
                        Cantidad = model.Cantidad,
                        PrecioUnitario = precioUnitario,
                        Impuestos = impuestos,
                        Imagen = model.Imagen ?? "/images/default-product.jpg"
                    };

                    _context.Carrito.Add(nuevoItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                int totalItems = await _context.Carrito.SumAsync(c => c.Cantidad);
                return Ok(new { cantidadCarrito = totalItems });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al agregar el producto al carrito.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AgregarServ([FromBody] CarritoModel model)
        {
            if (model == null || model.Cantidad <= 0 || model.Tipo != "servicio" || !model.IdServicio.HasValue)
            {
                return BadRequest(new { mensaje = "Datos inválidos para agregar un servicio." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal precioUnitario = 0m;
                decimal impuestos = 0m;
                string nombreServicio = "Servicio sin nombre";

                // 🔹 Buscar el servicio en la base de datos
                var servicio = await _context.Servicios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == model.IdServicio);

                if (servicio == null) return NotFound(new { mensaje = "Servicio no encontrado." });

                nombreServicio = servicio.Nombre ?? nombreServicio;
                precioUnitario = servicio.Costo;
                impuestos = precioUnitario * 0.10m;

                // 🔹 Buscar si el servicio ya está en el carrito
                var itemExistente = await _context.Carrito
                    .FirstOrDefaultAsync(c => c.IdServicio == model.IdServicio && c.Tipo == "servicio");

                if (itemExistente != null)
                {
                    // 🔹 Si ya existe, aumentar la cantidad
                    itemExistente.Cantidad += model.Cantidad;
                    _context.Carrito.Update(itemExistente);
                }
                else
                {
                    // 🔹 Si no existe, agregarlo correctamente
                    var nuevoItem = new CarritoModel
                    {
                        Tipo = "servicio",
                        IdServicio = model.IdServicio,
                        Nombre = nombreServicio,
                        Cantidad = model.Cantidad,
                        PrecioUnitario = precioUnitario,
                        Impuestos = impuestos,
                        Imagen = model.Imagen ?? "/images/default-service.jpg"
                    };

                    _context.Carrito.Add(nuevoItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                int totalItems = await _context.Carrito.SumAsync(c => c.Cantidad);
                return Ok(new { cantidadCarrito = totalItems });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al agregar el servicio al carrito.", error = ex.Message });
            }
        }





        [HttpGet]
        public async Task<IActionResult> GetCarritoCount()
        {
            int totalItems = await _context.Carrito.SumAsync(c => c.Cantidad);
            return Json(new { cantidadCarrito = totalItems });
        }




        [HttpPost]
        public async Task<IActionResult> EliminarItem([FromForm] int idCarrito)
        {
            try
            {
                if (idCarrito <= 0)
                {
                    return BadRequest(new { mensaje = "ID de carrito no válido." });
                }

                var item = await _context.Carrito.FindAsync(idCarrito);
                if (item == null)
                {
                    return NotFound(new { mensaje = "El ítem no existe en el carrito." });
                }

                // ✅ Si la cantidad es mayor a 1, solo restar 1
                if (item.Cantidad > 1)
                {
                    item.Cantidad -= 1;
                    _context.Carrito.Update(item);
                }
                else
                {
                    // ✅ Si la cantidad es 1, eliminar completamente
                    _context.Carrito.Remove(item);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Carrito"); // 🔹 Redirige a la vista del carrito
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Hubo un problema al eliminar el ítem.", error = ex.Message });
            }
        }


        // ✅ Vaciar los ítems del carrito
        [HttpPost]
        public async Task<IActionResult> VaciarCarrito()
        {
            try
            {
                var carrito = await _context.Carrito.ToListAsync();

                if (!carrito.Any())
                {
                    return Ok(new { mensaje = "El carrito ya está vacío." });
                }

                _context.Carrito.RemoveRange(carrito);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "El carrito ha sido vaciado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al vaciar el carrito.", error = ex.Message });
            }
        }

        // ✅ Vaciar carrito (Checkout)
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var carrito = await _context.Carrito.ToListAsync();
            if (!carrito.Any())
            {
                return BadRequest(new { mensaje = "El carrito está vacío." });
            }

            _context.Carrito.RemoveRange(carrito);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Compra realizada con éxito." });
            // ✅ Procesar pago y generar factura
            [HttpPost("ProcesarPago")]
             async Task<IActionResult> ProcesarPago([FromBody] FinanciamientoModel financiamiento)
            {
                if (financiamiento == null || financiamiento.IdPaciente <= 0 || financiamiento.MontoTotal <= 0)
                {
                    return BadRequest(new { mensaje = "Datos de financiamiento inválidos." });
                }

                try
                {
                    // ✅ Obtener los productos en el carrito
                    var carritoItems = await _context.Carrito.Include(c => c.Producto).ToListAsync();
                    if (carritoItems == null || !carritoItems.Any())
                    {
                        return BadRequest(new { mensaje = "No hay productos en el carrito." });
                    }

                    // ✅ Calcular el subtotal
                    decimal subtotal = carritoItems.Sum(item => (item.Cantidad * (item.PrecioUnitario ?? 0m)));

                    // ✅ Aplicar impuestos (13%)
                    decimal impuestos = Math.Round(subtotal * 0.13m, 2);

                    // ✅ Calcular el total final
                    decimal totalConImpuestos = subtotal + impuestos;

                    // ✅ Crear un nuevo registro de factura
                    var nuevaFactura = new FacturasModel
                    {
                        IdPaciente = financiamiento.IdPaciente,
                        IdEmpleado = null,
                        MetodoPago = "Financiamiento",
                        Total = totalConImpuestos,
                        Fecha = DateTime.UtcNow
                    };

                    _context.Facturas.Add(nuevaFactura);
                    await _context.SaveChangesAsync();

                    // ✅ Asociar los productos de la factura
                    foreach (var item in carritoItems)
                    {
                        var detalle = new FacturaDetalleModel
                        {
                            IdFactura = nuevaFactura.IdFactura,
                            IdProducto = item.IdProducto ?? 0,
                            Cantidad = item.Cantidad,
                            Subtotal = (item.Cantidad * (item.PrecioUnitario ?? 0m))
                        };

                        _context.FacturaDetalles.Add(detalle);
                    }

                    await _context.SaveChangesAsync();

                    // ✅ Generar un registro de financiamiento
                    var nuevoFinanciamiento = new FinanciamientoModel
                    {
                        IdPaciente = financiamiento.IdPaciente,
                        IdTratamiento = financiamiento.IdTratamiento,
                        MontoTotal = totalConImpuestos,
                        TasaInteres = financiamiento.TasaInteres,
                        Cuotas = financiamiento.Cuotas,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "Activo"
                    };

                    _context.Financiamientos.Add(nuevoFinanciamiento);
                    await _context.SaveChangesAsync();

                    // ✅ Vaciar el carrito después de procesar el pago
                    _context.Carrito.RemoveRange(carritoItems);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        mensaje = "Pago procesado exitosamente con financiamiento.",
                        facturaId = nuevaFactura.IdFactura,
                        subtotal,
                        impuestos,
                        totalConImpuestos
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { mensaje = "Hubo un error al procesar el financiamiento.", error = ex.Message });
                }
            }
        }
    }
}