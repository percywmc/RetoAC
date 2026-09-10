using CargaMasiva.Application.Abstractions;
using CargaMasiva.Domain.Enums;
using CargaMasiva.Infrastructure.Options;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace CargaMasiva.Infrastructure.Persistence;

public class DapperCargaEstadoService : ICargaEstadoService
{
    private readonly string _connectionString;

    public DapperCargaEstadoService(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ControlDatabase;
    }

    public async Task ActualizarEstadoAsync(Guid idCarga, EstadoCarga nuevoEstado, DateTime? fechaFin, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        var parametros = new DynamicParameters();
        parametros.Add("IdCarga", idCarga);
        parametros.Add("NuevoEstado", nuevoEstado.ToString());
        parametros.Add("FechaFin", fechaFin);

        var command = new CommandDefinition(
            "sp_ActualizarEstadoCarga",
            parametros,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<EstadoCarga?> ObtenerEstadoAsync(Guid idCarga, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            "SELECT Estado FROM CargaArchivo WHERE Id = @IdCarga",
            new { IdCarga = idCarga },
            cancellationToken: cancellationToken);

        var resultado = await connection.QuerySingleOrDefaultAsync<string>(command);
        if (resultado is null)
        {
            return null;
        }

        return Enum.Parse<EstadoCarga>(resultado);
    }

    public async Task<(Guid IdCarga, EstadoCarga Estado)?> ObtenerUltimaCargaPorPeriodoAsync(
        string periodo, Guid idCargaActual, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            @"SELECT TOP 1 c.Id, c.Estado FROM CargaArchivo c
              INNER JOIN DataProcesada d ON c.Id = d.IdCarga
              WHERE d.Periodo = @Periodo AND c.Id <> @IdCargaActual
              ORDER BY c.FechaRegistro DESC",
            new { Periodo = periodo, IdCargaActual = idCargaActual },
            cancellationToken: cancellationToken);

        var resultado = await connection.QuerySingleOrDefaultAsync<(Guid Id, string Estado)?>(command);
        if (resultado is null)
        {
            return null;
        }

        return (resultado.Value.Id, Enum.Parse<EstadoCarga>(resultado.Value.Estado));
    }
}
