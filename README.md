# 🇩🇴 El Criollo - Sistema POS para Restaurante Dominicano

![El Criollo Banner](https://img.shields.io/badge/El%20Criollo-Sistema%20POS-green?style=for-the-badge&logo=restaurant)

Sistema de Punto de Venta (POS) moderno para restaurante dominicano con menú de comida típica, gestión de mesas, reservas, inventario y facturación.

## 📋 Descripción del Proyecto

**El Criollo** es un sistema POS completo diseñado específicamente para restaurantes que sirven comida dominicana auténtica. El sistema permite gestionar todas las operaciones del restaurante desde la toma de pedidos hasta la facturación, incluyendo control de inventario, reservas de mesas y reportes de ventas.

### 🎯 Características Principales

- ✅ **Sistema de Autenticación** con JWT y roles de usuario
- ✅ **Gestión de Mesas** (Libre, Ocupada, Reservada, Mantenimiento)
- ✅ **Sistema de Reservas** con control de tiempo
- ✅ **Menú Digital** con 33 productos de comida dominicana
- ✅ **Control de Inventario** con alertas de stock bajo
- ✅ **Sistema de Órdenes** (Mesa, Llevar, Delivery)
- ✅ **Facturación** individual y grupal
- ✅ **Reportes de Ventas** y analytics
- ✅ **Sistema de Emails** para confirmaciones
- ✅ **Combos Especiales** de comida dominicana

## 🏗️ Arquitectura del Proyecto

### Tecnologías Utilizadas

| Capa              | Tecnología            | Versión            |
| ----------------- | --------------------- | ------------------ |
| **Base de Datos** | SQL Server            | 2022+              |
| **Backend**       | .NET Core Web API     | 8.0                |
| **ORM**           | Entity Framework Core | 8.0                |
| **Autenticación** | JWT Bearer Tokens     | -                  |
| **Documentación** | Swagger/OpenAPI       | 6.5.0              |
| **Logging**       | Serilog               | 8.0.0              |
| **Frontend**      | React                 | 18+ (Próximamente) |

### Estructura del Proyecto

```
ElCriollo/
├── 📄 elcriollo.sql                    # ✅ Script de Base de Datos
├── 📄 README.md                        # ✅ Documentación
├── 📄 .gitignore                       # ✅ Configuración Git
├── 📄 ElCriollo.sln                    # ✅ Solution File
└── 📂 src/
    └── 📂 ElCriollo.API/               # ✅ Proyecto Principal API
        ├── 📄 ElCriollo.API.csproj     # ✅ Configuración del Proyecto
        ├── 📄 Program.cs               # ✅ Punto de Entrada
        ├── 📄 appsettings.json         # ✅ Configuraciones
        ├── 📄 appsettings.Development.json # ✅ Config Desarrollo
        │
        ├── 📂 Configuration/           # ✅ Clases de Configuración
        │   ├── 📄 JwtSettings.cs       # ✅ Config JWT
        │   └── 📄 EmailSettings.cs     # ✅ Config Email
        │
        ├── 📂 Models/                  # ✅ Modelos de Datos (COMPLETADO)
        │   ├── 📂 Entities/            # ✅ 15 Entidades Completas
        │   │   ├── 📄 Rol.cs           # ✅ Roles de usuario
        │   │   ├── 📄 Usuario.cs       # ✅ Usuarios del sistema
        │   │   ├── 📄 Empleado.cs      # ✅ Empleados
        │   │   ├── 📄 Cliente.cs       # ✅ Clientes
        │   │   ├── 📄 Mesa.cs          # ✅ Mesas del restaurante
        │   │   ├── 📄 Reservacion.cs   # ✅ Sistema de reservas
        │   │   ├── 📄 Categoria.cs     # ✅ Categorías de productos
        │   │   ├── 📄 Producto.cs      # ✅ Productos del menú
        │   │   ├── 📄 Inventario.cs    # ✅ Control de inventario
        │   │   ├── 📄 Combo.cs         # ✅ Combos especiales
        │   │   ├── 📄 ComboProducto.cs # ✅ Relación Many-to-Many
        │   │   ├── 📄 Orden.cs         # ✅ Órdenes/comandas
        │   │   ├── 📄 DetalleOrden.cs  # ✅ Detalles de órdenes
        │   │   ├── 📄 Factura.cs       # ✅ Sistema de facturación
        │   │   └── 📄 EmailTransaccion.cs # ✅ Historial de emails
        │   │
        │   ├── 📂 DTOs/                # ✅ Data Transfer Objects
        │   │   ├── 📂 Request/         # ✅ DTOs para requests
        │   │   │   ├── 📄 LoginRequest.cs # ✅ Login
        │   │   │   ├── 📄 CreateUsuarioRequest.cs # ✅ Crear usuario
        │   │   │   ├── 📄 CreateReservacionRequest.cs # ✅ Crear reserva
        │   │   │   └── 📄 CreateOrdenRequest.cs # ✅ Crear orden
        │   │   │
        │   │   └── 📂 Response/        # ✅ DTOs para responses
        │   │       ├── 📄 LoginResponse.cs # ✅ Respuesta login
        │   │       ├── 📄 UsuarioResponse.cs # ✅ Datos usuario
        │   │       ├── 📄 ProductoResponse.cs # ✅ Datos producto
        │   │       ├── 📄 MesaResponse.cs # ✅ Estado mesa
        │   │       └── 📄 OrdenResponse.cs # ✅ Datos orden
        │   │
        │   └── 📂 ViewModels/          # ✅ ViewModels específicos
        │       └── 📄 DashboardViewModel.cs # ✅ Dashboard principal
        │
        ├── 📂 Data/                    # ✅ Acceso a datos (COMPLETADO)
        │   └── 📄 ElCriolloDbContext.cs # ✅ Entity Framework DbContext
        │
        ├── 📂 Helpers/                 # ✅ Utilidades (COMPLETADO)
        │   └── 📄 AutoMapperProfile.cs # ✅ Mapeo automático
        │
        ├── 📂 Controllers/             # ⏳ API Controllers (Próximo)
        ├── 📂 Interfaces/              # ⏳ Contratos/Interfaces (Próximo)
        ├── 📂 Repositories/            # ⏳ Repositorios (Próximo)
        ├── 📂 Services/                # ⏳ Servicios (Próximo)
        └── 📂 Middleware/              # ⏳ Middleware (Próximo)
```

## 🚀 Configuración del Proyecto

### Prerrequisitos

- **SQL Server** 2019+ o SQL Server Express
- **.NET 8 SDK** o superior
- **Visual Studio 2022** o VS Code
- **Postman** o similar para pruebas de API

### 🔧 Instalación

1. **Clonar el repositorio**

   ```bash
   git clone <repository-url>
   cd ElCriollo
   ```

2. **Configurar Base de Datos**

   ```bash
   # Ejecutar el script SQL en SQL Server Management Studio
   # o usando sqlcmd:
   sqlcmd -S localhost -i elcriollo.sql
   ```

3. **Configurar Connection String**

   Editar `src/ElCriollo.API/appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=TU_SERVIDOR;Database=ElCriolloRestaurante;Trusted_Connection=true;..."
     }
   }
   ```

4. **Restaurar Paquetes NuGet**

   ```bash
   cd src/ElCriollo.API
   dotnet restore
   ```

5. **Ejecutar la Aplicación**

   ```bash
   dotnet run
   ```

6. **Acceder a Swagger**

   Abrir: `https://localhost:7071` o `http://localhost:5071`

## 🗄️ Base de Datos y Modelo de Datos

### Estructura Completa

La base de datos incluye **15 tablas** principales con **15 entidades Entity Framework** completamente implementadas:

| Tabla                  | Entidad               | Descripción                  | Registros Iniciales |
| ---------------------- | --------------------- | ---------------------------- | ------------------- |
| **Roles**              | `Rol.cs`              | Roles de usuario del sistema | 4 roles             |
| **Usuarios**           | `Usuario.cs`          | Usuarios del sistema         | Admin inicial       |
| **Empleados**          | `Empleado.cs`         | Empleados del restaurante    | -                   |
| **Clientes**           | `Cliente.cs`          | Clientes del restaurante     | 5 clientes demo     |
| **Mesas**              | `Mesa.cs`             | Mesas del restaurante        | 12 mesas            |
| **Reservaciones**      | `Reservacion.cs`      | Reservas de mesas            | -                   |
| **Categorias**         | `Categoria.cs`        | Categorías de productos      | 8 categorías        |
| **Productos**          | `Producto.cs`         | Menú de comida dominicana    | 33 productos        |
| **Inventario**         | `Inventario.cs`       | Control de stock             | Stock inicial       |
| **Combos**             | `Combo.cs`            | Combos especiales            | 5 combos            |
| **ComboProductos**     | `ComboProducto.cs`    | Productos en combos          | Configurados        |
| **Ordenes**            | `Orden.cs`            | Órdenes/comandas             | -                   |
| **DetalleOrdenes**     | `DetalleOrden.cs`     | Detalles de órdenes          | -                   |
| **Facturas**           | `Factura.cs`          | Facturas generadas           | -                   |
| **EmailTransacciones** | `EmailTransaccion.cs` | Historial de emails          | -                   |

### 🏗️ **Características del Modelo de Datos**

- ✅ **67 propiedades calculadas** con lógica de negocio
- ✅ **+150 métodos de utilidad** en las entidades
- ✅ **Validaciones completas** con Data Annotations
- ✅ **Relaciones Foreign Key** perfectamente configuradas
- ✅ **Índices únicos** y constraints de verificación
- ✅ **Auditoría automática** de fechas de modificación

### 🍽️ Menú de Comida Dominicana

El sistema incluye **33 productos auténticos** organizados en 8 categorías:

- **Platos Principales**: Pollo Guisado, Pernil al Horno, Rabo Encendido, etc.
- **Acompañamientos**: Arroz Blanco, Habichuelas Rojas, Moro de Guandules, etc.
- **Frituras**: Tostones, Yuca Frita, Maduros, Chicharrones, etc.
- **Bebidas**: Morir Soñando, Jugo de Chinola, Mamajuana, etc.
- **Postres**: Tres Leches, Flan de Coco, Majarete, etc.
- **Desayunos**: Mangú, Tres Golpes, Huevos Rancheros, etc.
- **Sopas**: Sancocho, Sopa de Pollo, Mondongo, etc.
- **Mariscos**: Pescao Frito, Camarones al Ajillo, etc.

## 🔐 Sistema de Usuarios

### Usuario Administrador Predefinido

```
Usuario: thecuevas0123_
Contraseña: thepikachu0123_
Email: admin@elcriollo.com
Rol: Administrador
```

### Roles del Sistema

| Rol               | Descripción                 | Permisos                  |
| ----------------- | --------------------------- | ------------------------- |
| **Administrador** | Control total del sistema   | Todos los permisos        |
| **Recepcion**     | Gestión de reservas y mesas | Reservas, mesas, clientes |
| **Mesero**        | Toma de órdenes             | Órdenes, mesas, productos |
| **Cajero**        | Procesamiento de pagos      | Facturación, reportes     |

## 📊 Progreso del Desarrollo

### ✅ Fase 1: Base de Datos (100% Completada)

- [x] Script SQL completo con 15 tablas
- [x] Datos iniciales de productos dominicanos
- [x] Triggers automáticos
- [x] Procedimientos almacenados para reportes
- [x] Índices de optimización

### 🔄 Fase 2: Backend .NET (En Progreso - 20%)

- [x] Configuración inicial del proyecto
- [x] Estructura de archivos y carpetas
- [x] Configuración de JWT y Email
- [x] Program.cs con middleware configurado
- [ ] Modelos y Entidades (Próximo paso)
- [ ] DbContext y Configuraciones EF
- [ ] Repositorios
- [ ] Servicios de negocio
- [ ] Controllers y endpoints
- [ ] Middleware personalizado
- [ ] Sistema de validaciones

### ⏳ Fase 3: Frontend React (Pendiente)

- [ ] Configuración inicial de React
- [ ] Diseño con identidad dominicana
- [ ] Componentes de interfaz
- [ ] Integración con API
- [ ] Sistema de autenticación frontend
- [ ] Dashboard y reportes

## 🛠️ Próximos Pasos

### Inmediato (Fase 2 - Segundo Paso)

1. **Crear Modelos de Entidades**

   - Mapear todas las tablas de la BD
   - Crear DTOs para requests/responses
   - ViewModels para casos específicos

2. **Configurar Entity Framework**
   - ElCriolloDbContext
   - Configuraciones de entidades
   - Validaciones de datos

### Siguientes Pasos

3. **Implementar Repositorios**
4. **Desarrollar Servicios de Negocio**
5. **Crear Interfaces y Contratos**
6. **Desarrollar Controllers**

## 📧 Configuración de Emails

El sistema incluye notificaciones por email para:

- Confirmación de nuevos usuarios
- Confirmación de reservas
- Facturas por email
- Notificaciones promocionales

## 🔧 Configuraciones Importantes

### JWT Settings

```json
{
  "JwtSettings": {
    "SecretKey": "Clave_Secreta_Mínimo_32_Caracteres",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryInDays": 7
  }
}
```

### Email Settings

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "FromEmail": "elcriollo.restaurant@gmail.com"
  }
}
```

## 🚀 Comandos Útiles

```bash
# Ejecutar el proyecto en desarrollo
dotnet run --project src/ElCriollo.API

