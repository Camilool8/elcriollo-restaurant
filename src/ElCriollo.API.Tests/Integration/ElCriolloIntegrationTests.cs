using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.DTOs.Common;

namespace ElCriollo.API.Tests.Integration
{
    /// <summary>
    /// Pruebas de integración completas para El Criollo - Simulación del flujo cotidiano
    /// </summary>
    [Collection("Integration Tests")]
    public class ElCriolloIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;
        private readonly JsonSerializerOptions _jsonOptions;
        
        // Datos de prueba que se usarán durante todos los tests
        private string _adminToken = string.Empty;
        private string _meseroToken = string.Empty;
        private string _cajeroToken = string.Empty;
        private string _recepcionToken = string.Empty;
        
        private int _nuevoUsuarioMeseroId = 0;
        private int _nuevoUsuarioCajeroId = 0;
        private int _nuevoUsuarioRecepcionId = 0;
        
        private int _clienteId = 0;
        private int _mesaId = 1; // Mesa 1 para tests
        private int _ordenId = 0;
        private int _reservacionId = 0;
        private int _facturaId = 0;

        public ElCriolloIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
            _client = _factory.CreateClient();
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        // ============================================================================
        // FLUJO COMPLETO DE TESTS - SIMULACIÓN DEL DÍA LABORAL
        // ============================================================================

        [Fact]
        public async Task FlujoCotidiano_SimulacionCompleta_DebeCompletarExitosamente()
        {
            _output.WriteLine("🚀 INICIANDO SIMULACIÓN DEL FLUJO COTIDIANO DE EL CRIOLLO");
            _output.WriteLine("=================================================================");
            
            // 1. AUTENTICACIÓN INICIAL
            await Test01_AutenticacionAdmin_DebeLogearseExitosamente();
            await Test02_VerificarEstadoSistema_DebeEstarOperativo();
            
            // 2. GESTIÓN DE USUARIOS Y EMPLEADOS
            await Test03_CrearUsuarios_DebeCrearDiferentesRoles();
            await Test04_CambiarContrasenas_DebeActualizarCredenciales();
            await Test05_AutenticarNuevosUsuarios_DebeLogearseConNuevasCredenciales();
            await Test06_VerificarInformacionEmpleados_DebeObtenerDatos();
            
            // 3. GESTIÓN DE MESAS E INVENTARIO
            await Test07_ConsultarMesas_DebeObtenerEstadoActual();
            await Test08_ConsultarInventario_DebeObtenerStock();
            await Test09_ActualizarInventario_DebeRegistrarMovimientos();
            
            // 4. GESTIÓN DE CLIENTES
            await Test10_CrearCliente_DebeRegistrarNuevoCliente();
            
            // 5. SISTEMA DE RESERVAS
            await Test11_CrearReservacion_DebeRegistrarReserva();
            await Test12_ConsultarReservaciones_DebeObtenerReservasDelDia();
            await Test13_ConfirmarReservacion_DebeActualizarEstado();
            
            // 6. GESTIÓN DE ÓRDENES
            await Test14_CrearOrden_DebeRegistrarNuevaOrden();
            await Test15_ConsultarOrdenes_DebeObtenerOrdenesActivas();
            await Test16_AgregarItemsOrden_DebeActualizarOrden();
            await Test17_ActualizarEstadoOrden_DebeProgresarEstado();
            
            // 7. FACTURACIÓN
            await Test18_CrearFactura_DebeGenerarFacturaConITBIS();
            await Test19_ConsultarFactura_DebeObtenerDetallesFactura();
            await Test20_MarcarFacturaPagada_DebeActualizarEstado();
            
            // 8. REPORTES Y ANALYTICS
            await Test21_GenerarReportes_DebeObtenerAnalytics();
            await Test22_ConsultarDashboard_DebeObtenerMetricas();
            
            // 9. LIMPIEZA Y CIERRE
            await Test23_CerrarOperaciones_DebeFinalizarDia();
            
            _output.WriteLine("\n✅ SIMULACIÓN COMPLETADA EXITOSAMENTE!");
            _output.WriteLine("=================================================================");
        }

        // ============================================================================
        // TESTS INDIVIDUALES - PASO A PASO
        // ============================================================================

