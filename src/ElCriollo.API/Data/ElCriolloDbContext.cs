using Microsoft.EntityFrameworkCore;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Data;

/// <summary>
/// Contexto principal de base de datos para El Criollo Restaurant
/// </summary>
public class ElCriolloDbContext : DbContext
{
    public ElCriolloDbContext(DbContextOptions<ElCriolloDbContext> options) : base(options)
    {
    }

    // ============================================================================
    // DBSETS - TABLAS DE LA BASE DE DATOS
    // ============================================================================

    /// <summary>
    /// Roles de usuario del sistema
    /// </summary>
    public DbSet<Rol> Roles { get; set; }

    /// <summary>
    /// Usuarios del sistema (autenticación)
    /// </summary>
    public DbSet<Usuario> Usuarios { get; set; }

    /// <summary>
    /// Empleados del restaurante
    /// </summary>
    public DbSet<Empleado> Empleados { get; set; }

    /// <summary>
    /// Clientes del restaurante
    /// </summary>
    public DbSet<Cliente> Clientes { get; set; }

    /// <summary>
    /// Mesas del restaurante
    /// </summary>
    public DbSet<Mesa> Mesas { get; set; }

    /// <summary>
    /// Reservaciones de mesas
    /// </summary>
    public DbSet<Reservacion> Reservaciones { get; set; }

    /// <summary>
    /// Categorías de productos del menú
    /// </summary>
    public DbSet<Categoria> Categorias { get; set; }

    /// <summary>
    /// Productos del menú (comida dominicana)
    /// </summary>
    public DbSet<Producto> Productos { get; set; }

    /// <summary>
    /// Control de inventario de productos
    /// </summary>
    public DbSet<Inventario> Inventario { get; set; }

    /// <summary>
    /// Historial de movimientos de inventario
    /// </summary>
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

    /// <summary>
    /// Combos especiales del restaurante
    /// </summary>
    public DbSet<Combo> Combos { get; set; }

    /// <summary>
    /// Relación entre combos y productos
    /// </summary>
    public DbSet<ComboProducto> ComboProductos { get; set; }

    /// <summary>
    /// Órdenes/comandas del restaurante
    /// </summary>
    public DbSet<Orden> Ordenes { get; set; }

    /// <summary>
    /// Detalles de las órdenes (productos/combos específicos)
    /// </summary>
    public DbSet<DetalleOrden> DetalleOrdenes { get; set; }

    /// <summary>
    /// Facturas generadas
    /// </summary>
    public DbSet<Factura> Facturas { get; set; }

    /// <summary>
    /// Historial de emails enviados
    /// </summary>
    public DbSet<EmailTransaccion> EmailTransacciones { get; set; }

    // ============================================================================
    // CONFIGURACIÓN DEL MODELO
    // ============================================================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================================================
        // CONFIGURACIÓN DE ENTIDADES
        // ============================================================================

