-- =============================================================================
-- Atlantic City Casino Sports — Reto Técnico Backend Senior 2026
-- Script DDL de referencia: ControlDb + AuthDb
--
-- IMPORTANTE: Este script es REFERENCIA DOCUMENTAL.
-- Las tablas de AuthDb (Users, RefreshTokens) se crean automáticamente
-- via migraciones de EF Core al iniciar Auth.Api.
-- Las tablas de ControlDb (CargaArchivo) se crean automáticamente
-- via migraciones de EF Core al iniciar Control.Api.
-- Las tablas DataProcesada y CargaFallida, junto con los Stored Procedures,
-- se aplican mediante el script embebido en CargaMasiva.Infrastructure al iniciar.
-- =============================================================================

-- =============================================================================
-- BASE DE DATOS: AuthDb
-- Gestiona usuarios y refresh tokens del Microservicio de Autenticación.
-- =============================================================================

-- Tabla: Users
-- Creada por EF Core Migration en Auth.Infrastructure
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Username        NVARCHAR(256)    NOT NULL,
        Email           NVARCHAR(256)    NOT NULL,
        PasswordHash    NVARCHAR(512)    NOT NULL,
        Role            NVARCHAR(100)    NOT NULL DEFAULT 'CargaMasiva.Ejecutor',
        CreatedAt       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_Users_Username UNIQUE (Username),
        CONSTRAINT UQ_Users_Email    UNIQUE (Email)
    );
END
GO

-- Tabla: RefreshTokens
-- Creada por EF Core Migration en Auth.Infrastructure
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserId      UNIQUEIDENTIFIER NOT NULL,
        Token       NVARCHAR(512)    NOT NULL,
        ExpiresAt   DATETIME2        NOT NULL,
        IsRevoked   BIT              NOT NULL DEFAULT 0,
        CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_RefreshTokens_Token UNIQUE (Token)
    );

    CREATE INDEX IX_RefreshTokens_UserId  ON RefreshTokens(UserId);
    CREATE INDEX IX_RefreshTokens_Token   ON RefreshTokens(Token);
END
GO

-- =============================================================================
-- BASE DE DATOS: ControlDb
-- Gestiona trazabilidad de cargas, datos procesados y auditoría.
-- =============================================================================

-- Tabla: CargaArchivo (trazabilidad principal)
-- Creada por EF Core Migration en Control.Infrastructure
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CargaArchivo')
BEGIN
    CREATE TABLE CargaArchivo (
        Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        NombreArchivo   NVARCHAR(260)    NOT NULL,
        Usuario         NVARCHAR(256)    NOT NULL,
        Estado          NVARCHAR(30)     NOT NULL DEFAULT 'Pendiente',
        FechaRegistro   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        FechaFin        DATETIME2        NULL,
        RutaArchivo     NVARCHAR(1000)   NULL,
        MotivoRechazo   NVARCHAR(500)    NULL,
        CONSTRAINT CK_CargaArchivo_Estado CHECK (
            Estado IN ('Pendiente','En proceso','Cargado','Finalizado','Notificado','Rechazado')
        )
    );

    CREATE INDEX IX_CargaArchivo_Usuario  ON CargaArchivo(Usuario);
    CREATE INDEX IX_CargaArchivo_Estado   ON CargaArchivo(Estado);
END
GO

-- Tabla: DataProcesada (registros extraídos del Excel via Bulk Insert con Dapper)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DataProcesada')
BEGIN
    CREATE TABLE DataProcesada (
        Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        IdCarga         UNIQUEIDENTIFIER NOT NULL,
        CodigoProducto  NVARCHAR(100)    NOT NULL,
        NombreProducto  NVARCHAR(500)    NULL,
        Precio          DECIMAL(18,2)    NULL,
        Periodo         NVARCHAR(32)     NULL,
        FechaRegistro   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_DataProcesada_CargaArchivo
            FOREIGN KEY (IdCarga) REFERENCES CargaArchivo(Id)
    );

    CREATE INDEX IX_DataProcesada_CodigoProducto ON DataProcesada(CodigoProducto);
    CREATE INDEX IX_DataProcesada_IdCarga        ON DataProcesada(IdCarga);
