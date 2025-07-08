# 🇩🇴 El Criollo - Despliegue en k3s

Esta documentación describe cómo desplegar El Criollo POS System en un cluster k3s.

## 📋 Requisitos Previos

- Cluster k3s funcionando en arquitectura **x86_64/AMD64**
- kubectl configurado
- Docker instalado (para construir imágenes)
- Docker buildx habilitado (recomendado para builds multi-plataforma)
- Acceso a Docker Hub (opcional, para subir imágenes)
- Cloudflare Tunnel configurado

## 🏗️ Arquitectura de Despliegue

```
┌─────────────────────────────────────────────────────────────┐
│                    Internet                                 │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                Cloudflare Tunnel                            │
│  elcriolloapi.cjoga.cloud → API                            │
│  db.cjoga.cloud → Database                                  │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                k3s Cluster                                  │
│                                                             │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │   El Criollo    │    │   SQL Server    │                │
│  │      API        │◄──►│   Database      │                │
│  │   (2 replicas)  │    │   (1 replica)   │                │
│  └─────────────────┘    └─────────────────┘                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Estructura de Archivos

```
k8s/
├── namespace.yaml                 # Namespace para El Criollo
├── sqlserver-deployment.yaml     # Despliegue de SQL Server
├── sqlserver-init-job.yaml       # Job de inicialización de BD
├── elcriollo-api-deployment.yaml # Despliegue de la API
├── cloudflare-tunnel-config.yaml # Configuración del tunnel
├── deploy.sh                     # Script de despliegue
└── README.md                     # Esta documentación
```

## 🚀 Despliegue Rápido

### Opción 1: Usar imagen pre-construida

```bash
# Clonar el repositorio
git clone <repository-url>
cd elcriollo-restaurant/k8s

# Hacer ejecutable el script
chmod +x deploy.sh

# Desplegar
./deploy.sh
```

### Opción 2: Construir imagen localmente

```bash
# Construir para arquitectura x86_64 y desplegar
./deploy.sh --build

# Construir, subir a Docker Hub y desplegar
./deploy.sh --build --push

# Verificar que Docker buildx esté disponible (recomendado)
docker buildx version
```

## 🔧 Configuración Detallada

### 1. Variables de Entorno

La API utiliza las siguientes variables de entorno configuradas en `elcriollo-api-deployment.yaml`:

#### Configuración de Base de Datos

```yaml
ConnectionStrings__DefaultConnection: "Server=sqlserver-service,1433;Database=ElCriolloRestaurante;User Id=sa;Password=ElCriollo2024!;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=false;"
```

**Nota**: `Encrypt=false` y `TrustServerCertificate=true` son necesarios para SQL Server 2022 con certificados auto-firmados.

#### Configuración JWT

```yaml
JwtSettings__SecretKey: "ElCriollo2024_Production_SecretKey_SuperSecure_MinimumOf32Characters!"
JwtSettings__Issuer: "ElCriolloAPI"
JwtSettings__Audience: "ElCriolloClients"
JwtSettings__ExpiryInMinutes: "60"
```

#### Configuración de Email

```yaml
EmailSettings__SmtpServer: "smtp.gmail.com"
EmailSettings__SmtpPort: "587"
EmailSettings__EnableSsl: "true"
EmailSettings__Username: "josejoga.opx@gmail.com"
EmailSettings__Password: "udimvqxfjjkweubt"
```

### 2. Recursos Asignados

#### API (2 replicas)

```yaml
resources:
  requests:
    memory: "512Mi"
    cpu: "250m"
  limits:
    memory: "1Gi"
    cpu: "500m"
```

#### SQL Server (1 replica)

```yaml
resources:
  requests:
    memory: "1Gi"
    cpu: "500m"
  limits:
    memory: "2Gi"
    cpu: "1000m"
```

### 3. Almacenamiento

- **SQL Server**: 10Gi de almacenamiento persistente usando `local-path`
- **API Logs**: Volumen efímero (`emptyDir`)

## 🌐 Acceso a los Servicios

### URLs Públicas (a través de Cloudflare Tunnel)

- **API Principal**: `https://elcriolloapi.cjoga.cloud`
- **Swagger Documentation**: `https://elcriolloapi.cjoga.cloud/swagger`
- **Health Check**: `https://elcriolloapi.cjoga.cloud/health`
- **Base de Datos**: `db.cjoga.cloud:1433` (para administración)

### Acceso Interno (dentro del cluster)

- **API Service**: `elcriollo-api-service.elcriollo.svc.cluster.local:80`
- **Database Service**: `sqlserver-service.elcriollo.svc.cluster.local:1433`

## 🔍 Comandos de Monitoreo

### Ver el estado del despliegue

```bash
kubectl get all -n elcriollo
```

### Ver logs de la API

```bash
kubectl logs -n elcriollo -l app=elcriollo-api -f
```

### Ver logs de la base de datos

```bash
kubectl logs -n elcriollo -l app=sqlserver -f
```

### Ver logs del Job de inicialización

```bash
kubectl logs -n elcriollo job/sqlserver-init-job
```

### Re-ejecutar inicialización de base de datos

