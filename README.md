# Atlantic City Casino Sports — Sistema de Cargas Masivas

> **Reto Técnico Backend Senior · Casino Atlantic City · 2026**

Sistema distribuido de microservicios para la carga asíncrona de datos masivos desde archivos Excel, con notificación por correo al finalizar. Construido con **.NET 8**, **RabbitMQ**, **SQL Server**, **SeaweedFS** y **React**.

---

## 📐 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Cliente React (puerto 3000)                  │
└────────────────────────────────┬────────────────────────────────────┘
                                 │ HTTP
┌────────────────────────────────▼────────────────────────────────────┐
│              API Gateway — YARP (puerto 5000)                       │
│         JWT validation · Rate Limiting · Reverse Proxy              │
└──────────┬──────────────────────────────────────┬───────────────────┘
           │                                      │
    ┌──────▼──────┐                      ┌────────▼────────┐
    │ Auth.Api    │                      │ Control.Api     │
    │ :5001       │                      │ :5002           │
    │ Login/JWT   │                      │ Subida Excel    │
    │ Refresh     │                      │ Historial       │
    └──────┬──────┘                      └────────┬────────┘
           │ EF Core                              │ Dapper + EF Core
    ┌──────▼──────┐                              │
    │  AuthDb     │                     ┌────────▼────────┐
    │  SQL Server │                     │   ControlDb     │
    └─────────────┘                     │   SQL Server    │
                                        └─────────────────┘
                                                 │ Publica mensaje
                                        ┌────────▼────────────────────┐
                                        │  RabbitMQ — cola:           │
                                        │  carga_masiva               │
                                        └────────┬────────────────────┘
                                                 │ Consume
                                        ┌────────▼────────┐
                                        │ CargaMasiva     │
                                        │ Worker          │
                                        │ Procesa Excel   │
                                        │ Bulk Insert     │
                                        └────────┬────────┘
                                                 │ Publica mensaje
                                        ┌────────▼────────────────────┐
                                        │  RabbitMQ — cola:           │
                                        │  notificaciones             │
                                        └────────┬────────────────────┘
                                                 │ Consume
                                        ┌────────▼────────┐
                                        │ Notificaciones  │
                                        │ Worker          │
                                        │ Envía correo    │
                                        │ (MailKit)       │
                                        └─────────────────┘
