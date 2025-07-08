using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ElCriollo.API.Middleware
{
    /// <summary>
    /// Middleware global para manejo de errores y excepciones no controladas
    /// Proporciona respuestas consistentes y logging centralizado
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next, 
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error no controlado en {Method} {Path}", 
                    context.Request.Method, 
                    context.Request.Path);
                
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";
            
            var problemDetails = CreateProblemDetails(context, exception);
            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(problemDetails, options);
            await context.Response.WriteAsync(json);
        }

        private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
        {
            var problemDetails = exception switch
            {
                // Excepciones de validación y reglas de negocio
                InvalidOperationException invalidOp => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Operación inválida",
                    Detail = invalidOp.Message,
                    Type = "https://elcriollo.com/errors/invalid-operation"
                },

                // Excepciones de argumentos (orden importante: más específico primero)
                ArgumentNullException argNullEx => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Argumento requerido",
                    Detail = argNullEx.Message,
                    Type = "https://elcriollo.com/errors/null-argument"
                },

                ArgumentException argEx => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Argumento inválido",
                    Detail = argEx.Message,
                    Type = "https://elcriollo.com/errors/invalid-argument"
                },

                // Excepciones de autorización
                UnauthorizedAccessException => new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "No autorizado",
                    Detail = "No tiene permisos para acceder a este recurso",
                    Type = "https://elcriollo.com/errors/unauthorized"
                },

                // Excepciones de base de datos (orden importante: más específico primero)
                DbUpdateConcurrencyException => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflicto de concurrencia",
                    Detail = "El registro fue modificado por otro usuario. Por favor, recargue y vuelva a intentar",
                    Type = "https://elcriollo.com/errors/concurrency-conflict"
                },

                DbUpdateException dbEx => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Error de base de datos",
                    Detail = GetDbUpdateExceptionMessage(dbEx),
                    Type = "https://elcriollo.com/errors/database-conflict"
                },

                // Excepciones personalizadas del negocio (primero para prioridad)
                ValidationException validationEx => new ValidationProblemDetails(validationEx.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Error de validación",
                    Detail = "Uno o más campos tienen errores de validación",
                    Type = "https://elcriollo.com/errors/validation"
                },

                NotFoundException notFoundEx => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso no encontrado",
                    Detail = notFoundEx.Message,
                    Type = "https://elcriollo.com/errors/not-found"
                },

                BusinessException businessEx => new ProblemDetails
                {
                    Status = businessEx.StatusCode,
                    Title = businessEx.Title,
                    Detail = businessEx.Message,
                    Type = businessEx.Type
                },

                // Excepciones de timeout (orden importante: más específico primero)
                TaskCanceledException => new ProblemDetails
                {
                    Status = StatusCodes.Status408RequestTimeout,
                    Title = "Tiempo de espera agotado",
                    Detail = "La operación tardó demasiado tiempo en completarse",
                    Type = "https://elcriollo.com/errors/timeout"
                },

                OperationCanceledException => new ProblemDetails
                {
                    Status = StatusCodes.Status408RequestTimeout,
                    Title = "Operación cancelada",
                    Detail = "La operación fue cancelada antes de completarse",
                    Type = "https://elcriollo.com/errors/operation-cancelled"
                },

                // Excepción por defecto
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno del servidor",
                    Detail = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "Ha ocurrido un error inesperado. Por favor, contacte al administrador",
                    Type = "https://elcriollo.com/errors/internal-server-error"
                }
            };

            // Agregar información adicional común
            problemDetails.Instance = context.Request.Path;
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
            problemDetails.Extensions["method"] = context.Request.Method;

            // En desarrollo, incluir stack trace
            if (_environment.IsDevelopment() && exception is not BusinessException)
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                problemDetails.Extensions["innerException"] = exception.InnerException?.Message;
            }

            return problemDetails;
        }

        private string GetDbUpdateExceptionMessage(DbUpdateException dbEx)
        {
            // Analizar el mensaje interno para proporcionar un mensaje más amigable
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

            if (innerMessage.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
            {
                return "Ya existe un registro con esos datos. Verifique que no esté duplicando información";
            }

            if (innerMessage.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
            {
                return "No se puede completar la operación porque hay registros relacionados";
            }

            if (innerMessage.Contains("DELETE", StringComparison.OrdinalIgnoreCase))
            {
                return "No se puede eliminar el registro porque tiene datos asociados";
            }

            return "Error al guardar los cambios en la base de datos";
        }
    }

    /// <summary>
    /// Excepción base para errores de negocio
    /// </summary>
    public class BusinessException : Exception
    {
        public int StatusCode { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }

        public BusinessException(string message, int statusCode = 400, string title = "Error de negocio") : base(message)
        {
            StatusCode = statusCode;
            Title = title;
            Type = "https://elcriollo.com/errors/business-error";
        }
    }

    /// <summary>
    /// Excepción para recursos no encontrados
    /// </summary>
    public class NotFoundException : BusinessException
    {
        public NotFoundException(string resourceName, object key) 
            : base($"{resourceName} con identificador '{key}' no fue encontrado", 404, "Recurso no encontrado")
        {
            Type = "https://elcriollo.com/errors/not-found";
        }

        public NotFoundException(string message) 
            : base(message, 404, "Recurso no encontrado")
        {
            Type = "https://elcriollo.com/errors/not-found";
        }
    }

    /// <summary>
    /// Excepción para errores de validación con múltiples errores
    /// </summary>
    public class ValidationException : BusinessException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors) 
            : base("Uno o más errores de validación ocurrieron", 400, "Error de validación")
        {
            Errors = errors;
            Type = "https://elcriollo.com/errors/validation";
        }

        public ValidationException(string field, string error) 
            : base("Error de validación", 400, "Error de validación")
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { error } }
            };
            Type = "https://elcriollo.com/errors/validation";
        }
    }

    /// <summary>
    /// Extensión para registrar el middleware
    /// </summary>
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }

    /// <summary>
    /// Filtro de excepción alternativo para usar con controladores
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "❌ Excepción no controlada en {Action}", 
                context.ActionDescriptor.DisplayName);

            var problemDetails = new ProblemDetails
            {
                Instance = context.HttpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = context.HttpContext.TraceIdentifier,
                    ["timestamp"] = DateTime.UtcNow
                }
            };

            switch (context.Exception)
            {
                case ValidationException validationEx:
                    context.Result = new BadRequestObjectResult(new ValidationProblemDetails(validationEx.Errors)
                    {
                        Title = "Error de validación",
                        Status = StatusCodes.Status400BadRequest,
                        Instance = problemDetails.Instance
                    });
                    context.ExceptionHandled = true;
                    break;

                case NotFoundException notFoundEx:
                    problemDetails.Title = "Recurso no encontrado";
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    problemDetails.Detail = notFoundEx.Message;
                    
                    context.Result = new NotFoundObjectResult(problemDetails);
                    context.ExceptionHandled = true;
                    break;

                case BusinessException businessEx:
                    problemDetails.Title = businessEx.Title;
                    problemDetails.Status = businessEx.StatusCode;
                    problemDetails.Detail = businessEx.Message;
                    problemDetails.Type = businessEx.Type;
                    
                    context.Result = new ObjectResult(problemDetails)
                    {
                        StatusCode = businessEx.StatusCode
                    };
                    context.ExceptionHandled = true;
                    break;

                default:
                    problemDetails.Title = "Error interno del servidor";
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    problemDetails.Detail = _environment.IsDevelopment() 
                        ? context.Exception.Message 
                        : "Ha ocurrido un error inesperado";

                    if (_environment.IsDevelopment())
                    {
                        problemDetails.Extensions["stackTrace"] = context.Exception.StackTrace;
                    }

                    context.Result = new ObjectResult(problemDetails)
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                    context.ExceptionHandled = true;
                    break;
            }
        }
    }
}