using CargaMasiva.Domain.Enums;

namespace CargaMasiva.Domain.Entities;

public class RegistroFallido
{
    public Guid Id { get; private set; }
    public Guid IdCarga { get; private set; }
    public int Fila { get; private set; }
    public MotivoFallo Motivo { get; private set; }
    public string Detalle { get; private set; } = default!;
    public string? DatosOriginales { get; private set; }
    public DateTime FechaRegistro { get; private set; }

    private RegistroFallido() { }

    public RegistroFallido(Guid idCarga, int fila, MotivoFallo motivo, string detalle, string? datosOriginales)
    {
        Id = Guid.NewGuid();
        IdCarga = idCarga;
        Fila = fila;
        Motivo = motivo;
        Detalle = detalle;
        DatosOriginales = datosOriginales;
        FechaRegistro = DateTime.UtcNow;
    }
}
