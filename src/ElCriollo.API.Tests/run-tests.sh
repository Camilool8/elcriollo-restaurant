#!/bin/bash

# 🧪 Script de Ejecución de Pruebas - El Criollo
# ================================================

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Funciones de output
success() { echo -e "${GREEN}$1${NC}"; }
error() { echo -e "${RED}$1${NC}"; }
warning() { echo -e "${YELLOW}$1${NC}"; }
info() { echo -e "${CYAN}$1${NC}"; }

# Variables
SETUP_DB=false
COVERAGE=false
VERBOSE=false
CLEANUP=false
FILTER=""
HELP=false

# Procesar argumentos
while [[ $# -gt 0 ]]; do
  case $1 in
    --setup-db)
      SETUP_DB=true
      shift
      ;;
    --coverage)
      COVERAGE=true
      shift
      ;;
    --verbose)
      VERBOSE=true
      shift
      ;;
    --cleanup)
      CLEANUP=true
      shift
      ;;
    --filter)
      FILTER="$2"
      shift 2
      ;;
    --help)
      HELP=true
      shift
      ;;
    *)
      error "❌ Argumento desconocido: $1"
      exit 1
      ;;
  esac
done

# Mostrar ayuda
if [ "$HELP" = true ]; then
    info "🧪 El Criollo - Script de Pruebas Automatizadas"
    info "============================================="
    echo ""
    echo "Uso: ./run-tests.sh [opciones]"
    echo ""
    echo "Opciones:"
    echo "  --setup-db    Configurar base de datos de pruebas"
    echo "  --coverage    Generar reporte de cobertura"
    echo "  --verbose     Salida detallada"
    echo "  --cleanup     Limpiar archivos temporales"
    echo "  --filter      Filtrar pruebas específicas"
    echo "  --help        Mostrar esta ayuda"
    echo ""
    echo "Ejemplos:"
    echo "  ./run-tests.sh --setup-db --verbose"
    echo "  ./run-tests.sh --filter 'FlujoCotidiano*'"
    echo "  ./run-tests.sh --coverage"
    exit 0
fi

info "🚀 INICIANDO PRUEBAS DE EL CRIOLLO"
info "=================================="

# Verificar requisitos
info "🔍 Verificando requisitos..."

# Verificar .NET 8
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    if [[ $DOTNET_VERSION == 8.* ]]; then
        success "✅ .NET 8 encontrado: $DOTNET_VERSION"
    else
        error "❌ Se requiere .NET 8.0 o superior. Versión encontrada: $DOTNET_VERSION"
        exit 1
    fi
else
    error "❌ .NET no encontrado. Instalar .NET 8 SDK"
    exit 1
fi

# Verificar Docker para SQL Server (alternativa para Linux/Mac)
if command -v docker &> /dev/null; then
    success "✅ Docker encontrado"
    
    # Verificar si hay contenedor de SQL Server corriendo
    if docker ps | grep -i "mcr.microsoft.com/mssql/server"; then
        success "✅ SQL Server container encontrado"
    else
        warning "⚠️  SQL Server container no encontrado"
        info "💡 Para configurar SQL Server en Docker:"
        info "   docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=12qw12qw12qw.' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest"
    fi
else
    warning "⚠️  Docker no encontrado. Necesario para SQL Server en Linux/Mac"
fi

# Configurar base de datos si se solicita
if [ "$SETUP_DB" = true ]; then
    info "🗄️  Configurando base de datos de pruebas..."
    
    # Verificar si sqlcmd está disponible
    if command -v sqlcmd &> /dev/null; then
        # Crear base de datos de pruebas
        sqlcmd -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ElCriolloTest') CREATE DATABASE ElCriolloTest" 2>/dev/null
        if [ $? -eq 0 ]; then
            success "✅ Base de datos ElCriolloTest creada/verificada"
            
            # Ejecutar script de inicialización
            SCRIPT_PATH="../../elcriollo.sql"
            if [ -f "$SCRIPT_PATH" ]; then
                sqlcmd -d ElCriolloTest -i "$SCRIPT_PATH" 2>/dev/null
                if [ $? -eq 0 ]; then
                    success "✅ Script de inicialización ejecutado"
                else
                    error "❌ Error ejecutando script de inicialización"
                fi
            else
                warning "⚠️  Script elcriollo.sql no encontrado en $SCRIPT_PATH"
            fi
        else
            error "❌ Error creando base de datos"
        fi
    else
        warning "⚠️  sqlcmd no encontrado. Instalar SQL Server tools o usar Docker"
        info "💡 Para instalar sqlcmd en Ubuntu/Debian:"
        info "   curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -"
        info "   curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list | sudo tee /etc/apt/sources.list.d/msprod.list"
        info "   sudo apt-get update && sudo apt-get install mssql-tools"
    fi
