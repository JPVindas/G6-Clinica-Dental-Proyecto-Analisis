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
        public DbSet<ServiciosModel> Servicios { get; set; }
        public DbSet<ProductosModel> Productos { get; set; }
        public DbSet<TratamientosModel> Tratamientos { get; set; }
        public DbSet<HistorialMedicoModel> HistorialMedico { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🔹 Especificar nombres de tabla
            modelBuilder.Entity<UsuariosModel>().ToTable("Usuarios");
            modelBuilder.Entity<RolesModel>().ToTable("Roles");
            modelBuilder.Entity<PacientesModel>().ToTable("Pacientes");
            modelBuilder.Entity<EmpleadosModel>().ToTable("Empleados");
            modelBuilder.Entity<CitasModel>().ToTable("Citas");
            modelBuilder.Entity<ServiciosModel>().ToTable("Servicios");
            modelBuilder.Entity<ProductosModel>().ToTable("Productos");
            modelBuilder.Entity<TratamientosModel>().ToTable("Tratamientos");
            modelBuilder.Entity<HistorialMedicoModel>().ToTable("Historial_Medico");

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

            // 🔹 Relación Usuario - Empleado (1 a 1)
            modelBuilder.Entity<UsuariosModel>()
                .HasOne(u => u.Empleado)
                .WithOne(e => e.Usuario)
                .HasForeignKey<EmpleadosModel>(e => e.IdUsuario)
                .HasConstraintName("FK_Empleados_Usuarios");

            // 🔹 Relación Servicios - Tratamientos (1 a muchos)
            modelBuilder.Entity<TratamientosModel>()
                .HasOne(t => t.Servicio)
                .WithMany(s => s.Tratamientos)
                .HasForeignKey(t => t.IdServicio)
                .HasConstraintName("FK_Tratamientos_Servicios");

            // 🔹 Relación Paciente - Historial Médico (1 a muchos)
            modelBuilder.Entity<HistorialMedicoModel>()
                .HasOne(h => h.Paciente)
                .WithMany(p => p.HistorialMedico) // Un paciente puede tener múltiples registros médicos
                .HasForeignKey(h => h.IdPaciente)
                .HasConstraintName("FK_HistorialMedico_Pacientes");

            // 🔹 Relación Empleado (Doctor) - Historial Médico (1 a muchos)
            modelBuilder.Entity<HistorialMedicoModel>()
                .HasOne(h => h.Empleado)
                .WithMany() // Un doctor puede registrar varios historiales
                .HasForeignKey(h => h.IdEmpleado)
                .HasConstraintName("FK_HistorialMedico_Empleados");

            // 🔹 Nueva Relación: Historial Médico - Tratamientos (1 a 1)
            modelBuilder.Entity<HistorialMedicoModel>()
        .HasOne(h => h.Tratamiento)
        .WithMany(t => t.HistorialesMedicos) // Asegúrate de que esta colección exista en `TratamientosModel`
        .HasForeignKey(h => h.IdTratamiento)
        .HasConstraintName("FK_HistorialMedico_Tratamientos");


            base.OnModelCreating(modelBuilder);
        }
    }
}
