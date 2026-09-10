using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Persistence.Configurations;

public class CargaArchivoConfiguration : IEntityTypeConfiguration<CargaArchivo>
{
    public void Configure(EntityTypeBuilder<CargaArchivo> builder)
    {
        builder.ToTable("CargaArchivo");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.NombreArchivo).IsRequired().HasMaxLength(260);
        builder.Property(c => c.Usuario).IsRequired().HasMaxLength(256);
        builder.Property(c => c.RutaArchivo).IsRequired().HasMaxLength(500);
        builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(c => c.MotivoRechazo).HasMaxLength(500);

        builder.HasIndex(c => c.Usuario);
    }
}