```

### Componentes

| Componente | Puerto | Responsabilidad |
|---|---|---|
| **API Gateway** | `5000` | Punto de entrada único. Valida JWT, aplica Rate Limiting (100 req/min por IP), enruta vía YARP |
| **Auth.Api** | `5001` | Login, generación de JWT Bearer + Refresh Token. EF Core con migraciones automáticas |
| **Control.Api** | `5002` | Recibe archivos Excel, valida extensión/tamaño/rol, guarda trazabilidad, sube a SeaweedFS, publica en cola `carga_masiva` |
| **CargaMasiva.Worker** | — | Consumidor de `carga_masiva`. Lee Excel, valida periodo/duplicados, Bulk Insert con Dapper, publica en `notificaciones` |
| **Notificaciones.Worker** | — | Consumidor de `notificaciones`. Envía correo con MailKit (Polly Retry), actualiza estado a `Notificado` |
| **RabbitMQ** | `5672` / `15672` | Broker de mensajería. 2 colas: `carga_masiva` y `notificaciones` |
| **SQL Server** | `1433` | Persistencia. 2 bases: `AuthDb` (usuarios, refresh tokens) y `ControlDb` (trazabilidad, datos procesados) |
| **SeaweedFS** | `8888` (filer) | Almacenamiento distribuido de archivos `.xlsx` subidos |
| **React SPA** | `3000` | Dashboard: Login, Subida de Excel, Historial con polling, Detalle de carga |

---

## 🛠️ Stack tecnológico

| Categoría | Tecnología |
|---|---|
| **Backend** | C# 12 · .NET 8 LTS · ASP.NET Core · Worker Service |
| **Arquitectura** | Clean Architecture · CQRS (MediatR) · Dependency Injection |
| **Base de datos** | SQL Server 2022 · EF Core 8 (migraciones) · Dapper (queries/bulk) · Stored Procedures |
| **Mensajería** | RabbitMQ 3 · MassTransit 9 |
| **Almacenamiento** | SeaweedFS |
| **Email** | MailKit 4 |
| **Resiliencia** | Polly 8 (Retry, Circuit Breaker) · MassTransit Retry |
| **Logging** | Serilog (Console sink, enriquecido por microservicio) |
| **Gateway** | YARP (Yet Another Reverse Proxy) · Rate Limiting (.NET 8) |
| **Seguridad** | JWT Bearer · BCrypt (hash de contraseñas) · Refresh Tokens |
| **Frontend** | React 18 · Vite · React Router v6 · Axios |
| **Contenedores** | Docker · Docker Compose |
| **Pruebas** | xUnit · Moq |

---

## 📁 Estructura del repositorio

```
RetoAC/
├── .env.example                          ← Plantilla de variables de entorno
├── docker-compose.yml                    ← Orquestación completa
├── RetoAC.slnx                          ← Solución Visual Studio
│
├── AtlanticCity.postman_collection.json  ← Colección Postman v2.1
├── AtlanticCity.postman_environment.json ← Variables de entorno Postman
│
├── database/
│   └── scripts/
│       └── 001_ControlDb_DDL.sql        ← DDL de referencia (tablas + SPs)
│
├── frontend/                            ← React SPA (Vite)
│   ├── Dockerfile
│   ├── src/
│   │   ├── context/AuthContext.jsx
│   │   ├── services/                    ← apiClient, authService, cargasService
│   │   ├── hooks/usePolling.js
│   │   ├── components/                  ← EstadoBadge, Navbar, ProtectedRoute
│   │   └── pages/                       ← Login, Historial, Subida, Detalle
│   └── .env.example
│
└── src/
    ├── Gateway/
    │   └── Gateway.Api/                 ← YARP + JWT + Rate Limiting
    │
    ├── Auth/
    │   ├── Auth.Api/                    ← Controllers V1, Middleware
    │   ├── Auth.Application/            ← Commands (Login, RefreshToken) + Handlers
    │   ├── Auth.Domain/                 ← Entities (User, RefreshToken)
    │   └── Auth.Infrastructure/         ← EF Core, BCrypt, JWT Generator, Seeder
    │
    ├── Control/
    │   ├── Control.Api/                 ← Controllers V1, Middleware
    │   ├── Control.Application/         ← Commands + Queries (CQRS)
    │   ├── Control.Domain/              ← Entities, Value Objects
    │   └── Control.Infrastructure/      ← Dapper repos, SeaweedFS client, MassTransit
    │
    ├── CargaMasiva/
    │   ├── CargaMasiva.Worker/          ← MassTransit consumer, Program.cs
    │   ├── CargaMasiva.Application/     ← ProcesarCargaService
    │   ├── CargaMasiva.Domain/          ← Reglas de negocio (periodo, duplicados)
    │   └── CargaMasiva.Infrastructure/  ← Dapper (bulk insert), SeaweedFS, Scripts SQL
    │
    └── Notificaciones/
        └── Notificaciones.Worker/       ← MailKit, MassTransit consumer, Dapper, Polly
│
└── tests/
    ├── Control.Tests/                   ← xUnit — handlers CQRS de Control
    └── CargaMasiva.Tests/               ← xUnit — ProcesarCargaService
