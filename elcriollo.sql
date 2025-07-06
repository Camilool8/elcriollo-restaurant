-- =============================================
-- Script de Creación de Base de Datos: El Criollo
-- Sistema de Gestión para Restaurante Dominicano
-- Versión: 1.0 - Base de Datos Pura
-- =============================================

-- Verificar si la base de datos existe, si no, crearla
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ElCriolloRestaurante')
BEGIN
    CREATE DATABASE ElCriolloRestaurante;
    PRINT 'Base de datos ElCriolloRestaurante creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La base de datos ElCriolloRestaurante ya existe.';
END

USE ElCriolloRestaurante;

-- =============================================
-- CREACIÓN DE TABLAS
-- =============================================

-- Tabla de Roles de Usuario
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE Roles (
        RolID INT IDENTITY(1,1) PRIMARY KEY,
        NombreRol VARCHAR(50) NOT NULL UNIQUE,
        Descripcion VARCHAR(200),
        Estado BIT DEFAULT 1
    );
    PRINT 'Tabla Roles creada.';
END

-- Tabla de Usuarios del Sistema (Backend manejará el hash)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
BEGIN
    CREATE TABLE Usuarios (
        UsuarioID INT IDENTITY(1,1) PRIMARY KEY,
        Usuario VARCHAR(50) NOT NULL UNIQUE,
        ContrasenaHash VARCHAR(500) NOT NULL, -- El backend almacenará el hash completo
        RolID INT NOT NULL,
        Email VARCHAR(70),
        EmpleadoID INT NULL,
        RefreshToken VARCHAR(500),
        RefreshTokenExpiry DATETIME,
        FechaCreacion DATETIME DEFAULT GETDATE(),
        UltimoAcceso DATETIME,
        Estado BIT DEFAULT 1,
        RequiereCambioContrasena BIT DEFAULT 0,
        FOREIGN KEY (RolID) REFERENCES Roles(RolID)
    );
    PRINT 'Tabla Usuarios creada.';
END

-- Tabla de Empleados
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Empleados' AND xtype='U')
BEGIN
    CREATE TABLE Empleados (
        EmpleadoID INT IDENTITY(1,1) PRIMARY KEY,
        Cedula VARCHAR(16) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Apellido VARCHAR(50) NOT NULL,
        Sexo VARCHAR(15),
        Direccion VARCHAR(100),
        Telefono VARCHAR(50),
        Email VARCHAR(70),
        Salario DECIMAL(10,2),
        Departamento VARCHAR(100),
        FechaIngreso DATE DEFAULT GETDATE(),
        UsuarioID INT NULL,
        Estado BIT DEFAULT 1,
        FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID)
    );
    PRINT 'Tabla Empleados creada.';
END

-- Tabla de Clientes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clientes' AND xtype='U')
BEGIN
    CREATE TABLE Clientes (
        ClienteID INT IDENTITY(1,1) PRIMARY KEY,
        Cedula VARCHAR(16),
        Nombre VARCHAR(50) NOT NULL,
        Apellido VARCHAR(50) NOT NULL,
        Telefono VARCHAR(50),
        Email VARCHAR(70),
        FechaRegistro DATE DEFAULT GETDATE(),
        Estado BIT DEFAULT 1
    );
    PRINT 'Tabla Clientes creada.';
END

-- Tabla de Mesas
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Mesas' AND xtype='U')
BEGIN
    CREATE TABLE Mesas (
        MesaID INT IDENTITY(1,1) PRIMARY KEY,
        NumeroMesa INT NOT NULL UNIQUE,
        Capacidad INT NOT NULL,
        Ubicacion VARCHAR(50),
        Estado VARCHAR(20) DEFAULT 'Libre', -- Libre, Ocupada, Reservada, Mantenimiento
        FechaUltimaLimpieza DATETIME,
        FechaUltimaActualizacion DATETIME DEFAULT GETDATE()
    );
    PRINT 'Tabla Mesas creada.';
END

-- Tabla de Reservaciones
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reservaciones' AND xtype='U')
BEGIN
    CREATE TABLE Reservaciones (
        ReservacionID INT IDENTITY(1,1) PRIMARY KEY,
        MesaID INT NOT NULL,
        ClienteID INT NOT NULL,
        CantidadPersonas INT NOT NULL,
        FechaYHora DATETIME NOT NULL,
        DuracionEstimada INT DEFAULT 120, -- en minutos
        Observaciones VARCHAR(500),
        Estado VARCHAR(20) DEFAULT 'Pendiente', -- Pendiente, Confirmada, Completada, Cancelada
        UsuarioCreacion INT,
        FechaCreacion DATETIME DEFAULT GETDATE(),
        FechaModificacion DATETIME,
        FOREIGN KEY (MesaID) REFERENCES Mesas(MesaID),
        FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID),
        FOREIGN KEY (UsuarioCreacion) REFERENCES Usuarios(UsuarioID)
    );
    PRINT 'Tabla Reservaciones creada.';
