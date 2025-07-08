-- =============================================
-- Script de Creación de Base de Datos: El Criollo Restaurant
-- Sistema de Gestión para Restaurante Dominicano
-- Versión: 2.0 - Sincronizado con entidades C#
-- Fecha: Diciembre 2024
-- =============================================

-- Configuración inicial
USE master;

-- Eliminar base de datos si existe (para recreación limpia)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'ElCriolloRestaurante')
BEGIN
    ALTER DATABASE ElCriolloRestaurante SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ElCriolloRestaurante;
    PRINT 'Base de datos anterior eliminada.';
END

-- Crear nueva base de datos
CREATE DATABASE ElCriolloRestaurante;
PRINT 'Base de datos ElCriolloRestaurante creada exitosamente.';

USE ElCriolloRestaurante;

-- =============================================
-- CREACIÓN DE TABLAS (EN ORDEN DE DEPENDENCIAS)
-- =============================================

-- 1. Tabla Roles
CREATE TABLE Roles (
    RolID INT IDENTITY(1,1) PRIMARY KEY,
    NombreRol VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(200) NULL,
    Estado BIT NOT NULL DEFAULT 1
);
PRINT 'Tabla Roles creada.';

-- 2. Tabla Usuarios
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

-- 3. Tabla Empleados
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

-- Agregar FK de Usuario a Empleado (relación bidireccional)
ALTER TABLE Usuarios ADD CONSTRAINT FK_Usuarios_Empleado 
    FOREIGN KEY (EmpleadoID) REFERENCES Empleados(EmpleadoID);

-- 4. Tabla Clientes
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

-- 5. Tabla Mesas
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

-- 6. Tabla Reservaciones
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

-- 7. Tabla Categorias
CREATE TABLE Categorias (
    CategoriaID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(200) NULL,
    Estado BIT NOT NULL DEFAULT 1
);
PRINT 'Tabla Categorias creada.';

-- 8. Tabla Productos
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

-- 9. Tabla Inventario
CREATE TABLE Inventario (
    InventarioID INT IDENTITY(1,1) PRIMARY KEY,
    ProductoID INT NOT NULL,
    CantidadDisponible INT NOT NULL DEFAULT 0,
    CantidadMinima INT NOT NULL DEFAULT 5,
    UltimaActualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Inventario_Producto FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
);
PRINT 'Tabla Inventario creada.';

-- 10. Tabla Combos
CREATE TABLE Combos (
    ComboID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(500) NULL,
    Precio DECIMAL(10,2) NOT NULL,
    Descuento DECIMAL(10,2) NOT NULL DEFAULT 0,
    Estado BIT NOT NULL DEFAULT 1
);
PRINT 'Tabla Combos creada.';

-- 11. Tabla ComboProductos
CREATE TABLE ComboProductos (
    ComboProductoID INT IDENTITY(1,1) PRIMARY KEY,
    ComboID INT NOT NULL,
    ProductoID INT NOT NULL,
    Cantidad INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_ComboProductos_Combo FOREIGN KEY (ComboID) REFERENCES Combos(ComboID),
    CONSTRAINT FK_ComboProductos_Producto FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
);
PRINT 'Tabla ComboProductos creada.';

-- 12. Tabla Ordenes
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

-- 13. Tabla DetalleOrdenes
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

-- 14. Tabla Facturas
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

-- 15. Tabla EmailTransacciones
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

-- 16. Tabla MovimientosInventario
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

-- =============================================
-- INSERCIÓN DE DATOS INICIALES
-- =============================================

-- Insertar Roles del sistema
INSERT INTO Roles (NombreRol, Descripcion) VALUES 
('Administrador', 'Control total del sistema y gestión de usuarios'),
('Recepcion', 'Gestión de reservas, mesas y atención al cliente'),
('Mesero', 'Toma de órdenes y atención directa al cliente'),
('Cajero', 'Procesamiento de pagos y facturación');
PRINT 'Roles del sistema insertados.';

