using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using WebApplication1.DATA; // Ajusta a tu namespace real
using WebApplication1.Models;
using System.Globalization;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.IO;
using Microsoft.Extensions.Options;


namespace WebApplication1.Controllers
{
    [Authorize] // Requiere usuario autenticado
    public class CarritoController : Controller
    {
        private readonly MinombredeconexionDbContext _context;
        private readonly EmailSettings _emailSettings;
        private const decimal TAX_RATE = 0.13m; // 13% de impuesto en Costa Rica
        

        public CarritoController(MinombredeconexionDbContext context, IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _emailSettings = emailSettings.Value;
        }

        // ===============================================
        // 2) Conteo De PRODUCTOS O SERVICIOS AGREGADOS AL CARRITO
        // ===============================================
        [HttpGet]
        public IActionResult GetCarritoCount(int idPaciente)
        {
            int count = _context.CarritoItems
                .Where(ci => ci.Carrito.IdPaciente == idPaciente && ci.Carrito.Estado == "abierto")
                .Sum(ci => ci.Cantidad);
            return Json(new { cantidadCarrito = count });
        }




        // ===============================================
        // 2) Mostrar Carrito (GET /Carrito/Carrito)
        // ===============================================
        [HttpGet]
        public async Task<IActionResult> Carrito()
        {
            int userId = GetUserId();
            int rolId = GetRolId();

            if (userId == 0)
            {
                TempData["ErrorMessage"] = "No se pudo autenticar correctamente. Inicia sesión.";
                return RedirectToAction("Login", "Login");
            }

            if (rolId != 3)
            {
                TempData["ErrorMessage"] = "Solo usuarios con rol=3 (paciente) pueden usar el carrito.";
                return RedirectToAction("Index", "Home");
            }

            // Buscar el paciente asociado a este userId
            var pacienteId = await _context.Pacientes
                .Where(p => p.IdUsuario == userId)
                .Select(p => p.IdPaciente)
                .FirstOrDefaultAsync();

            if (pacienteId == 0)
            {
                TempData["ErrorMessage"] = "No existe un paciente vinculado a este usuario.";
                return RedirectToAction("Login", "Login");
            }

            // Asignar el ID del paciente al ViewBag para el layout
            ViewBag.IdPaciente = pacienteId;

            // Buscar o crear carrito abierto
            var carrito = await _context.Carrito
                .FirstOrDefaultAsync(c => c.IdPaciente == pacienteId && c.Estado == "abierto");

            if (carrito == null)
            {
                carrito = new CarritoModel
                {
                    IdPaciente = pacienteId,
                    FechaCreacion = DateTime.Now,
                    Estado = "abierto"
                };
                _context.Carrito.Add(carrito);
                await _context.SaveChangesAsync();
            }

            // Obtener los ítems del carrito
            var itemsCarrito = await _context.CarritoItems
                .Include(ci => ci.Producto)
                .Include(ci => ci.Servicio)
                .Where(ci => ci.IdCarrito == carrito.IdCarrito)
                .ToListAsync();

            return View("Carrito", itemsCarrito);
        }


