# Prompt 2: API Gateway y Microservicio de Autenticación

## Parte 1: API Gateway
Implementa un API Gateway en .NET 8 (puedes usar YARP - Yet Another Reverse Proxy).
- Configura el enrutamiento hacia los microservicios (`/api/v1/auth/*` hacia Auth, `/api/v1/cargas/*` hacia Control), respetando la regla general de versionado de API.
- Implementa un middleware global de validación JWT (Bearer).
- Implementa el patrón Rate Limiting usando las primitivas de .NET 8+ (`System.Threading.RateLimiting`).

## Parte 2: Microservicio de Autenticación
Diseña un microservicio aplicando Clean Architecture.
- **Endpoint:** `POST /api/v1/auth/login` y `POST /api/v1/auth/refresh` (**obligatorio implementar ambos**, no opcional).
- **Lógica:** Implementa un `CommandHandler` de CQRS para validar credenciales. Usa un **Generic Repository** contra una tabla `Users` real en SQL Server (con datos semilla vía migración EF Core) — **no mockear** la fuente de identidad.
- **Refresh Token:** Persiste el refresh token (tabla `RefreshTokens`: Id, UserId, Token, FechaExpiracion, Revocado) y valida su vigencia/rotación en `/api/v1/auth/refresh`.
- **Seguridad:** Generación de token JWT simétrico con claims básicos (UserId, Email, Roles). Usa `IOptions` para inyectar la configuración del JWT desde el `appsettings.json`. El claim `Roles` debe incluir al menos un rol (ej. `CargaMasiva.Ejecutor`) que el Microservicio de Control usará para autorización.
