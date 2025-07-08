# Dockerfile para El Criollo API
# Multi-stage build para optimizar el tamaño de la imagen
# Compilado específicamente para arquitectura x86_64 (AMD64)

# ============================================================================
# STAGE 1: Build - Compilar la aplicación
# ============================================================================
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiar archivos de proyecto y restaurar dependencias
COPY src/ElCriollo.API/*.csproj ./src/ElCriollo.API/
COPY src/ElCriollo.API/NuGet.config ./src/ElCriollo.API/
RUN dotnet restore src/ElCriollo.API/ElCriollo.Api.csproj --runtime linux-x64

# Copiar el resto del código fuente
COPY src/ElCriollo.API/ ./src/ElCriollo.API/

# Compilar la aplicación específicamente para linux-x64
WORKDIR /app/src/ElCriollo.API
RUN dotnet publish -c Release -o /app/publish --no-restore --runtime linux-x64 --self-contained false

# ============================================================================
# STAGE 2: Runtime - Imagen final optimizada
# ============================================================================
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Crear usuario no-root para seguridad
RUN groupadd -r elcriollo && useradd -r -g elcriollo elcriollo

# Instalar dependencias del sistema si son necesarias
RUN apt-get update && apt-get install -y \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copiar los archivos publicados desde el stage de build
COPY --from=build-env /app/publish .

# Crear directorios necesarios para logs y emails
RUN mkdir -p /app/logs /app/logs/emails && \
    chown -R elcriollo:elcriollo /app

# Cambiar al usuario no-root
USER elcriollo

# Configurar variables de entorno
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Exponer el puerto
EXPOSE 8080

# Punto de entrada
ENTRYPOINT ["dotnet", "ElCriollo.Api.dll"] 