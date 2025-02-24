using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using WebApplication1.Models;
using System.Linq;
using System.Threading.Tasks;

public class InventarioController : Controller
{
    private readonly MinombredeconexionDbContext _context;

    public InventarioController(MinombredeconexionDbContext context)
    {
        _context = context;
    }

    // GET: Inventario
    public async Task<IActionResult> Inventario()
    {
        var inventario = await _context.Inventario.Include(i => i.Producto).ToListAsync();
        return View(inventario);
    }

    // GET: Inventario/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var inventario = await _context.Inventario
            .Include(i => i.Producto)
            .FirstOrDefaultAsync(m => m.Id_Inventario == id);

        if (inventario == null)
            return NotFound();

        return View(inventario);
    }

    // GET: Inventario/Agregar
    public IActionResult Agregar()
    {
        ViewBag.Productos = _context.Productos.ToList();
        return View();
    }

    // POST: Inventario/Agregar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar([Bind("IdProducto, Cantidad_Actual")] InventarioModel inventario)
    {
        if (ModelState.IsValid)
        {
            _context.Add(inventario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Inventario));
        }
        ViewBag.Productos = _context.Productos.ToList();
        return View(inventario);
    }

    // GET: Inventario/Modificar/5
    public async Task<IActionResult> Modificar(int? id)
    {
        if (id == null)
            return NotFound();

        var inventario = await _context.Inventario.FindAsync(id);
        if (inventario == null)
            return NotFound();

        return View(inventario);
    }

    // POST: Inventario/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Modificar(int id, [Bind("Cantidad_Actual")] InventarioModel inventario)
    {
        var inventarioDb = await _context.Inventario.FindAsync(id);
        if (inventarioDb == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                inventarioDb.Cantidad_Actual = inventario.Cantidad_Actual;
                _context.Update(inventarioDb);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Inventario.Any(e => e.Id_Inventario == id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Inventario));
        }
        return View(inventarioDb);
    }

    // GET: Inventario/Eliminar/5
    public async Task<IActionResult> Eliminar(int? id)
    {
        if (id == null)
            return NotFound();

        var inventario = await _context.Inventario
            .Include(i => i.Producto)
            .FirstOrDefaultAsync(m => m.Id_Inventario == id);

        if (inventario == null)
            return NotFound();

        return View(inventario);
    }

    // POST: Inventario/Eliminar/5
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var inventario = await _context.Inventario.FindAsync(id);
        if (inventario != null)
        {
            _context.Inventario.Remove(inventario);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Inventario));
    }
}
