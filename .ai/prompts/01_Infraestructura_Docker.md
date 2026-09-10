# Prompt 1: Infraestructura y Orquestación

Genera el archivo `docker-compose.yml` completo y robusto que orquesta la solución COMPLETA (sin bloques comentados, todo activo y funcional):

## Infraestructura base
1. **Base de Datos:** SQL Server (`mcr.microsoft.com/mssql/server:2022-latest`) con volumen persistente y variables `SA_PASSWORD`/`ACCEPT_EULA`.
2. **Mensajería:** RabbitMQ con su plugin de Management habilitado (`rabbitmq:3-management`). Configura credenciales por defecto y expón los puertos `5672` y `15672`.
3. **Almacenamiento:** SeaweedFS. Configura el Master y el Volume server necesarios para permitir la subida de archivos vía API (puerto `8888` para el filer/API HTTP).
4. **Red:** Define una red bridge personalizada `atlantic_network` para que todos los servicios se comuniquen internamente.

## Microservicios (todos activos, no comentados)
5. Agrega un bloque de servicio Docker para cada uno de los siguientes, cada uno con su propio `Dockerfile` (multi-stage build: SDK para compilar, runtime `aspnet:8.0` o `runtime:8.0` para el final), variables de entorno para cadenas de conexión (SQL Server, RabbitMQ, SeaweedFS) y `depends_on` con `condition: service_healthy` donde aplique:
   - `gateway` (API Gateway, expone puerto público, ej. `5000:8080`)
   - `auth-service` (Microservicio de Autenticación)
   - `control-service` (Microservicio de Control/Publicador)
   - `cargamasiva-service` (Worker consumidor/publicador)
   - `notificaciones-service` (Worker consumidor de notificaciones)
6. Si se implementa el frontend React, agrega también el servicio `web-client` (build multi-stage con Node para compilar y Nginx para servir estáticos), expuesto en el puerto `3000` o `80`.
7. Define `healthcheck` para SQL Server, RabbitMQ y SeaweedFS para que los `depends_on` de los microservicios esperen su disponibilidad real antes de iniciar.

## Dockerfiles
8. Genera además el `Dockerfile` individual para cada microservicio (`Gateway`, `Auth`, `Control`, `CargaMasiva`, `Notificaciones`), siguiendo el patrón multi-stage estándar de .NET 8 (`build` → `publish` → `runtime final`).

Agrega comentarios técnicos explicando las variables de entorno de cada servicio.