        [HttpPost]
        public async Task<IActionResult> AgregarItem(int? idProducto, int? idServicio, int cantidad = 1)
        {
            int userId = GetUserId();
            int rolId = GetRolId();

            if (userId == 0)
            {
                TempData["ErrorMessage"] = "No se pudo autenticar correctamente.";
                return RedirectToAction("Login", "Login");
            }

            if (rolId != 3)
            {
                TempData["ErrorMessage"] = "Solo rol=3 (paciente) puede agregar ítems.";
                return RedirectToAction("Index", "Home");
            }

            // Buscar paciente asociado al usuario
            var pacienteId = await _context.Pacientes
                .Where(p => p.IdUsuario == userId)
                .Select(p => p.IdPaciente)
                .FirstOrDefaultAsync();

            if (pacienteId == 0)
            {
                TempData["ErrorMessage"] = "No se encontró un paciente vinculado a este usuario.";
                return RedirectToAction("Login", "Login");
            }

            // Buscar carrito abierto del paciente o crearlo si no existe
            var carrito = await _context.Carrito
                .FirstOrDefaultAsync(c => c.IdPaciente == pacienteId && c.Estado == "abierto");

            if (carrito == null)
            {
                carrito = new CarritoModel
                {
                    IdPaciente = pacienteId,
                    FechaCreacion = DateTime.Now,
                    Estado = "abierto"
                };
                _context.Carrito.Add(carrito);
                await _context.SaveChangesAsync();
            }

            // Determinar si es producto o servicio
            string tipo = (idProducto.HasValue) ? "producto" : "servicio";

            // Obtener precio unitario (y en este ejemplo, el nombre se obtiene en la vista)
            decimal precioUnitario = 0;
            if (idProducto.HasValue)
            {
                var producto = await _context.Productos.FindAsync(idProducto.Value);
                if (producto == null) return NotFound("Producto no encontrado.");
                precioUnitario = producto.Precio;
            }
            else if (idServicio.HasValue)
            {
                var servicio = await _context.Servicios.FindAsync(idServicio.Value);
                if (servicio == null) return NotFound("Servicio no encontrado.");
                precioUnitario = servicio.Costo;
            }

            // Buscar si ya existe ese ítem en el carrito:
            // Se compara correctamente: para producto, idProducto debe coincidir y idServicio debe ser null; para servicio, viceversa.
            var existingItem = await _context.CarritoItems
                .FirstOrDefaultAsync(ci =>
                    ci.IdCarrito == carrito.IdCarrito &&
                    ci.Tipo == tipo &&
                    ((idProducto.HasValue && ci.IdProducto == idProducto.Value) || (!idProducto.HasValue && ci.IdProducto == null)) &&
                    ((idServicio.HasValue && ci.IdServicio == idServicio.Value) || (!idServicio.HasValue && ci.IdServicio == null))
                );

            if (existingItem != null)
            {
                // Sumar la cantidad y recalcular impuestos
                existingItem.Cantidad += cantidad;
                decimal subtotal = existingItem.Cantidad * existingItem.PrecioUnitario;
                existingItem.Impuestos = subtotal * TAX_RATE; // Calcula el 13%
                _context.CarritoItems.Update(existingItem);
            }
            else
            {
                // Crear un nuevo ítem en el carrito
                decimal subtotal = precioUnitario * cantidad;
                decimal impuestos = subtotal * TAX_RATE;

                var newItem = new CarritoItemModel
                {
                    IdCarrito = carrito.IdCarrito,
                    Tipo = tipo,
                    IdProducto = idProducto,
                    IdServicio = idServicio,
                    Cantidad = cantidad,
                    PrecioUnitario = precioUnitario,
                    Impuestos = impuestos
                };

                _context.CarritoItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Carrito");
        }

        // ===============================================
        // 3) Eliminar ítem (POST /Carrito/EliminarItem)
        // ===============================================
        [HttpPost]
        public async Task<IActionResult> EliminarItem(int idCarritoItem)
        {
            int userId = GetUserId();
            int rolId = GetRolId();

            if (userId == 0 || rolId != 3)
            {
                TempData["ErrorMessage"] = "No autorizado para eliminar ítems.";
                return RedirectToAction("Index", "Home");
            }

            var item = await _context.CarritoItems.FindAsync(idCarritoItem);
            if (item == null)
                return NotFound("Ítem no encontrado.");

            if (item.Cantidad > 1)
            {
                // Si hay más de 1, restamos 1
                item.Cantidad -= 1;
                decimal subtotal = item.Cantidad * item.PrecioUnitario;
                item.Impuestos = subtotal * TAX_RATE; // recalcular el impuesto
                _context.CarritoItems.Update(item);
            }
            else
            {
                // Si la cantidad es 1, se elimina la fila
                _context.CarritoItems.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Carrito");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> VaciarCarrito()
        {
            int userId = GetUserId();
            int rolId = GetRolId();

            if (userId == 0 || rolId != 3)
            {
                TempData["ErrorMessage"] = "No autorizado para vaciar el carrito.";
                return RedirectToAction("Index", "Home");
            }

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.IdUsuario == userId);

            if (paciente == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Carrito");
            }

            var carrito = await _context.Carrito
                .FirstOrDefaultAsync(c => c.IdPaciente == paciente.IdPaciente && c.Estado == "abierto");

            if (carrito == null)
            {
                TempData["ErrorMessage"] = "No se encontró un carrito activo.";
                return RedirectToAction("Carrito");
            }

            var items = await _context.CarritoItems
                .Where(ci => ci.IdCarrito == carrito.IdCarrito)
                .ToListAsync();

            if (items.Any())
            {
                _context.CarritoItems.RemoveRange(items);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Todos los ítems han sido eliminados del carrito.";
            }
            else
            {
                TempData["InfoMessage"] = "El carrito ya está vacío.";
            }

            return RedirectToAction("Carrito");
        }



        // ===============================================
        // 5) AplicarDescuento => POST
        // ===============================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AplicarDescuento(string codigoDescuento)
        {
            if (string.IsNullOrWhiteSpace(codigoDescuento))
            {
                TempData["ErrorMessage"] = "Debe ingresar un código de descuento.";
                return RedirectToAction("Checkout");
            }

            int userId = GetUserId();

            // Obtener el paciente asociado al usuario
            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.IdUsuario == userId);

            if (paciente == null)
            {
                TempData["ErrorMessage"] = "No se pudo identificar al paciente.";
                return RedirectToAction("Checkout");
            }

            string codigo = codigoDescuento.Trim().ToLower();

            // Buscar si existe el código de descuento y está activo
            var descuento = await _context.Descuentos
                .FirstOrDefaultAsync(d =>
                    d.Codigo.ToLower() == codigo &&
                    d.Estado == true);

            if (descuento == null)
            {
                TempData["ErrorMessage"] = "El código ingresado no es válido o ya no está activo.";
                TempData.Remove("CodigoAplicado");
                TempData.Remove("PorcentajeDescuento");
                return RedirectToAction("Checkout");
            }

            // Verificar si el paciente ya ha usado este descuento anteriormente en una factura
            bool yaUsado = await _context.Facturas
                .AnyAsync(f => f.IdDescuento == descuento.IdDescuento && f.Compra.IdPaciente == paciente.IdPaciente);

            if (yaUsado)
            {
                TempData["ErrorMessage"] = "Ya has utilizado este código de descuento anteriormente.";
                TempData.Remove("CodigoAplicado");
                TempData.Remove("PorcentajeDescuento");
            }
            else
            {
                // Código válido y aún no utilizado por el paciente
                TempData["CodigoAplicado"] = descuento.Codigo;
                TempData["PorcentajeDescuento"] = descuento.PorcentajeDescuento.ToString("F2", CultureInfo.InvariantCulture);
                TempData["SuccessMessage"] = "¡Código aplicado correctamente!";
            }

            return RedirectToAction("Checkout");
        }





        // ===============================================
        // 5.1) Checkout => vista de pago
        // ===============================================

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            int userId = GetUserId();
            int rolId = GetRolId();

            if (userId == 0 || rolId != 3)
            {
                TempData["ErrorMessage"] = "No autorizado para Checkout.";
                return RedirectToAction("Index", "Home");
            }

            var pacienteId = await _context.Pacientes
                .Where(p => p.IdUsuario == userId)
                .Select(p => p.IdPaciente)
                .FirstOrDefaultAsync();

            var carrito = await _context.Carrito
                .FirstOrDefaultAsync(c => c.IdPaciente == pacienteId && c.Estado == "abierto");

            if (carrito == null)
            {
                TempData["ErrorMessage"] = "No hay un carrito abierto para continuar con el pago.";
                return RedirectToAction("Carrito");
            }

            var items = await _context.CarritoItems
                .Include(ci => ci.Producto)
                .Include(ci => ci.Servicio)
                .Where(ci => ci.IdCarrito == carrito.IdCarrito)
                .ToListAsync();

            // ================================
            // 🔍 Aplicación del Descuento
            // ================================
            string codigoDescuento = TempData["CodigoAplicado"] as string;
            string porcentajeDescuentoString = TempData["PorcentajeDescuento"] as string;

            if (!string.IsNullOrWhiteSpace(codigoDescuento))
            {
                var descuento = await _context.Descuentos
                    .FirstOrDefaultAsync(d => d.Codigo.ToLower() == codigoDescuento.ToLower() && d.Estado == true);

                if (descuento != null)
                {
                    ViewBag.CodigoAplicado = descuento.Codigo;
                    ViewBag.PorcentajeDescuento = descuento.PorcentajeDescuento;

                    // Guardar en TempData como string para evitar error de serialización
                    TempData["CodigoAplicado"] = descuento.Codigo;
                    TempData["PorcentajeDescuento"] = descuento.PorcentajeDescuento.ToString("F2", CultureInfo.InvariantCulture);
                }
                else
                {
                    TempData["ErrorMessage"] = "El código ingresado no es válido o está inactivo.";
                    TempData.Remove("CodigoAplicado");
                    TempData.Remove("PorcentajeDescuento");
                }
            }
            else if (!string.IsNullOrWhiteSpace(porcentajeDescuentoString))
            {
                if (decimal.TryParse(porcentajeDescuentoString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal porcentaje))
                {
                    ViewBag.PorcentajeDescuento = porcentaje;
                }

                ViewBag.CodigoAplicado = codigoDescuento;

                // Reasignamos para mantenerlo tras reloads
                TempData["CodigoAplicado"] = codigoDescuento;
                TempData["PorcentajeDescuento"] = porcentajeDescuentoString;
            }

            // ================================
            // Datos del paciente y métodos de pago
            // ================================
            var paciente = await _context.Pacientes
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.IdPaciente == pacienteId);

