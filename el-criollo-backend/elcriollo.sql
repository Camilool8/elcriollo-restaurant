-- =============================================
-- Script de Creación de Base de Datos: El Criollo Restaurant
-- Sistema de Gestión para Restaurante Dominicano
-- Versión: 2.1 - IDEMPOTENTE con GO batches
-- Fecha: Diciembre 2024
-- =============================================

-- Configuración inicial
USE master;
GO

-- Crear base de datos solo si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ElCriolloRestaurante')
BEGIN
    CREATE DATABASE ElCriolloRestaurante;
    PRINT 'Base de datos ElCriolloRestaurante creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Base de datos ElCriolloRestaurante ya existe.';
END
GO

USE ElCriolloRestaurante;
GO

-- =============================================
-- CREACIÓN DE TABLAS (EN ORDEN DE DEPENDENCIAS)
-- =============================================

-- 1. Tabla Roles
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
    CREATE TABLE Roles (
        RolID INT IDENTITY(1,1) PRIMARY KEY,
        NombreRol VARCHAR(50) NOT NULL UNIQUE,
        Descripcion VARCHAR(200) NULL,
        Estado BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla Roles creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Roles ya existe.';
END
GO

-- 2. Tabla Usuarios
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND type in (N'U'))
BEGIN
    CREATE TABLE Usuarios (
        UsuarioID INT IDENTITY(1,1) PRIMARY KEY,
        Usuario VARCHAR(50) NOT NULL UNIQUE,
        ContrasenaHash VARCHAR(500) NOT NULL,
        RolID INT NOT NULL,
        Email VARCHAR(70) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        UltimoAcceso DATETIME NULL,
        Estado BIT NOT NULL DEFAULT 1,
        RequiereCambioContrasena BIT NOT NULL DEFAULT 0,
        RefreshToken VARCHAR(500) NULL,
        RefreshTokenExpiry DATETIME NULL,
        EmpleadoID INT NULL,
        CONSTRAINT FK_Usuarios_Rol FOREIGN KEY (RolID) REFERENCES Roles(RolID)
    );
    PRINT 'Tabla Usuarios creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Usuarios ya existe.';
END
GO

-- 3. Tabla Empleados
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Empleados]') AND type in (N'U'))
BEGIN
    CREATE TABLE Empleados (
        EmpleadoID INT IDENTITY(1,1) PRIMARY KEY,
        Cedula VARCHAR(16) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Apellido VARCHAR(50) NOT NULL,
        Sexo VARCHAR(15) NULL,
        Direccion VARCHAR(100) NULL,
        Telefono VARCHAR(50) NULL,
        Email VARCHAR(70) NULL,
        FechaNacimiento DATE NULL,
        PreferenciasComida VARCHAR(500) NULL,
        Salario DECIMAL(18,2) NULL,
        Departamento VARCHAR(50) NULL,
        FechaIngreso DATE NOT NULL DEFAULT GETDATE(),
        UsuarioID INT NULL,
        Estado VARCHAR(20) NOT NULL DEFAULT 'Activo',
        CONSTRAINT FK_Empleados_Usuario FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID)
    );
    PRINT 'Tabla Empleados creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Empleados ya existe.';
END
GO

-- Agregar FK de Usuario a Empleado (relación bidireccional) solo si no existe
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Usuarios_Empleado')
BEGIN
    ALTER TABLE Usuarios ADD CONSTRAINT FK_Usuarios_Empleado 
        FOREIGN KEY (EmpleadoID) REFERENCES Empleados(EmpleadoID);
    PRINT 'Constraint FK_Usuarios_Empleado agregada.';
END
ELSE
BEGIN
    PRINT 'Constraint FK_Usuarios_Empleado ya existe.';
END
GO

-- 4. Tabla Clientes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND type in (N'U'))
BEGIN
    CREATE TABLE Clientes (
        ClienteID INT IDENTITY(1,1) PRIMARY KEY,
        Cedula VARCHAR(16) NULL,
        Nombre VARCHAR(50) NOT NULL,
        Apellido VARCHAR(50) NOT NULL,
        Telefono VARCHAR(50) NULL,
        Email VARCHAR(70) NULL,
        Direccion VARCHAR(200) NULL,
        FechaNacimiento DATE NULL,
        PreferenciasComida VARCHAR(500) NULL,
        FechaRegistro DATE NOT NULL DEFAULT GETDATE(),
        Estado VARCHAR(20) NOT NULL DEFAULT 'Activo'
    );
    PRINT 'Tabla Clientes creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Clientes ya existe.';
END
GO

-- 5. Tabla Mesas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Mesas]') AND type in (N'U'))
BEGIN
    CREATE TABLE Mesas (
        MesaID INT IDENTITY(1,1) PRIMARY KEY,
        NumeroMesa INT NOT NULL UNIQUE,
        Capacidad INT NOT NULL,
        Ubicacion VARCHAR(50) NULL,
        Estado VARCHAR(20) NOT NULL DEFAULT 'Libre',
        FechaUltimaLimpieza DATETIME NULL,
        FechaUltimaActualizacion DATETIME NULL
    );
    PRINT 'Tabla Mesas creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Mesas ya existe.';
END
GO

-- 6. Tabla Reservaciones
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reservaciones]') AND type in (N'U'))
BEGIN
    CREATE TABLE Reservaciones (
        ReservacionID INT IDENTITY(1,1) PRIMARY KEY,
        MesaID INT NOT NULL,
        ClienteID INT NOT NULL,
        CantidadPersonas INT NOT NULL,
        FechaYHora DATETIME NOT NULL,
        DuracionEstimada INT NOT NULL DEFAULT 120,
        Observaciones VARCHAR(500) NULL,
        Estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        UsuarioCreacion INT NULL,
        FechaModificacion DATETIME NULL,
        CONSTRAINT FK_Reservaciones_Mesa FOREIGN KEY (MesaID) REFERENCES Mesas(MesaID),
        CONSTRAINT FK_Reservaciones_Cliente FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID),
        CONSTRAINT FK_Reservaciones_Usuario FOREIGN KEY (UsuarioCreacion) REFERENCES Usuarios(UsuarioID)
    );
    PRINT 'Tabla Reservaciones creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Reservaciones ya existe.';
END
GO

-- 7. Tabla Categorias
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categorias]') AND type in (N'U'))
BEGIN
    CREATE TABLE Categorias (
        CategoriaID INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(50) NOT NULL UNIQUE,
        Descripcion VARCHAR(200) NULL,
        Estado BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla Categorias creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Categorias ya existe.';
END
GO

-- 8. Tabla Productos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Productos]') AND type in (N'U'))
BEGIN
    CREATE TABLE Productos (
        ProductoID INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(100) NOT NULL,
        Descripcion VARCHAR(200) NULL,
        CategoriaID INT NOT NULL,
        Precio DECIMAL(10,2) NOT NULL,
        CostoPreparacion DECIMAL(10,2) NULL,
        TiempoPreparacion INT NULL,
        Imagen VARCHAR(255) NULL,
        Estado BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_Productos_Categoria FOREIGN KEY (CategoriaID) REFERENCES Categorias(CategoriaID)
    );
    PRINT 'Tabla Productos creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Productos ya existe.';
END
GO

-- 9. Tabla Inventario
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Inventario]') AND type in (N'U'))
BEGIN
    CREATE TABLE Inventario (
        InventarioID INT IDENTITY(1,1) PRIMARY KEY,
        ProductoID INT NOT NULL,
        CantidadDisponible INT NOT NULL DEFAULT 0,
        CantidadMinima INT NOT NULL DEFAULT 5,
        UltimaActualizacion DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Inventario_Producto FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
    );
    PRINT 'Tabla Inventario creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Inventario ya existe.';
END
GO