# Ejecutar con hot reload
dotnet watch run --project src/ElCriollo.API

# Crear migración (cuando tengamos DbContext)
dotnet ef migrations add InitialCreate --project src/ElCriollo.API

# Aplicar migraciones
dotnet ef database update --project src/ElCriollo.API

# Verificar base de datos
dotnet ef database drop --project src/ElCriollo.API
```

## 📖 Documentación API

Una vez ejecutado el proyecto, la documentación completa estará disponible en:

- **Swagger UI**: `https://localhost:7071/swagger`
- **Swagger JSON**: `https://localhost:7071/swagger/v1/swagger.json`
- **Health Check**: `https://localhost:7071/health`

## 🎯 Objetivos del Proyecto

Este es un **proyecto universitario** que busca demostrar:

1. **Arquitectura moderna** con .NET Core y React
2. **Seguridad robusta** con JWT y roles
3. **Base de datos bien estructurada** con SQL Server
4. **Identidad cultural dominicana** en el diseño
5. **Funcionalidad completa** de un POS real
6. **Documentación profesional** con Swagger

## 👨‍💻 Información del Desarrollador

**Estudiante**: Mario Luis Cuevas (2020-0500)  
**Profesor**: Jaime Eduardo Paez  
**Materia**: Proyecto 2  
**Universidad**: Universidad Católica Santo Domingo

## 📝 Notas de Desarrollo

- El proyecto está siendo desarrollado paso a paso
- Cada fase se documenta completamente antes de continuar
- Se mantiene coherencia en la arquitectura y patrones utilizados
- El código está comentado en español para facilitar la comprensión
- Se incluyen validaciones y manejo de errores robusto

---

**🔗 Estado Actual**: Fase 2 - 50% completada - Modelos y DbContext finalizados  
**📅 Última Actualización**: Segundo paso completado - 35+ archivos creados  
**⏭️ Próximo Entregable**: Repositorios - Capa de acceso a datos
