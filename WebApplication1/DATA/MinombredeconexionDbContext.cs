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
            // 🔹 Especificar los nombres de tabla
            modelBuilder.Entity<UsuariosModel>().ToTable("Usuarios");
            modelBuilder.Entity<RolesModel>().ToTable("Roles");
            modelBuilder.Entity<PacientesModel>().ToTable("Pacientes");
            modelBuilder.Entity<EmpleadosModel>().ToTable("Empleados");
            modelBuilder.Entity<CitasModel>().ToTable("Citas");

            // 🔹 Relación Usuario - Paciente (1 a 1)
            modelBuilder.Entity<UsuariosModel>()
                .HasOne(u => u.Paciente)
                .WithOne(p => p.Usuario)
                .HasForeignKey<PacientesModel>(p => p.IdUsuario)
                .HasConstraintName("FK_Pacientes_Usuarios");

            // 🔹 Relación Usuario - Rol (Muchos a 1)
            modelBuilder.Entity<UsuariosModel>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .HasConstraintName("FK_Usuarios_Roles");

            // 🔹 Relación Paciente - Citas (1 a muchos)
            modelBuilder.Entity<CitasModel>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.IdPaciente)
                .HasConstraintName("FK_Citas_Pacientes");

            // 🔹 Relación Empleado - Citas (1 a muchos)
            modelBuilder.Entity<CitasModel>()
                .HasOne(c => c.Empleado)
                .WithMany(e => e.Citas)
                .HasForeignKey(c => c.IdEmpleado)
                .HasConstraintName("FK_Citas_Empleados");

            base.OnModelCreating(modelBuilder);
        }
    }
}