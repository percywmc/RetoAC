using CargaMasiva.Application.Abstractions;
using CargaMasiva.Domain.Entities;
using CargaMasiva.Infrastructure.Options;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CargaMasiva.Infrastructure.Persistence;

public class DapperCargaFallidaRepository : ICargaFallidaRepository
{
    private readonly string _connectionString;

    public DapperCargaFallidaRepository(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ControlDatabase;
    }

    public async Task AddRangeAsync(IReadOnlyCollection<CargaFallida> registros, CancellationToken cancellationToken)
    {
        if (registros.Count == 0)
        {
            return;
        }

        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            @"INSERT INTO CargaFallida (Id, IdCarga, Motivo, Detalle, FechaRegistro)
              VALUES (@Id, @IdCarga, @Motivo, @Detalle, @FechaRegistro)",
            registros.Select(r => new
            {
                r.Id,
                r.IdCarga,
                Motivo = r.Motivo.ToString(),
                r.Detalle,
                r.FechaRegistro
            }),
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}