```bash
# Eliminar Job anterior y crear uno nuevo
kubectl delete job sqlserver-init-job -n elcriollo
kubectl apply -f sqlserver-init-job.yaml

# Esperar a que complete
kubectl wait --for=condition=complete --timeout=600s job/sqlserver-init-job -n elcriollo
```

### Escalar la API

```bash
kubectl scale deployment elcriollo-api-deployment --replicas=3 -n elcriollo
```

### Reiniciar la API

```bash
kubectl rollout restart deployment elcriollo-api-deployment -n elcriollo
```

### Acceder a un pod de la API

```bash
kubectl exec -it -n elcriollo deployment/elcriollo-api-deployment -- /bin/bash
```

### Acceder a SQL Server

```bash
kubectl exec -it -n elcriollo deployment/sqlserver-deployment -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'ElCriollo2024!' -C
```

## 🛠️ Troubleshooting

### 1. La API no se conecta a la base de datos

```bash
# Verificar que SQL Server esté ejecutándose
kubectl get pods -n elcriollo -l app=sqlserver

# Verificar logs de SQL Server
kubectl logs -n elcriollo -l app=sqlserver

# Verificar conectividad desde la API
kubectl exec -it -n elcriollo deployment/elcriollo-api-deployment -- nc -zv sqlserver-service 1433
```

### 2. La API no responde

```bash
# Verificar estado de los pods
kubectl get pods -n elcriollo -l app=elcriollo-api

# Verificar logs de la API
kubectl logs -n elcriollo -l app=elcriollo-api

# Verificar health check
kubectl exec -it -n elcriollo deployment/elcriollo-api-deployment -- curl -f http://localhost:8080/health
```

### 3. Problemas de almacenamiento

```bash
# Verificar PVC
kubectl get pvc -n elcriollo

# Verificar PV
kubectl get pv

# Describir PVC para ver eventos
kubectl describe pvc sqlserver-data-pvc -n elcriollo
```

### 4. Problemas de red/ingress

```bash
# Verificar servicios
kubectl get svc -n elcriollo

# Verificar ingress routes
kubectl get ingressroute -n elcriollo

# Verificar middleware
kubectl get middleware -n elcriollo
```

### 5. Problemas SSL con SQL Server 2022

Si encuentras errores SSL como "certificate verify failed", usa el parámetro `-C`:

```bash
# Conectar a SQL Server deshabilitando verificación SSL
kubectl exec -it -n elcriollo deployment/sqlserver-deployment -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'ElCriollo2024!' -C -Q "SELECT @@VERSION"

# Verificar conectividad desde la API
kubectl exec -it -n elcriollo deployment/elcriollo-api-deployment -- nc -zv sqlserver-service 1433
```

## 🔐 Credenciales por Defecto

### API Admin User

- **Username**: `thecuevas0123_`
- **Password**: `thepikachu0123_`
- **Email**: `josejoga.opx@gmail.com`

### SQL Server

- **Usuario**: `sa`
- **Contraseña**: `ElCriollo2024!`
- **Base de Datos**: `ElCriolloRestaurante`
- **Nota**: Usar parámetro `-C` en sqlcmd para deshabilitar verificación SSL

## 📊 Métricas y Monitoreo

### Health Checks Configurados

- **API Health Check**: `GET /health`
- **SQL Server Health Check**: `sqlcmd -Q "SELECT 1"`

### Probes Configuradas

- **Liveness Probe**: Verifica que la aplicación esté viva
- **Readiness Probe**: Verifica que la aplicación esté lista para recibir tráfico

## 🔄 Actualizaciones

### Actualizar la imagen de la API

```bash
# Construir nueva imagen
docker build -t cjoga/elcriollo-api:v1.1 .

# Subir a Docker Hub
docker push cjoga/elcriollo-api:v1.1

# Actualizar deployment
kubectl set image deployment/elcriollo-api-deployment elcriollo-api=cjoga/elcriollo-api:v1.1 -n elcriollo

# Verificar rollout
kubectl rollout status deployment/elcriollo-api-deployment -n elcriollo
```

### Actualizar configuración

```bash
# Editar ConfigMap
kubectl edit configmap elcriollo-api-config -n elcriollo

# Reiniciar deployment para aplicar cambios
kubectl rollout restart deployment elcriollo-api-deployment -n elcriollo
```

## 🗑️ Limpieza

### Eliminar todo el despliegue

```bash
# Eliminar namespace (esto eliminará todo)
kubectl delete namespace elcriollo

# O eliminar recursos individualmente
kubectl delete -f elcriollo-api-deployment.yaml
kubectl delete -f sqlserver-deployment.yaml
kubectl delete -f namespace.yaml
```

### Limpiar volúmenes persistentes

```bash
# Listar PV
kubectl get pv

# Eliminar PV específico (si es necesario)
kubectl delete pv <pv-name>
```

## 📞 Soporte

Para problemas específicos del despliegue:

1. Verificar logs de los pods
2. Revisar eventos del namespace: `kubectl get events -n elcriollo`
3. Verificar recursos disponibles: `kubectl top nodes`
4. Contactar al equipo de desarrollo

## 🎯 Próximos Pasos

- [ ] Configurar monitoreo con Prometheus
- [ ] Implementar backup automático de la base de datos
- [ ] Configurar alertas
- [ ] Implementar CI/CD pipeline
- [ ] Configurar SSL/TLS para la base de datos
