using CargaMasiva.Application.Abstractions;
using CargaMasiva.Domain.Entities;
using CargaMasiva.Infrastructure.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CargaMasiva.Infrastructure.Persistence;

public class DapperRegistroFallidoRepository : IRegistroFallidoRepository
{
    private readonly string _connectionString;

    public DapperRegistroFallidoRepository(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ControlDatabase;
    }

    public async Task BulkInsertAsync(IReadOnlyCollection<RegistroFallido> registros, CancellationToken cancellationToken)
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
        table.Columns.Add("Fila", typeof(int));
        table.Columns.Add("Motivo", typeof(string));
        table.Columns.Add("Detalle", typeof(string));
        table.Columns.Add("DatosOriginales", typeof(string));
        table.Columns.Add("FechaRegistro", typeof(DateTime));

        foreach (var registro in registros)
        {
            table.Rows.Add(
                registro.Id, 
                registro.IdCarga, 
                registro.Fila,
                registro.Motivo.ToString(),
                registro.Detalle,
                (object?)registro.DatosOriginales ?? DBNull.Value,
                registro.FechaRegistro);
        }

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "RegistroFallido"
        };

        foreach (System.Data.DataColumn column in table.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(table, cancellationToken);
    }
}