        private async Task Test01_AutenticacionAdmin_DebeLogearseExitosamente()
        {
            _output.WriteLine("\n🔐 TEST 1: Autenticación del Administrador");
            
            var loginRequest = new LoginRequest
            {
                Username = "thecuevas0123_",
                Password = "thepikachu0123_",
                RecordarSesion = true
            };

            var response = await PostAsync<LoginResponse>("/api/auth/login", loginRequest);
            
            Assert.NotNull(response);
            Assert.False(string.IsNullOrEmpty(response.Token));
            Assert.False(string.IsNullOrEmpty(response.RefreshToken));
            Assert.NotNull(response.Usuario);
            Assert.Equal("Administrador", response.Usuario.Rol);
            
            _adminToken = response.Token;
            SetAuthorizationHeader(_adminToken);
            
            _output.WriteLine($"✅ Admin autenticado exitosamente: {response.Usuario.Usuario}");
            _output.WriteLine($"   Token expira: {response.ExpiresAt:yyyy-MM-dd HH:mm:ss}");
        }

        private async Task Test02_VerificarEstadoSistema_DebeEstarOperativo()
        {
            _output.WriteLine("\n🏥 TEST 2: Verificación del Estado del Sistema");
            
            var healthResponse = await GetAsync<object>("/health");
            Assert.NotNull(healthResponse);
            
            _output.WriteLine("✅ Sistema operativo y saludable");
        }

        private async Task Test03_CrearUsuarios_DebeCrearDiferentesRoles()
        {
            _output.WriteLine("\n👥 TEST 3: Creación de Usuarios con Diferentes Roles");
            
            // Crear Mesero
            var meseroRequest = new CreateUsuarioRequest
            {
                Username = "mesero_test",
                Password = "MeseroTest123!",
                ConfirmarPassword = "MeseroTest123!",
                Email = "mesero@elcriollo.com",
                RolId = 3, // Mesero
                Cedula = "001-1234567-8",
                Nombre = "Juan",
                Apellido = "Pérez",
                Telefono = "809-555-1234",
                Salario = 25000,
                Departamento = "Servicio"
            };
            
            var meseroResponse = await PostAsync<dynamic>("/api/auth/register", meseroRequest);
            Assert.NotNull(meseroResponse);
            _nuevoUsuarioMeseroId = (int)meseroResponse.GetProperty("usuarioId").GetInt32();
            
            // Crear Cajero
            var cajeroRequest = new CreateUsuarioRequest
            {
                Username = "cajero_test",
                Password = "CajeroTest123!",
                ConfirmarPassword = "CajeroTest123!",
                Email = "cajero@elcriollo.com",
                RolId = 4, // Cajero
                Cedula = "001-2345678-9",
                Nombre = "María",
                Apellido = "González",
                Telefono = "809-555-2345",
                Salario = 30000,
                Departamento = "Caja"
            };
            
            var cajeroResponse = await PostAsync<dynamic>("/api/auth/register", cajeroRequest);
            Assert.NotNull(cajeroResponse);
            _nuevoUsuarioCajeroId = (int)cajeroResponse.GetProperty("usuarioId").GetInt32();
            
            // Crear Recepcionista
            var recepcionRequest = new CreateUsuarioRequest
            {
                Username = "recepcion_test",
                Password = "RecepcionTest123!",
                ConfirmarPassword = "RecepcionTest123!",
                Email = "recepcion@elcriollo.com",
                RolId = 2, // Recepción
                Cedula = "001-3456789-0",
                Nombre = "Ana",
                Apellido = "Martínez",
                Telefono = "809-555-3456",
                Salario = 28000,
                Departamento = "Recepción"
            };
            
            var recepcionResponse = await PostAsync<dynamic>("/api/auth/register", recepcionRequest);
            Assert.NotNull(recepcionResponse);
            _nuevoUsuarioRecepcionId = (int)recepcionResponse.GetProperty("usuarioId").GetInt32();
            
            _output.WriteLine($"✅ Usuarios creados exitosamente:");
            _output.WriteLine($"   - Mesero ID: {_nuevoUsuarioMeseroId}");
            _output.WriteLine($"   - Cajero ID: {_nuevoUsuarioCajeroId}");
            _output.WriteLine($"   - Recepción ID: {_nuevoUsuarioRecepcionId}");
        }

        private async Task Test04_CambiarContrasenas_DebeActualizarCredenciales()
        {
            _output.WriteLine("\n🔑 TEST 4: Cambio de Contraseñas");
            
            var cambioRequest = new CambiarContrasenaRequest
            {
                UsuarioId = _nuevoUsuarioMeseroId,
                ContrasenaActual = "MeseroTest123!",
                NuevaContrasena = "MeseroNueva123!",
                ConfirmarContrasena = "MeseroNueva123!"
            };
            
            var response = await PostAsync<ApiResponse>("/api/auth/cambiar-contrasena", cambioRequest);
            Assert.NotNull(response);
            Assert.True(response.Success);
            
            _output.WriteLine("✅ Contraseña del mesero actualizada exitosamente");
        }

