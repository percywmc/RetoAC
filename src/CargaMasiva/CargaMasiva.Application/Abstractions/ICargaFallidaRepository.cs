using CargaMasiva.Domain.Entities;

namespace CargaMasiva.Application.Abstractions;

public interface ICargaFallidaRepository
{
    Task AddRangeAsync(IReadOnlyCollection<CargaFallida> registros, CancellationToken cancellationToken);
}
