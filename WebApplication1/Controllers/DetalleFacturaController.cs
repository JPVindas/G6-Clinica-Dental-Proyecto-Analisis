using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using X.PagedList; // ✅ Agregá esta línea
using System.Threading.Tasks;
using System.Linq;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [Authorize(Policy = "Roles1y2")]
    public class DetalleFacturaController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public DetalleFacturaController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var detalles = _context.DetallesFactura
                .Include(df => df.Producto)
                .Include(df => df.Servicio)
                .OrderByDescending(df => df.IdDetalleFactura)
                .ToPagedList(pageNumber, pageSize); // ← usa ToPagedList()

            return View(detalles);
        }

    }
}
