using Microsoft.EntityFrameworkCore;
using VistasClinicaSanRafael.Models; // Ajusta el namespace según tu estructura

namespace VistasClinicaSanRafael.Data
{
    public class ClinicaContext : DbContext
    {
        public ClinicaContext() { }

        public ClinicaContext(DbContextOptions<ClinicaContext> options) : base(options) { }

        // Define las tablas de la base de datos
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Tratamiento> Tratamientos { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<HistorialMedico> HistorialesMedicos { get; set; }
        public DbSet<Financiamiento> Financiamientos { get; set; }
        public DbSet<Contabilidad> Contabilidades { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=localhost;Database=ClinicaOdontologia;Trusted_Connection=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar claves primarias
            modelBuilder.Entity<Empleado>()
                .HasKey(e => e.IdEmpleado);

            modelBuilder.Entity<DetalleFactura>()
                .HasKey(df => new { df.IdFactura, df.IdProducto });

            // Configuración de la tabla Usuario con relación a Rol
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de la tabla Cita
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.IdPaciente)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Empleado)
                .WithMany(e => e.Citas)
                .HasForeignKey(c => c.IdEmpleado)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Tratamiento)
                .WithMany()
                .HasForeignKey(c => c.IdTratamiento)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de la tabla Factura con relación a Paciente y Empleado
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Paciente)
                .WithMany(p => p.Facturas)
                .HasForeignKey(f => f.IdPaciente)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Empleado)
                .WithMany(e => e.Facturas)
                .HasForeignKey(f => f.IdEmpleado)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Financiamiento)
                .WithMany()
                .HasForeignKey(f => f.IdFinanciamiento)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuración de la tabla HistorialMedico
            modelBuilder.Entity<HistorialMedico>()
                .HasOne(hm => hm.Paciente)
                .WithMany(p => p.HistorialesMedicos)
                .HasForeignKey(hm => hm.IdPaciente)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de la tabla Financiamiento
            modelBuilder.Entity<Financiamiento>()
                .HasOne(f => f.Paciente)
                .WithMany(p => p.Financiamientos)
                .HasForeignKey(f => f.IdPaciente)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Financiamiento>()
                .HasOne(f => f.Tratamiento)
                .WithMany()
                .HasForeignKey(f => f.IdTratamiento)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de la tabla Contabilidad
            modelBuilder.Entity<Contabilidad>()
                .HasOne(c => c.Factura)
                .WithMany()
                .HasForeignKey(c => c.IdFactura)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Contabilidad>()
                .HasOne(c => c.Financiamiento)
                .WithMany()
                .HasForeignKey(c => c.IdFinanciamiento)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
