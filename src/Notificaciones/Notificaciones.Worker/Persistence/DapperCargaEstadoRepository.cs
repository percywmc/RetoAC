using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Notificaciones.Worker.Abstractions;
using Notificaciones.Worker.Options;

namespace Notificaciones.Worker.Persistence;

public class DapperCargaEstadoRepository : ICargaEstadoRepository
{
    private readonly string _connectionString;

    public DapperCargaEstadoRepository(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ControlDatabase;
    }

    /// <summary>
    /// Consulta el estado actual de una carga para implementar idempotencia en el consumidor.
    /// Si el estado ya es "Notificado", el consumer no re-enviará el correo ni actualizará la BD.
    /// </summary>
    public async Task<string?> ObtenerEstadoAsync(Guid idCarga, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);

        var command = new CommandDefinition(
            "SELECT Estado FROM CargaArchivo WHERE Id = @IdCarga",
            new { IdCarga = idCarga },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<string>(command);
    }

    /// <summary>
    /// Actualiza el estado de la carga a "Notificado" invocando el Stored Procedure
    /// <c>sp_ActualizarEstadoCarga</c>, compartido con el Microservicio de Carga Masiva.
    /// </summary>
    public async Task MarcarNotificadoAsync(Guid idCarga, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);

        var parametros = new DynamicParameters();
        parametros.Add("IdCarga", idCarga);
        parametros.Add("NuevoEstado", "Notificado");
        parametros.Add("FechaFin", (DateTime?)null);

        var command = new CommandDefinition(
            "sp_ActualizarEstadoCarga",
            parametros,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}
