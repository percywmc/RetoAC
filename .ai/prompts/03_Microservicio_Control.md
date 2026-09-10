# Prompt 3: Microservicio de Control (Publicador)

Genera el código para el Microservicio de Control en .NET 8 aplicando Clean Architecture y CQRS.

## Requerimientos:
1. **Endpoint de carga:** `POST /api/v1/cargas` (Requiere autorización JWT **con el rol `CargaMasiva.Ejecutor`** vía `[Authorize(Roles = "CargaMasiva.Ejecutor")]` o policy equivalente — si el usuario no tiene el rol, retorna `403 Forbidden`). Recibe un archivo `.xlsx`.
2. **Validaciones (Application Layer):**
   - Tamaño máximo del archivo.
   - Extensión correcta (`.xlsx`).
   - Usa `FluentValidation` en el pipeline de MediatR.
3. **Persistencia (Infrastructure Layer):**
   - Registra la auditoría en la tabla `CargaArchivo` con estado inicial `Pendiente` (incluye NombreArchivo, Usuario, FechaRegistro, Periodo si se envía). Utiliza un patrón de Repositorio.
4. **Integración Externa:**
   - Sube el archivo a **SeaweedFS** usando `HttpClient` (implementa `IHttpClientFactory` y políticas de Polly para Circuit Breaker y Retry).
5. **Mensajería:**
   - Publica un evento de integración `CargaRegistradaEvent` (idCarga, rutaArchivo, usuario) en la cola `carga_masiva`. Utiliza `MassTransit` para la publicación en RabbitMQ.
6. **Endpoint de historial de cargas (Query CQRS):**
   - `GET /api/v1/cargas` — lista paginada del historial de cargas del usuario autenticado (o de todos si es Admin), mostrando Id, NombreArchivo, Estado, FechaRegistro, FechaFin.
   - `GET /api/v1/cargas/{id}` — detalle de una carga específica con su estado actual.
7. **Endpoint de consulta de contenido del Excel subido:**
   - `GET /api/v1/cargas/{id}/contenido` — descarga o previsualiza (ej. como JSON de filas) el archivo Excel original desde SeaweedFS asociado a esa carga, para que el cliente web pueda mostrarlo.