END

-- Tabla de Categorías de Productos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categorias' AND xtype='U')
BEGIN
    CREATE TABLE Categorias (
        CategoriaID INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(50) NOT NULL UNIQUE,
        Descripcion VARCHAR(200),
        Estado BIT DEFAULT 1
    );
    PRINT 'Tabla Categorias creada.';
END

-- Tabla de Productos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Productos' AND xtype='U')
BEGIN
    CREATE TABLE Productos (
        ProductoID INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(100) NOT NULL,
        Descripcion VARCHAR(200),
        CategoriaID INT NOT NULL,
        Precio DECIMAL(10,2) NOT NULL,
        CostoPreparacion DECIMAL(10,2),
        TiempoPreparacion INT, -- en minutos
        Imagen VARCHAR(255),
        Estado BIT DEFAULT 1,
        FOREIGN KEY (CategoriaID) REFERENCES Categorias(CategoriaID)
    );
    PRINT 'Tabla Productos creada.';
END

-- Tabla de Inventario
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Inventario' AND xtype='U')
BEGIN
    CREATE TABLE Inventario (
        InventarioID INT IDENTITY(1,1) PRIMARY KEY,
        ProductoID INT NOT NULL,
        CantidadDisponible INT NOT NULL DEFAULT 0,
        CantidadMinima INT DEFAULT 5,
        UltimaActualizacion DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
    );
    PRINT 'Tabla Inventario creada.';
END

-- Tabla de Combos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Combos' AND xtype='U')
BEGIN
    CREATE TABLE Combos (
        ComboID INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(100) NOT NULL,
        Descripcion VARCHAR(500),
        Precio DECIMAL(10,2) NOT NULL,
        Descuento DECIMAL(5,2) DEFAULT 0,
        Estado BIT DEFAULT 1
    );
    PRINT 'Tabla Combos creada.';
END

-- Tabla de Productos en Combos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ComboProductos' AND xtype='U')
BEGIN
    CREATE TABLE ComboProductos (
        ComboProductoID INT IDENTITY(1,1) PRIMARY KEY,
        ComboID INT NOT NULL,
        ProductoID INT NOT NULL,
        Cantidad INT DEFAULT 1,
        FOREIGN KEY (ComboID) REFERENCES Combos(ComboID),
        FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
    );
    PRINT 'Tabla ComboProductos creada.';
END

-- Tabla de Órdenes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Ordenes' AND xtype='U')
BEGIN
    CREATE TABLE Ordenes (
        OrdenID INT IDENTITY(1,1) PRIMARY KEY,
        NumeroOrden VARCHAR(20) NOT NULL UNIQUE,
        MesaID INT,
        ClienteID INT,
        EmpleadoID INT NOT NULL,
        FechaCreacion DATETIME DEFAULT GETDATE(),
        FechaActualizacion DATETIME,
        Estado VARCHAR(20) DEFAULT 'Pendiente', -- Pendiente, EnPreparacion, Lista, Entregada, Cancelada
        TipoOrden VARCHAR(20) DEFAULT 'Mesa', -- Mesa, Llevar, Delivery
        SubtotalCalculado DECIMAL(10,2),
        Impuesto DECIMAL(10,2),
        TotalCalculado DECIMAL(10,2),
        Observaciones VARCHAR(500),
        FOREIGN KEY (MesaID) REFERENCES Mesas(MesaID),
        FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID),
        FOREIGN KEY (EmpleadoID) REFERENCES Empleados(EmpleadoID)
    );
    PRINT 'Tabla Ordenes creada.';
END

-- Tabla de Detalle de Órdenes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DetalleOrdenes' AND xtype='U')
BEGIN
    CREATE TABLE DetalleOrdenes (
        DetalleOrdenID INT IDENTITY(1,1) PRIMARY KEY,
        OrdenID INT NOT NULL,
        ProductoID INT,
        ComboID INT,
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(10,2) NOT NULL,
        Descuento DECIMAL(10,2) DEFAULT 0,
        Subtotal AS (Cantidad * PrecioUnitario - Descuento) PERSISTED,
        Observaciones VARCHAR(250),
        FOREIGN KEY (OrdenID) REFERENCES Ordenes(OrdenID),
        FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID),
        FOREIGN KEY (ComboID) REFERENCES Combos(ComboID)
    );
    PRINT 'Tabla DetalleOrdenes creada.';
END