-- 10. Tabla Combos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Combos]') AND type in (N'U'))
BEGIN
    CREATE TABLE Combos (
        ComboID INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(100) NOT NULL,
        Descripcion VARCHAR(500) NULL,
        Precio DECIMAL(10,2) NOT NULL,
        Descuento DECIMAL(10,2) NOT NULL DEFAULT 0,
        Estado BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla Combos creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Combos ya existe.';
END
GO

-- 11. Tabla ComboProductos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ComboProductos]') AND type in (N'U'))
BEGIN
    CREATE TABLE ComboProductos (
        ComboProductoID INT IDENTITY(1,1) PRIMARY KEY,
        ComboID INT NOT NULL,
        ProductoID INT NOT NULL,
        Cantidad INT NOT NULL DEFAULT 1,
        CONSTRAINT FK_ComboProductos_Combo FOREIGN KEY (ComboID) REFERENCES Combos(ComboID),
        CONSTRAINT FK_ComboProductos_Producto FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
    );
    PRINT 'Tabla ComboProductos creada.';
END
ELSE
BEGIN
    PRINT 'Tabla ComboProductos ya existe.';
END
GO

-- 12. Tabla Ordenes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ordenes]') AND type in (N'U'))
BEGIN
    CREATE TABLE Ordenes (
        OrdenID INT IDENTITY(1,1) PRIMARY KEY,
        NumeroOrden VARCHAR(20) NOT NULL UNIQUE,
        MesaID INT NULL,
        ClienteID INT NULL,
        EmpleadoID INT NOT NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        Estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
        TipoOrden VARCHAR(20) NOT NULL DEFAULT 'Mesa',
        Observaciones VARCHAR(500) NULL,
        SubtotalCalculado DECIMAL(18,2) NOT NULL DEFAULT 0,
        Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalCalculado DECIMAL(18,2) NOT NULL DEFAULT 0,
        CONSTRAINT FK_Ordenes_Mesa FOREIGN KEY (MesaID) REFERENCES Mesas(MesaID),
        CONSTRAINT FK_Ordenes_Cliente FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID),
        CONSTRAINT FK_Ordenes_Empleado FOREIGN KEY (EmpleadoID) REFERENCES Empleados(EmpleadoID)
    );
    PRINT 'Tabla Ordenes creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Ordenes ya existe.';
END
GO

-- 13. Tabla DetalleOrdenes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DetalleOrdenes]') AND type in (N'U'))
BEGIN
    CREATE TABLE DetalleOrdenes (
        DetalleOrdenID INT IDENTITY(1,1) PRIMARY KEY,
        OrdenID INT NOT NULL,
        ProductoID INT NULL,
        ComboID INT NULL,
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(10,2) NOT NULL,
        Descuento DECIMAL(10,2) NOT NULL DEFAULT 0,
        Subtotal AS (Cantidad * PrecioUnitario - Descuento) PERSISTED,
        Observaciones VARCHAR(250) NULL,
        CONSTRAINT FK_DetalleOrdenes_Orden FOREIGN KEY (OrdenID) REFERENCES Ordenes(OrdenID),
        CONSTRAINT FK_DetalleOrdenes_Producto FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID),
        CONSTRAINT FK_DetalleOrdenes_Combo FOREIGN KEY (ComboID) REFERENCES Combos(ComboID)
    );
    PRINT 'Tabla DetalleOrdenes creada.';
END
ELSE
BEGIN
    PRINT 'Tabla DetalleOrdenes ya existe.';
END
GO

-- 14. Tabla Facturas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Facturas]') AND type in (N'U'))
BEGIN
    CREATE TABLE Facturas (
        FacturaID INT IDENTITY(1,1) PRIMARY KEY,
        NumeroFactura VARCHAR(20) NOT NULL UNIQUE,
        OrdenID INT NOT NULL,
        ClienteID INT NOT NULL,
        EmpleadoID INT NOT NULL,
        FechaFactura DATETIME NOT NULL DEFAULT GETDATE(),
        FechaPago DATETIME NULL,
        Subtotal DECIMAL(10,2) NOT NULL,
        Impuesto DECIMAL(10,2) NOT NULL DEFAULT 0,
        Descuento DECIMAL(10,2) NOT NULL DEFAULT 0,
        Propina DECIMAL(10,2) NOT NULL DEFAULT 0,
        Total DECIMAL(10,2) NOT NULL,
        MetodoPago VARCHAR(20) NOT NULL DEFAULT 'Efectivo',
        Estado VARCHAR(20) NOT NULL DEFAULT 'Pagada',
        ObservacionesPago VARCHAR(500) NULL,
        CONSTRAINT FK_Facturas_Orden FOREIGN KEY (OrdenID) REFERENCES Ordenes(OrdenID),
        CONSTRAINT FK_Facturas_Cliente FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID),
        CONSTRAINT FK_Facturas_Empleado FOREIGN KEY (EmpleadoID) REFERENCES Empleados(EmpleadoID)
    );
    PRINT 'Tabla Facturas creada.';
END
ELSE
BEGIN
    PRINT 'Tabla Facturas ya existe.';
END
GO

-- 15. Tabla EmailTransacciones
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailTransacciones]') AND type in (N'U'))
BEGIN
    CREATE TABLE EmailTransacciones (
        EmailID INT IDENTITY(1,1) PRIMARY KEY,
        DestinatarioEmail VARCHAR(100) NOT NULL,
        Asunto VARCHAR(200) NOT NULL,
        Mensaje TEXT NULL,
        TipoEmail VARCHAR(50) NULL,
        ReferenciaID INT NULL,
        FechaEnvio DATETIME NOT NULL DEFAULT GETDATE(),
        Estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
        MensajeError VARCHAR(500) NULL,
        IntentosEnvio INT NOT NULL DEFAULT 0
    );
    PRINT 'Tabla EmailTransacciones creada.';
END
ELSE
BEGIN
    PRINT 'Tabla EmailTransacciones ya existe.';
END
GO

-- 16. Tabla MovimientosInventario
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MovimientosInventario]') AND type in (N'U'))
BEGIN
    CREATE TABLE MovimientosInventario (
        MovimientoID INT IDENTITY(1,1) PRIMARY KEY,
        ProductoID INT NOT NULL,
        TipoMovimiento VARCHAR(20) NOT NULL,
        Cantidad INT NOT NULL,
        StockAnterior INT NOT NULL,
        StockResultante INT NOT NULL,
        CostoUnitario DECIMAL(10,2) NULL,
        Referencia VARCHAR(100) NULL,
        Usuario VARCHAR(100) NOT NULL,
        Observaciones VARCHAR(500) NULL,
        FechaMovimiento DATETIME NOT NULL DEFAULT GETDATE(),
        Motivo VARCHAR(200) NULL,
        Proveedor VARCHAR(200) NULL,
        CONSTRAINT FK_MovimientosInventario_Producto FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
    );
    PRINT 'Tabla MovimientosInventario creada.';
END
ELSE
BEGIN
    PRINT 'Tabla MovimientosInventario ya existe.';
END
GO

-- =============================================
-- INSERCIÓN DE DATOS INICIALES (IDEMPOTENTE)
-- =============================================

-- Insertar Roles del sistema (solo si no existen)
IF NOT EXISTS (SELECT 1 FROM Roles WHERE NombreRol = 'Administrador')
BEGIN
    INSERT INTO Roles (NombreRol, Descripcion) VALUES 
    ('Administrador', 'Control total del sistema y gestión de usuarios');
    PRINT 'Rol Administrador insertado.';
END
ELSE
BEGIN
    PRINT 'Rol Administrador ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Roles WHERE NombreRol = 'Recepcion')
BEGIN
    INSERT INTO Roles (NombreRol, Descripcion) VALUES 
    ('Recepcion', 'Gestión de reservas, mesas y atención al cliente');
    PRINT 'Rol Recepcion insertado.';
END
ELSE
BEGIN
    PRINT 'Rol Recepcion ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Roles WHERE NombreRol = 'Mesero')
BEGIN
    INSERT INTO Roles (NombreRol, Descripcion) VALUES 
    ('Mesero', 'Toma de órdenes y atención directa al cliente');
    PRINT 'Rol Mesero insertado.';
END
ELSE
BEGIN
    PRINT 'Rol Mesero ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Roles WHERE NombreRol = 'Cajero')
BEGIN
    INSERT INTO Roles (NombreRol, Descripcion) VALUES 
    ('Cajero', 'Procesamiento de pagos y facturación');
    PRINT 'Rol Cajero insertado.';