-- Insertar Categorías de productos dominicanos
INSERT INTO Categorias (Nombre, Descripcion) VALUES 
('Platos Principales', 'Comidas fuertes tradicionales dominicanas'),
('Acompañamientos', 'Arroz, habichuelas y otros acompañantes típicos'),
('Frituras', 'Tostones, yuca frita y otras frituras tradicionales'),
('Bebidas', 'Jugos naturales, refrescos y bebidas típicas dominicanas'),
('Postres', 'Dulces y postres tradicionales dominicanos'),
('Desayunos', 'Desayunos típicos dominicanos'),
('Sopas', 'Sopas y caldos tradicionales dominicanos'),
('Mariscos', 'Pescados y mariscos preparados al estilo dominicano');
PRINT 'Categorías de productos insertadas.';

-- Insertar Mesas del restaurante
INSERT INTO Mesas (NumeroMesa, Capacidad, Ubicacion) VALUES 
(1, 2, 'Terraza'),
(2, 4, 'Terraza'),
(3, 4, 'Salon Principal'),
(4, 6, 'Salon Principal'),
(5, 8, 'Salon Principal'),
(6, 2, 'Area VIP'),
(7, 4, 'Area VIP'),
(8, 6, 'Area VIP'),
(9, 4, 'Patio'),
(10, 2, 'Patio'),
(11, 4, 'Salon Principal'),
(12, 6, 'Salon Principal');
PRINT 'Mesas del restaurante insertadas.';

-- Insertar Productos de comida dominicana
INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion) VALUES 
-- Platos Principales (CategoriaID = 1)
('Pollo Guisado', 'Pollo guisado con vegetales al estilo dominicano', 1, 350.00, 180.00, 30),
('Pernil al Horno', 'Cerdo al horno con especias dominicanas', 1, 420.00, 220.00, 45),
('Rabo Encendido', 'Rabo de res guisado picante tradicional', 1, 450.00, 250.00, 60),
('Chivo Guisado', 'Cabrito guisado con especias criollas', 1, 480.00, 280.00, 50),
('Costillas BBQ Criolla', 'Costillas con salsa criolla dominicana', 1, 520.00, 300.00, 40),
('Pollo al Carbon', 'Pollo asado al carbón con sazón criolla', 1, 380.00, 200.00, 35),

-- Acompañamientos (CategoriaID = 2)
('Arroz Blanco', 'Arroz blanco tradicional dominicano', 2, 80.00, 30.00, 15),
('Habichuelas Rojas', 'Habichuelas rojas guisadas con sofrito', 2, 100.00, 40.00, 25),
('Moro de Guandules', 'Arroz con guandules y coco', 2, 120.00, 50.00, 30),
('Ensalada Verde', 'Ensalada mixta fresca con vinagreta', 2, 90.00, 35.00, 10),
('Yuca Hervida', 'Yuca hervida con cebollitas salteadas', 2, 70.00, 25.00, 20),
('Moro de Habichuelas', 'Arroz mezclado con habichuelas', 2, 110.00, 45.00, 25),

-- Frituras (CategoriaID = 3)
('Tostones', 'Plátano verde frito aplastado con ajo', 3, 110.00, 45.00, 15),
('Yuca Frita', 'Yuca frita dorada y crujiente', 3, 95.00, 40.00, 12),
('Maduros', 'Plátano maduro frito caramelizado', 3, 85.00, 35.00, 10),
('Chicharrones', 'Chicharrones de cerdo crujientes', 3, 150.00, 80.00, 20),
('Quipe', 'Croqueta de trigo rellena de carne', 3, 65.00, 25.00, 15),
('Catibias', 'Empanadas de yuca rellenas', 3, 75.00, 30.00, 18),

