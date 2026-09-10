using CargaMasiva.Domain.Enums;

namespace CargaMasiva.Application.Abstractions;

public interface ICargaEstadoService
{
    Task ActualizarEstadoAsync(Guid idCarga, EstadoCarga nuevoEstado, DateTime? fechaFin, CancellationToken cancellationToken);
    Task<EstadoCarga?> ObtenerEstadoAsync(Guid idCarga, CancellationToken cancellationToken);
    Task<(Guid IdCarga, EstadoCarga Estado)?> ObtenerUltimaCargaPorPeriodoAsync(string periodo, Guid idCargaActual, CancellationToken cancellationToken);
}
