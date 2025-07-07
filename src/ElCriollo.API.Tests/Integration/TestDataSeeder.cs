using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;

namespace ElCriollo.API.Tests.Integration
{
    /// <summary>
    /// Clase para generar datos de prueba para los tests de integración
    /// </summary>
    public static class TestDataSeeder
    {
        // ============================================================================
        // DATOS DE USUARIOS DE PRUEBA
        // ============================================================================

        public static CreateUsuarioRequest CrearUsuarioMesero(string suffix = "")
        {
            return new CreateUsuarioRequest
            {
                Username = $"mesero_test{suffix}",
                Password = "MeseroTest123!",
                ConfirmarPassword = "MeseroTest123!",
                Email = $"mesero{suffix}@elcriollo.com",
                RolId = 3, // Mesero
                Cedula = GenerarCedula(),
                Nombre = "Juan Carlos",
                Apellido = "Pérez",
                Telefono = GenerarTelefono(),
                Salario = 25000,
                Departamento = "Servicio"
            };
        }

        public static CreateUsuarioRequest CrearUsuarioCajero(string suffix = "")
        {
            return new CreateUsuarioRequest
            {
                Username = $"cajero_test{suffix}",
                Password = "CajeroTest123!",
                ConfirmarPassword = "CajeroTest123!",
                Email = $"cajero{suffix}@elcriollo.com",
                RolId = 4, // Cajero
                Cedula = GenerarCedula(),
                Nombre = "María Elena",
                Apellido = "González",
                Telefono = GenerarTelefono(),
                Salario = 30000,
                Departamento = "Caja"
            };
        }

        public static CreateUsuarioRequest CrearUsuarioRecepcion(string suffix = "")
        {
            return new CreateUsuarioRequest
            {
                Username = $"recepcion_test{suffix}",
                Password = "RecepcionTest123!",
                ConfirmarPassword = "RecepcionTest123!",
                Email = $"recepcion{suffix}@elcriollo.com",
                RolId = 2, // Recepción
                Cedula = GenerarCedula(),
                Nombre = "Ana Cristina",
                Apellido = "Martínez",
                Telefono = GenerarTelefono(),
                Salario = 28000,
                Departamento = "Recepción"
            };
        }

        // ============================================================================
        // DATOS DE CLIENTES DE PRUEBA
        // ============================================================================

        public static List<CrearClienteRequest> CrearClientesDePrueba(int cantidad = 5)
        {
            var clientes = new List<CrearClienteRequest>();
            
            string[] nombres = { "Roberto", "Carmen", "José", "Laura", "Miguel", "Sofía", "Carlos", "Elena" };
            string[] apellidos = { "Fernández", "Rodríguez", "García", "Martín", "López", "Herrera", "Vega", "Santos" };
            string[] preferencias = { 
                "Sin picante", 
                "Vegetariano ocasional", 
                "Alérgico a mariscos", 
                "Prefiere carne bien cocida",
                "Le gusta picante",
                "Dieta saludable",
                "Comida tradicional",
                "Ninguna preferencia especial"
            };

            for (int i = 0; i < cantidad; i++)
            {
                var nombre = nombres[i % nombres.Length];
                var apellido = apellidos[i % apellidos.Length];
                
                clientes.Add(new CrearClienteRequest
                {
                    NombreCompleto = $"{nombre} {apellido}",
                    Cedula = GenerarCedula(),
                    Telefono = GenerarTelefono(),
                    Email = $"{nombre.ToLower()}.{apellido.ToLower()}@email.com",
                    Direccion = $"Calle {Random.Shared.Next(1, 100)} #{Random.Shared.Next(1, 999)}, Santo Domingo",
                    PreferenciasComida = preferencias[i % preferencias.Length]
                });
            }

            return clientes;
        }

        // ============================================================================
        // DATOS DE ÓRDENES DE PRUEBA
        // ============================================================================

        public static CreateOrdenRequest CrearOrdenCompleta(int mesaId, int clienteId)
        {
            return new CreateOrdenRequest
            {
                MesaId = mesaId,
                ClienteId = clienteId,
                TipoOrden = "Mesa",
                Observaciones = "Orden de prueba automatizada",
                Items = new List<ItemOrdenRequest>
                {
                    new ItemOrdenRequest
                    {
                        ProductoId = 1, // Pollo Guisado
                        Cantidad = 2,
                        NotasEspeciales = "Bien condimentado"
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
                    },
                    new ItemOrdenRequest
                    {
                        ProductoId = 25, // Morir Soñando
                        Cantidad = 2,
                        NotasEspeciales = "Bien frío"
                    }
                }
            };
        }

