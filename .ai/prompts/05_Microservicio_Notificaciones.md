# Prompt 5: Microservicio de Notificaciones (Worker)

Crea un Worker Service ligero en .NET 8 enfocado exclusivamente en enviar correos.

## Requerimientos:
1. **Consumidor:** Usa MassTransit para escuchar la cola `notificaciones`.
2. **Email Provider:** Implementa el envío de correos utilizando la librería **MailKit**.
3. **Configuración:** Extrae las credenciales SMTP (Host, Port, User, Password) desde las variables de entorno o `appsettings.json` mediante el patrón Options.
4. **Persistencia:** Al confirmar el envío exitoso del correo, actualiza el estado de la tabla de trazabilidad `CargaArchivo` a `Notificado` invocando el Stored Procedure `sp_ActualizarEstadoCarga` (el mismo definido en el Microservicio de Carga Masiva) vía Dapper. Aplica inyección de dependencias para el repositorio que envuelve esta llamada.
5. **Resiliencia:** Si el envío de correo falla (SMTP no disponible), aplica Retry con Polly antes de descartar el mensaje; si persiste el fallo, registra el error mediante logging estructurado sin bloquear el consumo de otros mensajes.