            ViewBag.Paciente = paciente;

            var metodos = await _context.MetodoDePago.ToListAsync();
            ViewBag.MetodosDePago = metodos;
            ViewBag.IdPaciente = pacienteId;

            return View("Checkout", items);
        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RegistrarCompra(
     DateTime FechaCompra,
     string Estado,
     decimal MontoTotal,
     int IdMetodoDePago,
     bool SolicitaFinanciamiento,
     string Nombre)
        {
            int userId = GetUserId();
            int rolId = GetRolId();

            if (userId == 0 || rolId != 3)
            {
                TempData["ErrorMessage"] = "No autorizado para registrar compras.";
                return RedirectToAction("Index", "Home");
            }

            var paciente = await _context.Pacientes
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.IdUsuario == userId);

            if (paciente == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Carrito");
            }

            var carrito = await _context.Carrito
                .FirstOrDefaultAsync(c => c.IdPaciente == paciente.IdPaciente && c.Estado == "abierto");

            if (carrito == null)
            {
                TempData["ErrorMessage"] = "No hay un carrito abierto para continuar.";
                return RedirectToAction("Carrito");
            }

            var items = await _context.CarritoItems
                .Include(i => i.Servicio)
                .Where(i => i.IdCarrito == carrito.IdCarrito)
                .ToListAsync();

