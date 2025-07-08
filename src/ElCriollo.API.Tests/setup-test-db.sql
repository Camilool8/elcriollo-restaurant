-- =============================================
-- Setup Script for Test Database: El Criollo Restaurant
-- Test Database Setup - Simplified Version
-- =============================================

-- Use master to create the test database
USE master;
GO

-- Drop test database if it exists (for clean setup)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'ElCriolloTest')
BEGIN
    ALTER DATABASE ElCriolloTest SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ElCriolloTest;
    PRINT 'Previous test database dropped.';
END
GO

-- Create test database
CREATE DATABASE ElCriolloTest;
PRINT 'Test database ElCriolloTest created.';
GO

USE ElCriolloTest;
GO

-- =============================================
-- CREAR TABLAS (Estructura esencial solo)  
-- =============================================

-- 1. Roles
CREATE TABLE Roles (
    RolID INT IDENTITY(1,1) PRIMARY KEY,
    NombreRol VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(200) NULL,
    Estado BIT NOT NULL DEFAULT 1
);
GO

-- 2. Usuarios
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
GO

-- 3. Empleados
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
GO

-- Add bidirectional FK
ALTER TABLE Usuarios ADD CONSTRAINT FK_Usuarios_Empleado 
    FOREIGN KEY (EmpleadoID) REFERENCES Empleados(EmpleadoID);
GO

-- 4. Clientes
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
GO

-- 5. Mesas
CREATE TABLE Mesas (
    MesaID INT IDENTITY(1,1) PRIMARY KEY,
    NumeroMesa INT NOT NULL UNIQUE,
    Capacidad INT NOT NULL,
    Ubicacion VARCHAR(50) NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Libre',
    FechaUltimaLimpieza DATETIME NULL,
    FechaUltimaActualizacion DATETIME NULL
);
GO

-- 6. Reservaciones
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
GO

-- 7. Categorias
CREATE TABLE Categorias (
    CategoriaID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(200) NULL,
    Estado BIT NOT NULL DEFAULT 1
);
GO

-- 8. Productos
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
GO

-- 9. Inventario
CREATE TABLE Inventario (
    InventarioID INT IDENTITY(1,1) PRIMARY KEY,
    ProductoID INT NOT NULL,
    CantidadDisponible INT NOT NULL DEFAULT 0,
    CantidadMinima INT NOT NULL DEFAULT 5,
    UltimaActualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Inventario_Producto FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
);
GO

-- 10. Combos
CREATE TABLE Combos (
    ComboID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(500) NULL,
    Precio DECIMAL(10,2) NOT NULL,
    Descuento DECIMAL(10,2) NOT NULL DEFAULT 0,
    Estado BIT NOT NULL DEFAULT 1
);
GO

-- 11. ComboProductos
CREATE TABLE ComboProductos (
    ComboProductoID INT IDENTITY(1,1) PRIMARY KEY,
    ComboID INT NOT NULL,
    ProductoID INT NOT NULL,
    Cantidad INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_ComboProductos_Combo FOREIGN KEY (ComboID) REFERENCES Combos(ComboID),
    CONSTRAINT FK_ComboProductos_Producto FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
);
GO

-- 12. Ordenes
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
GO

-- 13. DetalleOrdenes
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
GO

-- 14. Facturas
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
GO

-- 15. EmailTransacciones
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
GO

-- 16. MovimientosInventario
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
GO

-- =============================================
-- INSERT TEST DATA (Minimal for testing)
-- =============================================

-- Insert test roles
INSERT INTO Roles (NombreRol, Descripcion) VALUES 
('Administrador', 'Admin de pruebas'),
('Recepcion', 'Recepcionista de pruebas'),
('Mesero', 'Mesero de pruebas'),
('Cajero', 'Cajero de pruebas');
GO

-- Insert test categories
INSERT INTO Categorias (Nombre, Descripcion) VALUES 
('Platos Principales', 'Comidas principales'),
('Bebidas', 'Bebidas diversas'),
('Postres', 'Postres varios');
GO

-- Insert test products
INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion) VALUES 
('Pollo Guisado', 'Pollo guisado tradicional', 1, 350.00, 180.00, 30),       -- ID 1
('Arroz Blanco', 'Arroz blanco', 1, 80.00, 30.00, 15),                         -- ID 2
('Jugo de Naranja', 'Jugo natural', 2, 100.00, 35.00, 5),                      -- ID 3
('Tres Leches', 'Postre tres leches', 3, 180.00, 80.00, 5),                   -- ID 4
('Rabo Encendido', 'Rabo de res guisado', 1, 450.00, 250.00, 90),             -- ID 5
('Sancocho', 'Sopa espesa con carnes y víveres', 1, 320.00, 150.00, 120),      -- ID 6
('Habichuelas Rojas', 'Habichuelas rojas guisadas', 1, 100.00, 40.00, 45),    -- ID 7
('Tostones', 'Plátanos verdes fritos', 1, 110.00, 50.00, 10),                 -- ID 8
('Morir Soñando', 'Bebida de naranja y leche', 2, 120.00, 40.00, 5);         -- ID 9
GO

-- Insert test inventory
INSERT INTO Inventario (ProductoID, CantidadDisponible, CantidadMinima)
SELECT ProductoID, 50, 10 FROM Productos;
GO

-- Insert test mesas
INSERT INTO Mesas (NumeroMesa, Capacidad, Ubicacion) VALUES 
(1, 4, 'Salon Principal'),
(2, 2, 'Terraza'),
(3, 6, 'Salon Principal');
GO

-- Insert test combo
INSERT INTO Combos (Nombre, Descripcion, Precio, Descuento) VALUES 
('Combo Prueba', 'Combo de prueba', 400.00, 50.00);
GO

-- Insert combo products
INSERT INTO ComboProductos (ComboID, ProductoID, Cantidad) VALUES 
(1, 1, 1), -- Pollo Guisado
(1, 2, 1); -- Arroz Blanco
GO

-- =============================================
-- CREATE ESSENTIAL INDEXES
-- =============================================

CREATE INDEX IX_Usuarios_Usuario ON Usuarios(Usuario);
CREATE INDEX IX_Productos_Estado ON Productos(Estado);
CREATE INDEX IX_Mesas_Estado ON Mesas(Estado);
CREATE INDEX IX_Ordenes_Estado ON Ordenes(Estado);
CREATE INDEX IX_Facturas_Estado ON Facturas(Estado);
GO

-- =============================================
-- FINAL MESSAGE
-- =============================================

PRINT '';
PRINT '=========================================';
PRINT '✅ TEST DATABASE SETUP COMPLETE';
PRINT '=========================================';
PRINT '';
PRINT 'Database: ElCriolloTest';
PRINT 'Tables created: 16';
PRINT 'Test data inserted: ✅';
PRINT '';
PRINT 'Ready for integration testing!';
PRINT '';
PRINT 'To drop this database later:';
PRINT 'DROP DATABASE ElCriolloTest;';
PRINT '=========================================';
GO 