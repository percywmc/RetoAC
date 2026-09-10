```markdown
# Prompt 8: Pruebas Unitarias (xUnit)

Genera proyectos de pruebas unitarias con **xUnit**, **Moq** (o NSubstitute) y **FluentAssertions** para cada microservicio, siguiendo la convención `NombreProyecto.Tests` referenciando el proyecto `Application` correspondiente.

## Alcance por microservicio

### Auth
- `LoginCommandHandler`: credenciales válidas retorna JWT; credenciales inválidas lanza excepción/retorna resultado fallido.
- `RefreshTokenCommandHandler`: token vigente genera nuevo par de tokens; token expirado/revocado es rechazado.

### Control
- Validaciones de `FluentValidation` para el comando de subida (extensión inválida, tamaño excedido).
- `CrearCargaCommandHandler`: registra `CargaArchivo` con estado `Pendiente`, invoca el repositorio y publica el evento (verificar con `Moq` que `IPublishEndpoint`/bus fue invocado).
- Handler de autorización por rol (verificar rechazo si el usuario no tiene el rol requerido).

### Carga Masiva (el más crítico — cubrir exhaustivamente)
- Validación de Periodo: casos para estado `Cargado`/`Finalizado`/`Notificado` (rechazo), `Pendiente`/`En proceso` (bloqueo), sin conflicto (continúa).
- Validación de `CodigoProducto` duplicado: fila con código existente se marca `Existente` y no se inserta.
- Manejo de campos vacíos: se asigna valor por defecto.
- Filas completamente vacías: se ignoran, no se insertan ni se cuentan como error.
- Transición de estados: `En proceso` -> `Cargado` -> `Finalizado` en el orden correcto (mockear el repositorio/Stored Procedure y verificar las llamadas).

### Notificaciones
- Consumidor: al recibir `CargaFinalizadaEvent`, se invoca el servicio de email y luego se actualiza el estado a `Notificado`.
- Si el envío de correo falla, se verifica el comportamiento de reintento/logging sin actualizar el estado.

## Convenciones
- Nomenclatura de test: `MetodoTesteado_Escenario_ResultadoEsperado`.
- Arrange-Act-Assert explícito en cada test.
- Un proyecto de test por microservicio (ej. `Control.Application.Tests`, `CargaMasiva.Application.Tests`), sin dependencias a infraestructura real (todo mockeado).
- Configura el proyecto para integrarse con `dotnet test` desde la raíz de la solución.
```
