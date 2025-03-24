using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.DATA
{
    public class MinombredeconexionDbContext : DbContext
    {
        public MinombredeconexionDbContext(DbContextOptions<MinombredeconexionDbContext> options)
            : base(options) { }

        // =======================================
        //             DbSet de tablas
        // =======================================
        public DbSet<UsuariosModel> Usuarios { get; set; }
        public DbSet<RolesModel> Roles { get; set; }
        public DbSet<PacientesModel> Pacientes { get; set; }
        public DbSet<EmpleadosModel> Empleados { get; set; }
        public DbSet<CitasModel> Citas { get; set; }
        public DbSet<ServiciosModel> Servicios { get; set; }
        public DbSet<ProductosModel> Productos { get; set; }
        public DbSet<TratamientosModel> Tratamientos { get; set; }
        public DbSet<HistorialMedicoModel> HistorialMedico { get; set; }

        // Carrito (encabezado) + ítems
        public DbSet<CarritoModel> Carrito { get; set; }
        public DbSet<CarritoItemModel> CarritoItems { get; set; }

        // Compras (encabezado) + detalle
        public DbSet<CompraModel> Compras { get; set; }
        public DbSet<DetalleCompraModel> DetallesCompra { get; set; }

        // Factura (encabezado) + detalle
        public DbSet<FacturaModel> Facturas { get; set; }
        public DbSet<DetalleFacturaModel> DetallesFactura { get; set; }

        // Otros catálogos/tablas auxiliares
        public DbSet<FinanciamientoModel> Financiamientos { get; set; }
        public DbSet<MetodoPagoModel> MetodoDePago { get; set; }
        public DbSet<DescuentoModel> Descuentos { get; set; }


        public DbSet<ContabilidadModel> Contabilidad { get; set; }
        public DbSet<HistorialComprasModel> HistorialCompras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ========================
            // 1) Mapeo de tablas
            // ========================
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
            modelBuilder.Entity<CarritoItemModel>().ToTable("carrito_item");

            modelBuilder.Entity<CompraModel>().ToTable("compra");
            modelBuilder.Entity<DetalleCompraModel>().ToTable("detalle_compra");

            modelBuilder.Entity<FacturaModel>().ToTable("factura");
            modelBuilder.Entity<DetalleFacturaModel>().ToTable("detalle_factura");

            modelBuilder.Entity<FinanciamientoModel>().ToTable("Financiamiento");
            modelBuilder.Entity<MetodoPagoModel>().ToTable("metodopago");
            modelBuilder.Entity<DescuentoModel>().ToTable("descuento");
            modelBuilder.Entity<ContabilidadModel>().ToTable("contabilidad");
            modelBuilder.Entity<HistorialComprasModel>().ToTable("historial_compras");


            // ========================
            // 2) Relaciones (Fluent API)
            // ========================

            // ---------- USUARIOS - ROLES ----------
            // (Muchos Usuarios -> 1 Rol)
            modelBuilder.Entity<UsuariosModel>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .HasConstraintName("FK_Usuarios_Roles");

            // ---------- USUARIO - PACIENTE (1 a 1) ----------
            modelBuilder.Entity<UsuariosModel>()
                .HasOne(u => u.Paciente)
                .WithOne(p => p.Usuario)
                .HasForeignKey<PacientesModel>(p => p.IdUsuario)
                .HasConstraintName("FK_Pacientes_Usuarios");

            // ---------- USUARIO - EMPLEADO (1 a 1) ----------
            modelBuilder.Entity<UsuariosModel>()
                .HasOne(u => u.Empleado)
                .WithOne(e => e.Usuario)
                .HasForeignKey<EmpleadosModel>(e => e.IdUsuario)
                .HasConstraintName("FK_Empleados_Usuarios");

            // ---------- PACIENTE - CITAS (1 a muchos) ----------
            modelBuilder.Entity<CitasModel>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.IdPaciente)
                .HasConstraintName("FK_Citas_Pacientes");

            // ---------- EMPLEADO - CITAS (1 a muchos) ----------
            modelBuilder.Entity<CitasModel>()
                .HasOne(c => c.Empleado)
                .WithMany(e => e.Citas)
                .HasForeignKey(c => c.IdEmpleado)
                .HasConstraintName("FK_Citas_Empleados");

            // ---------- SERVICIOS - TRATAMIENTOS (1 a muchos) ----------
            modelBuilder.Entity<TratamientosModel>()
                .HasOne(t => t.Servicio)
                .WithMany(s => s.Tratamientos)
                .HasForeignKey(t => t.IdServicio)
                .HasConstraintName("FK_Tratamientos_Servicios");

            // ---------- PACIENTE - HISTORIAL MÉDICO (1 a muchos) ----------
            modelBuilder.Entity<HistorialMedicoModel>()
                .HasOne(h => h.Paciente)
                .WithMany(p => p.HistorialesMedicos)
                .HasForeignKey(h => h.IdPaciente)
                .HasConstraintName("FK_HistorialMedico_Pacientes");

            // ---------- EMPLEADO - HISTORIAL MÉDICO (1 a muchos) ----------
            modelBuilder.Entity<HistorialMedicoModel>()
                .HasOne(h => h.Empleado)
                .WithMany()
                .HasForeignKey(h => h.IdEmpleado)
                .HasConstraintName("FK_HistorialMedico_Empleados");

            // ---------- HISTORIAL MÉDICO - TRATAMIENTOS (1 a muchos o 1 a 1) ----------
            modelBuilder.Entity<HistorialMedicoModel>()
                .HasOne(h => h.Tratamiento)
                .WithMany(t => t.HistorialesMedicos)
                .HasForeignKey(h => h.IdTratamiento)
                .HasConstraintName("FK_HistorialMedico_Tratamientos");

            // ---------------------------------------------------
            // CARRO DE COMPRAS
            // Encabezado: CarritoModel (tiene IdPaciente)
            // Detalle: CarritoItemModel (tiene IdCarrito, IdProducto, IdServicio)
            // ---------------------------------------------------
            modelBuilder.Entity<CarritoItemModel>()
                .HasOne(ci => ci.Carrito)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.IdCarrito)
                .HasConstraintName("FK_CarritoItems_Carrito");

            // Opcional: CarritoItem - Producto (1 a 1 o muchos a 1)
            modelBuilder.Entity<CarritoItemModel>()
                .HasOne(ci => ci.Producto)
                .WithMany()
                .HasForeignKey(ci => ci.IdProducto)
                .HasConstraintName("FK_CarritoItem_Productos")
                .OnDelete(DeleteBehavior.Cascade);

            // Opcional: CarritoItem - Servicio
            modelBuilder.Entity<CarritoItemModel>()
                .HasOne(ci => ci.Servicio)
                .WithMany()
                .HasForeignKey(ci => ci.IdServicio)
                .HasConstraintName("FK_CarritoItem_Servicios")
                .OnDelete(DeleteBehavior.Cascade);

            // ---------------------------------------------------
            // COMPRAS
            // Encabezado: CompraModel (IdPaciente, IdFinanciamiento)
            // Detalle: DetalleCompraModel (IdCompra, IdProducto, IdServicio)
            // ---------------------------------------------------
            modelBuilder.Entity<DetalleCompraModel>()
                .HasOne(dc => dc.Compra)
                .WithMany(c => c.Detalles)
                .HasForeignKey(dc => dc.IdCompra)
                .HasConstraintName("FK_DetalleCompra_Compra")
                .OnDelete(DeleteBehavior.Cascade);

            // Opcional: DetalleCompra - Producto
            modelBuilder.Entity<DetalleCompraModel>()
                .HasOne(dc => dc.Producto)
                .WithMany()
                .HasForeignKey(dc => dc.IdProducto)
                .HasConstraintName("FK_DetalleCompra_Productos")
                .OnDelete(DeleteBehavior.Cascade);

            // Opcional: DetalleCompra - Servicio
            modelBuilder.Entity<DetalleCompraModel>()
                .HasOne(dc => dc.Servicio)
                .WithMany()
                .HasForeignKey(dc => dc.IdServicio)
                .HasConstraintName("FK_DetalleCompra_Servicios")
                .OnDelete(DeleteBehavior.Cascade);

            // ---------------------------------------------------
            // FACTURAS
            // Encabezado: FacturaModel (IdCompra, IdMetodoPago, IdDescuento, IdFinanciamiento)
            // Detalle: DetalleFacturaModel (IdFactura, IdProducto, IdServicio)
            // ---------------------------------------------------
            // Factura - Compra (1 a 1 o 1 a muchos)
            modelBuilder.Entity<FacturaModel>()
     .HasOne(f => f.Compra)
     .WithMany(c => c.Facturas)
     .HasForeignKey(f => f.IdCompra)
     .HasConstraintName("FK_Facturas_Compras")
     .OnDelete(DeleteBehavior.Cascade);


            // Factura - MetodoPago
            modelBuilder.Entity<MetodoPagoModel>()
                .HasKey(m => m.IdMetodoPago);  // PK

            modelBuilder.Entity<FacturaModel>()
                .HasOne(f => f.MetodoPago)
                .WithMany(d => d.Facturas)  // En MetodoPagoModel => public virtual ICollection<FacturasModel.FacturaModel> Facturas { get; set; }
                .HasForeignKey(f => f.IdMetodoPago)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Facturas_MetodoDePago");

            // Factura - Financiamiento (opcional)
            modelBuilder.Entity<FacturaModel>()
                .HasOne(f => f.Financiamiento)
                .WithMany()
                .HasForeignKey(f => f.IdFinanciamiento)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Facturas_Financiamiento");

            // Factura - Descuento (opcional)
            modelBuilder.Entity<FacturaModel>()
       .HasOne(f => f.Descuento)
       .WithMany() // Sin especificar la propiedad
       .HasForeignKey(f => f.IdDescuento)
       .OnDelete(DeleteBehavior.SetNull)
       .HasConstraintName("FK_Facturas_Descuentos");


            // Factura - DetalleFactura (1 a muchos)
            modelBuilder.Entity<FacturaModel>()
                .HasMany(f => f.DetallesFactura)
                .WithOne(d => d.Factura)
                .HasForeignKey(d => d.IdFactura)
                .HasConstraintName("FK_DetalleFactura_Factura")
                .OnDelete(DeleteBehavior.Cascade);

            // DetalleFactura - Producto (opcional)
            modelBuilder.Entity<DetalleFacturaModel>()
                .HasOne(df => df.Producto)
                .WithMany()
                .HasForeignKey(df => df.IdProducto)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DetalleFactura_Productos");

            // DetalleFactura - Servicio (opcional)
            modelBuilder.Entity<DetalleFacturaModel>()
                .HasOne(df => df.Servicio)
                .WithMany()
                .HasForeignKey(df => df.IdServicio)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DetalleFactura_Servicios");

            // ---------------------------------------------------
            // FINANCIAMIENTO
            // Encabezado: FinanciamientoModel (IdPaciente, IdTratamiento)
            // ---------------------------------------------------
            // Financiamiento - Paciente (1 a muchos)
            modelBuilder.Entity<FinanciamientoModel>()
                .HasOne(f => f.Paciente)
                .WithMany()
                .HasForeignKey(f => f.IdPaciente)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Financiamiento_Pacientes");

            // Financiamiento - Tratamiento (1 a muchos)
            modelBuilder.Entity<FinanciamientoModel>()
                .HasOne(f => f.Tratamiento)
                .WithMany(t => t.Financiamientos) // si en TratamientosModel defines una ICollection<FinanciamientoModel>
                .HasForeignKey(f => f.IdTratamiento)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Financiamiento_Tratamientos");

            // ---------------------------------------------------
            // METODOPAGO (se definió PK arriba). 
            // Elimino relación con Financiamiento si no existe la columna en MetodoPagoModel.

            // ---------------------------------------------------
            // HISTORIAL_COMPRAS, CONTABILIDAD, etc.
            // (Configura si necesitas relaciones extras)

            base.OnModelCreating(modelBuilder);
        }
    }
}
