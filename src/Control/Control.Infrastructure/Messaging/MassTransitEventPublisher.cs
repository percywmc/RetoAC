using Control.Application.Abstractions;
using Control.Infrastructure.Options;
using MassTransit;
using Microsoft.Extensions.Options;
using RetoAC.IntegrationEvents;

namespace Control.Infrastructure.Messaging;

public class MassTransitEventPublisher : IIntegrationEventPublisher
{
    private readonly IBus _bus;
    private readonly RabbitMqOptions _options;

    public MassTransitEventPublisher(IBus bus, IOptions<RabbitMqOptions> options)
    {
        _bus = bus;
        _options = options.Value;
    }

    public async Task PublishCargaRegistradaAsync(CargaRegistradaEvent evento, CancellationToken cancellationToken)
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri($"queue:{_options.CargaMasivaQueue}"));
        await endpoint.Send(evento, cancellationToken);
    }
}
