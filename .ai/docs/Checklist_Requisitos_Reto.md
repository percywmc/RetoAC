# Checklist Maestro de Requisitos — Reto Técnico Backend Senior (Casino Atlantic City)

> Documento generado a partir de `RetoTecnicoBackendSenior.md`. Sirve como fuente única de verdad para validar que los prompts en `.ai/prompts/` cubren el 100% de lo solicitado (obligatorio + opcional). Cada item tiene un ID para trazabilidad cruzada con los prompts.

## Stack fijado para esta implementación
- **Framework:** .NET 8 (LTS, sin ambigüedad con .NET 9)
- **Base de datos:** SQL Server (con EF Core para migraciones + Dapper para bulk/alto rendimiento + Procedimientos Almacenados)
- **Mensajería:** RabbitMQ (vía MassTransit)
- **Almacenamiento:** SeaweedFS
- **Frontend:** React (se implementa, no solo Postman) + colección Postman como respaldo
- **Pruebas unitarias:** incluidas aunque no las pide el PDF (valor agregado)

---

## 1. Objetivo del reto
- [R1] Sistema de microservicios: subida de Excel → cola asíncrona → persistencia → notificación por correo.
- [R2] Debe ser 100% funcional, dockerizado, con buenas prácticas senior.

## 2. Componentes de arquitectura

### 2.1 Cliente Web (React) — Opcional pero valorado
- [R3] Login
- [R4] Subida de archivo Excel (.xlsx)
- [R5] Historial de cargas (tabla)
- [R6] Consultar contenido del archivo Excel subido
- [R7] Ver estado de cada procesamiento: Pendiente, En proceso, Cargado, Finalizado, Notificado
- [R8] Polling para estados en tiempo real

### 2.2 API Gateway (.NET 8)
- [R9] Punto de entrada centralizado
- [R10] Validación de JWT
- [R11] Reenvío de peticiones al microservicio correspondiente
- [R12] Rate Limiting (obligatorio)

### 2.3 Microservicio Auth (.NET 8)
- [R13] `POST /auth/login` — valida credenciales contra fuente de identidad real (BD)
- [R14] Genera JWT Bearer con claims de usuario (UserId, Email, Roles)
- [R15] `POST /auth/refresh` — Refresh Token (opcional pero valorado → **incluir**)

### 2.4 Microservicio Control / Publicador (.NET 8)
- [R16] Recibe solicitud de carga desde Gateway
- [R17] Valida que el usuario tenga **permiso/rol** para ejecutar cargas masivas (autorización)
- [R18] Valida tamaño máximo del archivo
- [R19] Valida extensión correcta (.xlsx)
- [R20] Registra auditoría (quién subió, cuándo)
- [R21] Guarda registro de trazabilidad con estado inicial `Pendiente`
- [R22] Sube archivo a SeaweedFS (HttpClient + Polly: Retry y Circuit Breaker — opcional valorado → **incluir**)
- [R23] Publica mensaje en cola `carga_masiva`: `{idCarga, rutaArchivo, usuario}`
- [R24] Expone endpoint de historial de cargas (query)
- [R25] Expone endpoint para consultar contenido del Excel subido

### 2.5 Microservicio Carga Masiva (Consumidor/Publicador) (.NET 8)
- [R26] Escucha cola `carga_masiva`
- [R27] **Validación de duplicidad por Periodo:**
  - Si existe carga previa con estado `Cargado`, `Finalizado` o `Notificado` → **RECHAZAR**
  - Si existe carga previa `Pendiente` o `En proceso` → **BLOQUEAR** (evitar concurrencia)
  - Si no hay conflicto → continuar y registrar `Pendiente`
  - Fallos → registrar en tabla de auditoría/trazabilidad
- [R28] **Validación de duplicidad de registros por CodigoProducto:**
  - Si el código ya existe → no registrar, reportar como `Existente`
  - Fallos → registrar en tabla de auditoría/trazabilidad
- [R29] Actualiza estado → `En proceso`
- [R30] Descarga archivo desde SeaweedFS
- [R31] Procesa filas: columna vacía → valor por defecto; fila totalmente vacía → ignorar
- [R32] Inserta datos vía **Dapper Bulk Insert** en `DataProcesada`
- [R33] Actualiza estado → `Cargado` (post-inserción) y luego `Finalizado` (fin de proceso) — **dos estados diferenciados**
- [R34] Publica mensaje en cola `notificaciones`: `{idCarga, usuario, fechaFin}`

