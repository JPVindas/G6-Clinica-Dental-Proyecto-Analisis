﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using WebApplication1.Models;
using X.PagedList;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [Authorize(Policy = "Roles1y2")]
    public class ComprasController : Controller
    {
        private readonly MinombredeconexionDbContext _context;
        private const int PageSize = 10;

        public ComprasController(MinombredeconexionDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Compras

        public async Task<IActionResult> Index(int? page)
        {
            int pageNumber = page ?? 1;

            var comprasQuery = _context.Compras
                .Include(c => c.Paciente)
                .Include(c => c.Facturas)
                .OrderBy(c =>
    c.Estado == "pendiente" ? 0 :
    c.Estado == "completada" ? 1 : 2)
.ThenByDescending(c => c.IdCompra);



            var comprasList = await comprasQuery
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            int totalCount = await comprasQuery.CountAsync();

            var comprasPaged = new StaticPagedList<CompraModel>(comprasList, pageNumber, PageSize, totalCount);

            return View(comprasPaged);
        }

        // GET: Compras/MarcarComoCompletada/5
        public async Task<IActionResult> MarcarComoCompletada(int? id)
        {
            if (id == null)
                return NotFound();

            var compra = await _context.Compras.FindAsync(id);
            if (compra == null)
                return NotFound();

            return View(compra);
        }
      

        // POST: Compras/MarcarComoCompletada/5
        [HttpPost, ActionName("MarcarComoCompletada")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarComoCompletadaConfirmed(int id)
        {
            var compra = await _context.Compras.FindAsync(id);
            if (compra == null)
                return NotFound();

            compra.Estado = "completada";
            _context.Compras.Update(compra);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Compra marcada como completada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Compras/CancelarCompra/5
        public async Task<IActionResult> CancelarCompra(int? id)
        {
            if (id == null)
                return NotFound();

            var compra = await _context.Compras.FindAsync(id);
            if (compra == null)
                return NotFound();

            return View(compra);
        }

        // POST: Compras/CancelarCompra/5
        [HttpPost, ActionName("CancelarCompra")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarCompraConfirmed(int id)
        {
            var compra = await _context.Compras.FindAsync(id);
            if (compra == null)
                return NotFound();

            compra.Estado = "cancelada";
            _context.Compras.Update(compra);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Compra cancelada.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> FiltrarCompras(DateTime? startDate = null, DateTime? endDate = null, int? page = 1)
        {
            int pageNumber = page ?? 1;

            var comprasQuery = _context.Compras
                .Include(c => c.Paciente)
                .Include(c => c.Facturas)
                .AsQueryable();

            // Aplicar filtros por fecha
            if (startDate.HasValue)
            {
                comprasQuery = comprasQuery.Where(c => c.FechaCompra.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                comprasQuery = comprasQuery.Where(c => c.FechaCompra.Date <= endDate.Value.Date);
            }

            // Ordenar por estado y luego por ID de forma descendente
            comprasQuery = comprasQuery.OrderBy(c =>
                c.Estado == "pendiente" ? 0 :
                c.Estado == "completada" ? 1 : 2)
                .ThenByDescending(c => c.IdCompra);

            var comprasList = await comprasQuery
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            int totalCount = await comprasQuery.CountAsync();

            var comprasPaged = new StaticPagedList<CompraModel>(comprasList, pageNumber, PageSize, totalCount);

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View("Index", comprasPaged);
        }

    }
}