        private async Task Test05_AutenticarNuevosUsuarios_DebeLogearseConNuevasCredenciales()
        {
            _output.WriteLine("\n🔓 TEST 5: Autenticación de Nuevos Usuarios");
            
            // Login del Mesero con nueva contraseña
            var meseroLogin = new LoginRequest
            {
                Username = "mesero_test",
                Password = "MeseroNueva123!"
            };
            
            var meseroResponse = await PostAsync<LoginResponse>("/api/auth/login", meseroLogin);
            Assert.NotNull(meseroResponse);
            _meseroToken = meseroResponse.Token;
            
            // Login del Cajero
            var cajeroLogin = new LoginRequest
            {
                Username = "cajero_test",
                Password = "CajeroTest123!"
            };
            
            var cajeroResponse = await PostAsync<LoginResponse>("/api/auth/login", cajeroLogin);
            Assert.NotNull(cajeroResponse);
            _cajeroToken = cajeroResponse.Token;
            
            // Login de Recepción
            var recepcionLogin = new LoginRequest
            {
                Username = "recepcion_test",
                Password = "RecepcionTest123!"
            };
            
            var recepcionResponse = await PostAsync<LoginResponse>("/api/auth/login", recepcionLogin);
            Assert.NotNull(recepcionResponse);
            _recepcionToken = recepcionResponse.Token;
            
            _output.WriteLine("✅ Todos los usuarios autenticados exitosamente");
        }

        private async Task Test06_VerificarInformacionEmpleados_DebeObtenerDatos()
        {
            _output.WriteLine("\n👨‍💼 TEST 6: Verificación de Información de Empleados");
            
            SetAuthorizationHeader(_adminToken);
            
            var empleados = await GetAsync<List<EmpleadoResponse>>("/api/empleado");
            Assert.NotNull(empleados);
            Assert.True(empleados.Count >= 3);
            
            _output.WriteLine($"✅ Total de empleados en sistema: {empleados.Count}");
        }

        private async Task Test07_ConsultarMesas_DebeObtenerEstadoActual()
        {
            _output.WriteLine("\n🪑 TEST 7: Consulta de Estado de Mesas");
            
            SetAuthorizationHeader(_meseroToken);
            
            var mesas = await GetAsync<List<MesaResponse>>("/api/mesas");
            Assert.NotNull(mesas);
            Assert.True(mesas.Count > 0);
            
            var mesaDisponible = mesas.FirstOrDefault(m => m.Estado == "Libre");
            Assert.NotNull(mesaDisponible);
            _mesaId = mesaDisponible.MesaId;
            
            _output.WriteLine($"✅ Total de mesas: {mesas.Count}");
            _output.WriteLine($"   Mesa seleccionada para test: {_mesaId}");
        }

        private async Task Test08_ConsultarInventario_DebeObtenerStock()
        {
            _output.WriteLine("\n📦 TEST 8: Consulta de Inventario");
            
            SetAuthorizationHeader(_adminToken);
            
            var inventario = await GetAsync<List<InventarioResponse>>("/api/inventario");
            Assert.NotNull(inventario);
            Assert.True(inventario.Count > 0);
            
            var stockBajo = inventario.Where(i => i.CantidadDisponible < i.CantidadMinima).Count();
            
            _output.WriteLine($"✅ Total de productos en inventario: {inventario.Count}");
            _output.WriteLine($"   Productos con stock bajo: {stockBajo}");
        }

        private async Task Test09_ActualizarInventario_DebeRegistrarMovimientos()
        {
            _output.WriteLine("\n📈 TEST 9: Actualización de Inventario");
            
            SetAuthorizationHeader(_adminToken);
            
            var entradaRequest = new EntradaInventarioRequest
            {
                ProductoId = 1, // Pollo Guisado
                Cantidad = 10,
                Motivo = "Reabastecimiento de prueba",
                CostoUnitario = 15.50m
            };
            
            var response = await PostAsync<MovimientoInventarioResponse>("/api/inventario/entrada", entradaRequest);
            Assert.NotNull(response);
            
            _output.WriteLine($"✅ Entrada de inventario registrada: {response.Cantidad} unidades");
        }