        // Configurar Roles
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.RolID);
            entity.HasIndex(e => e.NombreRol).IsUnique();
            entity.Property(e => e.NombreRol).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.Estado).HasDefaultValue(true);
        });

        // Configurar Usuarios
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioID);
            entity.HasIndex(e => e.UsuarioNombre).IsUnique();
            entity.Property(e => e.UsuarioNombre).IsRequired().HasMaxLength(50).HasColumnName("Usuario");
            entity.Property(e => e.ContrasenaHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(70);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.RequiereCambioContrasena).HasDefaultValue(false);

            // Relación con Roles
            entity.HasOne(e => e.Rol)
                  .WithMany(r => r.Usuarios)
                  .HasForeignKey(e => e.RolID)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurar Empleados
        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.EmpleadoID);
            entity.HasIndex(e => e.Cedula).IsUnique();
            entity.Property(e => e.Cedula).IsRequired().HasMaxLength(16);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Sexo).HasMaxLength(15);
            entity.Property(e => e.Direccion).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(70);
            entity.Property(e => e.FechaIngreso).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Estado).HasDefaultValue(true);

            // Relación con Usuarios (opcional)
            entity.HasOne(e => e.Usuario)
                  .WithOne(u => u.Empleado)
                  .HasForeignKey<Empleado>(e => e.UsuarioID)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurar Clientes
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ClienteID);
            entity.HasIndex(e => e.Cedula).IsUnique().HasFilter("[Cedula] IS NOT NULL");
            entity.Property(e => e.Cedula).HasMaxLength(16);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Telefono).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(70);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Estado).HasDefaultValue(true);
        });

        // Configurar Mesas
        modelBuilder.Entity<Mesa>(entity =>
        {
            entity.HasKey(e => e.MesaID);
            entity.HasIndex(e => e.NumeroMesa).IsUnique();
            entity.Property(e => e.NumeroMesa).IsRequired();
            entity.Property(e => e.Capacidad).IsRequired();
            entity.Property(e => e.Ubicacion).HasMaxLength(50);
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Libre");
        });

        // Configurar Reservaciones
        modelBuilder.Entity<Reservacion>(entity =>
        {
            entity.HasKey(e => e.ReservacionID);
            entity.Property(e => e.CantidadPersonas).IsRequired();
            entity.Property(e => e.FechaYHora).IsRequired();
            entity.Property(e => e.DuracionEstimada).HasDefaultValue(120);
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETUTCDATE()");

            // Relaciones
            entity.HasOne(e => e.Mesa)
                  .WithMany(m => m.Reservaciones)
                  .HasForeignKey(e => e.MesaID)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Reservaciones)
                  .HasForeignKey(e => e.ClienteID)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar Categorías
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CategoriaID);
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.Estado).HasDefaultValue(true);
        });

        // Configurar Productos
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ProductoID);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.Precio).IsRequired().HasColumnType("decimal(10,2)");
            entity.Property(e => e.CostoPreparacion).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Imagen).HasMaxLength(255);
            entity.Property(e => e.Estado).HasDefaultValue(true);

            // Relación con Categorías
            entity.HasOne(e => e.Categoria)
                  .WithMany(c => c.Productos)
                  .HasForeignKey(e => e.CategoriaID)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurar Inventario
        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.InventarioID);
            entity.HasIndex(e => e.ProductoID).IsUnique();
            entity.Property(e => e.CantidadDisponible).HasDefaultValue(0);
            entity.Property(e => e.CantidadMinima).HasDefaultValue(5);
            entity.Property(e => e.UltimaActualizacion).HasDefaultValueSql("GETUTCDATE()");

            // Relación con Productos (1:1)
            entity.HasOne(e => e.Producto)
                  .WithOne(p => p.Inventario)
                  .HasForeignKey<Inventario>(e => e.ProductoID)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar MovimientosInventario
        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.MovimientoID);
            entity.Property(e => e.TipoMovimiento).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Cantidad).IsRequired();
            entity.Property(e => e.StockAnterior).IsRequired();
            entity.Property(e => e.StockResultante).IsRequired();
            entity.Property(e => e.CostoUnitario).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Referencia).HasMaxLength(100);
            entity.Property(e => e.Usuario).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.FechaMovimiento).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Motivo).HasMaxLength(200);
            entity.Property(e => e.Proveedor).HasMaxLength(200);

            // Relación con Productos
            entity.HasOne(e => e.Producto)
                  .WithMany()
                  .HasForeignKey(e => e.ProductoID)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índices para optimizar consultas
            entity.HasIndex(e => e.ProductoID);
            entity.HasIndex(e => e.FechaMovimiento);
            entity.HasIndex(e => e.TipoMovimiento);
            entity.HasIndex(e => new { e.ProductoID, e.FechaMovimiento });
        });

        // Configurar Combos
        modelBuilder.Entity<Combo>(entity =>
        {
            entity.HasKey(e => e.ComboID);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Precio).IsRequired().HasColumnType("decimal(10,2)");
            entity.Property(e => e.Descuento).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            entity.Property(e => e.Estado).HasDefaultValue(true);
        });

        // Configurar ComboProductos
        modelBuilder.Entity<ComboProducto>(entity =>
        {
            entity.HasKey(e => e.ComboProductoID);
            entity.Property(e => e.Cantidad).HasDefaultValue(1);

            // Relaciones
            entity.HasOne(e => e.Combo)
                  .WithMany(c => c.ComboProductos)
                  .HasForeignKey(e => e.ComboID)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                  .WithMany(p => p.ComboProductos)
                  .HasForeignKey(e => e.ProductoID)
                  .OnDelete(DeleteBehavior.Cascade);

            // Índice único para evitar duplicados combo-producto
            entity.HasIndex(e => new { e.ComboID, e.ProductoID }).IsUnique();
        });

        // Configurar Órdenes
        modelBuilder.Entity<Orden>(entity =>
        {
            entity.HasKey(e => e.OrdenID);
            entity.HasIndex(e => e.NumeroOrden).IsUnique();
            entity.Property(e => e.NumeroOrden).IsRequired().HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Pendiente");
            entity.Property(e => e.TipoOrden).IsRequired().HasMaxLength(20).HasDefaultValue("Mesa");
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            // Relaciones
            entity.HasOne(e => e.Mesa)
                  .WithMany(m => m.Ordenes)
                  .HasForeignKey(e => e.MesaID)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Ordenes)
                  .HasForeignKey(e => e.ClienteID)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Empleado)
                  .WithMany(emp => emp.Ordenes)
                  .HasForeignKey(e => e.EmpleadoID)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurar DetalleOrdenes
        modelBuilder.Entity<DetalleOrden>(entity =>
        {
            entity.HasKey(e => e.DetalleOrdenID);
            entity.Property(e => e.Cantidad).IsRequired();
            entity.Property(e => e.PrecioUnitario).IsRequired().HasColumnType("decimal(10,2)");
            entity.Property(e => e.Descuento).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10,2)")
                  .HasComputedColumnSql("[Cantidad] * [PrecioUnitario] - [Descuento]", stored: true);
            entity.Property(e => e.Observaciones).HasMaxLength(250);

            // Relaciones
            entity.HasOne(e => e.Orden)
                  .WithMany(o => o.DetalleOrdenes)
                  .HasForeignKey(e => e.OrdenID)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                  .WithMany(p => p.DetalleOrdenes)
                  .HasForeignKey(e => e.ProductoID)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Combo)
                  .WithMany(c => c.DetalleOrdenes)
                  .HasForeignKey(e => e.ComboID)
                  .OnDelete(DeleteBehavior.Restrict);

            // Constraint: debe tener ProductoID o ComboID pero no ambos
            entity.ToTable(t => t.HasCheckConstraint("CK_DetalleOrden_ProductoOrCombo", 
                "([ProductoID] IS NOT NULL AND [ComboID] IS NULL) OR ([ProductoID] IS NULL AND [ComboID] IS NOT NULL)"));
        });

        // Configurar Facturas
        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.FacturaID);
            entity.HasIndex(e => e.NumeroFactura).IsUnique();
            entity.Property(e => e.NumeroFactura).IsRequired().HasMaxLength(20);
            entity.Property(e => e.FechaFactura).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Subtotal).IsRequired().HasColumnType("decimal(10,2)");
            entity.Property(e => e.Impuesto).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            entity.Property(e => e.Descuento).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            entity.Property(e => e.Propina).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            entity.Property(e => e.Total).IsRequired().HasColumnType("decimal(10,2)");
            entity.Property(e => e.MetodoPago).IsRequired().HasMaxLength(20).HasDefaultValue("Efectivo");
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Pagada");
            entity.Property(e => e.ObservacionesPago).HasMaxLength(500);
            entity.Property(e => e.FechaPago);

            // Relaciones
            entity.HasOne(e => e.Orden)
                  .WithMany(o => o.Facturas)
                  .HasForeignKey(e => e.OrdenID)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Facturas)
                  .HasForeignKey(e => e.ClienteID)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Empleado)
                  .WithMany(emp => emp.Facturas)
                  .HasForeignKey(e => e.EmpleadoID)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurar EmailTransacciones
        modelBuilder.Entity<EmailTransaccion>(entity =>
        {
            entity.HasKey(e => e.EmailID);
            entity.Property(e => e.DestinatarioEmail).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Asunto).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Mensaje).HasColumnType("TEXT");
            entity.Property(e => e.TipoEmail).HasMaxLength(50);
            entity.Property(e => e.FechaEnvio).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Pendiente");
        });

        // ============================================================================
        // DATOS INICIALES (SEEDING)
        // ============================================================================

        // Seed Roles
        modelBuilder.Entity<Rol>().HasData(
            new Rol { RolID = 1, NombreRol = "Administrador", Descripcion = "Control total del sistema", Estado = true },
            new Rol { RolID = 2, NombreRol = "Recepcion", Descripcion = "Gestión de reservas y mesas", Estado = true },
            new Rol { RolID = 3, NombreRol = "Mesero", Descripcion = "Toma de órdenes y atención al cliente", Estado = true },
            new Rol { RolID = 4, NombreRol = "Cajero", Descripcion = "Procesamiento de pagos y facturación", Estado = true }
        );

        // El usuario administrador se creará via código en el startup
        // para manejar el hash de contraseña correctamente
    }

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Guarda los cambios con manejo automático de fechas de auditoría
    /// </summary>
    public override int SaveChanges()
    {
        ActualizarFechasAuditoria();
        return base.SaveChanges();
    }

    /// <summary>
    /// Guarda los cambios de forma asíncrona con manejo automático de fechas de auditoría
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ActualizarFechasAuditoria();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Actualiza automáticamente las fechas de auditoría
    /// </summary>
    private void ActualizarFechasAuditoria()
    {
        var entradas = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entrada in entradas)
        {
            // Actualizar UltimaActualizacion para inventarios
            if (entrada.Entity is Inventario inventario && entrada.State == EntityState.Modified)
            {
                inventario.UltimaActualizacion = DateTime.UtcNow;
            }

            // Actualizar UltimoAcceso para usuarios en login
            if (entrada.Entity is Usuario usuario && entrada.Property("UltimoAcceso").IsModified)
            {
                usuario.UltimoAcceso = DateTime.UtcNow;
            }
        }
    }
}