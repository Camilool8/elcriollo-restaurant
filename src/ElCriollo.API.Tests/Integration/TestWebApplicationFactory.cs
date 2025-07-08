using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ElCriollo.API.Data;

namespace ElCriollo.API.Tests.Integration
{
    /// <summary>
    /// Factory personalizada para pruebas de integración con configuración Docker
    /// </summary>
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Limpiar configuraciones existentes
                config.Sources.Clear();
                
                // Agregar configuración de pruebas
                config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            });

            builder.ConfigureServices(services =>
            {
                // Remover el DbContext existente
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ElCriolloDbContext>));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Configurar DbContext para pruebas usando la cadena de conexión de Docker
                services.AddDbContext<ElCriolloDbContext>(options =>
                {
                    var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString);
                });

                // Configurar logging para pruebas
                services.Configure<LoggerFilterOptions>(options =>
                {
                    options.MinLevel = LogLevel.Information;
                });
            });

            builder.UseEnvironment("Test");
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
            }
            catch (ObjectDisposedException)
            {
                // Ignorar errores de disposed objects durante cleanup
                // Esto es común en pruebas de integración y no afecta la funcionalidad
            }
            catch (Exception ex)
            {
                // Log otros errores pero no fallar la limpieza
                Console.WriteLine($"Warning during test cleanup: {ex.Message}");
            }
        }
    }
} 