-- Tabla de Facturas
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Facturas' AND xtype='U')
BEGIN
    CREATE TABLE Facturas (
        FacturaID INT IDENTITY(1,1) PRIMARY KEY,
        NumeroFactura VARCHAR(20) NOT NULL UNIQUE,
        OrdenID INT NOT NULL,
        ClienteID INT NOT NULL,
        EmpleadoID INT NOT NULL,
        FechaFactura DATETIME DEFAULT GETDATE(),
        FechaPago DATETIME,
        Subtotal DECIMAL(10,2) NOT NULL,
        Impuesto DECIMAL(10,2) DEFAULT 0,
        Descuento DECIMAL(10,2) DEFAULT 0,
        Propina DECIMAL(10,2) DEFAULT 0,
        Total DECIMAL(10,2) NOT NULL,
        MetodoPago VARCHAR(20) DEFAULT 'Efectivo', -- Efectivo, Tarjeta, Transferencia
        Estado VARCHAR(20) DEFAULT 'Pagada', -- Pendiente, Pagada, Anulada
        ObservacionesPago VARCHAR(500),
        FOREIGN KEY (OrdenID) REFERENCES Ordenes(OrdenID),
        FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID),
        FOREIGN KEY (EmpleadoID) REFERENCES Empleados(EmpleadoID)
    );
    PRINT 'Tabla Facturas creada.';
END

-- Tabla de Transacciones de Email
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EmailTransacciones' AND xtype='U')
BEGIN
    CREATE TABLE EmailTransacciones (
        EmailID INT IDENTITY(1,1) PRIMARY KEY,
        DestinatarioEmail VARCHAR(100) NOT NULL,
        Asunto VARCHAR(200) NOT NULL,
        Mensaje TEXT,
        TipoEmail VARCHAR(50), -- Confirmacion, Reserva, Factura, Promocion, BienvenidaUsuario
        ReferenciaID INT, -- ID de la reserva, orden, factura o usuario
        FechaEnvio DATETIME DEFAULT GETDATE(),
        Estado VARCHAR(20) DEFAULT 'Pendiente', -- Pendiente, Enviado, Error
        MensajeError VARCHAR(500),
        IntentosEnvio INT DEFAULT 0
    );
    PRINT 'Tabla EmailTransacciones creada.';
END

-- =============================================
-- INSERCIÓN DE DATOS INICIALES
-- =============================================

-- Insertar Roles
IF NOT EXISTS (SELECT * FROM Roles)
BEGIN
    INSERT INTO Roles (NombreRol, Descripcion) VALUES 
    ('Administrador', 'Control total del sistema'),
    ('Recepcion', 'Gestión de reservas y mesas'),
    ('Mesero', 'Toma de órdenes y atención al cliente'),
    ('Cajero', 'Procesamiento de pagos y facturación');
    PRINT 'Roles insertados.';
END

-- NOTA: El usuario administrador será creado por el backend al inicializar

-- Insertar Empleados de ejemplo con los nuevos campos
IF NOT EXISTS (SELECT * FROM Empleados)
BEGIN
    INSERT INTO Empleados (Cedula, Nombre, Apellido, Sexo, Direccion, Telefono, Email, Salario, Departamento, FechaIngreso, Estado) VALUES 
    ('001-1234567-8', 'Maria', 'Gonzalez', 'Femenino', 'Calle Principal 123', '809-555-2001', 'maria.gonzalez@elcriollo.com', 35000.00, 'Recepción', '2023-01-15', 1),
    ('001-2345678-9', 'Carlos', 'Rodriguez', 'Masculino', 'Av. Central 456', '809-555-2002', 'carlos.rodriguez@elcriollo.com', 28000.00, 'Servicio', '2023-02-20', 1),
    ('001-3456789-0', 'Ana', 'Martinez', 'Femenino', 'Calle Norte 789', '809-555-2003', 'ana.martinez@elcriollo.com', 28000.00, 'Servicio', '2023-03-10', 1),
    ('001-4567890-1', 'Jose', 'Perez', 'Masculino', 'Av. Sur 321', '809-555-2004', 'jose.perez@elcriollo.com', 32000.00, 'Caja', '2023-01-20', 1),
    ('001-5678901-2', 'Laura', 'Santos', 'Femenino', 'Calle Este 654', '809-555-2005', 'laura.santos@elcriollo.com', 45000.00, 'Administración', '2022-12-01', 1),
    ('001-6789012-3', 'Miguel', 'Reyes', 'Masculino', 'Av. Oeste 987', '809-555-2006', 'miguel.reyes@elcriollo.com', 25000.00, 'Servicio', '2023-04-05', 1);
    PRINT 'Empleados de ejemplo insertados.';
END

