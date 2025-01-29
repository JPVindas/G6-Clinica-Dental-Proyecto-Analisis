using WebApplication1.Controllers;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;


namespace WebApplication1.DATA
{
    public class MinombredeconexionDbContext : DbContext
    {
        public MinombredeconexionDbContext(DbContextOptions<MinombredeconexionDbContext> options)
        : base(options) { }

        public DbSet<UsuariosModel> Usuarios { get; set; }
    }
}