            if (!items.Any())
            {
                TempData["ErrorMessage"] = "El carrito está vacío.";
                return RedirectToAction("Carrito");
            }

            int? idFinanciamiento = null;

            if (SolicitaFinanciamiento)
            {
                var itemServicio = items.FirstOrDefault(i => i.Tipo == "servicio" && i.IdServicio.HasValue);
                if (itemServicio != null)
                {
                    var tratamiento = await _context.Tratamientos
                        .FirstOrDefaultAsync(t => t.IdServicio == itemServicio.IdServicio);

                    if (tratamiento != null)
                    {
                        var nuevoFinanciamiento = new FinanciamientoModel
                        {
                            IdPaciente = paciente.IdPaciente,
                            IdTratamiento = tratamiento.IdTratamiento,
                            MontoTotal = tratamiento.Costo,
                            MontoPagado = 0m,
                            TasaInteres = 3.5m,
                            Cuotas = 12,
                            FechaInicio = DateTime.Today,
                            FechaFinal = DateTime.Today.AddMonths(12)
                        };

                        _context.Financiamientos.Add(nuevoFinanciamiento);
                        await _context.SaveChangesAsync();

                        idFinanciamiento = nuevoFinanciamiento.IdFinanciamiento;
                    }
                }
            }

            var compra = new CompraModel
            {
                IdPaciente = paciente.IdPaciente,
                FechaCompra = FechaCompra,
                Estado = Estado,
                MontoTotal = MontoTotal,
                IdFinanciamiento = idFinanciamiento
            };
            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                var detalle = new DetalleCompraModel
                {
                    IdCompra = compra.IdCompra,
                    Tipo = item.Tipo,
                    IdProducto = item.IdProducto,
                    IdServicio = item.IdServicio,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Impuestos = item.Impuestos
                };
                _context.DetallesCompra.Add(detalle);

