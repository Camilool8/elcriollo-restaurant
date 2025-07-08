# 🧪 El Criollo - Suite de Pruebas Automatizadas

Este proyecto contiene una suite completa de pruebas automatizadas para el sistema POS El Criollo, que simula el flujo cotidiano completo de un restaurante dominicano.

## 📋 Descripción General

Las pruebas simulan un día completo de operaciones en el restaurante, cubriendo:

- ✅ **Autenticación y Gestión de Usuarios**
- ✅ **Manejo de Roles y Permisos**
- ✅ **Gestión de Empleados y Clientes**
- ✅ **Control de Mesas e Inventario**
- ✅ **Sistema de Reservas**
- ✅ **Gestión de Órdenes (Mesa, Llevar, Delivery)**
- ✅ **Facturación con ITBIS Dominicano**
- ✅ **Reportes y Analytics**
- ✅ **Validaciones y Casos Edge**

## 🚀 Cómo Ejecutar las Pruebas

### Requisitos Previos

```bash
# Verificar que tienes .NET 8 instalado
dotnet --version

# Debe mostrar: 8.0.x o superior
```

### 1. Preparar Base de Datos de Pruebas

```bash
# Crear base de datos de pruebas
sqlcmd -S localhost -Q "CREATE DATABASE ElCriolloTest"

# Ejecutar script de inicialización
sqlcmd -S localhost -d ElCriolloTest -i ../elcriollo.sql
```

### 2. Ejecutar Todas las Pruebas

```bash
# Desde la raíz del proyecto
cd src/ElCriollo.API.Tests

# Restaurar paquetes
dotnet restore

# Ejecutar todas las pruebas
dotnet test --logger "console;verbosity=detailed"
```

### 3. Ejecutar Pruebas Específicas

```bash
# Solo pruebas de integración
dotnet test --filter "Category=Integration" --logger "console;verbosity=detailed"

# Solo el flujo cotidiano completo
dotnet test --filter "FlujoCotidiano_SimulacionCompleta_DebeCompletarExitosamente" --logger "console;verbosity=detailed"
```

### 4. Generar Reporte de Cobertura

```bash
# Con cobertura de código
dotnet test --collect:"XPlat Code Coverage" --logger "console;verbosity=detailed"
```

## 🔍 Descripción Detallada de las Pruebas

### 📊 Flujo Principal - Simulación Completa

La prueba `FlujoCotidiano_SimulacionCompleta_DebeCompletarExitosamente` ejecuta un flujo completo de 23 pasos que simula un día típico:

#### 🔐 **Fase 1: Autenticación y Setup (Tests 1-2)**

- ✅ Login del administrador con credenciales por defecto
- ✅ Verificación del estado de salud del sistema

#### 👥 **Fase 2: Gestión de Usuarios (Tests 3-6)**

- ✅ Creación de usuarios con diferentes roles (Mesero, Cajero, Recepción)
- ✅ Cambio de contraseñas y validación de seguridad
- ✅ Autenticación de nuevos usuarios con credenciales actualizadas
- ✅ Verificación de información de empleados asociados

#### 🏪 **Fase 3: Gestión de Restaurante (Tests 7-9)**

- ✅ Consulta del estado actual de todas las mesas
- ✅ Revisión del inventario y stock disponible
- ✅ Actualización de inventario con movimientos de entrada

#### 👤 **Fase 4: Gestión de Clientes (Test 10)**

- ✅ Registro de nuevos clientes con validación de datos dominicanos

#### 📅 **Fase 5: Sistema de Reservas (Tests 11-13)**

- ✅ Creación de reservaciones con validación de disponibilidad
- ✅ Consulta de reservaciones del día
- ✅ Confirmación de reservaciones con notificaciones

#### 🍽️ **Fase 6: Gestión de Órdenes (Tests 14-17)**

- ✅ Creación de órdenes con múltiples items
- ✅ Consulta de órdenes por estado
- ✅ Adición de items a órdenes existentes
- ✅ Actualización de estados de órdenes (Pendiente → EnPreparacion → Lista)

#### 💰 **Fase 7: Facturación (Tests 18-20)**

