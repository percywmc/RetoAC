using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Persistence;

public class ControlDbContext : DbContext
{
    public ControlDbContext(DbContextOptions<ControlDbContext> options) : base(options)
    {
    }

    public DbSet<CargaArchivo> CargasArchivo => Set<CargaArchivo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ControlDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