END
ELSE
BEGIN
    PRINT 'Rol Cajero ya existe.';
END
GO

-- Insertar Categorías de productos dominicanos (solo si no existen)
DECLARE @CategoriaData TABLE (Nombre VARCHAR(50), Descripcion VARCHAR(200));
INSERT INTO @CategoriaData VALUES 
('Platos Principales', 'Comidas fuertes tradicionales dominicanas'),
('Acompañamientos', 'Arroz, habichuelas y otros acompañantes típicos'),
('Frituras', 'Tostones, yuca frita y otras frituras tradicionales'),
('Bebidas', 'Jugos naturales, refrescos y bebidas típicas dominicanas'),
('Postres', 'Dulces y postres tradicionales dominicanos'),
('Desayunos', 'Desayunos típicos dominicanos'),
('Sopas', 'Sopas y caldos tradicionales dominicanos'),
('Mariscos', 'Pescados y mariscos preparados al estilo dominicano');

INSERT INTO Categorias (Nombre, Descripcion)
SELECT cd.Nombre, cd.Descripcion
FROM @CategoriaData cd
WHERE NOT EXISTS (SELECT 1 FROM Categorias c WHERE c.Nombre = cd.Nombre);

PRINT 'Categorías de productos verificadas/insertadas.';
GO

-- Insertar Mesas del restaurante (solo si no existen)
DECLARE @MesaData TABLE (NumeroMesa INT, Capacidad INT, Ubicacion VARCHAR(50));
INSERT INTO @MesaData VALUES 
(1, 2, 'Terraza'), (2, 4, 'Terraza'), (3, 4, 'Salon Principal'),
(4, 6, 'Salon Principal'), (5, 8, 'Salon Principal'), (6, 2, 'Area VIP'),
(7, 4, 'Area VIP'), (8, 6, 'Area VIP'), (9, 4, 'Patio'),
(10, 2, 'Patio'), (11, 4, 'Salon Principal'), (12, 6, 'Salon Principal');

INSERT INTO Mesas (NumeroMesa, Capacidad, Ubicacion)
SELECT md.NumeroMesa, md.Capacidad, md.Ubicacion
FROM @MesaData md
WHERE NOT EXISTS (SELECT 1 FROM Mesas m WHERE m.NumeroMesa = md.NumeroMesa);

PRINT 'Mesas del restaurante verificadas/insertadas.';
GO

-- Insertar Productos de comida dominicana (solo si no existen)
-- Primero verificamos que las categorías existan
IF EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Platos Principales')
BEGIN
    -- Platos Principales
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Pollo Guisado', 'Pollo guisado con vegetales al estilo dominicano', c.CategoriaID, 350.00, 180.00, 30
    FROM Categorias c 
    WHERE c.Nombre = 'Platos Principales' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Pollo Guisado');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Pernil al Horno', 'Cerdo al horno con especias dominicanas', c.CategoriaID, 420.00, 220.00, 45
    FROM Categorias c 
    WHERE c.Nombre = 'Platos Principales' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Pernil al Horno');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Rabo Encendido', 'Rabo de res guisado picante tradicional', c.CategoriaID, 450.00, 250.00, 60
    FROM Categorias c 
    WHERE c.Nombre = 'Platos Principales' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Rabo Encendido');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Chivo Guisado', 'Cabrito guisado con especias criollas', c.CategoriaID, 480.00, 280.00, 50
    FROM Categorias c 
    WHERE c.Nombre = 'Platos Principales' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Chivo Guisado');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Costillas BBQ Criolla', 'Costillas con salsa criolla dominicana', c.CategoriaID, 520.00, 300.00, 40
    FROM Categorias c 
    WHERE c.Nombre = 'Platos Principales' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Costillas BBQ Criolla');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Pollo al Carbon', 'Pollo asado al carbón con sazón criolla', c.CategoriaID, 380.00, 200.00, 35
    FROM Categorias c 
    WHERE c.Nombre = 'Platos Principales' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Pollo al Carbon');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Carne de Res Guisada', 'Carne de res guisada con vegetales', c.CategoriaID, 420.00, 230.00, 40
    FROM Categorias c 
    WHERE c.Nombre = 'Platos Principales' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Carne de Res Guisada');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Pollo al Horno', 'Pollo al horno con hierbas y especias', c.CategoriaID, 350.00, 180.00, 45
    FROM Categorias c 
    WHERE c.Nombre = 'Platos Principales' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Pollo al Horno');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Longaniza Dominicana', 'Longaniza dominicana a la parrilla', c.CategoriaID, 280.00, 140.00, 20
    FROM Categorias c 
    WHERE c.Nombre = 'Platos Principales' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Longaniza Dominicana');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Chuleta Ahumada', 'Chuleta de cerdo ahumada dominicana', c.CategoriaID, 320.00, 160.00, 25
    FROM Categorias c 
    WHERE c.Nombre = 'Platos Principales' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Chuleta Ahumada');
    
    PRINT 'Platos principales verificados/insertados.';
END
GO

-- Acompañamientos
IF EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Acompañamientos')
BEGIN
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Arroz Blanco', 'Arroz blanco tradicional dominicano', c.CategoriaID, 80.00, 30.00, 15
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Arroz Blanco');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Habichuelas Rojas', 'Habichuelas rojas guisadas con sofrito', c.CategoriaID, 100.00, 40.00, 25
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Habichuelas Rojas');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Moro de Guandules', 'Arroz con guandules y coco', c.CategoriaID, 120.00, 50.00, 30
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Moro de Guandules');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Ensalada Verde', 'Ensalada mixta fresca con vinagreta', c.CategoriaID, 90.00, 35.00, 10
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Ensalada Verde');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Yuca Hervida', 'Yuca hervida con cebollitas salteadas', c.CategoriaID, 70.00, 25.00, 20
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Yuca Hervida');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Moro de Habichuelas', 'Arroz mezclado con habichuelas', c.CategoriaID, 110.00, 45.00, 25
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Moro de Habichuelas');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Arroz con Pollo', 'Arroz amarillo con pollo y vegetales', c.CategoriaID, 180.00, 85.00, 30
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Arroz con Pollo');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Habichuelas Negras', 'Habichuelas negras guisadas', c.CategoriaID, 105.00, 42.00, 25
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Habichuelas Negras');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Puré de Papa', 'Puré de papa cremoso', c.CategoriaID, 85.00, 32.00, 20
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Puré de Papa');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Aguacate', 'Aguacate fresco en rodajas', c.CategoriaID, 65.00, 35.00, 5
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Aguacate');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Vegetales Salteados', 'Vegetales mixtos salteados', c.CategoriaID, 95.00, 40.00, 15
    FROM Categorias c 
    WHERE c.Nombre = 'Acompañamientos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Vegetales Salteados');
    
    PRINT 'Acompañamientos verificados/insertados.';
END
GO

-- Continuar con las demás categorías de productos...
-- (Frituras, Bebidas, Postres, Desayunos, Sopas, Mariscos)

-- Frituras
IF EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Frituras')
BEGIN
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Tostones', 'Plátano verde frito aplastado con ajo', c.CategoriaID, 110.00, 45.00, 15
    FROM Categorias c 
    WHERE c.Nombre = 'Frituras' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Tostones');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Yuca Frita', 'Yuca frita dorada y crujiente', c.CategoriaID, 95.00, 40.00, 12
    FROM Categorias c 
    WHERE c.Nombre = 'Frituras' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Yuca Frita');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Maduros', 'Plátano maduro frito caramelizado', c.CategoriaID, 85.00, 35.00, 10
    FROM Categorias c 
    WHERE c.Nombre = 'Frituras' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Maduros');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Chicharrones', 'Chicharrones de cerdo crujientes', c.CategoriaID, 150.00, 80.00, 20
    FROM Categorias c 
    WHERE c.Nombre = 'Frituras' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Chicharrones');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Quipe', 'Croqueta de trigo rellena de carne', c.CategoriaID, 65.00, 25.00, 15
    FROM Categorias c 
    WHERE c.Nombre = 'Frituras' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Quipe');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Catibias', 'Empanadas de yuca rellenas', c.CategoriaID, 75.00, 30.00, 18
    FROM Categorias c 
    WHERE c.Nombre = 'Frituras' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Catibias');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Pastelitos', 'Pastelitos fritos rellenos de carne', c.CategoriaID, 55.00, 20.00, 12
    FROM Categorias c 
    WHERE c.Nombre = 'Frituras' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Pastelitos');
    
    PRINT 'Frituras verificadas/insertadas.';
