using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;

namespace CargaMasiva.Infrastructure.Persistence;

public static class CargaMasivaDbInitializer
{
    public static async Task InitializeAsync(string connectionString, ILogger logger, CancellationToken cancellationToken = default)
    {
        var retryPolicy = Policy
            .Handle<SqlException>()
            .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(5),
                (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning("Intento {RetryCount} de conexión a DB fallido. Reintentando en {TimeSpan}s...", retryCount, timeSpan.TotalSeconds);
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Esperar a que exista la tabla CargaArchivo (creada por Control.Api)
            var checkTableCmd = new SqlCommand("SELECT 1 FROM sys.tables WHERE name = 'CargaArchivo'", connection);
            while (await checkTableCmd.ExecuteScalarAsync(cancellationToken) == null)
            {
                logger.LogInformation("Esperando a que Control.Api cree la tabla CargaArchivo...");
                await Task.Delay(3000, cancellationToken);
            }

            var sqlCommands = new[]
            {
                @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DataProcesada')
                  BEGIN
                      CREATE TABLE DataProcesada (
                          Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                          IdCarga UNIQUEIDENTIFIER NOT NULL,
                          CodigoProducto NVARCHAR(100) NOT NULL,
                          NombreProducto NVARCHAR(500) NULL,
                          Precio DECIMAL(18,2) NULL,
                          Periodo NVARCHAR(32) NULL,
                          FechaRegistro DATETIME2 NOT NULL,
                          CONSTRAINT FK_DataProcesada_CargaArchivo FOREIGN KEY (IdCarga) REFERENCES CargaArchivo(Id)
                      );
                      CREATE INDEX IX_DataProcesada_CodigoProducto ON DataProcesada(CodigoProducto);
                      CREATE INDEX IX_DataProcesada_IdCarga ON DataProcesada(IdCarga);
                  END",
                
                @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CargaFallida')
                  BEGIN
                      CREATE TABLE CargaFallida (
                          Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                          IdCarga UNIQUEIDENTIFIER NOT NULL,
                          Motivo NVARCHAR(50) NOT NULL,
                          Detalle NVARCHAR(500) NOT NULL,
                          FechaRegistro DATETIME2 NOT NULL,
                          CONSTRAINT FK_CargaFallida_CargaArchivo FOREIGN KEY (IdCarga) REFERENCES CargaArchivo(Id)
                      );
                      CREATE INDEX IX_CargaFallida_IdCarga ON CargaFallida(IdCarga);
                  END",

                @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RegistroFallido')
                  BEGIN
                      CREATE TABLE RegistroFallido (
                          Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                          IdCarga UNIQUEIDENTIFIER NOT NULL,
                          Fila INT NOT NULL,
                          Motivo NVARCHAR(50) NOT NULL,
                          Detalle NVARCHAR(500) NOT NULL,
                          DatosOriginales NVARCHAR(MAX) NULL,
                          FechaRegistro DATETIME2 NOT NULL,
                          CONSTRAINT FK_RegistroFallido_CargaArchivo FOREIGN KEY (IdCarga) REFERENCES CargaArchivo(Id)
                      );
                      CREATE INDEX IX_RegistroFallido_IdCarga ON RegistroFallido(IdCarga);
                  END",

                @"IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ActualizarEstadoCarga')
                      DROP PROCEDURE sp_ActualizarEstadoCarga",

                @"CREATE PROCEDURE sp_ActualizarEstadoCarga
                      @IdCarga UNIQUEIDENTIFIER,
                      @NuevoEstado NVARCHAR(30),
                      @FechaFin DATETIME2 = NULL
                  AS
                  BEGIN
                      SET NOCOUNT ON;
                      UPDATE CargaArchivo
                      SET Estado = @NuevoEstado,
                          FechaFin = COALESCE(@FechaFin, FechaFin)
                      WHERE Id = @IdCarga;
                  END",
                  
                @"IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerHistorialCargas')
                      DROP PROCEDURE sp_ObtenerHistorialCargas",

                @"CREATE PROCEDURE sp_ObtenerHistorialCargas
                      @Usuario NVARCHAR(256) = NULL,
                      @PageNumber INT = 1,
                      @PageSize INT = 20
                  AS
                  BEGIN
                      SET NOCOUNT ON;
                      SELECT Id, NombreArchivo, Usuario, Estado, FechaRegistro, FechaFin
                      FROM CargaArchivo
                      WHERE (@Usuario IS NULL OR Usuario = @Usuario)
                      ORDER BY FechaRegistro DESC
                      OFFSET (@PageNumber - 1) * @PageSize ROWS
                      FETCH NEXT @PageSize ROWS ONLY;

                      SELECT COUNT(*) AS TotalCount
                      FROM CargaArchivo
                      WHERE (@Usuario IS NULL OR Usuario = @Usuario);
                  END"
            };

            foreach (var cmdText in sqlCommands)
            {
                using var command = new SqlCommand(cmdText, connection);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            logger.LogInformation("Tablas y Stored Procedures de CargaMasiva creados/verificados correctamente.");
        });
    }
}
