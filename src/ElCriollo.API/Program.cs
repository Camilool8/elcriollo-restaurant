using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using Serilog;
using AutoMapper;
using FluentValidation;
using ElCriollo.API.Data;
using ElCriollo.API.Configuration;
using ElCriollo.API.Middleware;
using ElCriollo.API.Helpers;
using System.Reflection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// Configuración inicial de Serilog
Log.Logger = new LoggerConfiguration()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Iniciando El Criollo API...");

    var builder = WebApplication.CreateBuilder(args);

    // ============================================================================
    // CONFIGURACIÓN DE SERILOG
    // ============================================================================
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ============================================================================
    // CONFIGURACIÓN DE SERVICIOS
    // ============================================================================

    // Configuraciones personalizadas
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
    
    // Registrar configuraciones como servicios singleton
    builder.Services.AddSingleton(jwtSettings ?? throw new InvalidOperationException("JwtSettings no configurado"));
    builder.Services.AddSingleton(emailSettings ?? throw new InvalidOperationException("EmailSettings no configurado"));
    
    // También registrar como IOptions para servicios que lo necesiten
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

    // Entity Framework
    builder.Services.AddDbContext<ElCriolloDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString, sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); // Correcta ubicación
        });
    });

    // AutoMapper
    builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

    // Controllers
    builder.Services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
        });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ElCriolloCorsPolicy", policy =>
        {
            var corsSettings = builder.Configuration.GetSection("Cors");
            var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000", "https://localhost:3000" };
            var allowedMethods = corsSettings.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
            var allowedHeaders = corsSettings.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "Content-Type", "Authorization", "X-Requested-With", "Accept", "Origin" };
            var allowCredentials = corsSettings.GetValue<bool>("AllowCredentials");
            
            policy.WithOrigins(allowedOrigins)
                  .WithMethods(allowedMethods)
                  .WithHeaders(allowedHeaders);
            
            if (allowCredentials)
            {
                policy.AllowCredentials();
            }
        });
    });

    // Autenticación JWT
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings?.Issuer,
                ValidAudience = jwtSettings?.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? throw new InvalidOperationException("JWT SecretKey no configurada"))
                ),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Warning("🔐 Error de autenticación JWT: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Log.Debug("🔐 Token JWT validado exitosamente para usuario: {User}", 
                        context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                }
            };
        });

    // Autorización
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrador"));
        options.AddPolicy("RecepcionPolicy", policy => policy.RequireRole("Administrador", "Recepcion"));
        options.AddPolicy("MeseroPolicy", policy => policy.RequireRole("Administrador", "Mesero"));
        options.AddPolicy("CajeroPolicy", policy => policy.RequireRole("Administrador", "Cajero"));
    });

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "El Criollo API",
            Version = "v1.0",
            Description = "API REST para el Sistema POS del Restaurante El Criollo - Comida Dominicana Auténtica",
            Contact = new OpenApiContact
            {
                Name = "Equipo de Desarrollo El Criollo",
                Email = "desarrollo@elcriollo.com",
                Url = new Uri("https://github.com/elcriollo/api")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // JWT Security Definition
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });

        // XML Documentation
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        // Habilitar anotaciones de Swagger
        c.EnableAnnotations();
    });

    // FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Response Caching
    builder.Services.AddResponseCaching();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ElCriolloDbContext>("database")
        .AddCheck("api-health", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API está funcionando correctamente"));

    // Rate Limiting (si está habilitado)
    var rateLimitingEnabled = builder.Configuration.GetValue<bool>("RateLimiting:EnableRateLimiting");
    if (rateLimitingEnabled)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("GlobalPolicy", limiterOptions =>
            {
                limiterOptions.PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimit");
                limiterOptions.Window = TimeSpan.Parse(builder.Configuration.GetValue<string>("RateLimiting:Window") ?? "00:01:00");
                limiterOptions.QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:QueueLimit");
            });
        });
    }

    // ============================================================================
    // REGISTRO DE SERVICIOS Y REPOSITORIOS PERSONALIZADOS
    // ============================================================================
    
    // Repositorios
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IUsuarioRepository, ElCriollo.API.Repositories.UsuarioRepository>();
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IProductoRepository, ElCriollo.API.Repositories.ProductoRepository>();
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IOrdenRepository, ElCriollo.API.Repositories.OrdenRepository>();
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IFacturaRepository, ElCriollo.API.Repositories.FacturaRepository>();
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IMesaRepository, ElCriollo.API.Repositories.MesaRepository>();
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IReservacionRepository, ElCriollo.API.Repositories.ReservacionRepository>();
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IReporteRepository, ElCriollo.API.Repositories.ReporteRepository>();
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IEmpleadoRepository, ElCriollo.API.Repositories.EmpleadoRepository>();
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IInventarioRepository, ElCriollo.API.Repositories.InventarioRepository>();
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IClienteRepository, ElCriollo.API.Repositories.ClienteRepository>();
    builder.Services.AddScoped<ElCriollo.API.Interfaces.IBaseRepository<ElCriollo.API.Models.Entities.EmailTransaccion>, ElCriollo.API.Repositories.BaseRepository<ElCriollo.API.Models.Entities.EmailTransaccion>>();
    
    // Servicios
    builder.Services.AddScoped<ElCriollo.API.Services.IAuthService, ElCriollo.API.Services.AuthService>();
    builder.Services.AddScoped<ElCriollo.API.Services.IProductoService, ElCriollo.API.Services.ProductoService>();
    builder.Services.AddScoped<ElCriollo.API.Services.IOrdenService, ElCriollo.API.Services.OrdenService>();
    builder.Services.AddScoped<ElCriollo.API.Services.IFacturaService, ElCriollo.API.Services.FacturaService>();
    builder.Services.AddScoped<ElCriollo.API.Services.IMesaService, ElCriollo.API.Services.MesaService>();
    builder.Services.AddScoped<ElCriollo.API.Services.IReservacionService, ElCriollo.API.Services.ReservacionService>();
    builder.Services.AddScoped<ElCriollo.API.Services.IEmailService, ElCriollo.API.Services.EmailService>();
    builder.Services.AddScoped<ElCriollo.API.Services.IReporteService, ElCriollo.API.Services.ReporteService>();

    var app = builder.Build();

    // ============================================================================
    // CONFIGURACIÓN DEL PIPELINE DE MIDDLEWARE
    // ============================================================================

    // Request Logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "🌐 HTTP {RequestMethod} {RequestPath} respondido {StatusCode} en {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent);
        };
    });

    // Error Handling Middleware
    app.UseErrorHandling();


    // Swagger (solo en desarrollo)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "El Criollo API v1");
            c.RoutePrefix = string.Empty; // Swagger en la raíz
            c.DocumentTitle = "El Criollo API - Documentación";
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });
    }

    // HTTPS Redirection
    app.UseHttpsRedirection();

    // CORS
    app.UseCors("ElCriolloCorsPolicy");

    // Response Caching
    app.UseResponseCaching();

    // Rate Limiting
    if (rateLimitingEnabled)
    {
        app.UseRateLimiter();
    }

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Controllers
    app.MapControllers();

    // Health Checks
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            
            var result = new
            {
                Status = report.Status.ToString(),
                Checks = report.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration.TotalMilliseconds
                }),
                TotalDuration = report.TotalDuration.TotalMilliseconds
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));
        }
    });

    // Endpoint de bienvenida
    app.MapGet("/", () => new
    {
        message = "🇩🇴 ¡Bienvenido a El Criollo API! - Sistema POS para Restaurante Dominicano",
        version = "1.0.0",
        timestamp = DateTime.UtcNow,
        environment = app.Environment.EnvironmentName,
        documentation = "/swagger",
        health = "/health"
    });

    // ============================================================================
    // INICIALIZACIÓN Y MIGRACIÓN DE BASE DE DATOS
    // ============================================================================
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ElCriolloDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        try
        {
            // Verificar conexión a la base de datos
            await context.Database.CanConnectAsync();
            Log.Information("✅ Conexión a base de datos establecida correctamente");

            // Auto-migración (si está habilitada)
            if (configuration.GetValue<bool>("DeveloperSettings:EnableAutoMigration"))
            {
                Log.Information("🔄 Inicializando estructura de base de datos en {Environment}...", app.Environment.EnvironmentName);
                
                // Verificar si existen migraciones pendientes
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    Log.Information("📋 Aplicando {Count} migraciones pendientes...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                }
                else
                {
                    // Si no hay migraciones, crear la base de datos usando el modelo actual
                    Log.Information("🏗️ Creando base de datos usando Code First...");
                    await context.Database.EnsureCreatedAsync();
                }
                
                Log.Information("✅ Estructura de base de datos inicializada exitosamente");
            }

            // Seed data (si está habilitado)
            if (configuration.GetValue<bool>("DeveloperSettings:EnableSeedData"))
            {
                Log.Information("🌱 Verificando datos iniciales...");
                
                                 // Crear usuario administrador inicial si no existe
                 var authService = scope.ServiceProvider.GetRequiredService<ElCriollo.API.Services.IAuthService>();
                 try
                 {
                     var adminUser = await authService.CreateAdminUserAsync();
                     Log.Information("✅ Usuario administrador verificado/creado: {Usuario}", adminUser.Usuario);
                 }
                 catch (InvalidOperationException ex)
                 {
                     Log.Information("ℹ️ Usuario administrador ya existe: {Message}", ex.Message);
                 }
                 catch (Exception ex)
                 {
                     Log.Error(ex, "❌ Error al crear usuario administrador inicial");
                 }
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ Error fatal al conectar con la base de datos");
            throw;
        }
    }

    Log.Information("🎉 El Criollo API iniciada exitosamente en {Environment}", app.Environment.EnvironmentName);
    Log.Information("📖 Documentación disponible en: /swagger");
    Log.Information("🏥 Health Check disponible en: /health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Error fatal al iniciar El Criollo API");
}
finally
{
    Log.CloseAndFlush();
}

// Hacer la clase Program pública para pruebas de integración
public partial class Program { }