END
GO

-- Bebidas
IF EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Bebidas')
BEGIN
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Jugo de Chinola', 'Jugo natural de maracuyá fresco', c.CategoriaID, 90.00, 35.00, 5
    FROM Categorias c 
    WHERE c.Nombre = 'Bebidas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Jugo de Chinola');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Jugo de Naranja', 'Jugo natural de naranja recién exprimido', c.CategoriaID, 85.00, 30.00, 5
    FROM Categorias c 
    WHERE c.Nombre = 'Bebidas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Jugo de Naranja');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Morir Soñando', 'Bebida tradicional de leche con jugo de naranja', c.CategoriaID, 120.00, 50.00, 7
    FROM Categorias c 
    WHERE c.Nombre = 'Bebidas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Morir Soñando');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Jugo de Tamarindo', 'Jugo natural de tamarindo dominicano', c.CategoriaID, 95.00, 40.00, 6
    FROM Categorias c 
    WHERE c.Nombre = 'Bebidas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Jugo de Tamarindo');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Avena', 'Avena caliente tradicional dominicana', c.CategoriaID, 75.00, 25.00, 10
    FROM Categorias c 
    WHERE c.Nombre = 'Bebidas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Avena');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Café Negro', 'Café dominicano negro tradicional', c.CategoriaID, 45.00, 15.00, 5
    FROM Categorias c 
    WHERE c.Nombre = 'Bebidas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Café Negro');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Café con Leche', 'Café dominicano con leche caliente', c.CategoriaID, 60.00, 25.00, 6
    FROM Categorias c 
    WHERE c.Nombre = 'Bebidas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Café con Leche');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Coca Cola', 'Refresco Coca Cola 355ml', c.CategoriaID, 70.00, 30.00, 2
    FROM Categorias c 
    WHERE c.Nombre = 'Bebidas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Coca Cola');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Agua Mineral', 'Agua mineral 500ml', c.CategoriaID, 50.00, 20.00, 1
    FROM Categorias c 
    WHERE c.Nombre = 'Bebidas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Agua Mineral');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Presidente Cerveza', 'Cerveza Presidente 355ml', c.CategoriaID, 150.00, 80.00, 2
    FROM Categorias c 
    WHERE c.Nombre = 'Bebidas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Presidente Cerveza');
    
    PRINT 'Bebidas verificadas/insertadas.';
END
GO

-- Postres
IF EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Postres')
BEGIN
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Tres Leches', 'Pastel tres leches tradicional dominicano', c.CategoriaID, 180.00, 80.00, 15
    FROM Categorias c 
    WHERE c.Nombre = 'Postres' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Tres Leches');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Flan de Coco', 'Flan casero de coco dominicano', c.CategoriaID, 150.00, 60.00, 10
    FROM Categorias c 
    WHERE c.Nombre = 'Postres' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Flan de Coco');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Majarete', 'Postre de maíz con canela y coco', c.CategoriaID, 120.00, 45.00, 8
    FROM Categorias c 
    WHERE c.Nombre = 'Postres' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Majarete');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Arroz con Dulce', 'Arroz con dulce tradicional dominicano', c.CategoriaID, 110.00, 40.00, 6
    FROM Categorias c 
    WHERE c.Nombre = 'Postres' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Arroz con Dulce');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Dulce de Leche Cortada', 'Dulce tradicional de leche cortada', c.CategoriaID, 95.00, 35.00, 5
    FROM Categorias c 
    WHERE c.Nombre = 'Postres' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Dulce de Leche Cortada');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Helado de Coco', 'Helado artesanal de coco', c.CategoriaID, 85.00, 30.00, 3
    FROM Categorias c 
    WHERE c.Nombre = 'Postres' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Helado de Coco');
    
    PRINT 'Postres verificados/insertados.';
END
GO

-- Desayunos
IF EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Desayunos')
BEGIN
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Mangú', 'Puré de plátano verde con cebollitas', c.CategoriaID, 120.00, 50.00, 20
    FROM Categorias c 
    WHERE c.Nombre = 'Desayunos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Mangú');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Tres Golpes', 'Mangú con huevo, queso y salami', c.CategoriaID, 220.00, 95.00, 25
    FROM Categorias c 
    WHERE c.Nombre = 'Desayunos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Tres Golpes');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Arepa Dominicana', 'Arepa dulce dominicana tradicional', c.CategoriaID, 95.00, 35.00, 15
    FROM Categorias c 
    WHERE c.Nombre = 'Desayunos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Arepa Dominicana');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Huevos Fritos', 'Huevos fritos dominicanos', c.CategoriaID, 80.00, 30.00, 8
    FROM Categorias c 
    WHERE c.Nombre = 'Desayunos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Huevos Fritos');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Tostada con Aguacate', 'Tostada con aguacate y sal', c.CategoriaID, 105.00, 40.00, 10
    FROM Categorias c 
    WHERE c.Nombre = 'Desayunos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Tostada con Aguacate');
    
    PRINT 'Desayunos verificados/insertados.';
END
GO

-- Sopas
IF EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Sopas')
BEGIN
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Sancocho', 'Sancocho dominicano tradicional completo', c.CategoriaID, 350.00, 180.00, 90
    FROM Categorias c 
    WHERE c.Nombre = 'Sopas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Sancocho');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Sopa de Pollo', 'Sopa de pollo con vegetales', c.CategoriaID, 220.00, 100.00, 45
    FROM Categorias c 
    WHERE c.Nombre = 'Sopas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Sopa de Pollo');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Mondongo', 'Mondongo dominicano con vegetales', c.CategoriaID, 280.00, 140.00, 60
    FROM Categorias c 
    WHERE c.Nombre = 'Sopas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Mondongo');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Sopa de Habichuelas', 'Sopa espesa de habichuelas rojas', c.CategoriaID, 180.00, 75.00, 35
    FROM Categorias c 
    WHERE c.Nombre = 'Sopas' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Sopa de Habichuelas');
    
    PRINT 'Sopas verificadas/insertadas.';
END
GO

-- Mariscos
IF EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Mariscos')
BEGIN
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Pescao Frito', 'Pescado frito entero estilo dominicano', c.CategoriaID, 420.00, 220.00, 25
    FROM Categorias c 
    WHERE c.Nombre = 'Mariscos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Pescao Frito');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Filete de Pescado', 'Filete de pescado a la plancha', c.CategoriaID, 380.00, 200.00, 20
    FROM Categorias c 
    WHERE c.Nombre = 'Mariscos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Filete de Pescado');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Camarones al Ajillo', 'Camarones salteados con ajo', c.CategoriaID, 520.00, 280.00, 15
    FROM Categorias c 
    WHERE c.Nombre = 'Mariscos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Camarones al Ajillo');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Pulpo Guisado', 'Pulpo guisado con vegetales', c.CategoriaID, 480.00, 260.00, 35
    FROM Categorias c 
    WHERE c.Nombre = 'Mariscos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Pulpo Guisado');
    
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion)
    SELECT 'Langosta al Vapor', 'Langosta al vapor con mantequilla', c.CategoriaID, 1200.00, 800.00, 30
    FROM Categorias c 
    WHERE c.Nombre = 'Mariscos' 
    AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.Nombre = 'Langosta al Vapor');
    
    PRINT 'Mariscos verificados/insertados.';
END
GO

