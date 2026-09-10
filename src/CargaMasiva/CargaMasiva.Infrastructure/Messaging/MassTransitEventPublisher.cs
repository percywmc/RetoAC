using CargaMasiva.Application.Abstractions;
using CargaMasiva.Application.Dtos;
using CargaMasiva.Infrastructure.Options;
using MassTransit;
using Microsoft.Extensions.Options;
using RetoAC.IntegrationEvents;

namespace CargaMasiva.Infrastructure.Messaging;

public class MassTransitEventPublisher : IIntegrationEventPublisher
{
    private readonly IBus _bus;
    private readonly RabbitMqOptions _options;

    public MassTransitEventPublisher(IBus bus, IOptions<RabbitMqOptions> options)
    {
        _bus = bus;
        _options = options.Value;
    }

    public async Task PublishCargaFinalizadaAsync(CargaFinalizadaEvent evento, CancellationToken cancellationToken)
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri($"queue:{_options.NotificacionesQueue}"));
        await endpoint.Send(evento, cancellationToken);
    }
}
