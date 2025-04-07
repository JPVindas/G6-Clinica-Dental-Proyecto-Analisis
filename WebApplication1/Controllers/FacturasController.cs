using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using WebApplication1.Models;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authorization; // 👈 Importa el espacio de nombres

namespace WebApplication1.Controllers
{
    [Authorize(Policy = "Roles1y2")]
    public class FacturasController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public FacturasController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        // ✅ LISTAR TODAS LAS FACTURAS
        public IActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var facturas = _context.Facturas
                .Include(f => f.MetodoPago)
                .Include(f => f.Descuento)
                .Include(f => f.Compra)
                    .ThenInclude(c => c.Paciente)
                .OrderByDescending(f => f.Fecha)
                .ToPagedList(pageNumber, pageSize);

            return View("Index", facturas);
        }

        // ✅ VER DETALLES DE UNA FACTURA A PARTIR DEL ID DE COMPRA
        // ✅ VER DETALLES DE UNA FACTURA A PARTIR DEL ID DE COMPRA
        public async Task<IActionResult> DetallesPorCompra(int? idCompra)
        {
            if (idCompra == null)
                return NotFound();

            try
            {
                // Buscamos la factura asociada a esa compra
                var factura = await _context.Facturas
                    .Include(f => f.DetallesFactura)
                        .ThenInclude(df => df.Producto)
                    .Include(f => f.DetallesFactura)
                        .ThenInclude(df => df.Servicio)
                    .Include(f => f.MetodoPago)
                    .Include(f => f.Descuento)
                    .Include(f => f.Financiamiento)
                    .Include(f => f.Compra)
                        .ThenInclude(c => c.Paciente)
                            .ThenInclude(p => p.Usuario)
                    .FirstOrDefaultAsync(f => f.IdCompra == idCompra);

                if (factura == null)
                {
                    Console.WriteLine($"❌ No se encontró factura con IdCompra = {idCompra}");
                    TempData["Error"] = "❌ Esta compra no tiene factura asociada.";
                    return RedirectToAction("Index", "Compras");
                }

                // Asignar datos del cliente si están disponibles
                var usuario = factura.Compra?.Paciente?.Usuario;
                if (usuario != null)
                {
                    factura.NombreCliente = usuario.Nombre;
                    factura.ApellidosCliente = usuario.Apellido;
                    factura.CedulaCliente = usuario.Cedula;
                    factura.CorreoCliente = usuario.CorreoElectronico;
                }

                return View("Detalles", factura);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en DetallesPorCompra: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al mostrar la factura.";
                return RedirectToAction("Index", "Compras");
            }
        }

        // ✅ PROCESAR ELIMINACIÓN DE FACTURA (POST)
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null) return NotFound();

            _context.Facturas.Remove(factura);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ✅ VERIFICA SI LA FACTURA EXISTE
        private bool FacturaExiste(int id)
        {
            return _context.Facturas.Any(f => f.IdFactura == id);
        }
    }
}