-- Insertar inventario inicial para productos existentes (solo si no existe)
INSERT INTO Inventario (ProductoID, CantidadDisponible, CantidadMinima)
SELECT 
    p.ProductoID,
    CASE 
        WHEN c.Nombre IN ('Bebidas', 'Postres') THEN 50
        WHEN c.Nombre = 'Platos Principales' THEN 25
        WHEN c.Nombre = 'Mariscos' THEN 20
        ELSE 30
    END as CantidadDisponible,
    CASE 
        WHEN c.Nombre IN ('Bebidas', 'Postres') THEN 10
        WHEN c.Nombre = 'Mariscos' THEN 5
        ELSE 8
    END as CantidadMinima
FROM Productos p
INNER JOIN Categorias c ON p.CategoriaID = c.CategoriaID
WHERE NOT EXISTS (SELECT 1 FROM Inventario i WHERE i.ProductoID = p.ProductoID);

PRINT 'Inventario inicial verificado/insertado para productos existentes.';
GO

-- Insertar Combos especiales dominicanos (solo si no existen)
IF NOT EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'La Bandera Dominicana')
BEGIN
    INSERT INTO Combos (Nombre, Descripcion, Precio, Descuento) VALUES 
    ('La Bandera Dominicana', 'Arroz blanco, habichuelas rojas, pollo guisado y ensalada - El plato más típico de RD', 480.00, 50.00);
    PRINT 'Combo La Bandera Dominicana insertado.';
END
ELSE
BEGIN
    PRINT 'Combo La Bandera Dominicana ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'Combo Criollo Especial')
BEGIN
    INSERT INTO Combos (Nombre, Descripcion, Precio, Descuento) VALUES 
    ('Combo Criollo Especial', 'Pernil al horno, moro de guandules, tostones y jugo natural', 550.00, 70.00);
    PRINT 'Combo Combo Criollo Especial insertado.';
END
ELSE
BEGIN
    PRINT 'Combo Combo Criollo Especial ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'Desayuno Típico Dominicano')
BEGIN
    INSERT INTO Combos (Nombre, Descripcion, Precio, Descuento) VALUES 
    ('Desayuno Típico Dominicano', 'Tres golpes completo, avena caliente y jugo de chinola', 320.00, 40.00);
    PRINT 'Combo Desayuno Típico Dominicano insertado.';
END
ELSE
BEGIN
    PRINT 'Combo Desayuno Típico Dominicano ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'Parrillada Familiar')
BEGIN
    INSERT INTO Combos (Nombre, Descripcion, Precio, Descuento) VALUES 
    ('Parrillada Familiar', 'Costillas BBQ, chicharrones, yuca frita, tostones para 4 personas', 1200.00, 200.00);
    PRINT 'Combo Parrillada Familiar insertado.';
END
ELSE
BEGIN
    PRINT 'Combo Parrillada Familiar ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'Combo Marino Criollo')
BEGIN
    INSERT INTO Combos (Nombre, Descripcion, Precio, Descuento) VALUES 
    ('Combo Marino Criollo', 'Pescao frito, arroz blanco, ensalada verde y maduros', 520.00, 80.00);
    PRINT 'Combo Combo Marino Criollo insertado.';
END
ELSE
BEGIN
    PRINT 'Combo Combo Marino Criollo ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'Combo Vegetariano')
BEGIN
    INSERT INTO Combos (Nombre, Descripcion, Precio, Descuento) VALUES 
    ('Combo Vegetariano', 'Moro de guandules, yuca hervida, ensalada verde y jugo natural', 280.00, 30.00);
    PRINT 'Combo Combo Vegetariano insertado.';
END
ELSE
BEGIN
    PRINT 'Combo Combo Vegetariano ya existe.';
END
GO

-- Insertar productos en combos (solo si no existen)
IF EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'La Bandera Dominicana')
BEGIN
    INSERT INTO ComboProductos (ComboID, ProductoID, Cantidad)
    SELECT 1, p.ProductoID, 1
    FROM Productos p
    WHERE p.Nombre IN ('Arroz Blanco', 'Habichuelas Rojas', 'Pollo Guisado', 'Ensalada Verde')
    AND NOT EXISTS (SELECT 1 FROM ComboProductos cp WHERE cp.ComboID = 1 AND cp.ProductoID IN (SELECT ProductoID FROM Productos WHERE Nombre IN ('Arroz Blanco', 'Habichuelas Rojas', 'Pollo Guisado', 'Ensalada Verde')));
    PRINT 'Productos en La Bandera Dominicana verificados/insertados.';
END
GO

IF EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'Combo Criollo Especial')
BEGIN
    INSERT INTO ComboProductos (ComboID, ProductoID, Cantidad)
    SELECT 2, p.ProductoID, 1
    FROM Productos p
    WHERE p.Nombre IN ('Pernil al Horno', 'Moro de Guandules', 'Tostones', 'Jugo de Chinola')
    AND NOT EXISTS (SELECT 1 FROM ComboProductos cp WHERE cp.ComboID = 2 AND cp.ProductoID IN (SELECT ProductoID FROM Productos WHERE Nombre IN ('Pernil al Horno', 'Moro de Guandules', 'Tostones', 'Jugo de Chinola')));
    PRINT 'Productos en Combo Criollo Especial verificados/insertados.';
END
GO

IF EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'Desayuno Típico Dominicano')
BEGIN
    INSERT INTO ComboProductos (ComboID, ProductoID, Cantidad)
    SELECT 3, p.ProductoID, 1
    FROM Productos p
    WHERE p.Nombre IN ('Tres Golpes', 'Avena', 'Jugo de Chinola')
    AND NOT EXISTS (SELECT 1 FROM ComboProductos cp WHERE cp.ComboID = 3 AND cp.ProductoID IN (SELECT ProductoID FROM Productos WHERE Nombre IN ('Tres Golpes', 'Avena', 'Jugo de Chinola')));
    PRINT 'Productos en Desayuno Típico Dominicano verificados/insertados.';
END
GO

IF EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'Parrillada Familiar')
BEGIN
    INSERT INTO ComboProductos (ComboID, ProductoID, Cantidad)
    SELECT 4, p.ProductoID, 2
    FROM Productos p
    WHERE p.Nombre IN ('Costillas BBQ Criolla', 'Chicharrones', 'Yuca Frita', 'Tostones')
    AND NOT EXISTS (SELECT 1 FROM ComboProductos cp WHERE cp.ComboID = 4 AND cp.ProductoID IN (SELECT ProductoID FROM Productos WHERE Nombre IN ('Costillas BBQ Criolla', 'Chicharrones', 'Yuca Frita', 'Tostones')));
    PRINT 'Productos en Parrillada Familiar verificados/insertados.';
END
GO

IF EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'Combo Marino Criollo')
BEGIN
    INSERT INTO ComboProductos (ComboID, ProductoID, Cantidad)
    SELECT 5, p.ProductoID, 1
    FROM Productos p
    WHERE p.Nombre IN ('Pescao Frito', 'Arroz Blanco', 'Ensalada Verde', 'Maduros')
    AND NOT EXISTS (SELECT 1 FROM ComboProductos cp WHERE cp.ComboID = 5 AND cp.ProductoID IN (SELECT ProductoID FROM Productos WHERE Nombre IN ('Pescao Frito', 'Arroz Blanco', 'Ensalada Verde', 'Maduros')));
    PRINT 'Productos en Combo Marino Criollo verificados/insertados.';
END
GO

IF EXISTS (SELECT 1 FROM Combos WHERE Nombre = 'Combo Vegetariano')
BEGIN
    INSERT INTO ComboProductos (ComboID, ProductoID, Cantidad)
    SELECT 6, p.ProductoID, 1
    FROM Productos p
    WHERE p.Nombre IN ('Moro de Guandules', 'Yuca Hervida', 'Ensalada Verde', 'Jugo de Chinola')
    AND NOT EXISTS (SELECT 1 FROM ComboProductos cp WHERE cp.ComboID = 6 AND cp.ProductoID IN (SELECT ProductoID FROM Productos WHERE Nombre IN ('Moro de Guandules', 'Yuca Hervida', 'Ensalada Verde', 'Jugo de Chinola')));
    PRINT 'Productos en Combo Vegetariano verificados/insertados.';
END
GO