- ✅ Generación de facturas con cálculo automático de ITBIS (18%)
- ✅ Consulta de detalles de facturación
- ✅ Procesamiento de pagos y actualización de estados

#### 📊 **Fase 8: Reportes y Analytics (Tests 21-22)**

- ✅ Generación de reportes de ventas diarias
- ✅ Consulta de dashboard con métricas en tiempo real

#### 🏁 **Fase 9: Cierre de Operaciones (Test 23)**

- ✅ Verificación final del estado del sistema
- ✅ Validación de cierre de día

## 📝 Casos de Prueba Cubiertos

### 🔒 **Seguridad y Autenticación**

```csharp
✅ Login con credenciales válidas
✅ Tokens JWT válidos y expiración
✅ Refresh tokens funcionando correctamente
✅ Cambio de contraseñas con validación
✅ Autorización por roles (Admin, Mesero, Cajero, Recepción)
✅ Acceso restringido a endpoints protegidos
```

### 👥 **Gestión de Usuarios y Empleados**

```csharp
✅ Creación de usuarios con datos válidos
✅ Validación de cédula dominicana (XXX-XXXXXXX-X)
✅ Validación de teléfono dominicano (809/829/849-XXX-XXXX)
✅ Validación de email
✅ Asociación automática usuario-empleado
✅ Asignación de roles correcta
```

### 🪑 **Gestión de Mesas**

```csharp
✅ Consulta de estado de mesas
✅ Identificación de mesas disponibles
✅ Cambio de estados (Libre, Ocupada, Reservada, Mantenimiento)
✅ Capacidad y ubicación de mesas
```

### 📦 **Control de Inventario**

```csharp
✅ Consulta de stock actual
✅ Detección de productos con stock bajo
✅ Registro de movimientos de entrada
✅ Actualización automática de cantidades
✅ Trazabilidad de movimientos
```

### 👤 **Gestión de Clientes**

```csharp
✅ Registro de clientes con datos dominicanos
✅ Validación de cédula y teléfono
✅ Almacenamiento de preferencias alimentarias
✅ Historial de clientes
```

### 📅 **Sistema de Reservas**

```csharp
✅ Creación de reservas con validación de horarios
✅ Verificación de disponibilidad de mesas
✅ Confirmación de reservas
✅ Consulta de reservas por fecha
✅ Restaurante 24 horas - sin restricciones de horario
```

### 🍽️ **Gestión de Órdenes**

```csharp
✅ Creación de órdenes tipo Mesa, Llevar, Delivery
✅ Adición de múltiples items por orden
✅ Notas especiales por item
✅ Actualización de estados de órdenes
✅ Cálculo automático de totales
✅ Integración con inventario
```

### 💰 **Facturación Dominicana**

```csharp
✅ Cálculo automático de ITBIS (18%)
✅ Aplicación de descuentos
✅ Registro de propinas
✅ Múltiples métodos de pago
✅ Generación de números de factura únicos
✅ Liberación automática de mesas al facturar
```

### 📊 **Reportes y Analytics**

```csharp
✅ Dashboard con métricas en tiempo real
✅ Reportes de ventas diarias
✅ Análisis de productos más vendidos
✅ Estado de inventario
✅ Métricas de ocupación de mesas
```

## 🛠️ Datos de Prueba

### 🔑 **Credenciales de Prueba**

```json
{
  "admin": {
    "username": "thecuevas0123_",
    "password": "thepikachu0123_"
  },
  "usuarios_creados": {
    "mesero": "mesero_test / MeseroNueva123!",
    "cajero": "cajero_test / CajeroTest123!",
    "recepcion": "recepcion_test / RecepcionTest123!"
  }
}
```

### 🍽️ **Productos de Prueba Utilizados**

```csharp
ProductoId 1:  Pollo Guisado (RD$ 350.00)
ProductoId 7:  Arroz Blanco (RD$ 80.00)
ProductoId 8:  Habichuelas Rojas (RD$ 100.00)
ProductoId 19: Tostones (RD$ 110.00)
ProductoId 25: Morir Soñando (RD$ 120.00)
ProductoId 34: Sancocho (RD$ 320.00)
```