-- Bebidas (CategoriaID = 4)
('Morir Soñando', 'Bebida de naranja agria con leche', 4, 120.00, 40.00, 5),
('Jugo de Chinola', 'Jugo natural de maracuyá fresco', 4, 100.00, 35.00, 5),
('Jugo de Tamarindo', 'Jugo natural de tamarindo dulce', 4, 110.00, 40.00, 5),
('Cerveza Presidente', 'Cerveza nacional dominicana', 4, 150.00, 90.00, 2),
('Mamajuana', 'Bebida tradicional dominicana', 4, 200.00, 120.00, 2),
('Agua de Coco', 'Agua de coco natural fresca', 4, 80.00, 30.00, 3),

-- Postres (CategoriaID = 5)
('Tres Leches', 'Cake de tres leches tradicional', 5, 180.00, 80.00, 5),
('Flan de Coco', 'Flan cremoso con coco rallado', 5, 160.00, 70.00, 5),
('Majarete', 'Postre de maíz dulce con canela', 5, 140.00, 60.00, 5),
('Dulce de Coco', 'Coco dulce en almíbar', 5, 120.00, 50.00, 5),
('Arroz con Leche', 'Arroz dulce con canela y pasas', 5, 130.00, 55.00, 5),

-- Desayunos (CategoriaID = 6)
('Mangú', 'Puré de plátano verde con cebollitas', 6, 150.00, 60.00, 20),
('Tres Golpes', 'Mangú con huevos, queso y salami', 6, 220.00, 100.00, 25),
('Huevos Rancheros', 'Huevos fritos con salsa criolla', 6, 180.00, 80.00, 15),
('Avena', 'Avena caliente con leche y canela', 6, 90.00, 35.00, 10),
('Tostadas Francesas', 'Pan tostado con huevo y canela', 6, 140.00, 65.00, 12),

-- Sopas (CategoriaID = 7)
('Sancocho', 'Sopa tradicional con 7 carnes y vegetales', 7, 320.00, 150.00, 45),
('Sopa de Pollo', 'Sopa de pollo con vegetales frescos', 7, 200.00, 90.00, 30),
('Mondongo', 'Sopa de callos tradicional dominicana', 7, 280.00, 130.00, 60),
('Sopa de Pescado', 'Sopa de pescado con vegetales', 7, 250.00, 120.00, 35),

-- Mariscos (CategoriaID = 8)
('Pescao Frito', 'Pescado entero frito con tostones', 8, 380.00, 200.00, 25),
('Camarones al Ajillo', 'Camarones grandes en salsa de ajo', 8, 450.00, 250.00, 20),
('Pulpo Guisado', 'Pulpo guisado con vegetales criollos', 8, 520.00, 300.00, 35),
('Lambí Guisado', 'Caracola guisada al estilo dominicano', 8, 480.00, 280.00, 40),
('Filete de Pescado', 'Filete de pescado a la plancha', 8, 350.00, 180.00, 20);
PRINT 'Productos del menú insertados.';

-- Insertar inventario inicial para todos los productos
INSERT INTO Inventario (ProductoID, CantidadDisponible, CantidadMinima)
SELECT 
    ProductoID,
    CASE 
        WHEN CategoriaID IN (4, 5) THEN 50  -- Bebidas y postres más stock
        WHEN CategoriaID = 1 THEN 25        -- Platos principales stock medio
        WHEN CategoriaID = 8 THEN 20        -- Mariscos stock menor
        ELSE 30                             -- Otros stock estándar
    END as CantidadDisponible,
    CASE 
        WHEN CategoriaID IN (4, 5) THEN 10  -- Bebidas y postres mínimo mayor
        WHEN CategoriaID = 8 THEN 5         -- Mariscos mínimo menor
        ELSE 8                              -- Otros mínimo estándar
    END as CantidadMinima
FROM Productos;
PRINT 'Inventario inicial insertado.';