-- Insertar clientes de ejemplo (solo si no existen)
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-9876543-2')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-9876543-2', 'Juan Carlos', 'Ramirez Santos', '809-555-1001', 'juan.ramirez@gmail.com', 'Av. 27 de Febrero #123, Santo Domingo');
    PRINT 'Cliente Juan Carlos insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Juan Carlos ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-8765432-1')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-8765432-1', 'Lucia Maria', 'Fernandez Peña', '809-555-1002', 'lucia.fernandez@hotmail.com', 'Calle Mercedes #45, Santiago');
    PRINT 'Cliente Lucia Maria insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Lucia Maria ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-7654321-0')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-7654321-0', 'Pedro Antonio', 'Sanchez Rodriguez', '809-555-1003', 'pedro.sanchez@yahoo.com', 'Av. Independencia #67, Santo Domingo');
    PRINT 'Cliente Pedro Antonio insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Pedro Antonio ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-6543210-9')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-6543210-9', 'Carmen Rosa', 'Torres Martinez', '849-555-1004', 'carmen.torres@gmail.com', 'Calle Duarte #89, Puerto Plata');
    PRINT 'Cliente Carmen Rosa insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Carmen Rosa ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-5432109-8')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-5432109-8', 'Roberto Luis', 'Diaz Jimenez', '829-555-1005', 'roberto.diaz@outlook.com', 'Av. Las Americas #234, Boca Chica');
    PRINT 'Cliente Roberto Luis insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Roberto Luis ya existe.';
END
GO

-- Clientes adicionales para pruebas
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-4321098-7')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-4321098-7', 'Maria Elena', 'Gonzalez Valdez', '809-555-1006', 'maria.gonzalez@gmail.com', 'Calle El Conde #56, Zona Colonial');
    PRINT 'Cliente Maria Elena insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Maria Elena ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-3210987-6')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-3210987-6', 'Carlos Rafael', 'Mendoza Reyes', '829-555-1007', 'carlos.mendoza@yahoo.com', 'Av. John F. Kennedy #78, Piantini');
    PRINT 'Cliente Carlos Rafael insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Carlos Rafael ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-2109876-5')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-2109876-5', 'Ana Sofia', 'Herrera Castillo', '849-555-1008', 'ana.herrera@hotmail.com', 'Calle Beller #23, Santiago');
    PRINT 'Cliente Ana Sofia insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Ana Sofia ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-1098765-4')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-1098765-4', 'Miguel Angel', 'Vargas Rosario', '809-555-1009', 'miguel.vargas@gmail.com', 'Av. Máximo Gómez #145, Santo Domingo');
    PRINT 'Cliente Miguel Angel insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Miguel Angel ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-0987654-3')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-0987654-3', 'Yolanda Isabel', 'Morales Jiménez', '829-555-1010', 'yolanda.morales@outlook.com', 'Calle Padre Billini #89, Zona Colonial');
    PRINT 'Cliente Yolanda Isabel insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Yolanda Isabel ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-9876543-1')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-9876543-1', 'Rafael Antonio', 'Peña Rosario', '849-555-1011', 'rafael.pena@gmail.com', 'Av. Las Carreras #234, Santiago');
    PRINT 'Cliente Rafael Antonio insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Rafael Antonio ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-8765432-9')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-8765432-9', 'Dalila Mercedes', 'Cruz Martínez', '809-555-1012', 'dalila.cruz@yahoo.com', 'Calle Mella #67, San Pedro de Macorís');
    PRINT 'Cliente Dalila Mercedes insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Dalila Mercedes ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-7654321-8')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-7654321-8', 'Francisco Javier', 'Rodríguez Almonte', '829-555-1013', 'francisco.rodriguez@gmail.com', 'Av. Tiradentes #123, La Vega');
    PRINT 'Cliente Francisco Javier insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Francisco Javier ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-6543210-7')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-6543210-7', 'Esperanza Dolores', 'García Valerio', '849-555-1014', 'esperanza.garcia@hotmail.com', 'Calle Restauración #45, Puerto Plata');
    PRINT 'Cliente Esperanza Dolores insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Esperanza Dolores ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-5432109-6')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-5432109-6', 'Ramón Alberto', 'Santana Guerrero', '809-555-1015', 'ramon.santana@gmail.com', 'Av. Bolívar #78, Santo Domingo');
    PRINT 'Cliente Ramón Alberto insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Ramón Alberto ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-4321098-5')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-4321098-5', 'Milagros Antonia', 'Féliz Taveras', '829-555-1016', 'milagros.felix@outlook.com', 'Calle Sánchez #234, Azua');
    PRINT 'Cliente Milagros Antonia insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Milagros Antonia ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-3210987-4')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-3210987-4', 'Domingo Emilio', 'Núñez Familia', '849-555-1017', 'domingo.nunez@gmail.com', 'Av. Pedro Henríquez Ureña #456, Santo Domingo');
    PRINT 'Cliente Domingo Emilio insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Domingo Emilio ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-2109876-3')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-2109876-3', 'Gladys Esperanza', 'Mejía Polanco', '809-555-1018', 'gladys.mejia@yahoo.com', 'Calle Hostos #89, Moca');
    PRINT 'Cliente Gladys Esperanza insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Gladys Esperanza ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-1098765-2')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-1098765-2', 'Víctor Manuel', 'Cabrera Méndez', '829-555-1019', 'victor.cabrera@gmail.com', 'Av. Gregorio Luperón #123, Puerto Plata');
    PRINT 'Cliente Víctor Manuel insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Víctor Manuel ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Cedula = '001-0987654-1')
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
    ('001-0987654-1', 'Soledad María', 'Peña Contreras', '849-555-1020', 'soledad.pena@hotmail.com', 'Calle Emilio Prud Homme #67, Santiago');
    PRINT 'Cliente Soledad María insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Soledad María ya existe.';
END
GO

-- Clientes sin cédula (eventuales)
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Nombre = 'Cliente' AND Apellido = 'Eventual')
BEGIN
    INSERT INTO Clientes (Nombre, Apellido, Email) VALUES 
    ('Cliente', 'Eventual', 'josejoga.opx@gmail.com');
    PRINT 'Cliente Eventual insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Eventual ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Nombre = 'Turista' AND Apellido = 'Extranjero')
BEGIN
    INSERT INTO Clientes (Nombre, Apellido, Email) VALUES 
    ('Turista', 'Extranjero', 'josejoga.opx@gmail.com');
    PRINT 'Cliente Turista Extranjero insertado.';
END
ELSE
BEGIN
    PRINT 'Cliente Turista Extranjero ya existe.';
END
GO

PRINT 'Clientes dummy completados - Total: 22 clientes para pruebas.';
GO

-- =============================================
-- CREACIÓN DE ÍNDICES PARA OPTIMIZACIÓN (IDEMPOTENTE)
-- =============================================

