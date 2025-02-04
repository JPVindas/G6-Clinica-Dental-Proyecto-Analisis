using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.DATA;
using WebApplication1.Models;
using BCrypt.Net;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        public LoginController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == username);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(password, usuario.Contrasena))
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View();
            }

            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.RolId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true 
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home"); 
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }
    }
}