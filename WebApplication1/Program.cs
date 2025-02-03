using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Cargar la cadena de conexión desde el archivo de configuración
var connectionString = builder.Configuration.GetConnectionString("Minombredeconexion");

// Configurar el DbContext para MySQL
builder.Services.AddDbContext<MinombredeconexionDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// Registrar los servicios de MVC
builder.Services.AddControllersWithViews();

// Configurar la autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login"; // Ruta de login
        options.AccessDeniedPath = "/Home/AccessDenied"; // Ruta de acceso denegado
    });

// Configurar autorización basada en roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("Secretaria", policy => policy.RequireRole("Secretaria"));
    options.AddPolicy("Paciente", policy => policy.RequireRole("Paciente"));
});

// Registrar la configuración de correo electrónico
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

// Configurar la tubería de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();

// Habilitar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Configuración de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();