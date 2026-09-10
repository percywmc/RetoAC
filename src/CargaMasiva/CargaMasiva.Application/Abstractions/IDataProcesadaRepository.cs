using CargaMasiva.Domain.Entities;

namespace CargaMasiva.Application.Abstractions;

public interface IDataProcesadaRepository
{
    Task<HashSet<string>> GetCodigosExistentesAsync(IEnumerable<string> codigos, CancellationToken cancellationToken);
    Task BulkInsertAsync(IReadOnlyCollection<DataProcesada> registros, CancellationToken cancellationToken);
}