        public static CreateOrdenRequest CrearOrdenParaLlevar(int? clienteId = null)
        {
            return new CreateOrdenRequest
            {
                ClienteId = clienteId,
                TipoOrden = "Llevar",
                Observaciones = "Orden para llevar - Prueba automatizada",
                Items = new List<ItemOrdenRequest>
                {
                    new ItemOrdenRequest
                    {
                        ProductoId = 34, // Sancocho
                        Cantidad = 1,
                        NotasEspeciales = "Porción grande"
                    },
                    new ItemOrdenRequest
                    {
                        ProductoId = 20, // Yuca Frita
                        Cantidad = 1
                    },
                    new ItemOrdenRequest
                    {
                        ProductoId = 27, // Jugo de Tamarindo
                        Cantidad = 1
                    }
                }
            };
        }

        public static CreateOrdenRequest CrearOrdenDelivery(int? clienteId = null)
        {
            return new CreateOrdenRequest
            {
                ClienteId = clienteId,
                TipoOrden = "Delivery",
                Observaciones = "Delivery - Dirección: Av. 27 de Febrero #123",
                Items = new List<ItemOrdenRequest>
                {
                    new ItemOrdenRequest
                    {
                        ProductoId = 5, // Costillas BBQ Criolla
                        Cantidad = 1,
                        NotasEspeciales = "Salsa aparte"
                    },
                    new ItemOrdenRequest
                    {
                        ProductoId = 9, // Moro de Guandules
                        Cantidad = 1
                    },
                    new ItemOrdenRequest
                    {
                        ProductoId = 21, // Maduros
                        Cantidad = 1
                    }
                }
            };
        }

        // ============================================================================
        // DATOS DE RESERVACIONES DE PRUEBA
        // ============================================================================

        public static CreateReservacionRequest CrearReservacionEstandar(int mesaId, int clienteId)
        {
            return new CreateReservacionRequest
            {
                MesaId = mesaId,
                ClienteId = clienteId,
                CantidadPersonas = 4,
                FechaHora = DateTime.Now.AddHours(Random.Shared.Next(2, 8)),
                DuracionMinutos = 120,
                NotasEspeciales = "Reservación de prueba automatizada - Mesa cerca de la ventana si es posible"
            };
        }

        public static CreateReservacionRequest CrearReservacionGrupal(int mesaId, int clienteId)
        {
            return new CreateReservacionRequest
            {
                MesaId = mesaId,
                ClienteId = clienteId,
                CantidadPersonas = 8,
                FechaHora = DateTime.Now.AddHours(Random.Shared.Next(3, 10)),
                DuracionMinutos = 180,
                NotasEspeciales = "Reservación grupal - Celebración especial - Requiere mesa amplia"
            };
        }

        // ============================================================================
        // DATOS DE INVENTARIO DE PRUEBA
        // ============================================================================

        public static List<EntradaInventarioRequest> CrearMovimientosInventario()
        {
            return new List<EntradaInventarioRequest>
            {
                new EntradaInventarioRequest
                {
                    ProductoId = 1, // Pollo Guisado
                    Cantidad = 15,
                    Motivo = "Reabastecimiento semanal",
                    CostoUnitario = 12.50m
                },
                new EntradaInventarioRequest
                {
                    ProductoId = 7, // Arroz Blanco
                    Cantidad = 50,
                    Motivo = "Compra a granel",
                    CostoUnitario = 2.25m
                },
                new EntradaInventarioRequest
                {
                    ProductoId = 19, // Tostones
                    Cantidad = 25,
                    Motivo = "Reposición inventario",
                    CostoUnitario = 1.80m
                },
                new EntradaInventarioRequest
                {
                    ProductoId = 25, // Morir Soñando
                    Cantidad = 30,
                    Motivo = "Ingredientes para bebidas",
                    CostoUnitario = 3.75m
                }
            };
        }

        // ============================================================================
        // UTILIDADES DE GENERACIÓN DE DATOS
        // ============================================================================

        public static string GenerarCedula()
        {
            var parte1 = Random.Shared.Next(1, 999).ToString("D3");
            var parte2 = Random.Shared.Next(1, 9999999).ToString("D7");
            var parte3 = Random.Shared.Next(1, 9).ToString("D1");
            return $"{parte1}-{parte2}-{parte3}";
        }