-- Insertar Usuarios de ejemplo (el backend procesará las contraseñas)
-- NOTA: En producción, las contraseñas deben ser hasheadas por el backend
IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario != 'admin')
BEGIN
    -- Usuario para Maria Gonzalez (Recepción)
    INSERT INTO Usuarios (Usuario, ContrasenaHash, RolID, Email, EmpleadoID, Estado) 
    SELECT 'mgonzalez', 'temp_password_hash_1', 2, e.Email, e.EmpleadoID, 1
    FROM Empleados e WHERE e.Cedula = '001-1234567-8';
    
    -- Usuario para Carlos Rodriguez (Mesero)
    INSERT INTO Usuarios (Usuario, ContrasenaHash, RolID, Email, EmpleadoID, Estado) 
    SELECT 'crodriguez', 'temp_password_hash_2', 3, e.Email, e.EmpleadoID, 1
    FROM Empleados e WHERE e.Cedula = '001-2345678-9';
    
    -- Usuario para Ana Martinez (Mesera)
    INSERT INTO Usuarios (Usuario, ContrasenaHash, RolID, Email, EmpleadoID, Estado) 
    SELECT 'amartinez', 'temp_password_hash_3', 3, e.Email, e.EmpleadoID, 1
    FROM Empleados e WHERE e.Cedula = '001-3456789-0';
    
    -- Usuario para Jose Perez (Cajero)
    INSERT INTO Usuarios (Usuario, ContrasenaHash, RolID, Email, EmpleadoID, Estado) 
    SELECT 'jperez', 'temp_password_hash_4', 4, e.Email, e.EmpleadoID, 1
    FROM Empleados e WHERE e.Cedula = '001-4567890-1';
    
    -- Usuario para Laura Santos (Administradora)
    INSERT INTO Usuarios (Usuario, ContrasenaHash, RolID, Email, EmpleadoID, Estado) 
    SELECT 'lsantos', 'temp_password_hash_5', 1, e.Email, e.EmpleadoID, 1
    FROM Empleados e WHERE e.Cedula = '001-5678901-2';
    
    PRINT 'Usuarios de ejemplo insertados.';
    PRINT '⚠️  IMPORTANTE: Las contraseñas deben ser actualizadas por el backend con hashes reales.';
END

-- Actualizar la referencia UsuarioID en Empleados después de crear los usuarios
UPDATE e
SET e.UsuarioID = u.UsuarioID
FROM Empleados e
INNER JOIN Usuarios u ON u.EmpleadoID = e.EmpleadoID
WHERE e.UsuarioID IS NULL;

-- Insertar Clientes de ejemplo para demo
IF NOT EXISTS (SELECT * FROM Clientes)
BEGIN
    INSERT INTO Clientes (Cedula, Nombre, Apellido, Telefono, Email) VALUES 
    ('001-9876543-2', 'Juan Carlos', 'Ramirez', '809-555-1001', 'juan.ramirez@email.com'),
    ('001-8765432-1', 'Lucia Maria', 'Fernandez', '809-555-1002', 'lucia.fernandez@email.com'),
    ('001-7654321-0', 'Pedro Antonio', 'Sanchez', '809-555-1003', 'pedro.sanchez@email.com'),
    ('001-6543210-9', 'Carmen Rosa', 'Torres', '809-555-1004', 'carmen.torres@email.com'),
    ('001-5432109-8', 'Roberto Luis', 'Diaz', '809-555-1005', 'roberto.diaz@email.com');
    PRINT 'Clientes de ejemplo insertados.';
END

-- Insertar Mesas
IF NOT EXISTS (SELECT * FROM Mesas)
BEGIN
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
    PRINT 'Mesas insertadas.';
END

-- Insertar Categorías de Comida Dominicana
IF NOT EXISTS (SELECT * FROM Categorias)
BEGIN
    INSERT INTO Categorias (Nombre, Descripcion) VALUES 
    ('Platos Principales', 'Comidas fuertes tradicionales dominicanas'),
    ('Acompañamientos', 'Arroz, habichuelas y otros acompañantes'),
    ('Frituras', 'Tostones, yuca frita y otras frituras típicas'),
    ('Bebidas', 'Jugos naturales, refrescos y bebidas tradicionales'),
    ('Postres', 'Dulces y postres típicos dominicanos'),
    ('Desayunos', 'Desayunos tradicionales dominicanos'),
    ('Sopas', 'Sopas y caldos dominicanos'),
    ('Mariscos', 'Pescados y mariscos preparados al estilo dominicano');
    PRINT 'Categorías insertadas.';
END