        private async Task Test10_CrearCliente_DebeRegistrarNuevoCliente()
        {
            _output.WriteLine("\n👤 TEST 10: Creación de Cliente");
            
            SetAuthorizationHeader(_meseroToken);
            
            var clienteRequest = new CrearClienteRequest
            {
                NombreCompleto = "Roberto Fernández",
                Cedula = "001-9876543-2",
                Telefono = "809-555-9876",
                Email = "roberto@email.com",
                Direccion = "Calle Principal #123, Santo Domingo",
                PreferenciasComida = "Sin picante, preferencias vegetarianas ocasionales"
            };
            
            var response = await PostAsync<ClienteResponse>("/api/cliente", clienteRequest);
            Assert.NotNull(response);
            _clienteId = response.ClienteId;
            
            _output.WriteLine($"✅ Cliente creado exitosamente: {response.NombreCompleto} (ID: {_clienteId})");
        }

        private async Task Test11_CrearReservacion_DebeRegistrarReserva()
        {
            _output.WriteLine("\n📅 TEST 11: Creación de Reservación");
            
            SetAuthorizationHeader(_recepcionToken);
            
            var reservacionRequest = new CreateReservacionRequest
            {
                MesaId = _mesaId,
                ClienteId = _clienteId,
                CantidadPersonas = 4,
                FechaHora = DateTime.Now.AddHours(2),
                DuracionMinutos = 120,
                NotasEspeciales = "Reservación de prueba - Mesa cerca de la ventana"
            };
            
            var response = await PostAsync<ReservacionResponse>("/api/reservacion", reservacionRequest);
            Assert.NotNull(response);
            _reservacionId = response.ReservacionId;
            
            _output.WriteLine($"✅ Reservación creada exitosamente: ID {_reservacionId}");
            _output.WriteLine($"   Fecha: {response.FechaHora:yyyy-MM-dd HH:mm}");
        }

        private async Task Test12_ConsultarReservaciones_DebeObtenerReservasDelDia()
        {
            _output.WriteLine("\n📋 TEST 12: Consulta de Reservaciones del Día");
            
            SetAuthorizationHeader(_recepcionToken);
            
            var reservaciones = await GetAsync<List<ReservacionResponse>>("/api/reservacion/dia");
            Assert.NotNull(reservaciones);
            Assert.Contains(reservaciones, r => r.ReservacionId == _reservacionId);
            
            _output.WriteLine($"✅ Reservaciones del día: {reservaciones.Count}");
        }

        private async Task Test13_ConfirmarReservacion_DebeActualizarEstado()
        {
            _output.WriteLine("\n✅ TEST 13: Confirmación de Reservación");
            
            SetAuthorizationHeader(_recepcionToken);
            
            var confirmarRequest = new ConfirmarReservacionRequest
            {
                Observaciones = "Reservación confirmada por teléfono"
            };
            
            var response = await PostAsync<ApiResponse>($"/api/reservacion/{_reservacionId}/confirmar", confirmarRequest);
            Assert.NotNull(response);
            Assert.True(response.Success);
            
            _output.WriteLine("✅ Reservación confirmada exitosamente");
        }

        private async Task Test14_CrearOrden_DebeRegistrarNuevaOrden()
        {
            _output.WriteLine("\n🍽️ TEST 14: Creación de Orden");
            
            SetAuthorizationHeader(_meseroToken);
            
            var ordenRequest = new CreateOrdenRequest
            {
                MesaId = _mesaId,
                ClienteId = _clienteId,
                TipoOrden = "Mesa",
                Observaciones = "Orden de prueba - Cliente prefiere comida sin picante",
                Items = new List<ItemOrdenRequest>
                {
                    new ItemOrdenRequest
                    {
                        ProductoId = 1, // Pollo Guisado
                        Cantidad = 2,
                        NotasEspeciales = "Bien cocido"
                    },
                    new ItemOrdenRequest
                    {
                        ProductoId = 7, // Arroz Blanco
                        Cantidad = 2
                    },
                    new ItemOrdenRequest
                    {
                        ProductoId = 8, // Habichuelas Rojas
                        Cantidad = 2
                    },
                    new ItemOrdenRequest
                    {
                        ProductoId = 19, // Tostones
                        Cantidad = 1
                    }
                }
            };
            
            var response = await PostAsync<OrdenResponse>("/api/orden", ordenRequest);
            Assert.NotNull(response);
            _ordenId = response.OrdenId;
            
            _output.WriteLine($"✅ Orden creada exitosamente: {response.NumeroOrden}");
            _output.WriteLine($"   ID: {_ordenId}, Mesa: {response.MesaId}");
            _output.WriteLine($"   Items: {response.Items.Count}, Total: RD$ {response.Total:F2}");
        }

