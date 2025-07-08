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
    /// Controlador para la gestión de empleados del restaurante El Criollo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [SwaggerTag("Gestión de empleados, roles y nómina")]
    public class EmpleadoController : ControllerBase
    {
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<EmpleadoController> _logger;

        public EmpleadoController(
            IEmpleadoRepository empleadoRepository,
            IEmailService emailService,
            IMapper mapper,
            ILogger<EmpleadoController> logger)
        {
            _empleadoRepository = empleadoRepository;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================================================
        // GESTIÓN DE EMPLEADOS
        // ============================================================================

        /// <summary>
        /// Crear un nuevo empleado
        /// </summary>
        /// <param name="request">Datos del nuevo empleado</param>
        /// <returns>Empleado creado</returns>
        /// <response code="201">Empleado creado exitosamente</response>
        /// <response code="400">Datos inválidos o empleado ya existe</response>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [SwaggerOperation(
            Summary = "Registrar nuevo empleado",
            Description = "Registra un nuevo empleado en el sistema. Se envía email de bienvenida automáticamente",
            OperationId = "Empleado.Crear",
            Tags = new[] { "Gestión de Empleados" }
        )]
        [ProducesResponseType(typeof(EmpleadoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EmpleadoResponse>> CrearEmpleado([FromBody] CrearEmpleadoRequest request)
        {
            try
            {
                _logger.LogInformation("👤 Registrando nuevo empleado: {Email}", request.Email);

                // Verificar si el empleado ya existe por cédula
                if (!string.IsNullOrEmpty(request.Cedula))
                {
                    var empleadoExistente = await _empleadoRepository.GetByCedulaAsync(request.Cedula);
                    if (empleadoExistente != null)
                    {
                        return BadRequest(new ValidationProblemDetails
                        {
                            Title = "Empleado ya existe",
                            Detail = "Ya existe un empleado registrado con esta cédula",
                            Status = StatusCodes.Status400BadRequest
                        });
                    }
                }

                // Crear empleado manualmente para manejar NombreCompleto
                var empleado = new Models.Entities.Empleado();
                
                // Dividir NombreCompleto en Nombre y Apellido
                var partes = request.NombreCompleto.Split(' ', 2);
                empleado.Nombre = partes[0];
                empleado.Apellido = partes.Length > 1 ? partes[1] : string.Empty;
                
                // Mapear resto de propiedades
                empleado.Cedula = request.Cedula ?? string.Empty;
                empleado.Telefono = request.Telefono;
                empleado.Email = request.Email;
                empleado.Direccion = request.Direccion;
                empleado.FechaNacimiento = request.FechaNacimiento;
                empleado.PreferenciasComida = request.PreferenciasComida;
                empleado.FechaIngreso = DateTime.Now;
                empleado.Estado = "Activo";

                var empleadoCreado = await _empleadoRepository.CreateAsync(empleado);
                
                // Enviar email de bienvenida
                try
                {
                    // Enviar notificación personalizada para empleado nuevo
                    await _emailService.EnviarNotificacionPersonalizadaAsync(
                        empleadoCreado.Email ?? string.Empty,
                        "Bienvenido a El Criollo",
                        $"Hola {empleadoCreado.NombreCompleto}, te damos la bienvenida al equipo de El Criollo. Pronto recibirás más información sobre tu acceso al sistema.",
                        false
                    );
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "⚠️ No se pudo enviar email de bienvenida");
                }

                var response = _mapper.Map<EmpleadoResponse>(empleadoCreado);
                
                _logger.LogInformation("✅ Empleado {EmpleadoId} registrado exitosamente", empleadoCreado.EmpleadoID);
                
                return CreatedAtAction(
                    nameof(GetEmpleadoById), 
                    new { id = empleadoCreado.EmpleadoID }, 
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear empleado");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al registrar el empleado",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener un empleado por ID
        /// </summary>
        /// <param name="id">ID del empleado</param>
        /// <returns>Datos del empleado</returns>
        /// <response code="200">Empleado encontrado</response>
        /// <response code="404">Empleado no encontrado</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtener empleado por ID",
            Description = "Obtiene los datos completos de un empleado específico",
            OperationId = "Empleado.GetById",
            Tags = new[] { "Consulta de Empleados" }
        )]
        [ProducesResponseType(typeof(EmpleadoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmpleadoResponse>> GetEmpleadoById(int id)
        {
            try
            {
                var empleado = await _empleadoRepository.GetByIdAsync(id);
                
                if (empleado == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Empleado no encontrado",
                        Detail = $"No se encontró el empleado con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                var response = _mapper.Map<EmpleadoResponse>(empleado);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener empleado {EmpleadoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Buscar empleados
        /// </summary>
        /// <param name="filtro">Filtros de búsqueda</param>
        /// <returns>Lista de empleados que coinciden con el filtro</returns>
        /// <response code="200">Lista de empleados</response>
        [HttpGet("buscar")]
        [SwaggerOperation(
            Summary = "Buscar empleados",
            Description = "Busca empleados por nombre, email, teléfono o cédula",
            OperationId = "Empleado.Buscar",
            Tags = new[] { "Consulta de Empleados" }
        )]
        [ProducesResponseType(typeof(IEnumerable<EmpleadoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmpleadoResponse>>> BuscarEmpleados([FromQuery] BuscarEmpleadoRequest filtro)
        {
            try
            {
                _logger.LogInformation("🔍 Buscando empleados con filtro: {Filtro}", filtro.Termino);

                IEnumerable<Models.Entities.Empleado> empleados;

                // Usar búsqueda directa en todos los empleados
                var todosLosEmpleados = await _empleadoRepository.GetAllAsync();
                
                if (!string.IsNullOrEmpty(filtro.Termino))
                {
                    var termino = filtro.Termino.ToLower();
                    empleados = todosLosEmpleados.Where(e => 
                        e.NombreCompleto.ToLower().Contains(termino) ||
                        (e.Email != null && e.Email.ToLower().Contains(termino)) ||
                        (e.Telefono != null && e.Telefono.Contains(termino)) ||
                        (e.Cedula != null && e.Cedula.Contains(termino))
                    );
                }
                else
                {
                    empleados = todosLosEmpleados;
                }

                var response = _mapper.Map<IEnumerable<EmpleadoResponse>>(empleados);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar empleados");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener todos los empleados
        /// </summary>
        /// <param name="incluirInactivos">Incluir empleados inactivos</param>
        /// <returns>Lista de todos los empleados</returns>
        /// <response code="200">Lista de empleados</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtener todos los empleados",
            Description = "Devuelve la lista completa de empleados del restaurante",
            OperationId = "Empleado.GetTodos",
            Tags = new[] { "Consulta de Empleados" }
        )]
        [ProducesResponseType(typeof(IEnumerable<EmpleadoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmpleadoResponse>>> GetTodosLosEmpleados([FromQuery] bool incluirInactivos = false)
        {
            try
            {
                _logger.LogInformation("📋 Consultando todos los empleados");

                var empleados = incluirInactivos 
                    ? await _empleadoRepository.GetAllAsync()
                    : await _empleadoRepository.GetEmpleadosActivosAsync();

                var response = _mapper.Map<IEnumerable<EmpleadoResponse>>(empleados);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener empleados");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Actualizar datos de un empleado
        /// </summary>
        /// <param name="id">ID del empleado</param>
        /// <param name="request">Nuevos datos del empleado</param>
        /// <returns>Empleado actualizado</returns>
        /// <response code="200">Empleado actualizado exitosamente</response>
        /// <response code="404">Empleado no encontrado</response>
        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Actualizar empleado",
            Description = "Actualiza los datos de un empleado existente",
            OperationId = "Empleado.Actualizar",
            Tags = new[] { "Gestión de Empleados" }
        )]
        [ProducesResponseType(typeof(EmpleadoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmpleadoResponse>> ActualizarEmpleado(int id, [FromBody] ActualizarEmpleadoRequest request)
        {
            try
            {
                _logger.LogInformation("📝 Actualizando empleado {EmpleadoId}", id);

                var empleado = await _empleadoRepository.GetByIdAsync(id);
                if (empleado == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Empleado no encontrado",
                        Detail = $"No se encontró el empleado con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                // Actualizar propiedades
                // Actualizar Nombre y Apellido por separado
                if (!string.IsNullOrEmpty(request.NombreCompleto))
                {
                    var partes = request.NombreCompleto.Split(' ', 2);
                    empleado.Nombre = partes[0];
                    empleado.Apellido = partes.Length > 1 ? partes[1] : string.Empty;
                }
                empleado.Telefono = request.Telefono ?? empleado.Telefono;
                empleado.Direccion = request.Direccion ?? empleado.Direccion;
                empleado.Email = request.Email ?? empleado.Email;
                empleado.FechaNacimiento = request.FechaNacimiento ?? empleado.FechaNacimiento;
                empleado.PreferenciasComida = request.PreferenciasComida ?? empleado.PreferenciasComida;

                await _empleadoRepository.UpdateAsync(empleado);

                var response = _mapper.Map<EmpleadoResponse>(empleado);
                
                _logger.LogInformation("✅ Empleado {EmpleadoId} actualizado exitosamente", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar empleado {EmpleadoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Desactivar un empleado
        /// </summary>
        /// <param name="id">ID del empleado</param>
        /// <returns>Confirmación de desactivación</returns>
        /// <response code="200">Empleado desactivado</response>
        /// <response code="404">Empleado no encontrado</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        [SwaggerOperation(
            Summary = "Desactivar empleado",
            Description = "Marca un empleado como inactivo (no se elimina físicamente)",
            OperationId = "Empleado.Desactivar",
            Tags = new[] { "Gestión de Empleados" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DesactivarEmpleado(int id)
        {
            try
            {
                _logger.LogInformation("🚫 Desactivando empleado {EmpleadoId}", id);

                var empleado = await _empleadoRepository.GetByIdAsync(id);
                if (empleado == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Empleado no encontrado",
                        Detail = $"No se encontró el empleado con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                empleado.Estado = "Inactivo";
                await _empleadoRepository.UpdateAsync(empleado);

                _logger.LogInformation("✅ Empleado {EmpleadoId} desactivado", id);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Empleado desactivado exitosamente",
                    Data = new { EmpleadoId = id, Estado = "Inactivo" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al desactivar empleado {EmpleadoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // HISTORIAL Y ESTADÍSTICAS
        // ============================================================================

        /// <summary>
        /// Obtener historial de compras de un empleado
        /// </summary>
        /// <param name="id">ID del empleado</param>
        /// <param name="fechaInicio">Fecha inicio del historial</param>
        /// <param name="fechaFin">Fecha fin del historial</param>
        /// <returns>Historial de compras del empleado</returns>
        /// <response code="200">Historial de compras</response>
        /// <response code="404">Empleado no encontrado</response>
        [HttpGet("{id:int}/historial-compras")]
        [SwaggerOperation(
            Summary = "Historial de compras del empleado",
            Description = "Obtiene el historial detallado de todas las compras realizadas por el empleado",
            OperationId = "Empleado.HistorialCompras",
            Tags = new[] { "Estadísticas de Empleado" }
        )]
        [ProducesResponseType(typeof(EmpleadoHistorialComprasResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmpleadoHistorialComprasResponse>> GetHistorialCompras(
            int id, 
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                _logger.LogInformation("📊 Consultando historial de compras del empleado {EmpleadoId}", id);

                var empleado = await _empleadoRepository.GetByIdAsync(id);
                if (empleado == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Empleado no encontrado",
                        Detail = $"No se encontró el empleado con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                // Obtener historial de compras del empleado
                var historial = await _empleadoRepository.GetHistorialComprasAsync(id, fechaInicio, fechaFin);
                
                // Construir respuesta con resumen
                var response = new EmpleadoHistorialComprasResponse
                {
                    EmpleadoId = empleado.EmpleadoID,
                    NombreEmpleado = empleado.NombreCompleto,
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
        /// Obtener estadísticas del empleado
        /// </summary>
        /// <param name="id">ID del empleado</param>
        /// <returns>Estadísticas de consumo y preferencias</returns>
        /// <response code="200">Estadísticas del empleado</response>
        /// <response code="404">Empleado no encontrado</response>
        [HttpGet("{id:int}/estadisticas")]
        [SwaggerOperation(
            Summary = "Estadísticas del empleado",
            Description = "Obtiene estadísticas detalladas de consumo, preferencias y comportamiento del empleado",
            OperationId = "Empleado.Estadisticas",
            Tags = new[] { "Estadísticas de Empleado" }
        )]
        [ProducesResponseType(typeof(EstadisticasEmpleadoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EstadisticasEmpleadoResponse>> GetEstadisticasEmpleado(int id)
        {
            try
            {
                _logger.LogInformation("📊 Generando estadísticas del empleado {EmpleadoId}", id);

                var empleado = await _empleadoRepository.GetByIdAsync(id);
                if (empleado == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Empleado no encontrado",
                        Detail = $"No se encontró el empleado con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                // Obtener estadísticas del empleado
                var estadisticasData = await _empleadoRepository.GetEstadisticasEmpleadoAsync(id);

                if (estadisticasData == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Estadísticas no disponibles",
                        Detail = "No se pudieron obtener las estadísticas del empleado",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                var estadisticas = new EstadisticasEmpleadoResponse
                {
                    EmpleadoId = (int)estadisticasData.GetType().GetProperty("EmpleadoId")!.GetValue(estadisticasData)!,
                    NombreEmpleado = (string)estadisticasData.GetType().GetProperty("NombreEmpleado")!.GetValue(estadisticasData)!,
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
                _logger.LogError(ex, "❌ Error al generar estadísticas del empleado");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // PROGRAMA DE FIDELIZACIÓN
        // ============================================================================

        /// <summary>
        /// Obtener empleados frecuentes
        /// </summary>
        /// <param name="minVisitas">Mínimo de visitas para considerar frecuente</param>
        /// <returns>Lista de empleados frecuentes</returns>
        /// <response code="200">Lista de empleados frecuentes</response>
        [HttpGet("frecuentes")]
        [SwaggerOperation(
            Summary = "Obtener empleados frecuentes",
            Description = "Obtiene la lista de empleados frecuentes basado en cantidad de visitas",
            OperationId = "Empleado.Frecuentes",
            Tags = new[] { "Programa de Fidelización" }
        )]
        [ProducesResponseType(typeof(IEnumerable<EmpleadoFrecuenteResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmpleadoFrecuenteResponse>>> GetEmpleadosFrecuentes([FromQuery] int minVisitas = 5)
        {
            try
            {
                _logger.LogInformation("🌟 Consultando empleados frecuentes (mínimo {MinVisitas} visitas)", minVisitas);

                // Obtener empleados frecuentes
                var empleadosData = await _empleadoRepository.GetEmpleadosFrecuentesAsync(minVisitas);
                
                var empleadosFrecuentes = empleadosData.Select(e => new EmpleadoFrecuenteResponse
                {
                    EmpleadoId = (int)e.GetType().GetProperty("EmpleadoId")!.GetValue(e)!,
                    NombreCompleto = (string)e.GetType().GetProperty("NombreCompleto")!.GetValue(e)!,
                    Email = (string)e.GetType().GetProperty("Email")!.GetValue(e)!,
                    Telefono = (string)e.GetType().GetProperty("Telefono")!.GetValue(e)!,
                    TotalVisitas = (int)e.GetType().GetProperty("TotalVisitas")!.GetValue(e)!,
                    TotalGastado = (decimal)e.GetType().GetProperty("TotalGastado")!.GetValue(e)!,
                    UltimaVisita = (DateTime)e.GetType().GetProperty("UltimaVisita")!.GetValue(e)!,
                    TicketPromedio = (decimal)e.GetType().GetProperty("TicketPromedio")!.GetValue(e)!
                }).ToList();

                return Ok(empleadosFrecuentes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener empleados frecuentes");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener cumpleañeros del mes
        /// </summary>
        /// <param name="mes">Número del mes (1-12)</param>
        /// <returns>Lista de empleados que cumplen años en el mes</returns>
        /// <response code="200">Lista de cumpleañeros</response>
        [HttpGet("cumpleanos")]
        [SwaggerOperation(
            Summary = "Obtener cumpleañeros del mes",
            Description = "Obtiene la lista de empleados que cumplen años en el mes especificado",
            OperationId = "Empleado.Cumpleanos",
            Tags = new[] { "Programa de Fidelización" }
        )]
        [ProducesResponseType(typeof(IEnumerable<EmpleadoCumpleanosResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmpleadoCumpleanosResponse>>> GetCumpleaneros([FromQuery] int? mes = null)
        {
            try
            {
                var mesConsulta = mes ?? DateTime.Now.Month;
                _logger.LogInformation("🎂 Consultando cumpleañeros del mes {Mes}", mesConsulta);

                // Obtener empleados que cumplen años en el mes
                var cumpleanosData = await _empleadoRepository.GetEmpleadosCumpleanosAsync(mesConsulta);
                
                var cumpleaneros = cumpleanosData.Select(e => new EmpleadoCumpleanosResponse
                {
                    EmpleadoId = (int)e.GetType().GetProperty("EmpleadoId")!.GetValue(e)!,
                    NombreCompleto = (string)e.GetType().GetProperty("NombreCompleto")!.GetValue(e)!,
                    Email = (string)e.GetType().GetProperty("Email")!.GetValue(e)!,
                    Telefono = (string)e.GetType().GetProperty("Telefono")!.GetValue(e)!,
                    FechaNacimiento = (DateTime)e.GetType().GetProperty("FechaNacimiento")!.GetValue(e)!,
                    DiaCumpleanos = (int)e.GetType().GetProperty("DiaCumpleanos")!.GetValue(e)!,
                    Edad = (int)e.GetType().GetProperty("Edad")!.GetValue(e)!
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
        // REQUESTS Y RESPONSES ESPECÍFICOS
        // ============================================================================

        public class CrearEmpleadoRequest
        {
            public string NombreCompleto { get; set; } = string.Empty;
            public string? Cedula { get; set; }
            public string Telefono { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Direccion { get; set; }
            public DateTime? FechaNacimiento { get; set; }
            public string? PreferenciasComida { get; set; }
        }

        public class ActualizarEmpleadoRequest
        {
            public string? NombreCompleto { get; set; }
            public string? Telefono { get; set; }
            public string? Email { get; set; }
            public string? Direccion { get; set; }
            public DateTime? FechaNacimiento { get; set; }
            public string? PreferenciasComida { get; set; }
        }

        public class BuscarEmpleadoRequest
        {
            public string? Termino { get; set; }
        }





        public class EstadisticasEmpleadoResponse
        {
            public int EmpleadoId { get; set; }
            public string NombreEmpleado { get; set; } = string.Empty;
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

        public class EmpleadoFrecuenteResponse
        {
            public int EmpleadoId { get; set; }
            public string NombreCompleto { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
            public int TotalVisitas { get; set; }
            public decimal TotalGastado { get; set; }
            public DateTime UltimaVisita { get; set; }
            public decimal TicketPromedio { get; set; }
        }

        public class EmpleadoCumpleanosResponse
        {
            public int EmpleadoId { get; set; }
            public string NombreCompleto { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
            public DateTime FechaNacimiento { get; set; }
            public int DiaCumpleanos { get; set; }
            public int Edad { get; set; }
        }
    }
}