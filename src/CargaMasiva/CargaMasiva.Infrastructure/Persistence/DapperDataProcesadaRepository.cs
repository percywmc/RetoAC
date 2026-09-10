using CargaMasiva.Application.Abstractions;
using CargaMasiva.Domain.Entities;
using CargaMasiva.Infrastructure.Options;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CargaMasiva.Infrastructure.Persistence;

public class DapperDataProcesadaRepository : IDataProcesadaRepository
{
    private readonly string _connectionString;

    public DapperDataProcesadaRepository(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ControlDatabase;
    }

    public async Task<HashSet<string>> GetCodigosExistentesAsync(IEnumerable<string> codigos, CancellationToken cancellationToken)
    {
        var listaCodigos = codigos.ToList();
        if (listaCodigos.Count == 0)
        {
            return new HashSet<string>();
        }

        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            "SELECT CodigoProducto FROM DataProcesada WHERE CodigoProducto IN @Codigos",
            new { Codigos = listaCodigos },
            cancellationToken: cancellationToken);

        var existentes = await connection.QueryAsync<string>(command);
        return existentes.ToHashSet();
    }

    public async Task BulkInsertAsync(IReadOnlyCollection<DataProcesada> registros, CancellationToken cancellationToken)
    {
        if (registros.Count == 0)
        {
            return;
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var table = new System.Data.DataTable();
        table.Columns.Add("Id", typeof(Guid));
        table.Columns.Add("IdCarga", typeof(Guid));
        table.Columns.Add("CodigoProducto", typeof(string));
        table.Columns.Add("NombreProducto", typeof(string));
        table.Columns.Add("Precio", typeof(decimal));
        table.Columns.Add("Periodo", typeof(string));
        table.Columns.Add("FechaRegistro", typeof(DateTime));

        foreach (var registro in registros)
        {
            table.Rows.Add(
                registro.Id, registro.IdCarga, registro.CodigoProducto,
                (object?)registro.NombreProducto ?? DBNull.Value,
                (object?)registro.Precio ?? DBNull.Value,
                (object?)registro.Periodo ?? DBNull.Value,
                registro.FechaRegistro);
        }

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "DataProcesada"
        };

        foreach (System.Data.DataColumn column in table.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(table, cancellationToken);
    }
}