-- Insertar Productos de Comida Dominicana
IF NOT EXISTS (SELECT * FROM Productos)
BEGIN
    INSERT INTO Productos (Nombre, Descripcion, CategoriaID, Precio, CostoPreparacion, TiempoPreparacion) VALUES 
    -- Platos Principales
    ('Pollo Guisado', 'Pollo guisado con vegetales al estilo dominicano', 1, 350.00, 180.00, 30),
    ('Pernil al Horno', 'Cerdo al horno con especias dominicanas', 1, 420.00, 220.00, 45),
    ('Rabo Encendido', 'Rabo de res guisado picante', 1, 450.00, 250.00, 60),
    ('Chivo Guisado', 'Cabrito guisado con especias', 1, 480.00, 280.00, 50),
    ('Costillas BBQ Criolla', 'Costillas con salsa criolla', 1, 520.00, 300.00, 40),
    
    -- Acompañamientos
    ('Arroz Blanco', 'Arroz blanco tradicional', 2, 80.00, 30.00, 15),
    ('Habichuelas Rojas', 'Habichuelas rojas guisadas', 2, 100.00, 40.00, 25),
    ('Moro de Guandules', 'Arroz con guandules', 2, 120.00, 50.00, 30),
    ('Ensalada Verde', 'Ensalada mixta fresca', 2, 90.00, 35.00, 10),
    ('Yuca Hervida', 'Yuca hervida con cebollitas', 2, 70.00, 25.00, 20),
    
    -- Frituras
    ('Tostones', 'Plátano verde frito aplastado', 3, 110.00, 45.00, 15),
    ('Yuca Frita', 'Yuca frita dorada', 3, 95.00, 40.00, 12),
    ('Maduros', 'Plátano maduro frito', 3, 85.00, 35.00, 10),
    ('Chicharrones', 'Chicharrones de cerdo crujientes', 3, 150.00, 80.00, 20),
    ('Quipe', 'Croqueta de trigo rellena', 3, 65.00, 25.00, 15),
    
    -- Bebidas
    ('Morir Soñando', 'Bebida de naranja y leche', 4, 120.00, 40.00, 5),
    ('Jugo de Chinola', 'Jugo natural de maracuyá', 4, 100.00, 35.00, 5),
    ('Jugo de Tamarindo', 'Jugo natural de tamarindo', 4, 110.00, 40.00, 5),
    ('Cerveza Presidente', 'Cerveza nacional dominicana', 4, 150.00, 90.00, 2),
    ('Mamajuana', 'Bebida tradicional dominicana', 4, 200.00, 120.00, 2),
    
    -- Postres
    ('Tres Leches', 'Cake de tres leches tradicional', 5, 180.00, 80.00, 5),
    ('Flan de Coco', 'Flan con coco rallado', 5, 160.00, 70.00, 5),
    ('Majarete', 'Postre de maíz dulce', 5, 140.00, 60.00, 5),
    ('Dulce de Coco', 'Coco dulce en almíbar', 5, 120.00, 50.00, 5),
    
    -- Desayunos
    ('Mangú', 'Puré de plátano verde con cebollitas', 6, 150.00, 60.00, 20),
    ('Tres Golpes', 'Mangú con huevos, queso y salami', 6, 220.00, 100.00, 25),
    ('Huevos Rancheros', 'Huevos con salsa criolla', 6, 180.00, 80.00, 15),
    ('Avena', 'Avena caliente con leche y canela', 6, 90.00, 35.00, 10),
    
    -- Sopas
    ('Sancocho', 'Sopa tradicional con carnes y vegetales', 7, 320.00, 150.00, 45),
    ('Sopa de Pollo', 'Sopa de pollo con vegetales', 7, 200.00, 90.00, 30),
    ('Mondongo', 'Sopa de callos tradicional', 7, 280.00, 130.00, 60),
    
    -- Mariscos
    ('Pescao Frito', 'Pescado frito entero', 8, 380.00, 200.00, 25),
    ('Camarones al Ajillo', 'Camarones en salsa de ajo', 8, 450.00, 250.00, 20),
    ('Pulpo Guisado', 'Pulpo guisado con vegetales', 8, 520.00, 300.00, 35),
    ('Lambí Guisado', 'Caracola guisada criolla', 8, 480.00, 280.00, 40);
    PRINT 'Productos insertados.';
END

-- Insertar datos de Inventario
IF NOT EXISTS (SELECT * FROM Inventario)
BEGIN
    INSERT INTO Inventario (ProductoID, CantidadDisponible, CantidadMinima) 
    SELECT ProductoID, 
           CASE 
               WHEN CategoriaID IN (4, 5) THEN 50  -- Bebidas y postres más stock
               WHEN CategoriaID = 1 THEN 25        -- Platos principales stock medio
               ELSE 30                             -- Otros
           END,
           CASE 
               WHEN CategoriaID IN (4, 5) THEN 10  -- Bebidas y postres mínimo mayor
               ELSE 5                              -- Otros mínimo estándar
           END
    FROM Productos;
    PRINT 'Inventario inicial insertado.';
END

-- Insertar Combos Especiales
IF NOT EXISTS (SELECT * FROM Combos)
BEGIN
    INSERT INTO Combos (Nombre, Descripcion, Precio, Descuento) VALUES 
    ('La Bandera Dominicana', 'Arroz blanco, habichuelas rojas, pollo guisado y ensalada', 480.00, 50.00),
    ('Combo Criollo', 'Pernil, moro de guandules, tostones y jugo', 550.00, 70.00),
    ('Desayuno Típico', 'Tres golpes, avena y jugo de chinola', 320.00, 40.00),
    ('Parrillada Familiar', 'Costillas BBQ, chicharrones, yuca frita, tostones para 4 personas', 1200.00, 200.00),
    ('Combo Marino', 'Pescao frito, arroz blanco, ensalada y maduros', 520.00, 80.00);
    PRINT 'Combos insertados.';
