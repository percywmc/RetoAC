# Prompt 4: Microservicio de Carga Masiva (Worker/Consumidor)

Crea un Worker Service en .NET 8 que actúe como consumidor principal. Este es el core del negocio.

## Implementación de Consumo (MassTransit):
- Escucha la cola `carga_masiva`.
- Actualiza el estado del registro a `En proceso` (vía Stored Procedure, ver más abajo).
- Descarga el archivo desde SeaweedFS usando la ruta recibida.

## Lógica de Negocio y Procesamiento (Domain & Application Layers):
Utiliza `ExcelDataReader` o `ClosedXML` para leer el archivo. Aplica las siguientes reglas estrictas:
1. **Validación de Periodo:**
   - Consulta si existe una carga previa para la columna `Periodo`.
   - Si existe con estado `Cargado`, `Finalizado` o `Notificado` -> RECHAZAR la carga (no continuar el procesamiento).
   - Si existe con estado `Pendiente` o `En proceso` -> BLOQUEAR la carga (evitar concurrencia, reintentar o descartar el mensaje según política).
   - Si no hay conflicto, continuar el procesamiento normalmente.
   - Registra los rechazos/bloqueos en la tabla de auditoría `CargaFallida` (Id, IdCarga, Motivo, Detalle, FechaRegistro).
2. **Validación de Registros (CodigoProducto):**
   - Evita registrar filas cuyo `CodigoProducto` ya exista en la base de datos; repórtalas como `Existente` en `CargaFallida`.
   - Si un campo está vacío, asigna un valor por defecto.
   - Ignora filas completamente vacías.

## Persistencia de Alto Rendimiento:
- Utiliza **Dapper** para hacer un *Bulk Insert* de los registros válidos extraídos del Excel hacia la tabla `DataProcesada` para asegurar un procesamiento masivo eficiente.
- Al terminar el bulk insert exitosamente, actualiza el estado a `Cargado` (vía Stored Procedure).
- Una vez completadas todas las validaciones posteriores (o inmediatamente si no hay pasos adicionales), actualiza el estado a `Finalizado` (vía Stored Procedure). Estos son dos transiciones de estado diferenciadas y secuenciales: `En proceso` -> `Cargado` -> `Finalizado`.

## Uso obligatorio de Procedimientos Almacenados:
- Crea los Stored Procedures `sp_ActualizarEstadoCarga` (parámetros: IdCarga, NuevoEstado, FechaFin opcional) y `sp_ObtenerHistorialCargas` (parámetros de paginación/filtro), invocados desde Dapper. Esto cumple el requisito obligatorio del reto de usar procedimientos almacenados en la base de datos.

## Evento de Finalización:
- Publica un mensaje `CargaFinalizadaEvent` (idCarga, usuario, fechaFin) en la cola `notificaciones`.