```

---

## 🚀 Instrucciones de despliegue

### Requisitos previos

| Herramienta | Versión mínima |
|---|---|
| Docker Desktop | 4.x |
| .NET 8 SDK | 8.0.x (solo para desarrollo local) |
| Node.js | 20.x (solo si se corre el frontend fuera de Docker) |

### 1. Clonar el repositorio

```bash
git clone https://github.com/<tu-usuario>/RetoAC.git
cd RetoAC
```

### 2. Configurar variables de entorno

```bash
# Copia la plantilla y edita los valores
cp .env.example .env
```

Edita `.env` con tus valores reales. Como mínimo configura:

```env
ADMIN_EMAIL=tu-correo@gmail.com
SQLSERVER_SA_PASSWORD=TuContraseñaSegura#2026
RABBITMQ_PASSWORD=TuContraseñaSegura#2026
JWT_SECRET=UnSecretoLargoYSeguroDeAlMenos32Caracteres
SMTP_HOST=smtp.gmail.com
SMTP_USER=tu-correo@gmail.com
SMTP_PASSWORD=tu-app-password
SMTP_FROM_ADDRESS=tu-correo@gmail.com
```

### 3. Levantar todos los servicios

```bash
docker compose up --build
```

> **Primera vez:** tardará varios minutos en descargar las imágenes base. Las ejecuciones siguientes son mucho más rápidas gracias a la caché de Docker.

El comando levanta en orden:
1. SQL Server → RabbitMQ → SeaweedFS (infraestructura)
2. Auth.Api → Control.Api (microservicios API)
3. CargaMasiva.Worker → Notificaciones.Worker (workers)
4. Web Client React (servido por Nginx)

### 4. Migraciones automáticas de EF Core

Las migraciones **se aplican automáticamente** al iniciar cada microservicio en cualquier entorno. No es necesario ningún comando manual:

- **Auth.Api** aplica las migraciones de `AuthDb` y siembra el usuario de prueba al arrancar.
- **Control.Api** aplica las migraciones de `ControlDb` al arrancar.

> Si deseas aplicarlas manualmente en desarrollo:
> ```bash
> dotnet ef database update --project src/Auth/Auth.Infrastructure --startup-project src/Auth/Auth.Api
> dotnet ef database update --project src/Control/Control.Infrastructure --startup-project src/Control/Control.Api
> ```

### 5. URLs de acceso

| Servicio | URL | Notas |
|---|---|---|
| **React SPA** | http://localhost:3000 | Dashboard completo |
| **API Gateway** | http://localhost:5000 | Punto de entrada de la API |
| **RabbitMQ Management** | http://localhost:15672 | Usuario: `atlantic` / Pass: la de `.env` |
| **SeaweedFS Filer** | http://localhost:8888 | API de almacenamiento de archivos |
| **Auth API (directo)** | http://localhost:5001/swagger | Solo en desarrollo |
| **Control API (directo)** | http://localhost:5002/swagger | Solo en desarrollo |

### 6. Usuario de prueba (sembrado automáticamente)

| Campo | Valor |
|---|---|
| **Username** | `admin` |
| **Email** | El valor configurado en `ADMIN_EMAIL` dentro de `.env` |
| **Contraseña** | `Admin#2026` |
| **Rol** | `CargaMasiva.Ejecutor` |

---

## 🔄 Flujo de prueba end-to-end

```
1. Login
   └── POST /api/v1/auth/login  { username: "admin", password: "Admin#2026" }
       → Recibe JWT + RefreshToken

2. Subir Excel
   └── POST /api/v1/cargas  (multipart: archivo.xlsx)
       → Estado: Pendiente
       → Archivo guardado en SeaweedFS
       → Mensaje publicado en cola carga_masiva

3. Procesamiento asíncrono (CargaMasiva.Worker)
   └── Consume de cola carga_masiva
       → Estado: En proceso
       → Descarga Excel de SeaweedFS
       → Valida periodo y duplicados (CodigoProducto)
       → Bulk Insert en DataProcesada
       → Estado: Cargado → Finalizado
       → Publica en cola notificaciones

4. Notificación (Notificaciones.Worker)
   └── Consume de cola notificaciones
       → Envía correo con MailKit al usuario
       → Estado: Notificado

5. Consultar historial
   └── GET /api/v1/cargas?pageNumber=1&pageSize=20
       → El frontend actualiza el estado automáticamente via polling cada 6 segundos

6. Ver detalle
   └── GET /api/v1/cargas/{idCarga}
       → Muestra estado final: Notificado ✅
       └── GET /api/v1/cargas/{idCarga}/contenido
           → Descarga el archivo Excel original
```

### Estados posibles de una carga

```
Pendiente ──► En proceso ──► Cargado ──► Finalizado ──► Notificado
```

| Estado | Significado |
|---|---|
| `Pendiente` | Archivo recibido, esperando procesamiento |
| `En proceso` | Worker está procesando el Excel |
| `Cargado` | Datos insertados en BD exitosamente |
| `Finalizado` | Proceso completo, notificación enviada a cola |
| `Notificado` | Correo enviado al usuario ✅ |

---

## 🗄️ Scripts de base de datos