END

-- Insertar productos en combos
IF NOT EXISTS (SELECT * FROM ComboProductos)
BEGIN
    -- La Bandera Dominicana
    INSERT INTO ComboProductos (ComboID, ProductoID, Cantidad) VALUES 
    (1, 6, 1),  -- Arroz Blanco
    (1, 7, 1),  -- Habichuelas Rojas
    (1, 1, 1),  -- Pollo Guisado
    (1, 9, 1),  -- Ensalada Verde
    
    -- Combo Criollo
    (2, 2, 1),  -- Pernil
    (2, 8, 1),  -- Moro de Guandules
    (2, 11, 1), -- Tostones
    (2, 17, 1), -- Jugo de Chinola
    
    -- Desayuno Típico
    (3, 26, 1), -- Tres Golpes
    (3, 28, 1), -- Avena
    (3, 17, 1), -- Jugo de Chinola
    
    -- Parrillada Familiar
    (4, 5, 2),  -- Costillas BBQ
    (4, 14, 2), -- Chicharrones
    (4, 12, 2), -- Yuca Frita
    (4, 11, 2), -- Tostones
    
    -- Combo Marino
    (5, 31, 1), -- Pescao Frito
    (5, 6, 1),  -- Arroz Blanco
    (5, 9, 1),  -- Ensalada Verde
    (5, 13, 1); -- Maduros
    PRINT 'Productos en combos insertados.';
END

-- =============================================
-- CREACIÓN DE ÍNDICES PARA OPTIMIZACIÓN
-- =============================================

-- Índices en tablas principales
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuarios_Usuario')
    CREATE INDEX IX_Usuarios_Usuario ON Usuarios(Usuario);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Empleados_Cedula')
    CREATE INDEX IX_Empleados_Cedula ON Empleados(Cedula);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_Cedula')
    CREATE INDEX IX_Clientes_Cedula ON Clientes(Cedula);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Mesas_Estado')
    CREATE INDEX IX_Mesas_Estado ON Mesas(Estado);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ordenes_Estado')
    CREATE INDEX IX_Ordenes_Estado ON Ordenes(Estado);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Facturas_FechaFactura')
    CREATE INDEX IX_Facturas_FechaFactura ON Facturas(FechaFactura);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reservaciones_FechaYHora')
    CREATE INDEX IX_Reservaciones_FechaYHora ON Reservaciones(FechaYHora);

PRINT 'Índices creados para optimización.';

-- =============================================
-- TRIGGERS PARA AUTOMATIZACIÓN
-- =============================================

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
        SET Estado = ''Ocupada''
        WHERE MesaID IN (SELECT MesaID FROM inserted WHERE MesaID IS NOT NULL);
    END');
    PRINT 'Trigger tr_OrdenCreada_ActualizarMesa creado.';
END