        public static string GenerarTelefono()
        {
            var area = new[] { "809", "829", "849" };
            var codigoArea = area[Random.Shared.Next(area.Length)];
            var numero = Random.Shared.Next(100, 999).ToString("D3");
            var extension = Random.Shared.Next(1000, 9999).ToString("D4");
            return $"{codigoArea}-{numero}-{extension}";
        }

        public static string GenerarEmail(string nombre, string apellido)
        {
            return $"{nombre.ToLower()}.{apellido.ToLower()}@email.com";
        }

        // ============================================================================
        // VALIDACIONES DE DATOS
        // ============================================================================

        public static bool ValidarCedulaDominicana(string cedula)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^\d{3}-\d{7}-\d{1}$");
            return regex.IsMatch(cedula);
        }

        public static bool ValidarTelefonoDominicano(string telefono)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^(809|829|849)-\d{3}-\d{4}$");
            return regex.IsMatch(telefono);
        }

        public static bool ValidarEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // ============================================================================
        // ESCENARIOS DE PRUEBA ESPECÍFICOS
        // ============================================================================

        /// <summary>
        /// Genera datos para simular un día completo de operaciones
        /// </summary>
        public static class EscenarioDiaCompleto
        {
            public static List<CreateOrdenRequest> GenerarOrdenesDelDia(List<int> mesaIds, List<int> clienteIds)
            {
                var ordenes = new List<CreateOrdenRequest>();
                
                // Órdenes de desayuno (8:00 AM - 11:00 AM)
                for (int i = 0; i < 5; i++)
                {
                    ordenes.Add(new CreateOrdenRequest
                    {
                        MesaId = mesaIds[i % mesaIds.Count],
                        ClienteId = clienteIds[i % clienteIds.Count],
                        TipoOrden = "Mesa",
                        Observaciones = "Orden de desayuno",
                        Items = new List<ItemOrdenRequest>
                        {
                            new ItemOrdenRequest { ProductoId = 37, Cantidad = 1 }, // Tres Golpes
                            new ItemOrdenRequest { ProductoId = 39, Cantidad = 1 }, // Avena
                            new ItemOrdenRequest { ProductoId = 27, Cantidad = 1 }  // Jugo de Tamarindo
                        }
                    });
                }

                // Órdenes de almuerzo (12:00 PM - 3:00 PM)
                for (int i = 0; i < 8; i++)
                {
                    ordenes.Add(CrearOrdenCompleta(mesaIds[i % mesaIds.Count], clienteIds[i % clienteIds.Count]));
                }

                // Órdenes de cena (6:00 PM - 10:00 PM)
                for (int i = 0; i < 6; i++)
                {
                    ordenes.Add(new CreateOrdenRequest
                    {
                        MesaId = mesaIds[i % mesaIds.Count],
                        ClienteId = clienteIds[i % clienteIds.Count],
                        TipoOrden = "Mesa",
                        Observaciones = "Orden de cena",
                        Items = new List<ItemOrdenRequest>
                        {
                            new ItemOrdenRequest { ProductoId = 3, Cantidad = 1 }, // Rabo Encendido
                            new ItemOrdenRequest { ProductoId = 7, Cantidad = 1 }, // Arroz Blanco
                            new ItemOrdenRequest { ProductoId = 8, Cantidad = 1 }, // Habichuelas Rojas
                            new ItemOrdenRequest { ProductoId = 28, Cantidad = 1 } // Cerveza Presidente
                        }
                    });
                }

                return ordenes;
            }

            public static List<CreateReservacionRequest> GenerarReservacionesDelDia(List<int> mesaIds, List<int> clienteIds)
            {
                var reservaciones = new List<CreateReservacionRequest>();
                
                // Reservaciones para la próxima semana
                for (int i = 0; i < 10; i++)
                {
                    reservaciones.Add(new CreateReservacionRequest
                    {
                        MesaId = mesaIds[i % mesaIds.Count],
                        ClienteId = clienteIds[i % clienteIds.Count],
                        CantidadPersonas = Random.Shared.Next(2, 8),
                        FechaHora = DateTime.Now.AddDays(Random.Shared.Next(1, 7)).AddHours(Random.Shared.Next(11, 22)),
                        DuracionMinutos = Random.Shared.Next(60, 180),
                        NotasEspeciales = $"Reservación #{i + 1} - Generada automáticamente"
                    });
                }

                return reservaciones;
            }
        }

        // ============================================================================
        // RESPUESTAS DE PRUEBA PARA VALIDACIONES
        // ============================================================================

        // CreateClienteRequest y EntradaInventarioRequest se usan desde TestResponseModels.cs
    }
} 