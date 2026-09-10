using RetoAC.IntegrationEvents;

namespace Control.Application.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishCargaRegistradaAsync(CargaRegistradaEvent evento, CancellationToken cancellationToken);
}
