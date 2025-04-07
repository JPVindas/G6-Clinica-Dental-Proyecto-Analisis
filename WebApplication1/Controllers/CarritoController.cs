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

            // Determinar si es producto o servicio
            string tipo = (idProducto.HasValue) ? "producto" : "servicio";

            // Obtener precio unitario y tasa de impuesto (taxRate)
            decimal precioUnitario = 0;
            decimal taxRate = 0m; // expresado en valor decimal: 13% = 0.13
            if (idProducto.HasValue)
            {
                var producto = await _context.Productos.FindAsync(idProducto.Value);
                if (producto == null) return NotFound("Producto no encontrado.");
                precioUnitario = producto.Precio;
                // Si el producto está exento, taxRate = 0; de lo contrario, usar su porcentaje (dividido entre 100)
                taxRate = producto.Exento ? 0.00m : producto.PorcentajeIVA / 100m;
            }
            else if (idServicio.HasValue)
            {
                var servicio = await _context.Servicios.FindAsync(idServicio.Value);
                if (servicio == null) return NotFound("Servicio no encontrado.");
                precioUnitario = servicio.Costo;
                taxRate = servicio.Exento ? 0.00m : servicio.PorcentajeIVA / 100m;
            }

            // Buscar si ya existe ese ítem en el carrito
            var existingItem = await _context.CarritoItems
                .FirstOrDefaultAsync(ci =>
                    ci.IdCarrito == carrito.IdCarrito &&
                    ci.Tipo == tipo &&
                    ((idProducto.HasValue && ci.IdProducto == idProducto.Value) || (!idProducto.HasValue && ci.IdProducto == null)) &&
                    ((idServicio.HasValue && ci.IdServicio == idServicio.Value) || (!idServicio.HasValue && ci.IdServicio == null))
                );

            if (existingItem != null)
            {
                // Actualizar cantidad y recalcular impuestos
                existingItem.Cantidad += cantidad;
                decimal subtotal = existingItem.Cantidad * existingItem.PrecioUnitario;
                existingItem.Impuestos = subtotal * taxRate;
                _context.CarritoItems.Update(existingItem);
            }
            else
            {
                // Calcular impuestos para el nuevo ítem
                decimal subtotal = precioUnitario * cantidad;
                decimal impuestos = subtotal * taxRate;

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

            // Verificar si el paciente ya ha usado este descuento anteriormente
            bool yaUsado = await _context.Facturas
                .AnyAsync(f =>
                    f.IdDescuento == descuento.IdDescuento &&
                    f.Compra != null &&
                    f.Compra.IdPaciente == paciente.IdPaciente);

            if (yaUsado)
            {
                TempData["ErrorMessage"] = "Ya has utilizado este código de descuento anteriormente.";
                TempData.Remove("CodigoAplicado");
                TempData.Remove("PorcentajeDescuento");
            }
            else
            {
                // ✅ Código válido y no ha sido utilizado previamente
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

            // 🔹 Calcular impuestos y subtotales por ítem
            decimal subtotalGeneral = 0m;
            decimal impuestosTotales = 0m;

            foreach (var item in items)
            {
                decimal precioUnitario = item.PrecioUnitario;
                decimal porcentajeIVA = 0m;

                if (item.Tipo == "producto" && item.Producto != null)
                {
                    porcentajeIVA = item.Producto.Exento ? 0m : item.Producto.PorcentajeIVA;
                }
                else if (item.Tipo == "servicio" && item.Servicio != null)
                {
                    porcentajeIVA = item.Servicio.Exento ? 0m : item.Servicio.PorcentajeIVA;
                }

                decimal impuestoItem = (precioUnitario * item.Cantidad) * (porcentajeIVA / 100m);
                item.Impuestos = impuestoItem;

                subtotalGeneral += precioUnitario * item.Cantidad;
                impuestosTotales += impuestoItem;
            }

            decimal totalConImpuestos = subtotalGeneral + impuestosTotales;
            ViewBag.Subtotal = subtotalGeneral;
            ViewBag.Impuestos = impuestosTotales;
            ViewBag.TotalConImpuestos = totalConImpuestos;

            // ================================
            // Aplicación del Financiamiento
            // ================================
            bool hayServiciosFinanciables = items.Any(i => i.Tipo == "servicio" && i.IdServicio.HasValue);
            ViewBag.HayServiciosFinanciables = hayServiciosFinanciables;

            // ================================
            // Aplicación del Descuento
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

                    // Calcular descuento
                    decimal montoDescuento = totalConImpuestos * (descuento.PorcentajeDescuento / 100m);
                    decimal totalConDescuento = totalConImpuestos - montoDescuento;

                    ViewBag.MontoDescuento = montoDescuento;
                    ViewBag.TotalConDescuento = totalConDescuento;

                    // ✅ Eliminar para evitar que se vuelva a usar
                    TempData.Remove("CodigoAplicado");
                    TempData.Remove("PorcentajeDescuento");
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

                    decimal montoDescuento = totalConImpuestos * (porcentaje / 100m);
                    decimal totalConDescuento = totalConImpuestos - montoDescuento;

                    ViewBag.MontoDescuento = montoDescuento;
                    ViewBag.TotalConDescuento = totalConDescuento;
                }

                ViewBag.CodigoAplicado = codigoDescuento;

                // ✅ Limpiar de todos modos para evitar reuso
                TempData.Remove("CodigoAplicado");
                TempData.Remove("PorcentajeDescuento");
            }

            // ================================
            // Datos del Paciente y Métodos de Pago
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



        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RegistrarCompra(
     DateTime FechaCompra,
     string Estado,
     decimal MontoTotal,
     int IdMetodoDePago,
     bool SolicitaFinanciamiento,
     string Nombre,
     string CodigoDescuento,
     decimal? PorcentajeDescuento
 )
        {
            int userId = GetUserId();
            int rolId = GetRolId();

            if (userId == 0 || rolId != 3)
            {
                TempData["ErrorMessage"] = "No autorizado para registrar compras.";
                return RedirectToAction("Index", "Home");
            }

            var paciente = await _context.Pacientes.Include(p => p.Usuario)
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
                .Include(i => i.Producto)
                .Include(i => i.Servicio)
                .Where(i => i.IdCarrito == carrito.IdCarrito)
                .ToListAsync();

            if (!items.Any())
            {
                TempData["ErrorMessage"] = "El carrito está vacío.";
                return RedirectToAction("Carrito");
            }

            // Obtener descuento si se aplicó
            int? idDescuento = null;
            decimal porcentajeDescuentoReal = 0m;

            if (!string.IsNullOrWhiteSpace(CodigoDescuento) && PorcentajeDescuento.HasValue)
            {
                string codigo = CodigoDescuento.Trim().ToLower();
                var descuento = await _context.Descuentos
                    .FirstOrDefaultAsync(d => d.Codigo.ToLower() == codigo && d.Estado == true);

                if (descuento != null)
                {
                    idDescuento = descuento.IdDescuento;
                    porcentajeDescuentoReal = descuento.PorcentajeDescuento;
                }
            }

            // Calcular subtotal e impuestos totales
            decimal subtotal = 0m;
            decimal impuestos = 0m;

            foreach (var item in items)
            {
                decimal precioLinea = item.PrecioUnitario * item.Cantidad;
                subtotal += precioLinea;

                decimal porcentajeIVA = 0m;
                if (item.Tipo == "producto" && item.Producto != null && !item.Producto.Exento)
                    porcentajeIVA = item.Producto.PorcentajeIVA / 100m;
                else if (item.Tipo == "servicio" && item.Servicio != null && !item.Servicio.Exento)
                    porcentajeIVA = item.Servicio.PorcentajeIVA / 100m;

                item.Impuestos = precioLinea * porcentajeIVA;
                impuestos += item.Impuestos;
            }

            decimal totalSinDescuento = subtotal + impuestos;
            decimal montoDescuento = porcentajeDescuentoReal > 0 ? totalSinDescuento * porcentajeDescuentoReal / 100 : 0;
            decimal totalFinal = totalSinDescuento - montoDescuento;

            // Crear financiamiento si se solicitó
            List<int> idsFinanciamientos = new();
            FinanciamientoModel financiamientoAsignado = null;

            if (SolicitaFinanciamiento)
            {
                foreach (var item in items.Where(i => i.Tipo == "servicio" && i.IdServicio.HasValue))
                {
                    var tratamiento = await _context.Tratamientos.FirstOrDefaultAsync(t => t.IdServicio == item.IdServicio);
                    if (tratamiento == null || tratamiento.IdTratamiento == 15) continue;

                    _context.HistorialMedico.Add(new HistorialMedicoModel
                    {
                        IdPaciente = paciente.IdPaciente,
                        IdEmpleado = 1,
                        Diagnostico = "Financiamiento automático por compra de servicio",
                        IdTratamiento = tratamiento.IdTratamiento,
                        FechaActualizacion = DateTime.Today
                    });
                    await _context.SaveChangesAsync();

                    var financiamiento = await _context.Financiamientos
                        .Where(f => f.IdPaciente == paciente.IdPaciente &&
                                    f.IdTratamiento == tratamiento.IdTratamiento &&
                                    f.FechaInicio.Date == DateTime.Today)
                        .OrderByDescending(f => f.IdFinanciamiento)
                        .FirstOrDefaultAsync();

                    if (financiamiento != null)
                    {
                        int cuotas;
                        DateTime fechaFinal;
                        switch (tratamiento.IdTratamiento)
                        {
                            case 4: cuotas = 12; fechaFinal = DateTime.Today.AddMonths(12); break;
                            case 7: cuotas = 3; fechaFinal = DateTime.Today.AddMonths(3); break;
                            case 8: cuotas = 2; fechaFinal = DateTime.Today.AddDays(14); break;
                            case 10: cuotas = 3; fechaFinal = DateTime.Today.AddDays(21); break;
                            case 13: cuotas = 6; fechaFinal = DateTime.Today.AddMonths(6); break;
                            default: cuotas = 1; fechaFinal = DateTime.Today; break;
                        }

                        financiamiento.Cuotas = cuotas;
                        financiamiento.FechaFinal = fechaFinal;
                        _context.Financiamientos.Update(financiamiento);
                        await _context.SaveChangesAsync();

                        idsFinanciamientos.Add(financiamiento.IdFinanciamiento);
                    }
                }

                if (idsFinanciamientos.Count > 1)
                {
                    var fList = await _context.Financiamientos
                        .Where(f => idsFinanciamientos.Contains(f.IdFinanciamiento))
                        .ToListAsync();

                    financiamientoAsignado = new FinanciamientoModel
                    {
                        IdPaciente = paciente.IdPaciente,
                        MontoTotal = fList.Sum(f => f.MontoTotal),
                        MontoPagado = 0,
                        TasaInteres = 3.5m,
                        Cuotas = 12,
                        FechaInicio = DateTime.Today,
                        FechaFinal = DateTime.Today.AddMonths(12),
                        Estado = "activo"
                    };
                    _context.Financiamientos.Add(financiamientoAsignado);
                    await _context.SaveChangesAsync();
                }
                else if (idsFinanciamientos.Count == 1)
                {
                    financiamientoAsignado = await _context.Financiamientos.FindAsync(idsFinanciamientos.First());
                }
            }

            // Crear compra y factura
            var compra = new CompraModel
            {
                IdPaciente = paciente.IdPaciente,
                FechaCompra = FechaCompra,
                Estado = Estado,
                MontoTotal = totalFinal,
                IdFinanciamiento = financiamientoAsignado?.IdFinanciamiento
            };
            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();

            var factura = new FacturaModel
            {
                IdCompra = compra.IdCompra,
                NombreCliente = paciente.Nombre,
                ApellidosCliente = paciente.Apellidos,
                CedulaCliente = paciente.Usuario?.Cedula,
                CorreoCliente = paciente.Usuario?.CorreoElectronico,
                Fecha = DateTime.Now,
                IdMetodoPago = IdMetodoDePago,
                IdFinanciamiento = financiamientoAsignado?.IdFinanciamiento,
                IdDescuento = idDescuento
            };
            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                decimal subtotalItem = item.PrecioUnitario * item.Cantidad;

                _context.DetallesCompra.Add(new DetalleCompraModel
                {
                    IdCompra = compra.IdCompra,
                    Tipo = item.Tipo,
                    IdProducto = item.IdProducto,
                    IdServicio = item.IdServicio,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Impuestos = item.Impuestos
                });

                _context.DetallesFactura.Add(new DetalleFacturaModel
                {
                    IdFactura = factura.IdFactura,
                    Tipo = item.Tipo,
                    IdProducto = item.IdProducto,
                    IdServicio = item.IdServicio,
                    Cantidad = item.Cantidad,
                    Subtotal = subtotalItem,
                    Impuestos = item.Impuestos,
                    Total = subtotalItem + item.Impuestos
                });

                if (item.Tipo == "producto" && item.Producto != null)
                {
                    item.Producto.Stock -= item.Cantidad;
                    _context.Productos.Update(item.Producto);
                }
            }

            await _context.SaveChangesAsync();

            _context.HistorialCompras.Add(new HistorialComprasModel
            {
                IdPaciente = paciente.IdPaciente,
                FechaCompra = DateTime.Now,
                IdFactura = factura.IdFactura,
                IdFinanciamiento = financiamientoAsignado?.IdFinanciamiento,
                Tipo = "compra",
                MontoTotal = totalFinal
            });

            carrito.Estado = "cerrado";
            _context.Carrito.Update(carrito);
            _context.CarritoItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            try
            {
                var compraCompleta = await _context.Compras
                    .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                    .Include(c => c.Facturas).ThenInclude(f => f.MetodoPago)
                    .Include(c => c.Facturas).ThenInclude(f => f.DetallesFactura).ThenInclude(df => df.Producto)
                    .Include(c => c.Facturas).ThenInclude(f => f.DetallesFactura).ThenInclude(df => df.Servicio)
                    .FirstOrDefaultAsync(c => c.IdCompra == compra.IdCompra);

                EnviarFacturaPorCorreo(compraCompleta);
                TempData["SuccessMessage"] = "Compra registrada y factura enviada al correo.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"La compra fue registrada, pero ocurrió un error al enviar la factura: {ex.Message}";
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

            var factura = compra.Facturas?.FirstOrDefault();

            if (factura != null)
            {
                var subtotal = factura.DetallesFactura?.Sum(d => d.Subtotal) ?? 0;
                var impuestos = factura.DetallesFactura?.Sum(d => d.Impuestos) ?? 0;
                var totalSinDescuento = subtotal + impuestos;

                // Verificar si el descuento aún no está enlazado
                if (factura.IdDescuento == null && TempData.ContainsKey("CodigoAplicado"))
                {
                    string codigo = TempData["CodigoAplicado"]?.ToString();
                    var descuento = await _context.Descuentos.FirstOrDefaultAsync(d => d.Codigo == codigo);
                    if (descuento != null)
                    {
                        factura.IdDescuento = descuento.IdDescuento;
                        _context.Facturas.Update(factura);
                        await _context.SaveChangesAsync();

                        factura.Descuento = descuento; // forzar carga manual
                    }
                }

                decimal porcentajeDescuento = factura.Descuento?.PorcentajeDescuento ?? 0;
                decimal montoDescuento = porcentajeDescuento > 0 ? totalSinDescuento * porcentajeDescuento / 100 : 0;

                // ✅ Usar siempre el monto total real desde la base de datos
                decimal totalFinal = compra.MontoTotal;

                ViewBag.Subtotal = subtotal;
                ViewBag.Impuestos = impuestos;
                ViewBag.Descuento = montoDescuento;
                ViewBag.TotalSinDescuento = totalSinDescuento;
                ViewBag.TotalConDescuento = totalFinal;
                ViewBag.PorcentajeDescuento = porcentajeDescuento;
                ViewBag.MetodoPago = factura.MetodoPago?.Nombre;
            }

            return View(compra);
        }



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

            var factura = compra.Facturas?.FirstOrDefault();
            if (factura == null)
                return NotFound("Factura no encontrada para esta compra.");

            // Cálculos
            decimal subtotal = factura.DetallesFactura?.Sum(d => d.Subtotal) ?? 0;
            decimal impuestos = factura.DetallesFactura?.Sum(d => d.Impuestos) ?? 0;
            decimal totalSinDescuento = subtotal + impuestos;

            decimal porcentajeDescuento = factura.Descuento?.PorcentajeDescuento ?? 0;
            decimal montoDescuento = porcentajeDescuento > 0 ? totalSinDescuento * porcentajeDescuento / 100 : 0;
            decimal totalConDescuento = totalSinDescuento - montoDescuento;

            // Agregar a la factura para que el generador tenga los valores disponibles (si se usa en la vista PDF)
            factura.TotalSinDescuento = totalSinDescuento;
            factura.MontoDescuento = montoDescuento;
            factura.TotalFinal = totalConDescuento;

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