### 💰 **Cálculos de Facturación**

```csharp
Ejemplo de Factura:
- Subtotal: RD$ 1,000.00
- Descuento: RD$ 50.00
- Base Imponible: RD$ 950.00
- ITBIS (18%): RD$ 171.00
- Propina: RD$ 100.00
- TOTAL: RD$ 1,221.00
```

## 📈 Métricas de Cobertura Esperadas

```
✅ Cobertura de Controladores: 95%+
✅ Cobertura de Servicios: 90%+
✅ Cobertura de Repositorios: 85%+
✅ Cobertura de Modelos: 80%+
✅ Cobertura General: 85%+
```

## 🔧 Configuración de Pruebas

### Base de Datos

- **Conexión**: LocalDB para pruebas
- **Aislamiento**: Cada prueba usa una base limpia
- **Datos**: Carga inicial desde `elcriollo.sql`

### Configuración de Email

- **Modo**: Archivo (no se envían emails reales)
- **Directorio**: `TestEmails/`
- **Validación**: Se verifica que se generen los archivos

### Configuración JWT

- **Secret**: Clave específica para pruebas
- **Expiración**: 60 minutos
- **Refresh**: 7 días

## 🚨 Solución de Problemas

### Error: "Base de datos no encontrada"

```bash
# Crear base de datos de pruebas
sqlcmd -S localhost -Q "CREATE DATABASE ElCriolloTest"
```

### Error: "Token inválido"

```bash
# Verificar configuración JWT en appsettings.Test.json
# Asegurar que la clave secreta tenga al menos 32 caracteres
```

### Error: "Productos no encontrados"

```bash
# Ejecutar script de inicialización
sqlcmd -S localhost -d ElCriolloTest -i ../elcriollo.sql
```

### Error: "Puerto en uso"

```bash
# Cambiar puerto en launchSettings.json o usar puerto aleatorio
dotnet test --no-build
```

## 📋 Checklist de Validaciones

Antes de ejecutar las pruebas, verificar:

- [ ] SQL Server está ejecutándose
- [ ] Base de datos ElCriolloTest existe
- [ ] Script elcriollo.sql ejecutado correctamente
- [ ] Paquetes NuGet restaurados
- [ ] Puertos 7001/7002 disponibles
- [ ] Permisos de escritura en directorio TestEmails

## 🎯 Casos de Uso Validados

### ✅ **Flujo Completo de Restaurante**

1. **Llegada de Cliente**: Reserva → Confirmación → Asignación de Mesa
2. **Toma de Orden**: Mesero → Productos → Cocina → Preparación
3. **Entrega**: Cocina → Mesero → Cliente → Satisfacción
4. **Facturación**: Cajero → ITBIS → Pago → Liberación de Mesa
5. **Análisis**: Reportes → Métricas → Decisiones de Negocio

### ✅ **Casos Edge Cubiertos**

- Órdenes sin mesa (Llevar/Delivery)
- Clientes ocasionales vs registrados
- Productos agotados
- Reservas con más de 30 días de anticipación
- Cambios de estado inválidos
- Facturación de órdenes ya facturadas

## 🔍 Interpretación de Resultados

### ✅ **Prueba Exitosa**

```
✅ SIMULACIÓN COMPLETADA EXITOSAMENTE!
=================================================================
Test Run Successful.
Total tests: 1
     Passed: 1
     Failed: 0
     Skipped: 0
```

### ❌ **Prueba Fallida**

```
❌ Error en TEST X: [Descripción del error]
   - Verificar estado del sistema
   - Revisar logs de aplicación
   - Validar datos de prueba
```

## 📞 Soporte

Para problemas con las pruebas:

1. **Revisar logs** en directorio `logs/`
2. **Verificar configuración** en `appsettings.Test.json`
3. **Ejecutar pruebas individuales** para aislar problemas
4. **Consultar README principal** del proyecto

---

_Este suite de pruebas garantiza que el sistema El Criollo funciona correctamente en todos los escenarios de uso cotidiano antes de implementar el frontend._
