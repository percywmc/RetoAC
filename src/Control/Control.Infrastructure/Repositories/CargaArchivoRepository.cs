using Control.Domain.Entities;
using Control.Domain.Repositories;
using Control.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Repositories;

public class CargaArchivoRepository : ICargaArchivoRepository
{
    private readonly ControlDbContext _dbContext;

    public CargaArchivoRepository(ControlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CargaArchivo?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.CargasArchivo.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<CargaArchivo> Items, int TotalCount)> GetPagedAsync(
        string? usuario, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.CargasArchivo.AsQueryable();

        if (!string.IsNullOrWhiteSpace(usuario))
        {
            query = query.Where(c => c.Usuario == usuario);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.FechaRegistro)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(CargaArchivo cargaArchivo, CancellationToken cancellationToken)
    {
        await _dbContext.CargasArchivo.AddAsync(cargaArchivo, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
