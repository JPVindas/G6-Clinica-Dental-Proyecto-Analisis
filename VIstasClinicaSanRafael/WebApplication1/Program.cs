using Microsoft.EntityFrameworkCore;
using WebApplication1.DATA;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexi�n
var connectionString = builder.Configuration.GetConnectionString("Minombredeconexion");

builder.Services.AddDbContext<MinombredeconexionDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// Agregar servicios para MVC
builder.Services.AddControllersWithViews();

// Configurar la autenticaci�n con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Configuraci�n de la cookie de autenticaci�n
        options.LoginPath = "/Login/Login"; // Redirige a la p�gina de login si no est� autenticado
        options.AccessDeniedPath = "/Home/AccessDenied"; // Redirige si no tiene permisos
    });

// Agregar autorizaci�n basada en roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("Secretaria", policy => policy.RequireRole("Secretaria"));
    options.AddPolicy("Paciente", policy => policy.RequireRole("Paciente"));
});

var app = builder.Build();

// Configurar la tuber�a de solicitudes HTTP
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();