Los scripts de referencia se encuentran en [`/database/scripts/`](./database/scripts/):

| Script | Descripción |
|---|---|
| `001_ControlDb_DDL.sql` | DDL completo: tablas `CargaArchivo`, `DataProcesada`, `CargaFallida` + SPs `sp_ActualizarEstadoCarga` y `sp_ObtenerHistorialCargas` |

> **Nota:** Las migraciones de EF Core crean automáticamente las tablas `Users`, `RefreshTokens` y `CargaArchivo` al iniciar los servicios. Los scripts SQL son **respaldo documental** y referencia para revisión del evaluador.

### Tablas principales

```sql
-- AuthDb
Users           (Id, Username, Email, PasswordHash, Role, CreatedAt)
RefreshTokens   (Id, UserId, Token, ExpiresAt, IsRevoked)

-- ControlDb
CargaArchivo    (Id, NombreArchivo, Usuario, Periodo, Estado, FechaRegistro, FechaFin, RutaArchivo)
DataProcesada   (Id, IdCarga, CodigoProducto, Descripcion, Cantidad, Periodo, FechaRegistro)
CargaFallida    (Id, IdCarga, Motivo, Detalle, FechaRegistro)
```

---

## 📬 Colección Postman

Importar ambos archivos en Postman:

1. `AtlanticCity.postman_collection.json`
2. `AtlanticCity.postman_environment.json`

**Pasos:**
1. En Postman: **Import** → seleccionar ambos archivos
2. Activar el environment **"Atlantic City — Local"**
3. Ejecutar **`Auth > Login`** — el `token` y `refreshToken` se guardan automáticamente
4. Ejecutar **`Cargas > Subir Excel`** — el `idCarga` se guarda automáticamente
5. Ejecutar el resto en orden

Los scripts de test en cada request validan el status code y encadenan variables automáticamente.

---

## 🧪 Pruebas unitarias

```bash
# Ejecutar toda la suite
dotnet test

# Con detalle de cada test
dotnet test --verbosity normal

# Solo un proyecto
dotnet test tests/Control.Tests/
dotnet test tests/CargaMasiva.Tests/
```

Los tests cubren:
- `Control.Tests` — Handlers CQRS (`RegistrarCargaCommandHandler`) y validadores (`RegistrarCargaCommandValidator`)
- `CargaMasiva.Tests` — Servicio de procesamiento (`ProcesarCargaService`) con mocks de repositorios

---

## 🎥 Video demostrativo

> **Pendiente de grabación** — máx. 5 minutos mostrando el flujo completo:
>
> 1. **Login** en la SPA React con el usuario de prueba
> 2. **Subida** de un archivo Excel de ejemplo
> 3. **RabbitMQ Management UI** (`:15672`) mostrando el mensaje fluyendo entre colas
> 4. **Historial** actualizándose en tiempo real (polling visible)
> 5. **Correo recibido** con la notificación de finalización
> 6. **Estado final `Notificado`** en el detalle de la carga

---

## 🔐 Variables de entorno

Ver [`.env.example`](./.env.example) para la lista completa. Nunca commitear el archivo `.env` con valores reales.

---

## 📋 Criterios del reto cubiertos

| Criterio | Peso | Estado |
|---|---|---|
| Arquitectura (microservicios, Clean Architecture, CQRS, colas, estados) | 25% | ✅ |
| Funcionalidad (flujo completo, Excel real, persistencia, notificación) | 35% | ✅ |
| Docker / DevOps (docker-compose funcional, Dockerfiles por servicio) | 20% | ✅ |
| Frontend React + Colección Postman | 20% | ✅ |

### Extras implementados (valoran positivamente)
- ✅ Refresh Token
- ✅ Circuit Breaker y Retry con Polly
- ✅ Health Checks (`/health`) en todos los microservicios
- ✅ Idempotencia en consumidores MassTransit
- ✅ Logging estructurado con Serilog
- ✅ Rate Limiting en el Gateway
- ✅ Validación con FluentValidation
- ✅ Pruebas unitarias (xUnit + Moq)
- ✅ Dockerfile por microservicio
- ✅ Bulk Insert con Dapper para rendimiento masivo

---

*© 2026 Atlantic City Casino Sports — Reto Técnico Backend Senior*