### 2.6 Microservicio Notificaciones (Consumidor) (.NET 8)
- [R35] Escucha cola `notificaciones`
- [R36] Envía correo con **MailKit**
- [R37] Actualiza estado final → `Notificado`
- [R38] Configuración SMTP vía variables de entorno (Options Pattern)

### 2.7 Servicio de Colas (RabbitMQ)
- [R39] Mínimo 2 colas: `carga_masiva`, `notificaciones`
- [R40] Exchange directo o topic (definir explícitamente en MassTransit)

### 2.8 Base de Datos (SQL Server)
- [R41] Tabla `CargaArchivo` (trazabilidad: Id, NombreArchivo, Usuario, FechaRegistro, Estado, FechaFin, Periodo)
- [R42] Tabla `DetalleCarga` / auditoría de fallos (rechazos por periodo, duplicados por código)
- [R43] Tabla `DataProcesada` (registros extraídos del Excel)
- [R44] **Migraciones automáticas** (EF Core, aplicadas al iniciar)
- [R45] **Uso de Procedimientos Almacenados** (obligatorio — ej. actualización de estado, consultas de historial)
- [R46] Uso de Dapper y/o EF Core (ambos, cada uno en su capa apropiada)

### 2.9 SeaweedFS
- [R47] Servicio dockerizado, endpoint para subir archivos

---

## 3. Requerimientos Técnicos Obligatorios (todos los microservicios)
- [T1] .NET 8
- [T2] Clean Architecture
- [T3] CQRS (MediatR) + Inyección de dependencias
- [T4] Principios SOLID
- [T5] JWT Bearer (+ Refresh Token — valorado)
- [T6] Manejo global de excepciones (middleware)
- [T7] Logging estructurado (Serilog)
- [T8] Dockerfile propio por microservicio (opcional valorado → **incluir**)
- [T9] docker-compose general orquestando TODO (opcional valorado → **incluir completo, no comentado**)
- [T10] Rate Limiting (Gateway)
- [T11] Dapper y/o EF Core
- [T12] Circuit Breaker (Polly) — opcional valorado → **incluir**
- [T13] Retry Pattern (Polly) — opcional valorado → **incluir**

## 4. Frontend (Opcional pero valorado)
- [F1] React 16+
- [F2] Uso de componentes
- [F3] Pantalla Login
- [F4] Pantalla Subida de Excel
- [F5] Pantalla Historial de cargas (tabla)
- [F6] Pantalla Detalle del estado de una carga

## 5. Base de datos
- [D1] Migraciones automáticas
- [D2] Procedimientos almacenados
- [D3] SQL Server

## 6. Mensajería
- [M1] Exchange directo o topic
- [M2] Mínimo 2 colas

## 7. Almacenamiento
- [S1] SeaweedFS dockerizado con endpoint de subida

## 8. Correo
- [E1] MailKit
- [E2] Configurable por variables de entorno

## 9. Criterios de Evaluación (pesos)
- [C1] Arquitectura — 25% (microservicios independientes, código limpio, manejo de colas y estados)
- [C2] Funcionalidad — 35% (flujo completo operativo, procesamiento real del Excel, persistencia correcta)
- [C3] Docker/DevOps — 20% (docker-compose funcional, servicios levantan sin errores)
- [C4] Frontend o Postman — 20% (colección Postman completa o interfaz funcional con UX básica)

## 10. Entrega final
- [G1] Repositorio GitHub con código fuente completo
- [G2] README con documentación
- [G3] Instrucciones de despliegue (opcional valorado)
- [G4] Scripts de base de datos
- [G5] Postman collection (obligatoria si no hay frontend; recomendable igual como respaldo)
- [G6] Video corto (máx. 5 min) mostrando el flujo completo funcionando

## 11. Extra (no pedido, agregado por decisión propia)
- [X1] Pruebas unitarias (xUnit) sobre la capa Application (handlers CQRS) de cada microservicio, con mocks de repositorios/infraestructura.