fi

# Restaurar paquetes
info "📦 Restaurando paquetes NuGet..."
if dotnet restore --nologo; then
    success "✅ Paquetes restaurados"
else
    error "❌ Error restaurando paquetes"
    exit 1
fi

# Limpiar archivos temporales si se solicita
if [ "$CLEANUP" = true ]; then
    info "🧹 Limpiando archivos temporales..."
    
    # Limpiar directorios de build
    rm -rf bin obj TestEmails TestResults
    
    success "✅ Archivos temporales eliminados"
fi

# Crear directorio para emails de prueba
mkdir -p TestEmails

# Configurar argumentos de dotnet test
TEST_ARGS=("test" "--nologo")

if [ "$VERBOSE" = true ]; then
    TEST_ARGS+=("--logger" "console;verbosity=detailed")
else
    TEST_ARGS+=("--logger" "console;verbosity=normal")
fi

if [ "$COVERAGE" = true ]; then
    TEST_ARGS+=("--collect:XPlat Code Coverage")
    info "📊 Cobertura de código habilitada"
fi

if [ -n "$FILTER" ]; then
    TEST_ARGS+=("--filter" "$FILTER")
    info "🔍 Filtro aplicado: $FILTER"
fi

# Ejecutar pruebas
info "🧪 Ejecutando pruebas..."
info "Comando: dotnet ${TEST_ARGS[*]}"
echo ""

START_TIME=$(date +%s)

if dotnet "${TEST_ARGS[@]}"; then
    EXIT_CODE=0
else
    EXIT_CODE=$?
fi

END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

echo ""
info "⏱️  Duración total: ${DURATION}s"

if [ $EXIT_CODE -eq 0 ]; then
    success "🎉 ¡TODAS LAS PRUEBAS PASARON EXITOSAMENTE!"
    success "   El sistema El Criollo está listo para desarrollo del frontend"
else
    error "❌ Algunas pruebas fallaron (Código de salida: $EXIT_CODE)"
    error "   Revisar logs para más detalles"
fi

# Mostrar información de cobertura si está habilitada
if [ "$COVERAGE" = true ] && [ $EXIT_CODE -eq 0 ]; then
    info ""
    info "📊 Reporte de cobertura generado en TestResults/"
    
    # Buscar archivos de cobertura
    if find TestResults -name "*.xml" -type f 2>/dev/null | grep -q .; then
        success "✅ Archivos de cobertura encontrados:"
        find TestResults -name "*.xml" -type f | while read -r file; do
            echo "   📄 $file"
        done
    fi
fi

# Mostrar información de emails de prueba
EMAIL_COUNT=$(find TestEmails -type f 2>/dev/null | wc -l)
if [ "$EMAIL_COUNT" -gt 0 ]; then
    info ""
    info "📧 Emails de prueba generados: $EMAIL_COUNT"
    success "✅ Directorio: TestEmails/"
fi

# Resumen final
echo ""
info "📋 RESUMEN DE EJECUCIÓN"
info "======================="
echo "🕐 Duración: ${DURATION}s"
echo "🎯 Estado: $(if [ $EXIT_CODE -eq 0 ]; then echo 'ÉXITO'; else echo 'FALLO'; fi)"

if [ $EXIT_CODE -eq 0 ]; then
    echo ""
    success "🚀 ¡El sistema está listo para el desarrollo del frontend!"
    success "   Todos los endpoints y funcionalidades han sido validados"
    echo ""
    info "Próximos pasos:"
    info "1. Crear proyecto frontend (React, Angular, Vue, etc.)"
    info "2. Configurar consumo de API en https://localhost:7001"
    info "3. Implementar autenticación JWT"
    info "4. Desarrollar interfaces de usuario"
fi

exit $EXIT_CODE 