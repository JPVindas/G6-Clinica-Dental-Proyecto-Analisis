using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.DATA
{
    public class MinombredeconexionDbContext : DbContext
    {
        public MinombredeconexionDbContext(DbContextOptions<MinombredeconexionDbContext> options)
            : base(options) { }

        // 🔹 Tablas en la base de datos
        public DbSet<UsuariosModel> Usuarios { get; set; }
        public DbSet<RolesModel> Roles { get; set; }
        public DbSet<PacientesModel> Pacientes { get; set; }
        public DbSet<EmpleadosModel> Empleados { get; set; }
        public DbSet<CitasModel> Citas { get; set; }
        public DbSet<ServiciosModel> Servicios { get; set; }
        public DbSet<ProductosModel> Productos { get; set; }
        public DbSet<TratamientosModel> Tratamientos { get; set; }
        public DbSet<HistorialMedicoModel> HistorialMedico { get; set; }
        public DbSet<CarritoModel> Carrito { get; set; }
        public DbSet<FacturasModel> Facturas { get; set; }
        public DbSet<FacturaDetalleModel> FacturaDetalles { get; set; }
        public DbSet<FinanciamientoModel> Financiamientos { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🔹 Mapeo de tablas
            modelBuilder.Entity<UsuariosModel>().ToTable("Usuarios");
            modelBuilder.Entity<RolesModel>().ToTable("Roles");
            modelBuilder.Entity<PacientesModel>().ToTable("Pacientes");
            modelBuilder.Entity<EmpleadosModel>().ToTable("Empleados");
            modelBuilder.Entity<CitasModel>().ToTable("Citas");
            modelBuilder.Entity<ServiciosModel>().ToTable("Servicios");
            modelBuilder.Entity<ProductosModel>().ToTable("Productos");
            modelBuilder.Entity<TratamientosModel>().ToTable("Tratamientos");
            modelBuilder.Entity<HistorialMedicoModel>().ToTable("Historial_Medico");
            modelBuilder.Entity<CarritoModel>().ToTable("Carrito");
            modelBuilder.Entity<FacturasModel>().ToTable("Facturas");
            modelBuilder.Entity<FacturaDetalleModel>().ToTable("Detalles_Factura");

            // 🔹 Relaciones entre entidades

            // Usuario - Paciente (1 a 1)
            modelBuilder.Entity<UsuariosModel>()
                .HasOne(u => u.Paciente)
                .WithOne(p => p.Usuario)
                .HasForeignKey<PacientesModel>(p => p.IdUsuario)
                .HasConstraintName("FK_Pacientes_Usuarios");

            // Usuario - Rol (Muchos a 1)
            modelBuilder.Entity<UsuariosModel>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .HasConstraintName("FK_Usuarios_Roles");

            // Paciente - Citas (1 a muchos)
            modelBuilder.Entity<CitasModel>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.IdPaciente)
                .HasConstraintName("FK_Citas_Pacientes");

            // Empleado - Citas (1 a muchos)
            modelBuilder.Entity<CitasModel>()
                .HasOne(c => c.Empleado)
                .WithMany(e => e.Citas)
                .HasForeignKey(c => c.IdEmpleado)
                .HasConstraintName("FK_Citas_Empleados");

            // Usuario - Empleado (1 a 1)
            modelBuilder.Entity<UsuariosModel>()
                .HasOne(u => u.Empleado)
                .WithOne(e => e.Usuario)
                .HasForeignKey<EmpleadosModel>(e => e.IdUsuario)
                .HasConstraintName("FK_Empleados_Usuarios");

            // Servicios - Tratamientos (1 a muchos)
            modelBuilder.Entity<TratamientosModel>()
                .HasOne(t => t.Servicio)
                .WithMany(s => s.Tratamientos)
                .HasForeignKey(t => t.IdServicio)
                .HasConstraintName("FK_Tratamientos_Servicios");

            // Paciente - Historial Médico (1 a muchos)
            modelBuilder.Entity<HistorialMedicoModel>()
                .HasOne(h => h.Paciente)
                .WithMany(p => p.HistorialMedico)
                .HasForeignKey(h => h.IdPaciente)
                .HasConstraintName("FK_HistorialMedico_Pacientes");

            // Empleado (Doctor) - Historial Médico (1 a muchos)
            modelBuilder.Entity<HistorialMedicoModel>()
                .HasOne(h => h.Empleado)
                .WithMany()
                .HasForeignKey(h => h.IdEmpleado)
                .HasConstraintName("FK_HistorialMedico_Empleados");

            // Historial Médico - Tratamientos (1 a 1)
            modelBuilder.Entity<HistorialMedicoModel>()
                .HasOne(h => h.Tratamiento)
                .WithMany(t => t.HistorialesMedicos)
                .HasForeignKey(h => h.IdTratamiento)
                .HasConstraintName("FK_HistorialMedico_Tratamientos");

            // Carrito - Productos (Opcional, ON DELETE CASCADE)
            modelBuilder.Entity<CarritoModel>()
                .HasOne(c => c.Producto)
                .WithMany()
                .HasForeignKey(c => c.IdProducto)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Carrito_Productos");

            // Carrito - Servicios (Opcional, ON DELETE CASCADE)
            modelBuilder.Entity<CarritoModel>()
                .HasOne(c => c.Servicio)
                .WithMany()
                .HasForeignKey(c => c.IdServicio)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Carrito_Servicios");

            // Factura - Paciente (1 a muchos)
            modelBuilder.Entity<FacturasModel>()
                .HasOne(f => f.Paciente)
                .WithMany()
                .HasForeignKey(f => f.IdPaciente)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Facturas_Pacientes");

            // Factura - Empleado (Opcional)
            modelBuilder.Entity<FacturasModel>()
                .HasOne(f => f.Empleado)
                .WithMany()
                .HasForeignKey(f => f.IdEmpleado)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Facturas_Empleados");

            // Factura - Financiamiento (Opcional)
            modelBuilder.Entity<FacturasModel>()
                .HasOne(f => f.Financiamiento)
                .WithMany()
                .HasForeignKey(f => f.IdFinanciamiento)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Facturas_Financiamiento");

            // Factura - Detalles Factura (1 a muchos)
            modelBuilder.Entity<FacturaDetalleModel>()
                .HasOne(d => d.Factura)
                .WithMany(f => f.Detalles)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DetallesFactura_Facturas");

            // Detalles Factura - Productos (1 a muchos)
            modelBuilder.Entity<FacturaDetalleModel>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DetallesFactura_Productos");

            base.OnModelCreating(modelBuilder);
        }
    }
}