                if (item.Tipo == "producto" && item.IdProducto.HasValue)
                {
                    var producto = await _context.Productos.FindAsync(item.IdProducto);
                    if (producto != null)
                    {
                        producto.Stock -= item.Cantidad;
                        _context.Productos.Update(producto);
                    }
                }
            }
            await _context.SaveChangesAsync();

            var codigo = TempData["CodigoAplicado"] as string;
            var descuento = !string.IsNullOrWhiteSpace(codigo)
                ? await _context.Descuentos.FirstOrDefaultAsync(d => d.Codigo == codigo && d.Estado == true)
                : null;

            var factura = new FacturaModel
            {
                IdCompra = compra.IdCompra,
                NombreCliente = paciente.Nombre,
                ApellidosCliente = paciente.Apellidos,
                CedulaCliente = paciente.Usuario?.Cedula,
                CorreoCliente = paciente.Usuario?.CorreoElectronico,
                Fecha = DateTime.Now,
                IdMetodoPago = IdMetodoDePago,
                IdDescuento = descuento?.IdDescuento,
                IdFinanciamiento = idFinanciamiento
            };
            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                var subtotal = item.PrecioUnitario * item.Cantidad;
                var detalleFactura = new DetalleFacturaModel
                {
                    IdFactura = factura.IdFactura,
                    Tipo = item.Tipo,
                    IdProducto = item.IdProducto,
                    IdServicio = item.IdServicio,
                    Cantidad = item.Cantidad,
                    Subtotal = subtotal,
                    Impuestos = item.Impuestos,
                    Total = subtotal + item.Impuestos
                };
                _context.DetallesFactura.Add(detalleFactura);
            }
            await _context.SaveChangesAsync();

            var historial = new HistorialComprasModel
            {
                IdPaciente = paciente.IdPaciente,
                FechaCompra = DateTime.Now,
                IdFactura = factura.IdFactura,
                IdFinanciamiento = idFinanciamiento,
                Tipo = "compra",
                MontoTotal = compra.MontoTotal
            };
            _context.HistorialCompras.Add(historial);
            await _context.SaveChangesAsync();

            carrito.Estado = "cerrado";
            _context.Carrito.Update(carrito);
            _context.CarritoItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            // ✅ Enviar factura
            try
            {
                var compraConDetalles = await _context.Compras
                    .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                    .Include(c => c.Facturas).ThenInclude(f => f.MetodoPago)
                    .Include(c => c.Facturas).ThenInclude(f => f.Descuento)
                    .Include(c => c.Facturas).ThenInclude(f => f.DetallesFactura).ThenInclude(df => df.Producto)
                    .Include(c => c.Facturas).ThenInclude(f => f.DetallesFactura).ThenInclude(df => df.Servicio)
                    .FirstOrDefaultAsync(c => c.IdCompra == compra.IdCompra);

                EnviarFacturaPorCorreo(compraConDetalles);
                TempData["SuccessMessage"] = "Compra registrada y factura enviada al correo.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"La compra fue registrada, pero ocurrió un error al enviar la factura por correo: {ex.Message}";
            }

            return RedirectToAction("CompraCompletada", new { idCompra = compra.IdCompra });
        }




        // ===============================================
        // 7) Confirmación final
        // ===============================================
        [Authorize]
        public async Task<IActionResult> CompraCompletada(int idCompra)
        {
            var compra = await _context.Compras
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .Include(c => c.Facturas)
                    .ThenInclude(f => f.MetodoPago)
                .Include(c => c.Facturas)
                    .ThenInclude(f => f.Descuento)
                .Include(c => c.Facturas)
                    .ThenInclude(f => f.DetallesFactura)
                        .ThenInclude(df => df.Producto)
                .Include(c => c.Facturas)
                    .ThenInclude(f => f.DetallesFactura)
                        .ThenInclude(df => df.Servicio)
                .FirstOrDefaultAsync(c => c.IdCompra == idCompra);

            if (compra == null)
                return NotFound("Compra no encontrada.");

            return View(compra);
        }

        // ========== Generar Factura =================
        [Authorize]
        public async Task<IActionResult> GenerarFacturaPDF(int idCompra)
        {
            var compra = await _context.Compras
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .Include(c => c.Facturas).ThenInclude(f => f.MetodoPago)
                .Include(c => c.Facturas).ThenInclude(f => f.Descuento)
                .Include(c => c.Facturas).ThenInclude(f => f.DetallesFactura).ThenInclude(df => df.Producto)
                .Include(c => c.Facturas).ThenInclude(f => f.DetallesFactura).ThenInclude(df => df.Servicio)
                .FirstOrDefaultAsync(c => c.IdCompra == idCompra);

            if (compra == null)
                return NotFound("Compra no encontrada.");

            // ⚙️ Generar el PDF usando QuestPDF
            var pdfBytes = FacturaPDFGenerator.GenerarFactura(compra);

            return File(pdfBytes, "application/pdf", $"Factura_{compra.IdCompra}.pdf");
        }





        // ========== Métodos auxiliares =================
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            int.TryParse(userIdClaim, out int userId);
            return userId;
        }
        private int GetRolId()
        {
            var rolIdClaim = User.FindFirst("RolID")?.Value;
            int.TryParse(rolIdClaim, out int rolId);
            return rolId;
        }
        // ========== Enviar Factura por Correo =================
        private void EnviarFacturaPorCorreo(CompraModel compra)
        {
            try
            {
                var paciente = compra.Paciente;
                var correoDestino = paciente?.Usuario?.CorreoElectronico;

                if (string.IsNullOrWhiteSpace(correoDestino))
                {
                    Console.WriteLine("⚠️ El correo del paciente no está definido.");
                    return;
                }

                var pdf = FacturaPDFGenerator.GenerarFactura(compra);

                using var mensaje = new MailMessage();
                mensaje.From = new MailAddress(_emailSettings.From, "Clínica Dental San Rafael");
                mensaje.To.Add(correoDestino);
                mensaje.Subject = "Factura electrónica de tu compra";
                mensaje.Body = $"Hola {paciente.Nombre},\n\nAdjunto encontrarás la factura de tu compra realizada en la Clínica Dental San Rafael.\n\n¡Gracias por tu preferencia!";

                // Adjuntar la factura PDF
                mensaje.Attachments.Add(new Attachment(new MemoryStream(pdf), $"Factura_{compra.IdCompra}.pdf", "application/pdf"));

                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
                {
                    Port = int.Parse(_emailSettings.Port),
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = true
                };

                smtpClient.Send(mensaje);
                Console.WriteLine("✅ Factura enviada correctamente por correo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar la factura: {ex.Message}");
            }
        }

    }
}
