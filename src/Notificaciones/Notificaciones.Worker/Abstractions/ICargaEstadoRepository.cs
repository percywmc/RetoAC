namespace Notificaciones.Worker.Abstractions;

public interface ICargaEstadoRepository
{
    Task<string?> ObtenerEstadoAsync(Guid idCarga, CancellationToken cancellationToken);
    Task MarcarNotificadoAsync(Guid idCarga, CancellationToken cancellationToken);
}