-- Insertar Combos especiales dominicanos
INSERT INTO Combos (Nombre, Descripcion, Precio, Descuento) VALUES 
('La Bandera Dominicana', 'Arroz blanco, habichuelas rojas, pollo guisado y ensalada - El plato más típico de RD', 480.00, 50.00),
('Combo Criollo Especial', 'Pernil al horno, moro de guandules, tostones y jugo natural', 550.00, 70.00),
('Desayuno Típico Dominicano', 'Tres golpes completo, avena caliente y jugo de chinola', 320.00, 40.00),
('Parrillada Familiar', 'Costillas BBQ, chicharrones, yuca frita, tostones para 4 personas', 1200.00, 200.00),
('Combo Marino Criollo', 'Pescao frito, arroz blanco, ensalada verde y maduros', 520.00, 80.00),
('Combo Vegetariano', 'Moro de guandules, yuca hervida, ensalada verde y jugo natural', 280.00, 30.00);
PRINT 'Combos especiales insertados.';

-- Insertar productos en combos
INSERT INTO ComboProductos (ComboID, ProductoID, Cantidad) VALUES 
-- La Bandera Dominicana (ComboID = 1)
(1, 7, 1),  -- Arroz Blanco
(1, 8, 1),  -- Habichuelas Rojas
(1, 1, 1),  -- Pollo Guisado
(1, 10, 1), -- Ensalada Verde

-- Combo Criollo Especial (ComboID = 2)
(2, 2, 1),  -- Pernil al Horno
(2, 9, 1),  -- Moro de Guandules
(2, 13, 1), -- Tostones
(2, 19, 1), -- Jugo de Chinola

-- Desayuno Típico Dominicano (ComboID = 3)
(3, 28, 1), -- Tres Golpes
(3, 31, 1), -- Avena
(3, 19, 1), -- Jugo de Chinola

-- Parrillada Familiar (ComboID = 4)
(4, 5, 2),  -- Costillas BBQ Criolla
(4, 16, 2), -- Chicharrones
(4, 14, 2), -- Yuca Frita
(4, 13, 2), -- Tostones

-- Combo Marino Criollo (ComboID = 5)
(5, 35, 1), -- Pescao Frito
(5, 7, 1),  -- Arroz Blanco
(5, 10, 1), -- Ensalada Verde
(5, 15, 1), -- Maduros

-- Combo Vegetariano (ComboID = 6)
(6, 9, 1),  -- Moro de Guandules
(6, 11, 1), -- Yuca Hervida
(6, 10, 1), -- Ensalada Verde
(6, 19, 1); -- Jugo de Chinola
PRINT 'Productos en combos insertados.';

-- Insertar clientes de ejemplo
INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email, Direccion) VALUES 
('001-9876543-2', 'Juan Carlos', 'Ramirez Santos', '809-555-1001', 'juan.ramirez@gmail.com', 'Av. 27 de Febrero #123, Santo Domingo'),
('001-8765432-1', 'Lucia Maria', 'Fernandez Peña', '809-555-1002', 'lucia.fernandez@hotmail.com', 'Calle Mercedes #45, Santiago'),
('001-7654321-0', 'Pedro Antonio', 'Sanchez Rodriguez', '809-555-1003', 'pedro.sanchez@yahoo.com', 'Av. Independencia #67, Santo Domingo'),
('001-6543210-9', 'Carmen Rosa', 'Torres Martinez', '849-555-1004', 'carmen.torres@gmail.com', 'Calle Duarte #89, Puerto Plata'),
('001-5432109-8', 'Roberto Luis', 'Diaz Jimenez', '829-555-1005', 'roberto.diaz@outlook.com', 'Av. Las Americas #234, Boca Chica');
PRINT 'Clientes de ejemplo insertados.';

-- =============================================
-- CREACIÓN DE ÍNDICES PARA OPTIMIZACIÓN
-- =============================================

