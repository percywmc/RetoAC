using Control.Domain.Entities;

namespace Control.Domain.Repositories;

public interface ICargaArchivoRepository
{
    Task<CargaArchivo?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<CargaArchivo> Items, int TotalCount)> GetPagedAsync(
        string? usuario, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(CargaArchivo cargaArchivo, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
