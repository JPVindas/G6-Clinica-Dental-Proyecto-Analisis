using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
        public IActionResult Clinica()
        {
            return View();
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PortalExpediente()
        {
            return View();
        }

        public IActionResult PortalExpedienteAdmin()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }

        public IActionResult RecuperarPassword()
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


        public IActionResult Usuarios()
        {
            return View();
        }

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

        public IActionResult AgregarUsuario()
        {
            return View();
        }

        public IActionResult ModificarUsuario()
        {
            return View();
        }

        public IActionResult ModificarExpediente()
        {
            return View();
        }

        public IActionResult AgregarProducto()
        {
            return View();
        }

        public IActionResult AgregarServicio()
        {
            return View();
        }

        public IActionResult ModificarProducto()
        {
            return View();
        }

        public IActionResult ModificarServicio()
        {
            return View();
        }
        public IActionResult Odontologos()
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