-- Crear índices solo si no existen
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuarios_Usuario' AND object_id = OBJECT_ID('Usuarios'))
    CREATE INDEX IX_Usuarios_Usuario ON Usuarios(Usuario);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuarios_Email' AND object_id = OBJECT_ID('Usuarios'))
    CREATE INDEX IX_Usuarios_Email ON Usuarios(Email);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Empleados_Cedula' AND object_id = OBJECT_ID('Empleados'))
    CREATE INDEX IX_Empleados_Cedula ON Empleados(Cedula);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Empleados_Email' AND object_id = OBJECT_ID('Empleados'))
    CREATE INDEX IX_Empleados_Email ON Empleados(Email);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_Cedula' AND object_id = OBJECT_ID('Clientes'))
    CREATE INDEX IX_Clientes_Cedula ON Clientes(Cedula);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_Email' AND object_id = OBJECT_ID('Clientes'))
    CREATE INDEX IX_Clientes_Email ON Clientes(Email);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Mesas_Estado' AND object_id = OBJECT_ID('Mesas'))
    CREATE INDEX IX_Mesas_Estado ON Mesas(Estado);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Mesas_NumeroMesa' AND object_id = OBJECT_ID('Mesas'))
    CREATE INDEX IX_Mesas_NumeroMesa ON Mesas(NumeroMesa);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Productos_Nombre' AND object_id = OBJECT_ID('Productos'))
    CREATE INDEX IX_Productos_Nombre ON Productos(Nombre);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Productos_Estado' AND object_id = OBJECT_ID('Productos'))
    CREATE INDEX IX_Productos_Estado ON Productos(Estado);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ordenes_Estado' AND object_id = OBJECT_ID('Ordenes'))
    CREATE INDEX IX_Ordenes_Estado ON Ordenes(Estado);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ordenes_FechaCreacion' AND object_id = OBJECT_ID('Ordenes'))
    CREATE INDEX IX_Ordenes_FechaCreacion ON Ordenes(FechaCreacion);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ordenes_NumeroOrden' AND object_id = OBJECT_ID('Ordenes'))
    CREATE INDEX IX_Ordenes_NumeroOrden ON Ordenes(NumeroOrden);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Facturas_NumeroFactura' AND object_id = OBJECT_ID('Facturas'))
    CREATE INDEX IX_Facturas_NumeroFactura ON Facturas(NumeroFactura);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Facturas_FechaFactura' AND object_id = OBJECT_ID('Facturas'))
    CREATE INDEX IX_Facturas_FechaFactura ON Facturas(FechaFactura);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Facturas_Estado' AND object_id = OBJECT_ID('Facturas'))
    CREATE INDEX IX_Facturas_Estado ON Facturas(Estado);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reservaciones_FechaYHora' AND object_id = OBJECT_ID('Reservaciones'))
    CREATE INDEX IX_Reservaciones_FechaYHora ON Reservaciones(FechaYHora);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reservaciones_Estado' AND object_id = OBJECT_ID('Reservaciones'))
    CREATE INDEX IX_Reservaciones_Estado ON Reservaciones(Estado);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EmailTransacciones_Estado' AND object_id = OBJECT_ID('EmailTransacciones'))
    CREATE INDEX IX_EmailTransacciones_Estado ON EmailTransacciones(Estado);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EmailTransacciones_TipoEmail' AND object_id = OBJECT_ID('EmailTransacciones'))
    CREATE INDEX IX_EmailTransacciones_TipoEmail ON EmailTransacciones(TipoEmail);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MovimientosInventario_TipoMovimiento' AND object_id = OBJECT_ID('MovimientosInventario'))
    CREATE INDEX IX_MovimientosInventario_TipoMovimiento ON MovimientosInventario(TipoMovimiento);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MovimientosInventario_FechaMovimiento' AND object_id = OBJECT_ID('MovimientosInventario'))
    CREATE INDEX IX_MovimientosInventario_FechaMovimiento ON MovimientosInventario(FechaMovimiento);

PRINT 'Índices de optimización verificados/creados.';
GO

-- =============================================
-- TRIGGERS AUTOMÁTICOS (IDEMPOTENTE)
-- =============================================

