using CargaMasiva.Domain.Enums;

namespace CargaMasiva.Domain.Entities;

public class CargaFallida
{
    public Guid Id { get; private set; }
    public Guid IdCarga { get; private set; }
    public MotivoFallo Motivo { get; private set; }
    public string Detalle { get; private set; } = default!;
    public DateTime FechaRegistro { get; private set; }

    private CargaFallida() { }

    public CargaFallida(Guid idCarga, MotivoFallo motivo, string detalle)
    {
        Id = Guid.NewGuid();
        IdCarga = idCarga;
        Motivo = motivo;
        Detalle = detalle;
        FechaRegistro = DateTime.UtcNow;
    }
}
