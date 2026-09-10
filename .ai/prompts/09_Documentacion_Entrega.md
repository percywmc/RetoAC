```markdown
# Prompt 9: Documentación y Entrega Final

Genera el `README.md` raíz del repositorio y los artefactos de entrega solicitados por el reto.

## README.md — secciones requeridas
1. **Descripción del proyecto** — resumen del sistema y objetivo del reto.
2. **Arquitectura** — incrusta o referencia el diagrama [arquitectura.mmd](../docs/arquitectura.mmd) y explica brevemente cada componente (Gateway, Auth, Control, Carga Masiva, Notificaciones, RabbitMQ, SeaweedFS, SQL Server).
3. **Stack tecnológico** — .NET 8, SQL Server, RabbitMQ (MassTransit), SeaweedFS, MailKit, React, Docker.
4. **Estructura del repositorio** — árbol de carpetas de la solución (cada microservicio con sus capas Domain/Application/Infrastructure/Presentation).
5. **Instrucciones de despliegue:**
   - Requisitos previos (Docker Desktop, .NET 8 SDK, Node.js si se corre el frontend fuera de Docker).
   - Comando `docker compose up --build` y explicación de qué levanta.
   - Cómo se aplican las migraciones automáticas de EF Core al iniciar cada microservicio.
   - URLs de acceso: Gateway, RabbitMQ Management UI, SeaweedFS, frontend React.
   - Usuario/contraseña de prueba sembrados en la migración inicial.
6. **Flujo de prueba end-to-end** — pasos numerados: login → subir Excel → ver historial → ver detalle → recibir correo → estado `Notificado`.
7. **Scripts de base de datos** — referencia a la carpeta `/database/scripts` con el DDL de tablas y Stored Procedures, indicando que las migraciones EF Core los aplican automáticamente.
8. **Colección Postman** — referencia a los archivos generados en el Prompt 7 y cómo importarlos.
9. **Pruebas unitarias** — comando `dotnet test` para ejecutar toda la suite.
10. **Video demostrativo** — placeholder con instrucciones de qué debe mostrar el video (máx. 5 min: login, subida, procesamiento asíncrono visible en RabbitMQ Management, historial actualizándose, correo recibido).

## Artefactos adicionales a generar
- Carpeta `/database/scripts/` con el DDL de referencia de todas las tablas (`CargaArchivo`, `RefreshTokens`, `Users`, `DataProcesada`, `CargaFallida`) y los Stored Procedures (`sp_ActualizarEstadoCarga`, `sp_ObtenerHistorialCargas`), como respaldo documental además de las migraciones EF Core.
- Archivo `.env.example` en la raíz con todas las variables de entorno necesarias (cadenas de conexión, credenciales RabbitMQ, SMTP, JWT secret) sin valores sensibles reales.
```
