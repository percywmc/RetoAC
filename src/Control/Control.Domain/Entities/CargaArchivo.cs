using Control.Domain.Enums;

namespace Control.Domain.Entities;

public class CargaArchivo
{
    public Guid Id { get; private set; }
    public string NombreArchivo { get; private set; } = default!;
    public string Usuario { get; private set; } = default!;
    public string RutaArchivo { get; private set; } = default!;
    public EstadoCarga Estado { get; private set; }
    public DateTime FechaRegistro { get; private set; }
    public DateTime? FechaFin { get; private set; }
    public string? MotivoRechazo { get; private set; }

    private CargaArchivo() { }

    public CargaArchivo(Guid id, string nombreArchivo, string usuario, string rutaArchivo)
    {
        Id = id;
        NombreArchivo = nombreArchivo;
        Usuario = usuario;
        RutaArchivo = rutaArchivo;
        Estado = EstadoCarga.Pendiente;
        FechaRegistro = DateTime.UtcNow;
    }

    public void MarcarEnProceso()
    {
        Estado = EstadoCarga.EnProceso;
    }

    public void MarcarCargado()
    {
        Estado = EstadoCarga.Cargado;
    }

    public void MarcarFinalizado()
    {
        Estado = EstadoCarga.Finalizado;
        FechaFin = DateTime.UtcNow;
    }

    public void MarcarNotificado()
    {
        Estado = EstadoCarga.Notificado;
    }

    public void Rechazar(string motivo)
    {
        Estado = EstadoCarga.Rechazado;
        MotivoRechazo = motivo;
        FechaFin = DateTime.UtcNow;
    }
}
