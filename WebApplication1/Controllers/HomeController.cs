using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Models;
using WebApplication1.DATA;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public HomeController(MinombredeconexionDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Checkout()
        {
            return View(); 
        }

        
        public IActionResult Factura()
        {
            return View();
        }
        public IActionResult Cart()
        {
            return View();
        }

        public IActionResult Contacts()
        {
            return View();
        }
        public async Task<IActionResult> Clinica()
        {
            if (User.Identity.IsAuthenticated)
            {
                var rolId = User.FindFirst("RolID")?.Value;
                if (rolId == "3")
                {
                    int userId = 0;
                    int.TryParse(User.FindFirst("UserID")?.Value, out userId);
                    if (userId > 0)
                    {
                        var pacienteId = await _context.Pacientes
                            .Where(p => p.IdUsuario == userId)
                            .Select(p => p.IdPaciente)
                            .FirstOrDefaultAsync();
                        ViewBag.IdPaciente = pacienteId;
                    }
                }
            }
            return View();
        }



        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var rolId = User.FindFirst("RolID")?.Value;
                if (rolId == "3")
                {
                    int userId = 0;
                    int.TryParse(User.FindFirst("UserID")?.Value, out userId);
                    if (userId > 0)
                    {
                        var pacienteId = await _context.Pacientes
                            .Where(p => p.IdUsuario == userId)
                            .Select(p => p.IdPaciente)
                            .FirstOrDefaultAsync();
                        ViewBag.IdPaciente = pacienteId;
                    }
                }
            }
            return View();
        }


        public IActionResult PortalExpediente()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult PortalExpedienteAdmin()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult MiPerfil()
        {
            return View();
        }

        public IActionResult Citas()
        {
            return View();
        }

        public IActionResult Crear()
        {
            return View();
        }
        public IActionResult ReprogramarCita()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AgregarCita(string doctor = null)
        {
            ViewBag.SelectedDoctor = doctor;
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult Usuarios()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult Inventario()
        {
            return View();
        }

        public IActionResult Productos()
        {
            return View();
        }

        public IActionResult Servicios()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult AgregarUsuario()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult ModificarUsuario()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult ModificarExpediente()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult AgregarServicio()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult ModificarProducto()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult ModificarServicio()
        {
            return View();
        }
        public IActionResult Odontologos()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult Ventas()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public IActionResult GestionarDescuento()
        {
            return View();
        }













        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
