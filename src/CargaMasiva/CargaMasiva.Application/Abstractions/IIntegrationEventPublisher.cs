using RetoAC.IntegrationEvents;

namespace CargaMasiva.Application.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishCargaFinalizadaAsync(CargaFinalizadaEvent evento, CancellationToken cancellationToken);
}
