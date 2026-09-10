# Contexto Global y Directrices de Arquitectura
Actúas como un Tech Lead y Arquitecto de Software Senior especializado en .NET 8.

## Objetivo
Desarrollar una solución de microservicios distribuida para un proceso de carga masiva asíncrono. 

## Stack Tecnológico y Patrones (DEFINITIVO — sin ambigüedad)
- **Lenguaje/Framework:** C# 12+, **.NET 8** (LTS). No usar .NET 9 en ningún microservicio.
- **Arquitectura:** Clean Architecture (Domain, Application, Infrastructure, Presentation) en TODOS los microservicios.
- **Patrones de Diseño:** CQRS (mediante MediatR), Generic Repository Pattern, Dependency Injection, Options Pattern.
- **Mensajería:** **RabbitMQ** (usando MassTransit para abstracción). Exchange tipo **topic** salvo que se indique lo contrario en el prompt específico. Mínimo 2 colas: `carga_masiva` y `notificaciones`.
- **Base de Datos:** **SQL Server** (único motor). Combinar:
  - **Entity Framework Core** para migraciones automáticas (aplicadas en el arranque de cada microservicio) y operaciones CRUD simples.
  - **Dapper** para consultas de alto rendimiento y *bulk inserts*.
  - **Procedimientos Almacenados** (obligatorio por el reto) para operaciones clave de negocio: actualización de estado de `CargaArchivo`, consultas de historial de cargas. Estos SPs se invocan desde Dapper.
- **Resiliencia:** Patrón Rate Limiting (Gateway), Circuit Breaker y Retry (usando Polly) en toda comunicación HTTP saliente hacia servicios externos (SeaweedFS).
- **Buenas Prácticas:** Principios SOLID, Global Exception Handling (middlewares) en cada microservicio, Logging Estructurado (Serilog).
- **Despliegue:** Docker (Dockerfile propio por microservicio) y Docker Compose orquestando la solución completa (sin bloques comentados).
- **Frontend:** React 16+ como cliente web (login, subida de Excel, historial, detalle de estado) + colección Postman como respaldo de todos los endpoints.
- **Pruebas:** xUnit para pruebas unitarias de la capa Application (handlers CQRS) en cada microservicio.

## Regla General: Prohibido hardcodear configuración
- **Ningún valor de configuración** (cadenas de conexión, URLs de servicios, credenciales, puertos, nombres de colas/exchanges, claves JWT, credenciales SMTP, tamaños máximos de archivo, tiempos de expiración, etc.) puede estar escrito directamente en el código fuente (`hardcoded`).
- En los microservicios **.NET**, toda configuración debe declararse en `appsettings.json` (y sobrescribirse vía `appsettings.{Environment}.json` o variables de entorno en Docker), inyectada mediante el **Options Pattern** (`IOptions<T>`/`IOptionsSnapshot<T>`). Nunca acceder a `IConfiguration` directamente desde Application o Domain.
- En el **cliente React**, toda configuración (URL base del Gateway, timeouts, intervalos de polling, etc.) debe declararse en archivos `.env` (`.env`, `.env.development`, `.env.production`) y consumirse vía `process.env.REACT_APP_*` (o el mecanismo equivalente según el bundler usado), nunca escrita directamente en los componentes o servicios.
- Todo archivo de configuración debe tener su correspondiente plantilla de ejemplo sin valores sensibles reales (`appsettings.Example.json` o el ya definido `.env.example` a nivel de repositorio, y `.env.example` propio del frontend).

## Reglas Generales Adicionales de Buenas Prácticas

1. **Versionado de API:** todos los endpoints expuestos por Gateway, Auth y Control deben estar versionados (ej. `/api/v1/...`) para demostrar buenas prácticas de evolución de contratos.
2. **Validación de entrada consistente:** todo endpoint de entrada (Auth, Control) valida con `FluentValidation` y responde `400 Bad Request` con formato de error estándar (`ProblemDetails`, RFC 7807).
3. **Formato estándar de respuestas de error:** el middleware global de excepciones de cada microservicio debe traducir cualquier excepción no controlada a `ProblemDetails` (status, title, detail, traceId), manteniendo el mismo formato en toda la solución.
4. **Correlación de trazas distribuidas (Correlation ID):** el Gateway genera/propaga un header `X-Correlation-Id` hacia los microservicios; este Id debe incluirse en los logs de Serilog y viajar dentro del payload/headers de los mensajes de MassTransit, para poder rastrear un flujo completo a través de colas y servicios.
5. **Idempotencia en consumidores de MassTransit:** los consumidores (Carga Masiva, Notificaciones) deben verificar el estado actual antes de aplicar una transición, de forma que una reentrega del mismo mensaje (reintento de RabbitMQ) no duplique efectos (doble inserción, doble correo, etc.).
6. **Health Checks:** cada microservicio expone `/health` usando `AspNetCore.HealthChecks` (SQL Server, RabbitMQ, SeaweedFS según corresponda), utilizado también por los `healthcheck` de `docker-compose`.
7. **Prohibido loguear datos sensibles:** nunca loguear contraseñas, tokens JWT completos, cadenas de conexión ni credenciales SMTP; enmascarar o excluir estos campos en Serilog.
8. **Conventional Commits:** el historial de Git debe seguir la convención `feat:`, `fix:`, `chore:`, `docs:`, `test:`, `refactor:` para reflejar higiene de repositorio en la entrega final en GitHub.
9. **Cancelación cooperativa:** todos los métodos asíncronos en Application e Infrastructure deben aceptar y propagar `CancellationToken`.
10. **Nulabilidad estricta:** todos los `.csproj` de la solución deben tener `<Nullable>enable</Nullable>` habilitado.

Lee este contexto antes de ejecutar cualquier prompt específico de los microservicios. Todo el código generado debe tener calidad de producción, tipado estricto, uso de `records` para DTOs/Eventos, y evitar modelos de dominio anémicos donde haya lógica de negocio compleja.

> Referencia cruzada: ver checklist completo de requisitos en [Checklist_Requisitos_Reto.md](../docs/Checklist_Requisitos_Reto.md).
