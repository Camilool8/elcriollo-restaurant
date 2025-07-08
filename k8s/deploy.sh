#!/bin/bash

# Script de despliegue para El Criollo en k3s
# Autor: Jose Joga
# Fecha: 2025

set -e

echo "🇩🇴 Desplegando El Criollo POS System en k3s..."

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para logging
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
    exit 1
}

warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

# Verificar que kubectl está disponible
if ! command -v kubectl &> /dev/null; then
    error "kubectl no está instalado o no está en el PATH"
fi

# Verificar conexión al cluster
if ! kubectl cluster-info &> /dev/null; then
    error "No se puede conectar al cluster de Kubernetes"
fi

log "Verificando conexión al cluster k3s..."
kubectl cluster-info

# Paso 1: Crear namespace
log "Creando namespace 'elcriollo'..."
kubectl apply -f namespace.yaml

# Paso 2: Desplegar SQL Server
log "Desplegando SQL Server..."
kubectl apply -f sqlserver-deployment.yaml

# Esperar a que SQL Server esté listo
log "Esperando a que SQL Server esté listo..."
kubectl wait --for=condition=available --timeout=300s deployment/sqlserver-deployment -n elcriollo

# Verificar que SQL Server está funcionando
log "Verificando estado de SQL Server..."
kubectl get pods -n elcriollo -l app=sqlserver

# Paso 2.5: Ejecutar Job de inicialización de base de datos
log "Ejecutando Job de inicialización de base de datos..."
kubectl apply -f sqlserver-init-job.yaml

# Esperar a que el Job complete
log "Esperando a que la inicialización de la base de datos complete..."
kubectl wait --for=condition=complete --timeout=600s job/sqlserver-init-job -n elcriollo

# Verificar el resultado del Job
log "Verificando resultado de la inicialización..."
kubectl logs -n elcriollo job/sqlserver-init-job

# Paso 3: Construir imagen Docker (si es necesario)
if [ "$1" = "--build" ]; then
    log "Construyendo imagen Docker de El Criollo API para arquitectura x86_64..."
    cd ..
    
    # Verificar si Docker buildx está disponible para multi-platform builds
    if docker buildx version &> /dev/null; then
        log "Usando Docker buildx para compilación multi-plataforma..."
        docker buildx build --platform linux/amd64 -t cjoga/elcriollo-api:latest .
    else
        log "Usando Docker build estándar..."
        docker build --platform linux/amd64 -t cjoga/elcriollo-api:latest .
    fi
    
    # Push a registry (opcional)
    if [ "$2" = "--push" ]; then
        log "Subiendo imagen a Docker Hub..."
        docker push cjoga/elcriollo-api:latest
    fi
    cd k8s
fi

# Paso 4: Desplegar El Criollo API
log "Desplegando El Criollo API..."
kubectl apply -f elcriollo-api-deployment.yaml

# Esperar a que la API esté lista
log "Esperando a que El Criollo API esté listo..."
kubectl wait --for=condition=available --timeout=300s deployment/elcriollo-api-deployment -n elcriollo

# Verificar el estado del despliegue
log "Verificando estado del despliegue..."
kubectl get all -n elcriollo

# Mostrar información de los servicios
log "Información de servicios:"
kubectl get services -n elcriollo

# Mostrar información de ingress
log "Información de ingress:"
kubectl get ingressroute -n elcriollo

# Mostrar logs recientes de la API
log "Logs recientes de El Criollo API:"
kubectl logs -n elcriollo -l app=elcriollo-api --tail=20

# Información final
echo ""
echo "🎉 ¡Despliegue completado!"
echo ""
echo "📊 Información del despliegue:"
echo "  • Namespace: elcriollo"
echo "  • API URL: http://elcriolloapi.cjoga.cloud"
echo "  • Database: db.cjoga.cloud:1433"
echo "  • Swagger: http://elcriolloapi.cjoga.cloud/swagger"
echo "  • Health Check: http://elcriolloapi.cjoga.cloud/health"
echo ""
echo "🔍 Comandos útiles:"
echo "  • Ver pods: kubectl get pods -n elcriollo"
echo "  • Ver logs API: kubectl logs -n elcriollo -l app=elcriollo-api -f"
echo "  • Ver logs DB: kubectl logs -n elcriollo -l app=sqlserver -f"
echo "  • Ver logs Init Job: kubectl logs -n elcriollo job/sqlserver-init-job"
echo "  • Re-ejecutar Init Job: kubectl delete job sqlserver-init-job -n elcriollo && kubectl apply -f sqlserver-init-job.yaml"
echo "  • Escalar API: kubectl scale deployment elcriollo-api-deployment --replicas=3 -n elcriollo"
echo "  • Reiniciar API: kubectl rollout restart deployment elcriollo-api-deployment -n elcriollo"
echo ""
echo "🚀 ¡El Criollo POS System está listo para usar!" 