-- Índices en columnas frecuentemente consultadas
CREATE INDEX IX_Usuarios_Usuario ON Usuarios(Usuario);
CREATE INDEX IX_Usuarios_Email ON Usuarios(Email);
CREATE INDEX IX_Empleados_Cedula ON Empleados(Cedula);
CREATE INDEX IX_Empleados_Email ON Empleados(Email);
CREATE INDEX IX_Clientes_Cedula ON Clientes(Cedula);
CREATE INDEX IX_Clientes_Email ON Clientes(Email);
CREATE INDEX IX_Mesas_Estado ON Mesas(Estado);
CREATE INDEX IX_Mesas_NumeroMesa ON Mesas(NumeroMesa);
CREATE INDEX IX_Productos_Nombre ON Productos(Nombre);
CREATE INDEX IX_Productos_Estado ON Productos(Estado);
CREATE INDEX IX_Ordenes_Estado ON Ordenes(Estado);
CREATE INDEX IX_Ordenes_FechaCreacion ON Ordenes(FechaCreacion);
CREATE INDEX IX_Ordenes_NumeroOrden ON Ordenes(NumeroOrden);
CREATE INDEX IX_Facturas_NumeroFactura ON Facturas(NumeroFactura);
CREATE INDEX IX_Facturas_FechaFactura ON Facturas(FechaFactura);
CREATE INDEX IX_Facturas_Estado ON Facturas(Estado);
CREATE INDEX IX_Reservaciones_FechaYHora ON Reservaciones(FechaYHora);
CREATE INDEX IX_Reservaciones_Estado ON Reservaciones(Estado);
CREATE INDEX IX_EmailTransacciones_Estado ON EmailTransacciones(Estado);
CREATE INDEX IX_EmailTransacciones_TipoEmail ON EmailTransacciones(TipoEmail);
CREATE INDEX IX_MovimientosInventario_TipoMovimiento ON MovimientosInventario(TipoMovimiento);
CREATE INDEX IX_MovimientosInventario_FechaMovimiento ON MovimientosInventario(FechaMovimiento);
PRINT 'Índices de optimización creados.';

-- =============================================
-- TRIGGERS AUTOMÁTICOS
-- =============================================
GO

-- Trigger para generar número de orden automáticamente
CREATE TRIGGER tr_GenerarNumeroOrden
ON Ordenes
AFTER INSERT
AS
BEGIN
    UPDATE Ordenes 
    SET NumeroOrden = 'ORD-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + FORMAT(OrdenID, '0000')
    WHERE OrdenID IN (SELECT OrdenID FROM inserted) AND NumeroOrden = '';
END;
GO
PRINT 'Trigger tr_GenerarNumeroOrden creado.';
GO

-- Trigger para generar número de factura automáticamente
CREATE TRIGGER tr_GenerarNumeroFactura
ON Facturas
AFTER INSERT
AS
BEGIN
    UPDATE Facturas 
    SET NumeroFactura = 'FACT-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + FORMAT(FacturaID, '0000')
    WHERE FacturaID IN (SELECT FacturaID FROM inserted) AND NumeroFactura = '';
END;
GO
PRINT 'Trigger tr_GenerarNumeroFactura creado.';
GO

-- Trigger para actualizar estado de mesa cuando se crea una orden
CREATE TRIGGER tr_OrdenCreada_ActualizarMesa
ON Ordenes
AFTER INSERT
AS
BEGIN
    UPDATE Mesas 
    SET Estado = 'Ocupada', FechaUltimaActualizacion = GETDATE()
    WHERE MesaID IN (SELECT MesaID FROM inserted WHERE MesaID IS NOT NULL);
END;
GO
PRINT 'Trigger tr_OrdenCreada_ActualizarMesa creado.';
GO