-- Trigger para liberar mesa cuando se genera factura
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_FacturaGenerada_LiberarMesa')
BEGIN
    EXEC('
    CREATE TRIGGER tr_FacturaGenerada_LiberarMesa
    ON Facturas
    AFTER INSERT
    AS
    BEGIN
        UPDATE Mesas 
        SET Estado = ''Libre''
        WHERE MesaID IN (
            SELECT o.MesaID 
            FROM inserted i
            INNER JOIN Ordenes o ON i.OrdenID = o.OrdenID
            WHERE o.MesaID IS NOT NULL
        );
        
        UPDATE Ordenes 
        SET Estado = ''Completada''
        WHERE OrdenID IN (SELECT OrdenID FROM inserted);
    END');
    PRINT 'Trigger tr_FacturaGenerada_LiberarMesa creado.';
END

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
        WHERE OrdenID IN (SELECT OrdenID FROM inserted);
    END');
    PRINT 'Trigger tr_GenerarNumeroOrden creado.';
END

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
        WHERE FacturaID IN (SELECT FacturaID FROM inserted);
    END');
    PRINT 'Trigger tr_GenerarNumeroFactura creado.';
END

-- =============================================
-- PROCEDIMIENTOS ALMACENADOS PARA REPORTES
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
            MIN(f.Total) as VentaMinima
        FROM Facturas f
        WHERE CAST(f.FechaFactura AS DATE) BETWEEN @FechaInicio AND @FechaFin
            AND f.Estado = ''Pagada''
        GROUP BY CAST(f.FechaFactura AS DATE)
        ORDER BY Fecha DESC;
    END');
    PRINT 'Procedimiento sp_ReporteVentasPorFecha creado.';
END

-- Procedimiento para obtener productos más vendidos
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_ProductosMasVendidos')
BEGIN
    EXEC('
    CREATE PROCEDURE sp_ProductosMasVendidos
        @TopN INT = 10
    AS
    BEGIN
        SELECT TOP (@TopN)
            p.Nombre,
            c.Nombre as Categoria,
            SUM(do.Cantidad) as TotalVendido,
            SUM(do.Subtotal) as IngresoTotal,
            AVG(do.PrecioUnitario) as PrecioPromedio
        FROM DetalleOrdenes do
        INNER JOIN Productos p ON do.ProductoID = p.ProductoID
        INNER JOIN Categorias c ON p.CategoriaID = c.CategoriaID
        INNER JOIN Ordenes o ON do.OrdenID = o.OrdenID
        INNER JOIN Facturas f ON o.OrdenID = f.OrdenID
        WHERE f.Estado = ''Pagada''
        GROUP BY p.ProductoID, p.Nombre, c.Nombre
        ORDER BY TotalVendido DESC;
    END');
    PRINT 'Procedimiento sp_ProductosMasVendidos creado.';
END

-- Procedimiento para obtener estado de mesas
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
            END as Cliente
        FROM Mesas m
        LEFT JOIN Ordenes o ON m.MesaID = o.MesaID AND o.Estado NOT IN (''Completada'', ''Cancelada'')
        LEFT JOIN Clientes c1 ON o.ClienteID = c1.ClienteID
        LEFT JOIN Reservaciones r ON m.MesaID = r.MesaID AND r.Estado = ''Confirmada'' 
            AND r.FechaYHora BETWEEN GETDATE() AND DATEADD(HOUR, 2, GETDATE())
        LEFT JOIN Clientes c2 ON r.ClienteID = c2.ClienteID
        ORDER BY m.NumeroMesa;
    END');
    PRINT 'Procedimiento sp_EstadoMesas creado.';
END

PRINT '=============================================';
PRINT 'BASE DE DATOS EL CRIOLLO CREADA EXITOSAMENTE';
PRINT '=============================================';
PRINT '';
PRINT '🎯 CONFIGURACIÓN PARA DESARROLLO:';
PRINT '✅ Base de datos lista para backend';
PRINT '✅ Usuario administrador será creado por backend';
PRINT '✅ Estructura completa de 15 tablas';
PRINT '✅ Sistema de emails preparado';
PRINT '✅ Triggers automáticos configurados';
PRINT '';
PRINT '📊 DATOS INICIALES INCLUIDOS:';
PRINT '- 4 roles de usuario definidos';
PRINT '- 6 empleados con salarios y departamentos';
PRINT '- 5 usuarios de ejemplo (1 admin, 1 recepción, 2 meseros, 1 cajero)';
PRINT '- 33 productos de comida dominicana';
PRINT '- 8 categorías de productos';
PRINT '- 12 mesas configuradas';
PRINT '- 5 combos especiales';
PRINT '- 5 clientes de ejemplo';
PRINT '- Inventario inicial completo';
PRINT '';
PRINT '🔧 FUNCIONALIDADES AUTOMATIZADAS:';
PRINT '- Generación automática de números de orden';
PRINT '- Generación automática de números de factura';
PRINT '- Liberación automática de mesas al facturar';
PRINT '- Índices optimizados para consultas';
PRINT '- Procedimientos para reportes';
PRINT '';
PRINT '=============================================';

-- =============================================
-- ACTUALIZACIÓN DE TABLAS EXISTENTES
-- Estas sentencias ALTER TABLE solo se ejecutarán si las columnas no existen
-- =============================================

PRINT '';
PRINT '=============================================';
PRINT '📋 ACTUALIZANDO ESTRUCTURA DE TABLAS EXISTENTES';
PRINT '=============================================';

-- Actualizar tabla Usuarios
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'EmpleadoID')
BEGIN
    ALTER TABLE Usuarios ADD EmpleadoID INT NULL;
    PRINT '✅ Campo EmpleadoID agregado a tabla Usuarios';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'RefreshToken')
BEGIN
    ALTER TABLE Usuarios ADD RefreshToken VARCHAR(500);
    PRINT '✅ Campo RefreshToken agregado a tabla Usuarios';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'RefreshTokenExpiry')
BEGIN
    ALTER TABLE Usuarios ADD RefreshTokenExpiry DATETIME;
    PRINT '✅ Campo RefreshTokenExpiry agregado a tabla Usuarios';
END

-- Actualizar tabla Empleados
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Empleados') AND name = 'Salario')
BEGIN
    ALTER TABLE Empleados ADD Salario DECIMAL(10,2);
    PRINT '✅ Campo Salario agregado a tabla Empleados';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Empleados') AND name = 'Departamento')