END
GO

-- Tabla: CargaFallida (auditoría de rechazos y duplicados)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CargaFallida')
BEGIN
    CREATE TABLE CargaFallida (
        Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        IdCarga         UNIQUEIDENTIFIER NOT NULL,
        Motivo          NVARCHAR(50)     NOT NULL,   -- 'DuplicadoPeriodo', 'DuplicadoCodigo', 'FilaInvalida'
        Detalle         NVARCHAR(500)    NOT NULL,
        FechaRegistro   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_CargaFallida_CargaArchivo
            FOREIGN KEY (IdCarga) REFERENCES CargaArchivo(Id)
    );

    CREATE INDEX IX_CargaFallida_IdCarga ON CargaFallida(IdCarga);
END
GO

-- Tabla: RegistroFallido (auditoría a nivel de fila)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RegistroFallido')
BEGIN
    CREATE TABLE RegistroFallido (
        Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        IdCarga         UNIQUEIDENTIFIER NOT NULL,
        Fila            INT              NOT NULL,
        Motivo          INT              NOT NULL,
        Detalle         NVARCHAR(500)    NOT NULL,
        DatosOriginales NVARCHAR(MAX)    NULL,
        FechaRegistro   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_RegistroFallido_CargaArchivo
            FOREIGN KEY (IdCarga) REFERENCES CargaArchivo(Id)
    );

    CREATE INDEX IX_RegistroFallido_IdCarga ON RegistroFallido(IdCarga);
END
GO

-- =============================================================================
-- STORED PROCEDURE: sp_ActualizarEstadoCarga
-- Invocado por: CargaMasiva.Worker (vía Dapper) y Notificaciones.Worker (vía Dapper)
-- Transiciones: Pendiente → En proceso → Cargado → Finalizado → Notificado
-- =============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ActualizarEstadoCarga')
    DROP PROCEDURE sp_ActualizarEstadoCarga
GO

CREATE PROCEDURE sp_ActualizarEstadoCarga
    @IdCarga    UNIQUEIDENTIFIER,
    @NuevoEstado NVARCHAR(30),
    @FechaFin   DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE CargaArchivo
    SET    Estado   = @NuevoEstado,
           FechaFin = COALESCE(@FechaFin, FechaFin)
    WHERE  Id = @IdCarga;
END
GO

-- =============================================================================
-- STORED PROCEDURE: sp_ObtenerHistorialCargas
-- Invocado por: Control.Api (vía Dapper) para el endpoint GET /api/v1/cargas
-- Soporta filtrado por usuario (NULL = todos los registros, para rol Admin)
-- y paginación con OFFSET/FETCH.
-- =============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerHistorialCargas')
    DROP PROCEDURE sp_ObtenerHistorialCargas
GO

CREATE PROCEDURE sp_ObtenerHistorialCargas
    @Usuario    NVARCHAR(256) = NULL,
    @PageNumber INT           = 1,
    @PageSize   INT           = 20
AS
BEGIN
    SET NOCOUNT ON;

    -- Resultado paginado
    SELECT Id,
           NombreArchivo,
           Usuario,
           Estado,
           FechaRegistro,
           FechaFin
    FROM   CargaArchivo
    WHERE  (@Usuario IS NULL OR Usuario = @Usuario)
    ORDER  BY FechaRegistro DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH  NEXT @PageSize ROWS ONLY;

    -- Total de registros (para paginación en el cliente)
    SELECT COUNT(*) AS TotalCount
    FROM   CargaArchivo
    WHERE  (@Usuario IS NULL OR Usuario = @Usuario);
END
GO

-- =============================================================================
-- DATOS SEMILLA (referencia — el seeder de Auth.Api los inserta automáticamente)
-- Usuario de prueba: admin / Admin#2026 / Rol: CargaMasiva.Ejecutor
-- El hash BCrypt real es generado por el seeder en tiempo de ejecución.
-- =============================================================================
/*
INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedAt)
VALUES (
    NEWID(),
    'admin',
    'admin@atlanticcity.local',
    '<bcrypt-hash-generado-por-el-seeder>',
    'CargaMasiva.Ejecutor',
    GETUTCDATE()
);
*/