-- Trigger para generar número de orden automáticamente
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_GenerarNumeroOrden')
BEGIN
    EXEC('
    CREATE TRIGGER tr_GenerarNumeroOrden
    ON Ordenes
    AFTER INSERT
    AS
    BEGIN
        UPDATE Ordenes 
        SET NumeroOrden = ''ORD-'' + FORMAT(GETDATE(), ''yyyyMMdd'') + ''-'' + FORMAT(OrdenID, ''0000'')
        WHERE OrdenID IN (SELECT OrdenID FROM inserted) AND (NumeroOrden = '''' OR NumeroOrden IS NULL);
    END');
    PRINT 'Trigger tr_GenerarNumeroOrden creado.';
END
ELSE
BEGIN
    PRINT 'Trigger tr_GenerarNumeroOrden ya existe.';
END
GO

-- Trigger para generar número de factura automáticamente
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_GenerarNumeroFactura')
BEGIN
    EXEC('
    CREATE TRIGGER tr_GenerarNumeroFactura
    ON Facturas
    AFTER INSERT
    AS
    BEGIN
        UPDATE Facturas 
        SET NumeroFactura = ''FACT-'' + FORMAT(GETDATE(), ''yyyyMMdd'') + ''-'' + FORMAT(FacturaID, ''0000'')
        WHERE FacturaID IN (SELECT FacturaID FROM inserted) AND (NumeroFactura = '''' OR NumeroFactura IS NULL);
    END');
    PRINT 'Trigger tr_GenerarNumeroFactura creado.';
END
ELSE
BEGIN
    PRINT 'Trigger tr_GenerarNumeroFactura ya existe.';
END
GO

-- Trigger para actualizar estado de mesa cuando se crea una orden
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_OrdenCreada_ActualizarMesa')
BEGIN
    EXEC('
    CREATE TRIGGER tr_OrdenCreada_ActualizarMesa
    ON Ordenes
    AFTER INSERT
    AS
    BEGIN
        UPDATE Mesas 
        SET Estado = ''Ocupada'', FechaUltimaActualizacion = GETDATE()
        WHERE MesaID IN (SELECT MesaID FROM inserted WHERE MesaID IS NOT NULL);
    END');
    PRINT 'Trigger tr_OrdenCreada_ActualizarMesa creado.';
END
ELSE
BEGIN
    PRINT 'Trigger tr_OrdenCreada_ActualizarMesa ya existe.';
END
GO

-- Trigger para liberar mesa cuando se genera factura
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_FacturaGenerada_LiberarMesa')
BEGIN
    EXEC('
    CREATE TRIGGER tr_FacturaGenerada_LiberarMesa
    ON Facturas
    AFTER INSERT
    AS
    BEGIN
        -- Liberar mesa
        UPDATE Mesas 
        SET Estado = ''Libre'', FechaUltimaActualizacion = GETDATE()
        WHERE MesaID IN (
            SELECT o.MesaID 
            FROM inserted i
            INNER JOIN Ordenes o ON i.OrdenID = o.OrdenID
            WHERE o.MesaID IS NOT NULL
        );
        
        -- Marcar orden como completada
        UPDATE Ordenes 
        SET Estado = ''Entregada'', FechaActualizacion = GETDATE()
        WHERE OrdenID IN (SELECT OrdenID FROM inserted);
    END');
    PRINT 'Trigger tr_FacturaGenerada_LiberarMesa creado.';
END
ELSE
BEGIN
    PRINT 'Trigger tr_FacturaGenerada_LiberarMesa ya existe.';
END
GO

-- Trigger para actualizar inventario cuando se crea detalle de orden
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_DetalleOrden_ActualizarInventario')
BEGIN
    EXEC('
    CREATE TRIGGER tr_DetalleOrden_ActualizarInventario
    ON DetalleOrdenes
    AFTER INSERT
    AS
    BEGIN
        -- Reducir inventario para productos
        UPDATE i
        SET CantidadDisponible = CantidadDisponible - ins.Cantidad,
            UltimaActualizacion = GETDATE()
        FROM Inventario i
        INNER JOIN inserted ins ON i.ProductoID = ins.ProductoID
        WHERE ins.ProductoID IS NOT NULL;
        
        -- Reducir inventario para productos en combos
        UPDATE i
        SET CantidadDisponible = CantidadDisponible - (ins.Cantidad * cp.Cantidad),
            UltimaActualizacion = GETDATE()
        FROM Inventario i
        INNER JOIN ComboProductos cp ON i.ProductoID = cp.ProductoID
        INNER JOIN inserted ins ON cp.ComboID = ins.ComboID
        WHERE ins.ComboID IS NOT NULL;
        
        -- Registrar movimientos de inventario para productos
        INSERT INTO MovimientosInventario (ProductoID, TipoMovimiento, Cantidad, StockAnterior, StockResultante, Usuario, Referencia, Observaciones)
        SELECT 
            ins.ProductoID,
            ''Salida'',
            -ins.Cantidad,
            i.CantidadDisponible + ins.Cantidad,
            i.CantidadDisponible,
            ''Sistema'',
            ''Orden #'' + o.NumeroOrden,
            ''Venta de producto''
        FROM inserted ins
        INNER JOIN Inventario i ON ins.ProductoID = i.ProductoID
        INNER JOIN Ordenes o ON ins.OrdenID = o.OrdenID
        WHERE ins.ProductoID IS NOT NULL;
    END');
    PRINT 'Trigger tr_DetalleOrden_ActualizarInventario creado.';
END
ELSE
BEGIN
    PRINT 'Trigger tr_DetalleOrden_ActualizarInventario ya existe.';
END
GO

-- =============================================
-- PROCEDIMIENTOS ALMACENADOS (IDEMPOTENTE)
-- =============================================

-- Procedimiento para obtener reporte de ventas por fecha
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_ReporteVentasPorFecha')
BEGIN
    EXEC('
    CREATE PROCEDURE sp_ReporteVentasPorFecha
        @FechaInicio DATE,
        @FechaFin DATE
    AS
    BEGIN
        SELECT 
            CAST(f.FechaFactura AS DATE) as Fecha,
            COUNT(f.FacturaID) as TotalFacturas,
            SUM(f.Total) as VentasTotales,
            AVG(f.Total) as PromedioVenta,
            MAX(f.Total) as VentaMaxima,
            MIN(f.Total) as VentaMinima,
            SUM(f.Propina) as TotalPropinas,
            COUNT(DISTINCT f.ClienteID) as ClientesUnicos
        FROM Facturas f
        WHERE CAST(f.FechaFactura AS DATE) BETWEEN @FechaInicio AND @FechaFin
            AND f.Estado = ''Pagada''
        GROUP BY CAST(f.FechaFactura AS DATE)
        ORDER BY Fecha DESC;
    END');
    PRINT 'Procedimiento sp_ReporteVentasPorFecha creado.';
END
ELSE
BEGIN
    PRINT 'Procedimiento sp_ReporteVentasPorFecha ya existe.';
END
GO

-- Procedimiento para obtener productos más vendidos
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_ProductosMasVendidos')
BEGIN
    EXEC('
    CREATE PROCEDURE sp_ProductosMasVendidos
        @TopN INT = 10,
        @FechaInicio DATE = NULL,
        @FechaFin DATE = NULL
    AS
    BEGIN
        SELECT TOP (@TopN)
            p.Nombre,
            c.Nombre as Categoria,
            SUM(do.Cantidad) as TotalVendido,
            SUM(do.Subtotal) as IngresoTotal,
            AVG(do.PrecioUnitario) as PrecioPromedio,
            COUNT(DISTINCT o.OrdenID) as OrdenesConEsteProducto
        FROM DetalleOrdenes do
        INNER JOIN Productos p ON do.ProductoID = p.ProductoID
        INNER JOIN Categorias c ON p.CategoriaID = c.CategoriaID
        INNER JOIN Ordenes o ON do.OrdenID = o.OrdenID
        INNER JOIN Facturas f ON o.OrdenID = f.OrdenID
        WHERE f.Estado = ''Pagada''
            AND (@FechaInicio IS NULL OR CAST(f.FechaFactura AS DATE) >= @FechaInicio)
            AND (@FechaFin IS NULL OR CAST(f.FechaFactura AS DATE) <= @FechaFin)
        GROUP BY p.ProductoID, p.Nombre, c.Nombre
        ORDER BY TotalVendido DESC;
    END');
    PRINT 'Procedimiento sp_ProductosMasVendidos creado.';
END
ELSE
BEGIN
    PRINT 'Procedimiento sp_ProductosMasVendidos ya existe.';
END
GO

-- Procedimiento para obtener estado actual de mesas
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_EstadoMesas')
BEGIN
    EXEC('
    CREATE PROCEDURE sp_EstadoMesas
    AS
    BEGIN
        SELECT 
            m.MesaID,
            m.NumeroMesa,
            m.Capacidad,
            m.Ubicacion,
            m.Estado,
            CASE 
                WHEN m.Estado = ''Ocupada'' THEN o.OrdenID
                WHEN m.Estado = ''Reservada'' THEN r.ReservacionID
                ELSE NULL
            END as ReferenciaID,
            CASE 
                WHEN m.Estado = ''Ocupada'' THEN c1.Nombre + '' '' + c1.Apellido
                WHEN m.Estado = ''Reservada'' THEN c2.Nombre + '' '' + c2.Apellido
                ELSE NULL
            END as Cliente,
            CASE 
                WHEN m.Estado = ''Ocupada'' THEN o.FechaCreacion
                WHEN m.Estado = ''Reservada'' THEN r.FechaYHora
                ELSE NULL
            END as FechaOcupacion
        FROM Mesas m
        LEFT JOIN Ordenes o ON m.MesaID = o.MesaID 
            AND o.Estado NOT IN (''Entregada'', ''Cancelada'')
        LEFT JOIN Clientes c1 ON o.ClienteID = c1.ClienteID
        LEFT JOIN Reservaciones r ON m.MesaID = r.MesaID 
            AND r.Estado = ''Confirmada'' 
            AND r.FechaYHora BETWEEN GETDATE() AND DATEADD(HOUR, 2, GETDATE())
        LEFT JOIN Clientes c2 ON r.ClienteID = c2.ClienteID
        ORDER BY m.NumeroMesa;
    END');
    PRINT 'Procedimiento sp_EstadoMesas creado.';
END
ELSE
BEGIN
    PRINT 'Procedimiento sp_EstadoMesas ya existe.';
END
GO

-- Procedimiento para obtener dashboard de administrador
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_DashboardAdmin')
BEGIN
    EXEC('
    CREATE PROCEDURE sp_DashboardAdmin
    AS
    BEGIN
        DECLARE @Hoy DATE = CAST(GETDATE() AS DATE);
        
        -- Estadísticas del día
        SELECT 
            ''VentasHoy'' as Metrica,
            ISNULL(SUM(Total), 0) as Valor
        FROM Facturas 
        WHERE CAST(FechaFactura AS DATE) = @Hoy AND Estado = ''Pagada''
        
        UNION ALL
        
        SELECT 
            ''OrdenesHoy'' as Metrica,
            COUNT(*) as Valor
        FROM Ordenes 
        WHERE CAST(FechaCreacion AS DATE) = @Hoy
        
        UNION ALL
        
        SELECT 
            ''ClientesHoy'' as Metrica,
            COUNT(DISTINCT ClienteID) as Valor
        FROM Ordenes 
        WHERE CAST(FechaCreacion AS DATE) = @Hoy AND ClienteID IS NOT NULL
        
        UNION ALL
        
        SELECT 
            ''MesasOcupadas'' as Metrica,
            COUNT(*) as Valor
        FROM Mesas 
        WHERE Estado = ''Ocupada''
        
        UNION ALL
        
        SELECT 
            ''ProductosBajoStock'' as Metrica,
            COUNT(*) as Valor
        FROM Inventario i
        INNER JOIN Productos p ON i.ProductoID = p.ProductoID
        WHERE i.CantidadDisponible <= i.CantidadMinima AND p.Estado = 1;
    END');
    PRINT 'Procedimiento sp_DashboardAdmin creado.';
END
ELSE
BEGIN
    PRINT 'Procedimiento sp_DashboardAdmin ya existe.';
END
GO

-- =============================================
-- MENSAJE FINAL
-- =============================================

PRINT '';
PRINT '=============================================';
PRINT '🎉 SCRIPT IDEMPOTENTE DE EL CRIOLLO COMPLETADO';
PRINT '=============================================';
PRINT '';
PRINT '✅ ESTRUCTURA VERIFICADA/CREADA:';
PRINT '- Base de datos ElCriolloRestaurante';
PRINT '- 16 tablas con relaciones Foreign Key';
PRINT '- Datos iniciales básicos insertados';
PRINT '- Índices de optimización aplicados';
PRINT '- Triggers automáticos configurados';
PRINT '- Procedimientos almacenados disponibles';
PRINT '';
PRINT '🔄 SCRIPT IDEMPOTENTE:';
PRINT '- Puede ejecutarse múltiples veces sin errores';
PRINT '- Verifica existencia antes de crear objetos';
PRINT '- Mantiene datos existentes intactos';
PRINT '- Usa GO batches para mejor compatibilidad';
PRINT '';
PRINT '🚀 LA BASE DE DATOS ESTÁ LISTA PARA PRODUCCIÓN';
PRINT '=============================================';
GO