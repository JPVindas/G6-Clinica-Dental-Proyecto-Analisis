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
        public DbSet<RolesModel> Roles { get; set; } 
      
        public DbSet<PacientesModel> Pacientes { get; set; }
        public DbSet<EmpleadosModel> Empleados { get; set; }
        public DbSet<CitasModel> Citas { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsuariosModel>().ToTable("Usuarios");
            modelBuilder.Entity<RolesModel>().ToTable("Roles");
            
            modelBuilder.Entity<PacientesModel>().ToTable("Pacientes");
            modelBuilder.Entity<EmpleadosModel>().ToTable("Empleados");
            modelBuilder.Entity<CitasModel>().ToTable("Citas");


        }
    }
}
