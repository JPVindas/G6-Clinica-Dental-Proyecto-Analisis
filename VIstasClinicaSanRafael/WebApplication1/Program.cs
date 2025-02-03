using Microsoft.EntityFrameworkCore;

using WebApplication1.DATA;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Cargar la cadena de conexi�n desde el archivo de configuraci�n
var connectionString = builder.Configuration.GetConnectionString("Minombredeconexion");

// Configurar el DbContext para MySQL
builder.Services.AddDbContext<MinombredeconexionDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// Registrar los servicios de MVC

using VistasClinicaSanRafael.Data; 

var builder = WebApplication.CreateBuilder(args);

// Configurar Entity Framework Core para usar la cadena de conexi�n
// Habilitar logging en Program.cs para depurar consultas
builder.Services.AddDbContext<ClinicaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClinicaDB"))
           .EnableSensitiveDataLogging() // Solo en desarrollo
           .LogTo(Console.WriteLine));   // Imprime las consultas SQL



// Agregar servicios de MVC

builder.Services.AddControllersWithViews();

// Configurar la autenticaci�n con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login"; // Ruta de login
        options.AccessDeniedPath = "/Home/AccessDenied"; // Ruta de acceso denegado
    });

// Configurar autorizaci�n basada en roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("Secretaria", policy => policy.RequireRole("Secretaria"));
    options.AddPolicy("Paciente", policy => policy.RequireRole("Paciente"));
});

// Registrar la configuraci�n de correo electr�nico
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();


// Configurar la tuber�a de solicitudes HTTP

// Configurar el pipeline HTTP

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar autenticaci�n y autorizaci�n
app.UseAuthentication();
app.UseAuthorization();


// Configuraci�n de rutas

// Ruta predeterminada

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();