BEGIN
    ALTER TABLE Empleados ADD Departamento VARCHAR(100);
    PRINT '✅ Campo Departamento agregado a tabla Empleados';
END

-- Actualizar tabla Mesas
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Mesas') AND name = 'FechaUltimaActualizacion')
BEGIN
    ALTER TABLE Mesas ADD FechaUltimaActualizacion DATETIME DEFAULT GETDATE();
    PRINT '✅ Campo FechaUltimaActualizacion agregado a tabla Mesas';
END

-- Actualizar tabla Reservaciones
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Reservaciones') AND name = 'UsuarioCreacion')
BEGIN
    ALTER TABLE Reservaciones ADD UsuarioCreacion INT;
    ALTER TABLE Reservaciones ADD CONSTRAINT FK_Reservaciones_UsuarioCreacion 
        FOREIGN KEY (UsuarioCreacion) REFERENCES Usuarios(UsuarioID);
    PRINT '✅ Campo UsuarioCreacion agregado a tabla Reservaciones';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Reservaciones') AND name = 'FechaModificacion')
BEGIN
    ALTER TABLE Reservaciones ADD FechaModificacion DATETIME;
    PRINT '✅ Campo FechaModificacion agregado a tabla Reservaciones';
END

-- Actualizar tabla Ordenes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ordenes') AND name = 'FechaActualizacion')
BEGIN
    ALTER TABLE Ordenes ADD FechaActualizacion DATETIME;
    PRINT '✅ Campo FechaActualizacion agregado a tabla Ordenes';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ordenes') AND name = 'SubtotalCalculado')
BEGIN
    ALTER TABLE Ordenes ADD SubtotalCalculado DECIMAL(10,2);
    PRINT '✅ Campo SubtotalCalculado agregado a tabla Ordenes';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ordenes') AND name = 'Impuesto')
BEGIN
    ALTER TABLE Ordenes ADD Impuesto DECIMAL(10,2);
    PRINT '✅ Campo Impuesto agregado a tabla Ordenes';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ordenes') AND name = 'TotalCalculado')
BEGIN
    ALTER TABLE Ordenes ADD TotalCalculado DECIMAL(10,2);
    PRINT '✅ Campo TotalCalculado agregado a tabla Ordenes';
END

-- Actualizar tabla Facturas
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Facturas') AND name = 'FechaPago')
BEGIN
    ALTER TABLE Facturas ADD FechaPago DATETIME;
    PRINT '✅ Campo FechaPago agregado a tabla Facturas';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Facturas') AND name = 'ObservacionesPago')
BEGIN
    ALTER TABLE Facturas ADD ObservacionesPago VARCHAR(500);
    PRINT '✅ Campo ObservacionesPago agregado a tabla Facturas';
END

-- Actualizar tabla EmailTransacciones
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('EmailTransacciones') AND name = 'MensajeError')
BEGIN
    ALTER TABLE EmailTransacciones ADD MensajeError VARCHAR(500);
    PRINT '✅ Campo MensajeError agregado a tabla EmailTransacciones';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('EmailTransacciones') AND name = 'IntentosEnvio')
BEGIN
    ALTER TABLE EmailTransacciones ADD IntentosEnvio INT DEFAULT 0;
    PRINT '✅ Campo IntentosEnvio agregado a tabla EmailTransacciones';
END

-- Agregar constraint para EmpleadoID en Usuarios si no existe
IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE name = 'FK_Usuarios_EmpleadoID' 
    AND parent_object_id = OBJECT_ID('Usuarios')
)
BEGIN
    ALTER TABLE Usuarios ADD CONSTRAINT FK_Usuarios_EmpleadoID 
        FOREIGN KEY (EmpleadoID) REFERENCES Empleados(EmpleadoID);
    PRINT '✅ Foreign key FK_Usuarios_EmpleadoID agregada';
END

PRINT '';
PRINT '=============================================';
PRINT '✨ ACTUALIZACIÓN DE ESTRUCTURA COMPLETADA';
PRINT '=============================================';
PRINT 'Los nuevos campos han sido agregados a las tablas existentes.';
PRINT '';
PRINT '📌 NUEVOS CAMPOS AGREGADOS:';
PRINT '- Usuarios: EmpleadoID, RefreshToken, RefreshTokenExpiry';
PRINT '- Empleados: Salario, Departamento';
PRINT '- Mesas: FechaUltimaActualizacion';
PRINT '- Reservaciones: UsuarioCreacion, FechaModificacion';
PRINT '- Ordenes: FechaActualizacion, SubtotalCalculado, Impuesto, TotalCalculado';
PRINT '- Facturas: FechaPago, ObservacionesPago';
PRINT '- EmailTransacciones: MensajeError, IntentosEnvio';
PRINT '';
PRINT '=============================================';
PRINT '🎉 SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '=============================================';
-- FIN DEL SCRIPT