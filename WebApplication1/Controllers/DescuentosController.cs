using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DATA;
using WebApplication1.Models;
using X.PagedList; // Para StaticPagedList

namespace WebApplication1.Controllers
{
    [Authorize(Policy = "Roles1y2")]
    public class DescuentosController : Controller
    {
        private readonly MinombredeconexionDbContext _context;
        private const int PageSize = 5;

        public DescuentosController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        // GET: Descuentos
        [HttpGet]
        public async Task<IActionResult> Descuentos(int? page)
        {
            int pageNumber = page ?? 1;
            // Crear la consulta ordenada por Código
            var descuentosQuery = _context.Descuentos.OrderBy(d => d.Codigo);
            // Extraer sólo los elementos para la página actual
            var descuentosList = await descuentosQuery
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
            // Obtener el total de registros para la paginación
            int totalCount = await descuentosQuery.CountAsync();
            // Crear el objeto paginado
            var descuentosPaged = new StaticPagedList<DescuentoModel>(descuentosList, pageNumber, PageSize, totalCount);
            return View(descuentosPaged);
        }

        // GET: Descuentos/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Descuentos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(DescuentoModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.ParsePorcentaje(); // Convierte el string a decimal según "es-CR"

                // Crear nuevo objeto descuento con los valores ya convertidos
                var nuevoDescuento = new DescuentoModel
                {
                    Codigo = model.Codigo,
                    PorcentajeDescuento = model.PorcentajeDescuento,
                    Estado = true // Por defecto activo
                };

                _context.Descuentos.Add(nuevoDescuento);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Descuento creado exitosamente.";
                return RedirectToAction("Descuentos");
            }
            catch (FormatException)
            {
                ModelState.AddModelError("PorcentajeDescuentoString", "Porcentaje inválido. Usá coma como separador decimal.");
                return View(model);
            }
        }

        // GET: Descuentos/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
                return NotFound();

            var descuento = await _context.Descuentos.FindAsync(id);
            if (descuento == null)
                return NotFound();

            // Rellenar el string con formato es-CR
            descuento.PorcentajeDescuentoString = descuento.PorcentajeDescuento.ToString("N2", new CultureInfo("es-CR"));

            return View(descuento);
        }

        // POST: Descuentos/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, DescuentoModel model)
        {
            if (id != model.IdDescuento)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                model.ParsePorcentaje();

                var descuentoExistente = await _context.Descuentos.FindAsync(id);
                if (descuentoExistente == null)
                    return NotFound();

                descuentoExistente.Codigo = model.Codigo;
                descuentoExistente.PorcentajeDescuento = model.PorcentajeDescuento;
                descuentoExistente.Estado = model.Estado;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Descuento actualizado correctamente.";
                return RedirectToAction("Descuentos");
            }
            catch (FormatException)
            {
                ModelState.AddModelError("PorcentajeDescuentoString", "Porcentaje inválido. Usá coma como separador decimal.");
                return View(model);
            }
        }


        private bool DescuentoExists(int id)
        {
            return _context.Descuentos.Any(e => e.IdDescuento == id);
        }
    






// GET: Descuentos/ArchivarDescuento/5
public async Task<IActionResult> ArchivarDescuento(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var descuento = await _context.Descuentos.FirstOrDefaultAsync(d => d.IdDescuento == id);
            if (descuento == null)
            {
                return NotFound();
            }
            return View(descuento);
        }

        // POST: Descuentos/ArchivarDescuento/5
        [HttpPost, ActionName("ArchivarDescuento")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchivarDescuentoConfirmed(int id)
        {
            var descuento = await _context.Descuentos.FindAsync(id);
            if (descuento == null)
            {
                return NotFound();
            }
            // Archivar: se desactiva el descuento
            descuento.Estado = false;
            _context.Descuentos.Update(descuento);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Descuento archivado.";
            return RedirectToAction("Descuentos");
        }

        // GET: Descuentos/ReactivarDescuento/5
        [HttpGet]
        public async Task<IActionResult> ReactivarDescuento(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var descuento = await _context.Descuentos.FirstOrDefaultAsync(d => d.IdDescuento == id);
            if (descuento == null)
            {
                return NotFound();
            }
            return View(descuento);
        }

        // POST: Descuentos/ReactivarDescuento/5
        [HttpPost, ActionName("ReactivarDescuento")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivarDescuentoConfirmed(int id)
        {
            var descuento = await _context.Descuentos.FindAsync(id);
            if (descuento == null)
            {
                return NotFound();
            }
            // Reactivar: se activa el descuento
            descuento.Estado = true;
            _context.Descuentos.Update(descuento);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Descuento reactivado.";
            return RedirectToAction("Descuentos");
        }


       
    }
}