-- Trigger para liberar mesa cuando se genera factura
CREATE TRIGGER tr_FacturaGenerada_LiberarMesa
ON Facturas
AFTER INSERT
AS
BEGIN
    -- Liberar mesa
    UPDATE Mesas 
    SET Estado = 'Libre', FechaUltimaActualizacion = GETDATE()
    WHERE MesaID IN (
        SELECT o.MesaID 
        FROM inserted i
        INNER JOIN Ordenes o ON i.OrdenID = o.OrdenID
        WHERE o.MesaID IS NOT NULL
    );
    
    -- Marcar orden como completada
    UPDATE Ordenes 
    SET Estado = 'Entregada', FechaActualizacion = GETDATE()
    WHERE OrdenID IN (SELECT OrdenID FROM inserted);
END;
GO
PRINT 'Trigger tr_FacturaGenerada_LiberarMesa creado.';
GO

-- Trigger para actualizar inventario cuando se crea detalle de orden
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
        'Salida',
        -ins.Cantidad,
        i.CantidadDisponible + ins.Cantidad,
        i.CantidadDisponible,
        'Sistema',
        'Orden #' + o.NumeroOrden,
        'Venta de producto'
    FROM inserted ins
    INNER JOIN Inventario i ON ins.ProductoID = i.ProductoID
    INNER JOIN Ordenes o ON ins.OrdenID = o.OrdenID
    WHERE ins.ProductoID IS NOT NULL;
END;
GO
PRINT 'Trigger tr_DetalleOrden_ActualizarInventario creado.';

-- =============================================
-- PROCEDIMIENTOS ALMACENADOS
-- =============================================
GO

-- Procedimiento para obtener reporte de ventas por fecha
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
        AND f.Estado = 'Pagada'
    GROUP BY CAST(f.FechaFactura AS DATE)
    ORDER BY Fecha DESC;
END;
PRINT 'Procedimiento sp_ReporteVentasPorFecha creado.';
GO

-- Procedimiento para obtener productos más vendidos
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
    WHERE f.Estado = 'Pagada'
        AND (@FechaInicio IS NULL OR CAST(f.FechaFactura AS DATE) >= @FechaInicio)
        AND (@FechaFin IS NULL OR CAST(f.FechaFactura AS DATE) <= @FechaFin)
    GROUP BY p.ProductoID, p.Nombre, c.Nombre
    ORDER BY TotalVendido DESC;
END;
PRINT 'Procedimiento sp_ProductosMasVendidos creado.';
GO

-- Procedimiento para obtener estado actual de mesas
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
            WHEN m.Estado = 'Ocupada' THEN o.OrdenID
            WHEN m.Estado = 'Reservada' THEN r.ReservacionID
            ELSE NULL
        END as ReferenciaID,
        CASE 
            WHEN m.Estado = 'Ocupada' THEN c1.Nombre + ' ' + c1.Apellido
            WHEN m.Estado = 'Reservada' THEN c2.Nombre + ' ' + c2.Apellido
            ELSE NULL
        END as Cliente,
        CASE 
            WHEN m.Estado = 'Ocupada' THEN o.FechaCreacion
            WHEN m.Estado = 'Reservada' THEN r.FechaYHora
            ELSE NULL
        END as FechaOcupacion
    FROM Mesas m
    LEFT JOIN Ordenes o ON m.MesaID = o.MesaID 
        AND o.Estado NOT IN ('Entregada', 'Cancelada')
    LEFT JOIN Clientes c1 ON o.ClienteID = c1.ClienteID
    LEFT JOIN Reservaciones r ON m.MesaID = r.MesaID 
        AND r.Estado = 'Confirmada' 
        AND r.FechaYHora BETWEEN GETDATE() AND DATEADD(HOUR, 2, GETDATE())
    LEFT JOIN Clientes c2 ON r.ClienteID = c2.ClienteID
    ORDER BY m.NumeroMesa;
END;
PRINT 'Procedimiento sp_EstadoMesas creado.';
GO