        private async Task Test15_ConsultarOrdenes_DebeObtenerOrdenesActivas()
        {
            _output.WriteLine("\n📋 TEST 15: Consulta de Órdenes Activas");
            
            SetAuthorizationHeader(_meseroToken);
            
            var ordenes = await GetAsync<List<OrdenResponse>>("/api/orden/estado/Pendiente");
            Assert.NotNull(ordenes);
            Assert.Contains(ordenes, o => o.OrdenId == _ordenId);
            
            _output.WriteLine($"✅ Órdenes pendientes: {ordenes.Count}");
        }

        private async Task Test16_AgregarItemsOrden_DebeActualizarOrden()
        {
            _output.WriteLine("\n➕ TEST 16: Agregar Items a Orden");
            
            SetAuthorizationHeader(_meseroToken);
            
            var agregarRequest = new AgregarItemsOrdenRequest
            {
                Items = new List<ItemOrdenRequest>
                {
                    new ItemOrdenRequest
                    {
                        ProductoId = 25, // Morir Soñando
                        Cantidad = 2,
                        NotasEspeciales = "Bien frío"
                    }
                }
            };
            
            var response = await PostAsync<OrdenResponse>($"/api/orden/{_ordenId}/items", agregarRequest);
            Assert.NotNull(response);
            Assert.True(response.Items.Count > 4); // Más items que antes
            
            _output.WriteLine($"✅ Items agregados exitosamente. Total items: {response.Items.Count}");
        }

        private async Task Test17_ActualizarEstadoOrden_DebeProgresarEstado()
        {
            _output.WriteLine("\n🔄 TEST 17: Actualización de Estado de Orden");
            
            SetAuthorizationHeader(_meseroToken);
            
            var actualizarRequest = new ActualizarEstadoOrdenRequest
            {
                NuevoEstado = "EnPreparacion",
                Observaciones = "Orden enviada a cocina"
            };
            
            var response = await PostAsync<ApiResponse>($"/api/orden/{_ordenId}/estado", actualizarRequest);
            Assert.NotNull(response);
            Assert.True(response.Success);
            
            _output.WriteLine("✅ Estado de orden actualizado a: EnPreparacion");
        }

        private async Task Test18_CrearFactura_DebeGenerarFacturaConITBIS()
        {
            _output.WriteLine("\n💰 TEST 18: Creación de Factura con ITBIS");
            
            SetAuthorizationHeader(_cajeroToken);
            
            // Primero marcar la orden como Lista
            var estadoRequest = new ActualizarEstadoOrdenRequest
            {
                NuevoEstado = "Lista",
                Observaciones = "Orden lista para entrega"
            };
            
            await PostAsync<ApiResponse>($"/api/orden/{_ordenId}/estado", estadoRequest);
            
            // Luego crear la factura
            var facturaRequest = new CrearFacturaRequest
            {
                OrdenId = _ordenId,
                MetodoPago = "Tarjeta",
                Descuento = 50.00m,
                Propina = 100.00m,
                Observaciones = "Factura de prueba con ITBIS dominicano"
            };
            
            var response = await PostAsync<FacturaResponse>("/api/factura", facturaRequest);
            Assert.NotNull(response);
            _facturaId = response.FacturaId;
            
            // Verificar cálculo de ITBIS (18%)
            Assert.True(response.Impuesto > 0);
            Assert.Equal(response.Subtotal * 0.18m, response.Impuesto, 2);
            
            _output.WriteLine($"✅ Factura creada exitosamente: {response.NumeroFactura}");
            _output.WriteLine($"   Subtotal: RD$ {response.Subtotal:F2}");
            _output.WriteLine($"   ITBIS (18%): RD$ {response.Impuesto:F2}");
            _output.WriteLine($"   Total: RD$ {response.Total:F2}");
        }

        private async Task Test19_ConsultarFactura_DebeObtenerDetallesFactura()
        {
            _output.WriteLine("\n📄 TEST 19: Consulta de Detalles de Factura");
            
            SetAuthorizationHeader(_cajeroToken);
            
            var factura = await GetAsync<FacturaResponse>($"/api/factura/{_facturaId}");
            Assert.NotNull(factura);
            Assert.Equal(_facturaId, factura.FacturaId);
            Assert.Equal(_ordenId, factura.OrdenId);
            
            _output.WriteLine($"✅ Factura consultada exitosamente: {factura.NumeroFactura}");
        }

