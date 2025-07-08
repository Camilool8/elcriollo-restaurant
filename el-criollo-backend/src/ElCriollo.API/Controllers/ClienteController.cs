using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.DTOs.Common;
using ElCriollo.API.Services;
using ElCriollo.API.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using AutoMapper;

namespace ElCriollo.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de clientes del restaurante El Criollo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [SwaggerTag("Gestión de clientes, fidelización y programas de lealtad")]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<ClienteController> _logger;

        public ClienteController(
            IClienteRepository clienteRepository,
            IEmailService emailService,
            IMapper mapper,
            ILogger<ClienteController> logger)
        {
            _clienteRepository = clienteRepository;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================================================
        // GESTIÓN DE CLIENTES
        // ============================================================================

        /// <summary>
        /// Crear un nuevo cliente
        /// </summary>
        /// <param name="request">Datos del nuevo cliente</param>
        /// <returns>Cliente creado</returns>
        /// <response code="201">Cliente creado exitosamente</response>
        /// <response code="400">Datos inválidos o cliente ya existe</response>
        [HttpPost]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Registrar nuevo cliente",
            Description = "Registra un nuevo cliente en el sistema. Se envía email de bienvenida automáticamente",
            OperationId = "Cliente.Crear",
            Tags = new[] { "Gestión de Clientes" }
        )]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClienteResponse>> CrearCliente([FromBody] CrearClienteRequest request)
        {
            try
            {
                _logger.LogInformation("👤 Registrando nuevo cliente: {Nombre}", request.NombreCompleto);

                // Validaciones básicas
                if (string.IsNullOrEmpty(request.NombreCompleto))
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Datos inválidos",
                        Detail = "El nombre del cliente es requerido"
                    });
                }

                // Verificar si ya existe un cliente con la misma cédula
                if (!string.IsNullOrEmpty(request.Cedula))
                {
                    var clienteExistente = await _clienteRepository.BuscarPorCedulaAsync(request.Cedula);
                    if (clienteExistente != null)
                    {
                        return BadRequest(new ValidationProblemDetails
                        {
                            Title = "Cliente ya existe",
                            Detail = $"Ya existe un cliente registrado con la cédula {request.Cedula}"
                        });
                    }
                }

                // Crear el cliente
                var cliente = new Models.Entities.Cliente
                {
                    Nombre = request.NombreCompleto.Split(' ')[0],
                    Apellido = request.NombreCompleto.Contains(' ') ? request.NombreCompleto.Split(' ', 2)[1] : string.Empty,
                    Cedula = request.Cedula,
                    Telefono = request.Telefono,
                    Email = request.Email,
                    Direccion = request.Direccion,
                    FechaNacimiento = request.FechaNacimiento,
                    PreferenciasComida = request.PreferenciasComida,
                    FechaRegistro = DateTime.Now.Date,
                    Estado = "Activo"
                };

                var clienteCreado = await _clienteRepository.CreateAsync(cliente);

                // Enviar email de bienvenida (opcional)
                try
                {
                    await _emailService.EnviarConfirmacionRegistroAsync(clienteCreado);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "⚠️ No se pudo enviar email de bienvenida");
                }

                var response = _mapper.Map<ClienteResponse>(clienteCreado);
                
                _logger.LogInformation("✅ Cliente {ClienteId} registrado exitosamente", clienteCreado.ClienteID);
                
                return CreatedAtAction(
                    nameof(GetClienteById), 
                    new { id = clienteCreado.ClienteID }, 
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear cliente");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al registrar el cliente",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener un cliente por ID
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <returns>Datos del cliente</returns>
        /// <response code="200">Cliente encontrado</response>
        /// <response code="404">Cliente no encontrado</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtener cliente por ID",
            Description = "Obtiene los datos completos de un cliente específico",
            OperationId = "Cliente.GetById",
            Tags = new[] { "Consulta de Clientes" }
        )]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteResponse>> GetClienteById(int id)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(id);
                
                if (cliente == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Cliente no encontrado",
                        Detail = $"No se encontró el cliente con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                var response = _mapper.Map<ClienteResponse>(cliente);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cliente {ClienteId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Buscar clientes
        /// </summary>
        /// <param name="filtro">Filtros de búsqueda</param>
        /// <returns>Lista de clientes que coinciden con el filtro</returns>
        /// <response code="200">Lista de clientes</response>
        [HttpGet("buscar")]
        [SwaggerOperation(
            Summary = "Buscar clientes",
            Description = "Busca clientes por nombre, email, teléfono o cédula",
            OperationId = "Cliente.Buscar",
            Tags = new[] { "Consulta de Clientes" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ClienteResponse>>> BuscarClientes([FromQuery] BuscarClienteRequest filtro)
        {
            try
            {
                _logger.LogInformation("🔍 Buscando clientes con filtro: {Filtro}", filtro.Termino);

                IEnumerable<Models.Entities.Cliente> clientes;

                if (!string.IsNullOrEmpty(filtro.Termino))
                {
                    // Usar el método optimizado del repositorio para búsqueda por nombre
                    clientes = await _clienteRepository.BuscarPorNombreAsync(filtro.Termino);
                }
                else
                {
                    clientes = await _clienteRepository.GetAllAsync();
                }

                var response = _mapper.Map<IEnumerable<ClienteResponse>>(clientes);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar clientes");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener todos los clientes
        /// </summary>
        /// <param name="incluirInactivos">Incluir clientes inactivos</param>
        /// <returns>Lista de todos los clientes</returns>
        /// <response code="200">Lista de clientes</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtener todos los clientes",
            Description = "Devuelve la lista completa de clientes del restaurante",
            OperationId = "Cliente.GetTodos",
            Tags = new[] { "Consulta de Clientes" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ClienteResponse>>> GetTodosLosClientes([FromQuery] bool incluirInactivos = false)
        {
            try
            {
                _logger.LogInformation("📋 Consultando todos los clientes");

                var clientes = incluirInactivos 
                    ? await _clienteRepository.GetAllAsync()
                    : await _clienteRepository.GetClientesActivosAsync();

                var response = _mapper.Map<IEnumerable<ClienteResponse>>(clientes);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener clientes");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Actualizar datos de un cliente
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <param name="request">Nuevos datos del cliente</param>
        /// <returns>Cliente actualizado</returns>
        /// <response code="200">Cliente actualizado exitosamente</response>
        /// <response code="404">Cliente no encontrado</response>
        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Actualizar cliente",
            Description = "Actualiza los datos de un cliente existente",
            OperationId = "Cliente.Actualizar",
            Tags = new[] { "Gestión de Clientes" }
        )]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteResponse>> ActualizarCliente(int id, [FromBody] ActualizarClienteRequest request)
        {
            try
            {
                _logger.LogInformation("📝 Actualizando cliente {ClienteId}", id);

                var cliente = await _clienteRepository.GetByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Cliente no encontrado",
                        Detail = $"No se encontró el cliente con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                // Actualizar propiedades
                // Actualizar Nombre y Apellido por separado
                if (!string.IsNullOrEmpty(request.NombreCompleto))
                {
                    var partes = request.NombreCompleto.Split(' ', 2);
                    cliente.Nombre = partes[0];
                    cliente.Apellido = partes.Length > 1 ? partes[1] : string.Empty;
                }
                cliente.Telefono = request.Telefono ?? cliente.Telefono;
                cliente.Direccion = request.Direccion ?? cliente.Direccion;
                cliente.Email = request.Email ?? cliente.Email;
                cliente.FechaNacimiento = request.FechaNacimiento ?? cliente.FechaNacimiento;
                cliente.PreferenciasComida = request.PreferenciasComida ?? cliente.PreferenciasComida;

                await _clienteRepository.UpdateAsync(cliente);

                var response = _mapper.Map<ClienteResponse>(cliente);
                
                _logger.LogInformation("✅ Cliente {ClienteId} actualizado exitosamente", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar cliente {ClienteId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Desactivar un cliente
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <returns>Confirmación de desactivación</returns>
        /// <response code="200">Cliente desactivado</response>
        /// <response code="404">Cliente no encontrado</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        [SwaggerOperation(
            Summary = "Desactivar cliente",
            Description = "Marca un cliente como inactivo (no se elimina físicamente)",
            OperationId = "Cliente.Desactivar",
            Tags = new[] { "Gestión de Clientes" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DesactivarCliente(int id)
        {
            try
            {
                _logger.LogInformation("🚫 Desactivando cliente {ClienteId}", id);

                var cliente = await _clienteRepository.GetByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Cliente no encontrado",
                        Detail = $"No se encontró el cliente con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                cliente.Estado = "Inactivo";
                await _clienteRepository.UpdateAsync(cliente);

                _logger.LogInformation("✅ Cliente {ClienteId} desactivado", id);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Cliente desactivado exitosamente",
                    Data = new { ClienteId = id, Estado = "Inactivo" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al desactivar cliente {ClienteId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // HISTORIAL Y ESTADÍSTICAS
        // ============================================================================

        /// <summary>
        /// Obtener historial de compras de un cliente
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <param name="fechaInicio">Fecha inicio del historial</param>
        /// <param name="fechaFin">Fecha fin del historial</param>
        /// <returns>Historial de compras del cliente</returns>
        /// <response code="200">Historial de compras</response>
        /// <response code="404">Cliente no encontrado</response>
        [HttpGet("{id:int}/historial-compras")]
        [SwaggerOperation(
            Summary = "Historial de compras del cliente",
            Description = "Obtiene el historial detallado de todas las compras realizadas por el cliente",
            OperationId = "Cliente.HistorialCompras",
            Tags = new[] { "Estadísticas de Cliente" }
        )]
        [ProducesResponseType(typeof(ClienteHistorialComprasResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteHistorialComprasResponse>> GetHistorialCompras(
            int id, 
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                _logger.LogInformation("📊 Consultando historial de compras del cliente {ClienteId}", id);

                var cliente = await _clienteRepository.GetByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Cliente no encontrado",
                        Detail = $"No se encontró el cliente con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                // Obtener historial de compras del cliente
                var historial = await _clienteRepository.GetHistorialComprasAsync(id, fechaInicio, fechaFin);
                
                // Construir respuesta con resumen
                var response = new ClienteHistorialComprasResponse
                {
                    ClienteId = cliente.ClienteID,
                    NombreCliente = cliente.NombreCompleto,
                    FechaInicio = fechaInicio ?? DateTime.Now.AddMonths(-6),
                    FechaFin = fechaFin ?? DateTime.Now,
                    TotalCompras = historial.Count(),
                    MontoTotal = historial.Sum(h => (decimal)h.GetType().GetProperty("Monto")!.GetValue(h)!),
                    TicketPromedio = historial.Any() ? historial.Average(h => (decimal)h.GetType().GetProperty("Monto")!.GetValue(h)!) : 0,
                    Compras = historial.Select(h => new CompraHistorialItem
                    {
                        Fecha = (DateTime)h.GetType().GetProperty("Fecha")!.GetValue(h)!,
                        NumeroFactura = (string)h.GetType().GetProperty("NumeroFactura")!.GetValue(h)!,
                        Monto = (decimal)h.GetType().GetProperty("Monto")!.GetValue(h)!,
                        MetodoPago = (string)h.GetType().GetProperty("MetodoPago")!.GetValue(h)!,
                        ProductosComprados = (List<string>)h.GetType().GetProperty("ProductosComprados")!.GetValue(h)!
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener historial de compras");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener estadísticas del cliente
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <returns>Estadísticas de consumo y preferencias</returns>
        /// <response code="200">Estadísticas del cliente</response>
        /// <response code="404">Cliente no encontrado</response>
        [HttpGet("{id:int}/estadisticas")]
        [SwaggerOperation(
            Summary = "Estadísticas del cliente",
            Description = "Obtiene estadísticas detalladas de consumo, preferencias y comportamiento del cliente",
            OperationId = "Cliente.Estadisticas",
            Tags = new[] { "Estadísticas de Cliente" }
        )]
        [ProducesResponseType(typeof(EstadisticasClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EstadisticasClienteResponse>> GetEstadisticasCliente(int id)
        {
            try
            {
                _logger.LogInformation("📊 Generando estadísticas del cliente {ClienteId}", id);

                var cliente = await _clienteRepository.GetByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Cliente no encontrado",
                        Detail = $"No se encontró el cliente con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                // Obtener estadísticas del cliente
                var estadisticasData = await _clienteRepository.GetEstadisticasClienteAsync(id);

                if (estadisticasData == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Estadísticas no disponibles",
                        Detail = "No se pudieron obtener las estadísticas del cliente",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                var estadisticas = new EstadisticasClienteResponse
                {
                    ClienteId = (int)estadisticasData.GetType().GetProperty("ClienteId")!.GetValue(estadisticasData)!,
                    NombreCliente = (string)estadisticasData.GetType().GetProperty("NombreCliente")!.GetValue(estadisticasData)!,
                    TotalVisitas = (int)estadisticasData.GetType().GetProperty("TotalVisitas")!.GetValue(estadisticasData)!,
                    TotalGastado = (decimal)estadisticasData.GetType().GetProperty("TotalGastado")!.GetValue(estadisticasData)!,
                    TicketPromedio = (decimal)estadisticasData.GetType().GetProperty("TicketPromedio")!.GetValue(estadisticasData)!,
                    ProductoFavorito = (string?)estadisticasData.GetType().GetProperty("ProductoFavorito")!.GetValue(estadisticasData),
                    CategoriaFavorita = (string?)estadisticasData.GetType().GetProperty("CategoriaFavorita")!.GetValue(estadisticasData),
                    DiaSemanaFrecuente = (string?)estadisticasData.GetType().GetProperty("DiaSemanaFrecuente")!.GetValue(estadisticasData),
                    HoraFrecuente = (string?)estadisticasData.GetType().GetProperty("HoraFrecuente")!.GetValue(estadisticasData),
                    DiasDesdeUltimaVisita = (int)estadisticasData.GetType().GetProperty("DiasDesdeUltimaVisita")!.GetValue(estadisticasData)!,
                    NivelFidelidad = (string)estadisticasData.GetType().GetProperty("NivelFidelidad")!.GetValue(estadisticasData)!
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar estadísticas del cliente");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // PROGRAMA DE FIDELIZACIÓN
        // ============================================================================

        /// <summary>
        /// Obtener clientes frecuentes
        /// </summary>
        /// <param name="minVisitas">Mínimo de visitas para considerar frecuente</param>
        /// <returns>Lista de clientes frecuentes</returns>
        /// <response code="200">Lista de clientes frecuentes</response>
        [HttpGet("frecuentes")]
        [SwaggerOperation(
            Summary = "Obtener clientes frecuentes",
            Description = "Obtiene la lista de clientes frecuentes basado en cantidad de visitas",
            OperationId = "Cliente.Frecuentes",
            Tags = new[] { "Programa de Fidelización" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ClienteFrecuenteResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ClienteFrecuenteResponse>>> GetClientesFrecuentes([FromQuery] int minVisitas = 5)
        {
            try
            {
                _logger.LogInformation("🌟 Consultando clientes frecuentes (mínimo {MinVisitas} visitas)", minVisitas);

                var clientesFrecuentes = await _clienteRepository.GetClientesFrecuentesAsync(minVisitas);
                return Ok(clientesFrecuentes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener clientes frecuentes");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener cumpleañeros del mes
        /// </summary>
        /// <param name="mes">Número del mes (1-12)</param>
        /// <returns>Lista de clientes que cumplen años en el mes</returns>
        /// <response code="200">Lista de cumpleañeros</response>
        [HttpGet("cumpleanos")]
        [SwaggerOperation(
            Summary = "Obtener cumpleañeros del mes",
            Description = "Obtiene la lista de clientes que cumplen años en el mes especificado",
            OperationId = "Cliente.Cumpleanos",
            Tags = new[] { "Programa de Fidelización" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ClienteCumpleanosResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ClienteCumpleanosResponse>>> GetCumpleaneros([FromQuery] int? mes = null)
        {
            try
            {
                var mesConsulta = mes ?? DateTime.Now.Month;
                _logger.LogInformation("🎂 Consultando cumpleañeros del mes {Mes}", mesConsulta);

                // Obtener clientes que cumplen años en el mes
                var cumpleanosData = await _clienteRepository.GetClientesCumpleanosAsync(mesConsulta);
                
                var cumpleaneros = cumpleanosData.Select(c => new ClienteCumpleanosResponse
                {
                    ClienteId = (int)c.GetType().GetProperty("ClienteId")!.GetValue(c)!,
                    NombreCompleto = (string)c.GetType().GetProperty("NombreCompleto")!.GetValue(c)!,
                    Email = (string)c.GetType().GetProperty("Email")!.GetValue(c)!,
                    Telefono = (string)c.GetType().GetProperty("Telefono")!.GetValue(c)!,
                    FechaNacimiento = (DateTime)c.GetType().GetProperty("FechaNacimiento")!.GetValue(c)!,
                    DiaCumpleanos = (int)c.GetType().GetProperty("DiaCumpleanos")!.GetValue(c)!,
                    Edad = (int)c.GetType().GetProperty("Edad")!.GetValue(c)!
                }).ToList();

                return Ok(cumpleaneros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cumpleañeros");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // REQUESTS ESPECÍFICOS (solo los que no existen en DTOs)
        // ============================================================================

        public class CrearClienteRequest
        {
            public string NombreCompleto { get; set; } = string.Empty;
            public string? Cedula { get; set; }
            public string Telefono { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Direccion { get; set; }
            public DateTime? FechaNacimiento { get; set; }
            public string? PreferenciasComida { get; set; }
        }

        public class ActualizarClienteRequest
        {
            public string? NombreCompleto { get; set; }
            public string? Telefono { get; set; }
            public string? Email { get; set; }
            public string? Direccion { get; set; }
            public DateTime? FechaNacimiento { get; set; }
            public string? PreferenciasComida { get; set; }
        }

        public class BuscarClienteRequest
        {
            public string? Termino { get; set; }
        }

        // ============================================================================
        // RESPONSES ESPECÍFICOS (solo los que no existen en DTOs)
        // ============================================================================

        public class EstadisticasClienteResponse
        {
            public int ClienteId { get; set; }
            public string NombreCliente { get; set; } = string.Empty;
            public int TotalVisitas { get; set; }
            public decimal TotalGastado { get; set; }
            public decimal TicketPromedio { get; set; }
            public string? ProductoFavorito { get; set; }
            public string? CategoriaFavorita { get; set; }
            public string? DiaSemanaFrecuente { get; set; }
            public string? HoraFrecuente { get; set; }
            public int DiasDesdeUltimaVisita { get; set; }
            public string NivelFidelidad { get; set; } = string.Empty;
        }

        public class ClienteFrecuenteResponse
        {
            public int ClienteId { get; set; }
            public string NombreCompleto { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
            public int TotalVisitas { get; set; }
            public decimal TotalGastado { get; set; }
            public DateTime UltimaVisita { get; set; }
            public decimal TicketPromedio { get; set; }
        }

        public class ClienteCumpleanosResponse
        {
            public int ClienteId { get; set; }
            public string NombreCompleto { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
            public DateTime FechaNacimiento { get; set; }
            public int DiaCumpleanos { get; set; }
            public int Edad { get; set; }
        }
    }
}