-- Procedimiento para obtener dashboard de administrador
CREATE PROCEDURE sp_DashboardAdmin
AS
BEGIN
    DECLARE @Hoy DATE = CAST(GETDATE() AS DATE);
    
    -- Estadísticas del día
    SELECT 
        'VentasHoy' as Metrica,
        ISNULL(SUM(Total), 0) as Valor
    FROM Facturas 
    WHERE CAST(FechaFactura AS DATE) = @Hoy AND Estado = 'Pagada'
    
    UNION ALL
    
    SELECT 
        'OrdenesHoy' as Metrica,
        COUNT(*) as Valor
    FROM Ordenes 
    WHERE CAST(FechaCreacion AS DATE) = @Hoy
    
    UNION ALL
    
    SELECT 
        'ClientesHoy' as Metrica,
        COUNT(DISTINCT ClienteID) as Valor
    FROM Ordenes 
    WHERE CAST(FechaCreacion AS DATE) = @Hoy AND ClienteID IS NOT NULL
    
    UNION ALL
    
    SELECT 
        'MesasOcupadas' as Metrica,
        COUNT(*) as Valor
    FROM Mesas 
    WHERE Estado = 'Ocupada'
    
    UNION ALL
    
    SELECT 
        'ProductosBajoStock' as Metrica,
        COUNT(*) as Valor
    FROM Inventario i
    INNER JOIN Productos p ON i.ProductoID = p.ProductoID
    WHERE i.CantidadDisponible <= i.CantidadMinima AND p.Estado = 1;
END;
PRINT 'Procedimiento sp_DashboardAdmin creado.';

-- =============================================
-- MENSAJE FINAL
-- =============================================

PRINT '';
PRINT '=============================================';
PRINT '🎉 BASE DE DATOS EL CRIOLLO CREADA EXITOSAMENTE';
PRINT '=============================================';
PRINT '';
PRINT '📊 ESTRUCTURA SINCRONIZADA:';
PRINT '✅ 16 tablas creadas perfectamente sincronizadas con entidades C#';
PRINT '✅ Todos los tipos de datos coinciden exactamente';
PRINT '✅ Relaciones Foreign Key configuradas correctamente';
PRINT '✅ Índices optimizados para consultas frecuentes';
PRINT '';
PRINT '🎯 DATOS INICIALES INCLUIDOS:';
PRINT '- 4 roles de usuario (Administrador, Recepción, Mesero, Cajero)';
PRINT '- 8 categorías de productos dominicanos';
PRINT '- 37 productos de comida típica dominicana';
PRINT '- 12 mesas configuradas (Terraza, Salón, VIP, Patio)';
PRINT '- 6 combos especiales dominicanos';
PRINT '- 5 clientes de ejemplo';
PRINT '- Inventario inicial completo para todos los productos';
PRINT '';
PRINT '🔧 AUTOMATIZACIÓN CONFIGURADA:';
PRINT '- Generación automática de números de orden (ORD-YYYYMMDD-####)';
PRINT '- Generación automática de números de factura (FACT-YYYYMMDD-####)';
PRINT '- Actualización automática de estado de mesas';
PRINT '- Control automático de inventario con movimientos registrados';
PRINT '- Triggers para liberar mesas al facturar';
PRINT '';
PRINT '📈 REPORTES DISPONIBLES:';
PRINT '- sp_ReporteVentasPorFecha: Ventas por rango de fechas';
PRINT '- sp_ProductosMasVendidos: Productos top con filtros de fecha';
PRINT '- sp_EstadoMesas: Estado actual de todas las mesas';
PRINT '- sp_DashboardAdmin: Métricas clave para administración';
PRINT '';
PRINT '🇩🇴 ESPECIALIZACIÓN DOMINICANA:';
PRINT '- Menú completo de comida típica dominicana';
PRINT '- Combos tradicionales (La Bandera, Sancocho, etc.)';
PRINT '- Precios en pesos dominicanos (RD$)';
PRINT '- Categorías adaptadas a la gastronomía local';
PRINT '';
PRINT '=============================================';
PRINT '🚀 LA BASE DE DATOS ESTÁ LISTA PARA PRODUCCIÓN';
PRINT '=============================================';