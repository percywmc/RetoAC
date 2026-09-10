using CargaMasiva.Domain.Entities;

namespace CargaMasiva.Application.Abstractions;

public interface IRegistroFallidoRepository
{
    Task BulkInsertAsync(IReadOnlyCollection<RegistroFallido> registros, CancellationToken cancellationToken);
}
