# 🇩🇴 El Criollo Frontend - Sistema POS

![React](https://img.shields.io/badge/React-18.2-61DAFB?style=for-the-badge&logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-5.3-3178C6?style=for-the-badge&logo=typescript)
![Vite](https://img.shields.io/badge/Vite-5.0-646CFF?style=for-the-badge&logo=vite)
![Tailwind CSS](https://img.shields.io/badge/Tailwind-3.3-06B6D4?style=for-the-badge&logo=tailwindcss)

Sistema de Punto de Venta (POS) Frontend para el Restaurante Dominicano **El Criollo**. Desarrollado con React, TypeScript y Tailwind CSS, conectando con la API backend en `https://elcriolloapi.cjoga.cloud`.

## 🎯 Características Principales

- ✅ **Autenticación JWT** con refresh tokens automáticos
- ✅ **Sistema de roles** (Administrador, Cajero, Mesero, Recepción, Cocina)
- ✅ **Identidad dominicana** con colores de la bandera y elementos culturales
- ✅ **Responsive design** mobile-first con Tailwind CSS
- ✅ **TypeScript completo** para type safety
- ✅ **Interceptors de Axios** para manejo automático de tokens
- ✅ **Rutas protegidas** por rol de usuario
- ✅ **Toast notifications** para feedback del usuario
- ✅ **Context API** para estado global simple pero efectivo

## 🚀 Inicio Rápido

### Prerrequisitos

- **Node.js** >= 18.0.0
- **npm** >= 9.0.0
- Conexión a internet para acceder a la API

### Instalación

```bash
# 1. Clonar el repositorio
git clone <tu-repositorio-url>
cd el-criollo-frontend

# 2. Instalar dependencias
npm install

# 3. Iniciar servidor de desarrollo
npm run dev

# 4. Abrir navegador en http://localhost:3000
```

### Credenciales Demo

Para probar el sistema, usa las credenciales del administrador:

```
Usuario: thecuevas0123_
Contraseña: thepikachu0123_
```

## 📁 Estructura del Proyecto

```
src/
├── 📁 components/           # Componentes reutilizables
│   ├── 📁 auth/            # Componentes de autenticación
│   │   ├── LoginForm.tsx   # Formulario de login
│   │   └── ProtectedRoute.tsx # Rutas protegidas
│   ├── 📁 ui/              # Componentes básicos de UI
│   │   ├── Button.tsx      # Botón con tema dominicano
│   │   ├── Input.tsx       # Input con validaciones
│   │   ├── Card.tsx        # Cards con diseño caribeño
│   │   ├── Modal.tsx       # Modales responsivos
│   │   └── Badge.tsx       # Badges para estados
│   └── 📁 layout/          # Componentes de layout
├── 📁 contexts/            # Context providers
│   └── AuthContext.tsx     # Estado global de autenticación
├── 📁 hooks/              # Custom hooks
├── 📁 pages/              # Páginas principales
│   ├── LoginPage.tsx      # Página de login
│   └── DashboardPage.tsx  # Dashboard principal
├── 📁 services/           # Servicios de API
│   ├── api.ts             # Cliente Axios configurado
│   └── authService.ts     # Servicios de autenticación
├── 📁 types/              # Tipos de TypeScript
│   └── index.ts           # Tipos principales del sistema
├── 📁 utils/              # Utilidades y helpers
├── 📁 styles/             # Estilos globales
│   └── globals.css        # CSS global con tema dominicano
└── 📁 assets/             # Imágenes, iconos, etc.
```

## 🎨 Diseño y Tema Dominicano

### Paleta de Colores

```css
/* Colores de la bandera dominicana */
--dominican-red: #cf142b;
--dominican-blue: #002d62;
--dominican-white: #ffffff;

/* Colores complementarios del Caribe */
--caribbean-gold: #ffd700;
--palm-green: #228b22;
--sunset-orange: #ff6b35;

/* Neutros cálidos */
--stone-gray: #6b7280;
--warm-beige: #f5f5dc;
```

### Tipografía

- **Headings**: Poppins (para títulos elegantes)
- **Body**: Inter (para texto legible)
- **Monospace**: JetBrains Mono (para código)

## 🔐 Sistema de Autenticación

### Flujo de Autenticación

1. **Login** → Envío de credenciales a `/api/auth/login`
2. **Recepción de tokens** → JWT + Refresh Token
3. **Almacenamiento** → LocalStorage seguro
4. **Auto-renovación** → Interceptor de Axios renueva tokens expirados
5. **Logout** → Limpieza de tokens y redirección

### Roles Implementados

| Rol               | Permisos      | Funcionalidades                                |
| ----------------- | ------------- | ---------------------------------------------- |
| **Administrador** | Control total | Gestión de usuarios, reportes, configuración   |
| **Cajero**        | Facturación   | Procesar pagos, generar facturas, inventario   |
| **Mesero**        | Órdenes       | Tomar órdenes, gestionar mesas, productos      |
| **Recepción**     | Reservas      | Gestión de reservaciones y atención al cliente |
| **Cocina**        | Preparación   | Órdenes de cocina, control de inventario       |

## 📡 Conexión con la API

### Configuración

La aplicación se conecta automáticamente a:

```
Base URL: https://elcriolloapi.cjoga.cloud/api
```

### Interceptors Configurados

- **Request**: Añade automáticamente token JWT
- **Response**: Maneja errores HTTP y renueva tokens expirados
- **Error Handling**: Toast notifications para errores comunes

### Endpoints Principales Utilizados

```typescript
// Autenticación
POST / auth / login; // Login de usuario
POST / auth / refresh; // Renovar token
POST / auth / logout; // Cerrar sesión
GET / auth / me; // Obtener usuario actual

// Dashboard (próximamente)
GET / reporte / dashboard; // Métricas del dashboard
```

## 🛠️ Scripts Disponibles

```bash
# Desarrollo
npm run dev              # Servidor de desarrollo (puerto 3000)
npm run type-check       # Verificar tipos de TypeScript

# Construcción
npm run build           # Build para producción
npm run preview         # Preview del build

# Calidad de código
npm run lint            # Linter ESLint
npm run lint:fix        # Fix automático de ESLint
npm run format          # Formatear con Prettier
npm run format:check    # Verificar formato

# Testing (próximamente)
npm run test            # Ejecutar pruebas
npm run test:ui         # UI de pruebas
npm run coverage        # Cobertura de código

# Utilidades
npm run clean           # Limpiar node_modules y dist
npm run reinstall       # Reinstalar dependencias
npm run analyze         # Analizar bundle
```

## 🧪 Testing (Planificado)

El proyecto está configurado para testing con:

- **Vitest** para unit tests
- **React Testing Library** para testing de componentes
- **Jest DOM** para matchers adicionales

## 📱 Responsive Design

### Breakpoints

```css
/* Mobile First Approach */
sm: 640px   /* Tablets pequeñas */
md: 768px   /* Tablets */
lg: 1024px  /* Desktop pequeño */
xl: 1280px  /* Desktop */
2xl: 1536px /* Desktop grande */
```

### Adaptaciones

- **Móvil**: Stack vertical, navegación tipo hamburger
- **Tablet**: Layout híbrido, sidebar colapsable
- **Desktop**: Layout completo con sidebar fijo

## 🔧 Configuración Personalizada

### Variables de Entorno (Opcional)

Crea un archivo `.env.local` para configuraciones personalizadas:

```env
# URL de la API (opcional, por defecto usa la URL pública)
VITE_API_BASE_URL=https://elcriolloapi.cjoga.cloud/api

# Título de la aplicación
VITE_APP_TITLE=El Criollo POS

# Entorno
VITE_NODE_ENV=development
```

### Personalizar Tema

Edita `tailwind.config.js` para personalizar colores y estilos:

```javascript
// Cambiar colores principales
theme: {
  extend: {
    colors: {
      'dominican-red': '#TU_COLOR',
      'dominican-blue': '#TU_COLOR',
      // ...
    }
  }
}
```

## 🚀 Deployment

### Build para Producción

```bash
# Crear build optimizado
npm run build

# Previsualizar build
npm run preview
```

### Optimizaciones Incluidas

- ✅ **Code splitting** automático por rutas
- ✅ **Tree shaking** para eliminar código no usado
- ✅ **Minificación** con Terser
- ✅ **Compresión** de assets
- ✅ **Source maps** para debugging
- ✅ **Vendor chunks** separados para mejor caching

### Hosting Recomendado

- **Vercel** (recomendado para React)
- **Netlify**
- **GitHub Pages**
- **Firebase Hosting**

## 🐛 Troubleshooting

### Problemas Comunes

#### Error de conexión a la API

```bash
# Verificar que la API esté disponible
curl https://elcriolloapi.cjoga.cloud/api/auth/validate-token

# Verificar configuración de red
npm run dev -- --host 0.0.0.0
```

#### Errores de TypeScript

```bash
# Limpiar caché de TypeScript
rm -rf node_modules/.cache
npm run type-check
```

#### Problemas de dependencias

```bash
# Reinstalar dependencias
npm run reinstall

# Verificar versiones
npm list --depth=0
```

### Logs de Debugging

En desarrollo, el sistema mostrará logs detallados en la consola:

- 🚀 Requests a la API
- ✅ Responses exitosos
- ❌ Errores y sus detalles
- 🔄 Renovación de tokens

## 📞 Soporte

### Contacto

- **Email**: josejoga.opx@gmail.com
- **Proyecto**: Sistema POS El Criollo
- **Tipo**: Proyecto universitario

### Reportar Issues

1. Verifica que no exista un issue similar
2. Incluye información del entorno (OS, Node.js, navegador)
3. Proporciona pasos para reproducir el error
4. Adjunta logs relevantes

## 🎓 Contexto Académico

Este es un **proyecto universitario** que demuestra:

- ✅ Desarrollo Frontend moderno con React + TypeScript
- ✅ Integración con API REST
- ✅ Sistema de autenticación robusto
- ✅ Diseño responsive y accesible
- ✅ Gestión de estado global
- ✅ Buenas prácticas de desarrollo
- ✅ Identidad cultural dominicana

### Objetivos Cumplidos

- [x] **Login funcional** con credenciales del backend
- [x] **Sistema de roles** implementado
- [x] **Diseño responsivo** con identidad dominicana
- [x] **Conexión API** con interceptors y error handling
- [x] **TypeScript** para type safety
- [x] **Arquitectura escalable** y mantenible

### Próximos Pasos

- [ ] Implementar módulos específicos por rol
- [ ] Agregar más componentes de UI
- [ ] Implementar testing completo
- [ ] Optimizaciones de performance
- [ ] PWA features

## 📄 Licencia

MIT License - Proyecto universitario con fines educativos.

---

🇩🇴 **Desarrollado con sabor dominicano para El Criollo Restaurant** ☕