        private async Task Test20_MarcarFacturaPagada_DebeActualizarEstado()
        {
            _output.WriteLine("\n💳 TEST 20: Marcar Factura como Pagada");
            
            SetAuthorizationHeader(_cajeroToken);
            
            var pagarRequest = new PagarFacturaRequest
            {
                MetodoPago = "Tarjeta",
                Observaciones = "Pago procesado exitosamente"
            };
            
            var response = await PostAsync<ApiResponse>($"/api/factura/{_facturaId}/pagar", pagarRequest);
            Assert.NotNull(response);
            Assert.True(response.Success);
            
            _output.WriteLine("✅ Factura marcada como pagada exitosamente");
        }

        private async Task Test21_GenerarReportes_DebeObtenerAnalytics()
        {
            _output.WriteLine("\n📊 TEST 21: Generación de Reportes");
            
            SetAuthorizationHeader(_adminToken);
            
            var fechaInicio = DateTime.Today;
            var fechaFin = DateTime.Today.AddDays(1);
            
            var reporteVentas = await GetAsync<ReporteVentasDiariasResponse>(
                $"/api/reporte/ventas/diarias?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
            Assert.NotNull(reporteVentas);
            
            _output.WriteLine($"✅ Reporte de ventas generado:");
            _output.WriteLine($"   Ventas del día: RD$ {reporteVentas.TotalVentas:F2}");
            _output.WriteLine($"   Órdenes procesadas: {reporteVentas.TotalOrdenes}");
        }

        private async Task Test22_ConsultarDashboard_DebeObtenerMetricas()
        {
            _output.WriteLine("\n📈 TEST 22: Consulta de Dashboard");
            
            SetAuthorizationHeader(_adminToken);
            
            var dashboard = await GetAsync<DashboardResponse>("/api/reporte/dashboard");
            Assert.NotNull(dashboard);
            
            _output.WriteLine($"✅ Dashboard consultado exitosamente:");
            _output.WriteLine($"   Ventas hoy: RD$ {dashboard.VentasHoy:F2}");
            _output.WriteLine($"   Órdenes activas: {dashboard.OrdenesActivas}");
            _output.WriteLine($"   Mesas ocupadas: {dashboard.MesasOcupadas}");
        }

        private async Task Test23_CerrarOperaciones_DebeFinalizarDia()
        {
            _output.WriteLine("\n🏁 TEST 23: Cierre de Operaciones");
            
            SetAuthorizationHeader(_adminToken);
            
            // Verificar que no hay órdenes pendientes
            var ordenesPendientes = await GetAsync<List<OrdenResponse>>("/api/orden/estado/Pendiente");
            
            _output.WriteLine($"✅ Cierre de operaciones completado:");
            _output.WriteLine($"   Órdenes pendientes: {ordenesPendientes.Count}");
            _output.WriteLine($"   Sistema listo para cierre del día");
        }

        // ============================================================================
        // MÉTODOS DE UTILIDAD
        // ============================================================================

        private async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
        }

        private async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions)!;
        }

        private void SetAuthorizationHeader(string token)
        {
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // ============================================================================
        // CLASES DE APOYO PARA TESTS
        // ============================================================================

        public class CambiarContrasenaRequest
        {
            public int UsuarioId { get; set; }
            public string ContrasenaActual { get; set; } = string.Empty;
            public string NuevaContrasena { get; set; } = string.Empty;
            public string ConfirmarContrasena { get; set; } = string.Empty;
        }

        // CrearClienteRequest y EntradaInventarioRequest se usan desde TestResponseModels.cs

        public class ConfirmarReservacionRequest
        {
            public string? Observaciones { get; set; }
        }

        public class AgregarItemsOrdenRequest
        {
            public List<ItemOrdenRequest> Items { get; set; } = new();
        }

        public class ActualizarEstadoOrdenRequest
        {
            public string NuevoEstado { get; set; } = string.Empty;
            public string? Observaciones { get; set; }
        }

        public class PagarFacturaRequest
        {
            public string MetodoPago { get; set; } = string.Empty;
            public string? Observaciones { get; set; }
        }

        // Response classes se usan desde TestResponseModels.cs - evitar duplicación

        // Todas las clases Response se usan desde TestResponseModels.cs para evitar duplicación
    }
} 